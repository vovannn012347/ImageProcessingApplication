using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Common;
using Common.Model.Areas;

using Newtonsoft.Json;

using ProcessorAlgorithm;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;
using System.IO;
using Common.Model;
using System.Net.Http.Headers;

namespace TelegramBot
{
    public class ChatState
    {
        public RequestedOperation Operation { get; set; }
        public string loginJwtToken { get; set; }

    }

    public class RequestedOperation
    {
        public string OperationId { get; set; }
        public List<ArgumentDefinition> ArgumentDefinitionsInput { get; set; }
        public List<ArgumentDefinition> ArgumentDefinitionsOutput { get; set; }
        public Dictionary<string, Argument> Arguments { get; set; } = new Dictionary<string, Argument>();
        public Dictionary<string, string> Files { get; set; } = new Dictionary<string, string>();
    }

    public class TelegramBot
    {
        private readonly TelegramBotClient _botClient;
        private CancellationTokenSource cts;
        private string ServerJwtToken;

        public TelegramBot(string token)
        {
            _botClient = new TelegramBotClient(token);
        }

        public async Task StartReceiving()
        {
            await GetServerJwtToken();

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.WriteLine("Bot is receiving updates...");
        }

        private async Task GetServerJwtToken()
        {
            using (var _httpClient = new HttpClient())
            {
                var loginModel = new LoginModel
                {
                    Login = SecretConstants.BotUserName,
                    Password = SecretConstants.BotPassword
                };
                var json = JsonConvert.SerializeObject(loginModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var serverUrl = "https://localhost:44374/api/user";

                var response = await _httpClient.PostAsync(serverUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadAsStringAsync();
                    var token = JsonConvert.DeserializeObject<TokenModel>(tokenResponse);
                    ServerJwtToken = token.Token;
                }
            }

            return;
        }

        private static readonly Dictionary<long, ChatState> UserDatabase = new Dictionary<long, ChatState>();


        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                var userId = update.Message.From.Id;
                var chatId = update.Message.Chat.Id;

                if (!UserDatabase.ContainsKey(userId))
                {
                    using (var _httpClient = new HttpClient())
                    {
                        var user = update.Message.From;
                        var loginModel = new TelegramLoginModel
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            UserName = user.Username
                        };
                        var json = JsonConvert.SerializeObject(loginModel);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var serverUrl = "https://localhost:44374/api/user/telegram";

                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServerJwtToken);
                        var response = await _httpClient.PostAsync(serverUrl, content);
                        if (response.IsSuccessStatusCode)
                        {
                            var tokenResponse = await response.Content.ReadAsStringAsync();
                            var token = JsonConvert.DeserializeObject<TokenModel>(tokenResponse);

                            UserDatabase.Add(user.Id, new ChatState
                            {
                                loginJwtToken = token.Token
                            });
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Failed to login to server.",
                                cancellationToken: cancellationToken
                            );
                            return;
                        }
                    }
                }

                string messageText;
                if (update.Message.Type == MessageType.Photo)
                {
                    messageText = update.Message.Caption?.ToLower();
                }
                else
                {
                    messageText = update.Message.Text?.ToLower();
                }

                switch (messageText)
                {
                    case "/start":
                        await botClient.SendTextMessageAsync(chatId, "Welcome! Type /help to see available commands.", cancellationToken: cancellationToken);
                        break;

                    case "/help":
                        await botClient.SendTextMessageAsync(chatId, 
                            "Available commands:\n" +
                            "/start - Start the bot\n" +
                            "/help - List commands\n" +
                            "/list - list available algorithms.\n" +
                            "/process [algorithm] - select algorithm to process, follow instructions to load arguments\n" +
                            "/process - list required arguments (including).\n" +
                            "/cancel - stop collecting arguments for algorithm", 
                            cancellationToken: cancellationToken);
                        break;

                    case "/list":
                        var userData4 = UserDatabase[userId];
                        if (userData4.loginJwtToken != null)
                        {
                            using (var _httpClient = new HttpClient())
                            {
                                var serverUrl = $"https://localhost:44374/api/process";

                                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userData4.loginJwtToken);
                                var response = await _httpClient.GetAsync(serverUrl);

                                if (response.IsSuccessStatusCode)
                                {
                                    var resultResponse = await response.Content.ReadAsStringAsync();
                                    var processReturn = JsonConvert.DeserializeObject<List<ProcessReturn>>(resultResponse);


                                    StringBuilder operationStringBuilder = new StringBuilder("List of algorithms:\n");

                                    foreach (var algorithm in processReturn)
                                    {
                                        operationStringBuilder.AppendLine($"{algorithm.ProcessId}: {algorithm.FriendlyName}");
                                    }

                                    await botClient.SendTextMessageAsync(chatId, operationStringBuilder.ToString(), cancellationToken: cancellationToken);
                                }
                            }
                        }
                        break;

                    case "/process":
                        var userData2 = UserDatabase[userId];
                        if (userData2.Operation != null)
                        {
                            StringBuilder operationStringBuilder = new StringBuilder("List of arguments to enter, enter one by one - strings and numbers in form of message \"[argumentname] [argument value]\" separated by space without brackets, images and files in form of attached file with \"[argument id]\" as message:\n");

                            foreach (var inputArgument in userData2.Operation.ArgumentDefinitionsInput)
                            {
                                if (userData2.Operation.Arguments.ContainsKey(inputArgument.Name))
                                {
                                    operationStringBuilder.AppendLine($"{inputArgument.Name}: {inputArgument.Type.ToString()} \n");
                                }
                                else
                                {
                                    var argument = userData2.Operation.Arguments[inputArgument.Name];
                                    operationStringBuilder.AppendLine($"{inputArgument.Name}: {inputArgument.Type.ToString()} ({argument.Value})\n");
                                }
                            }
                            await botClient.SendTextMessageAsync(chatId, operationStringBuilder.ToString(), cancellationToken: cancellationToken);

                            break;
                        }
                        await botClient.SendTextMessageAsync(chatId, "No operation pending", cancellationToken: cancellationToken);
                        break;

                    case "/cancel":
                        var userData1 = UserDatabase[userId];
                        if (userData1.Operation != null)
                        {
                            if (userData1.Operation.Files.Any())
                            {
                                foreach(var keyvalueP in userData1.Operation.Files)
                                {
                                    var filePath = Path.Combine("Bot_Data", keyvalueP.Value);
                                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                                }
                            }

                            userData1.Operation = null;
                            await botClient.SendTextMessageAsync(chatId, "Processing cancelled", cancellationToken: cancellationToken);
                            break;
                        }

                        await botClient.SendTextMessageAsync(chatId, $"No operation pending", cancellationToken: cancellationToken);
                        break;

                    default:
                        if (messageText.StartsWith("/process", StringComparison.OrdinalIgnoreCase))
                        {
                            var userData = UserDatabase[userId];
                            if(userData.Operation != null)
                            {
                                await botClient.SendTextMessageAsync(chatId, $"Awaiting input for operation: {userData.Operation.OperationId}", cancellationToken: cancellationToken);
                                break;
                            }

                            var process_id = messageText.Substring("/process".Length).Trim();

                            using (var _httpClient = new HttpClient())
                            {

                                var serverUrl = $"https://localhost:44374/api/process/{process_id}";

                                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userData.loginJwtToken);
                                var response = await _httpClient.GetAsync(serverUrl);

                                if (response.IsSuccessStatusCode)
                                {
                                    var resultResponse = await response.Content.ReadAsStringAsync();
                                    var processReturn = JsonConvert.DeserializeObject<ProcessReturn>(resultResponse);

                                    var operation = new RequestedOperation()
                                    {
                                        OperationId = process_id,
                                        ArgumentDefinitionsInput = processReturn.InputParams?.ToList(),
                                        ArgumentDefinitionsOutput = processReturn.OutputParams?.ToList()
                                    };

                                    userData.Operation = operation;
                                    StringBuilder operationStringBuilder = new StringBuilder("List of arguments to enter, enter one by one - strings and numbers in form of message \"[argumentname] [argument value]\" separated by space without brackets, images and files in form of attached file with \"[argument id]\" as message:\n");

                                    foreach (var inputArgument in userData.Operation.ArgumentDefinitionsInput)
                                    {
                                        if (userData.Operation.Arguments.ContainsKey(inputArgument.Name))
                                        {
                                            var argument = userData.Operation.Arguments[inputArgument.Name];
                                            operationStringBuilder.AppendLine($"{inputArgument.Name}: {inputArgument.Type.ToString()} ({argument.Value})\n");
                                        }
                                        else
                                        {
                                            operationStringBuilder.AppendLine($"{inputArgument.Name}: {inputArgument.Type.ToString()} \n");
                                        }
                                    }
                                    await botClient.SendTextMessageAsync(chatId, operationStringBuilder.ToString(), cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Invalid process id.",
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                        }
                        else
                        {
                            var userData = UserDatabase[userId];
                            if (userData.Operation == null)
                            {
                                await botClient.SendTextMessageAsync(chatId, $"Unknown input, no operation pending, use /help", cancellationToken: cancellationToken);
                                break;
                            }

                            var argument_id = messageText.Split(' ')[0];

                            var argumentDefinition = userData.Operation.ArgumentDefinitionsInput.FirstOrDefault(arg => arg.Name == argument_id);
                            if(argumentDefinition != null)
                            {
                                switch (argumentDefinition.Type)
                                {
                                    case ArgumentType.String:
                                        userData.Operation.Arguments[argument_id] = new Argument
                                        {
                                            Name = argument_id,
                                            Value = messageText.Substring(argument_id.Length).Trim()
                                        };
                                        break;
                                    case ArgumentType.Integer:
                                        if(int.TryParse(messageText.Substring(argument_id.Length).Trim(), out int integ))
                                        {
                                            userData.Operation.Arguments[argument_id] = new Argument
                                            {
                                                Name = argument_id,
                                                Value = integ.ToString()
                                            };

                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"Acknowleged {argument_id}",
                                                cancellationToken: cancellationToken
                                            );
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"Cannot parse integer.",
                                                cancellationToken: cancellationToken
                                            );
                                        }
                                        break;
                                    case ArgumentType.Double:
                                        if (double.TryParse(messageText.Substring(argument_id.Length).Trim(), out double number))
                                        {
                                            userData.Operation.Arguments[argument_id] = new Argument
                                            {
                                                Name = argument_id,
                                                Value = number.ToString()
                                            };

                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"Acknowleged {argument_id}",
                                                cancellationToken: cancellationToken
                                            );
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"Cannot parse float-point value.",
                                                cancellationToken: cancellationToken
                                            );
                                        }
                                        break;
                                    case ArgumentType.Image:
                                        argument_id = update.Message.Caption;
                                        if (update.Message.Photo != null && update.Message.Photo.Length > 0)
                                        {
                                            var file = update.Message.Photo[update.Message.Photo.Length - 1];
                                            var fileId = file.FileId;
                                            var filePath = await botClient.GetFileAsync(fileId);

                                            var fileExtension = Path.GetExtension(filePath.FilePath);

                                            // Download and save the file
                                            var fileName = $"{Guid.NewGuid()}{fileExtension}";
                                            var destinationPath = Path.Combine("Images", fileName);
                                            Directory.CreateDirectory("Images");

                                            using (var fileStream = new FileStream(destinationPath, FileMode.Create))
                                            {
                                                await botClient.DownloadFileAsync(filePath.FilePath, fileStream);

                                            }

                                            userData.Operation.Arguments[argument_id] = new Argument
                                            {
                                                Name = argument_id,
                                                Value = Path.GetFileName(filePath.FilePath)
                                            };

                                            if (userData.Operation.Files.ContainsKey(argument_id))
                                            {
                                                System.IO.File.Delete(userData.Operation.Files[argument_id]);
                                            }
                                            userData.Operation.Files[argument_id] = destinationPath;

                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"Acknowleged {argument_id}",
                                                cancellationToken: cancellationToken
                                            );
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"No image found here.",
                                                cancellationToken: cancellationToken
                                            );
                                        }
                                        break;
                                }

                                if(userData.Operation.ArgumentDefinitionsInput
                                    .All(arg => userData.Operation.Arguments.ContainsKey(arg.Name)))
                                {
                                    Console.WriteLine("Processing");

                                    using (var _httpClient = new HttpClient())
                                    using (var _form = new MultipartFormDataContent())
                                    {
                                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userData.loginJwtToken);

                                        foreach (var inputArgument in userData.Operation.ArgumentDefinitionsInput)
                                        {
                                            switch (inputArgument.Type)
                                            {
                                                case ArgumentType.String:
                                                case ArgumentType.Integer:
                                                case ArgumentType.Double:
                                                    _form.Add(new StringContent(userData.Operation.Arguments[inputArgument.Name].Value), inputArgument.Name);
                                                    break;
                                                case ArgumentType.Image:
                                                    var imageContent = new ByteArrayContent(System.IO.File.ReadAllBytes(userData.Operation.Files[inputArgument.Name]));
                                                    imageContent.Headers.ContentType = 
                                                        new System.Net.Http.Headers.MediaTypeHeaderValue(GetContentType(
                                                            Path.GetExtension(userData.Operation.Files[inputArgument.Name])));
                                                    _form.Add(imageContent, inputArgument.Name, userData.Operation.Arguments[inputArgument.Name].Value);
                                                    break;
                                            }
                                        }

                                        var apiUrl = $"https://localhost:44374/api/process/{userData.Operation.OperationId}";
                                        var response = await _httpClient.PostAsync(apiUrl, _form);

                                        var resultResponse = await response.Content.ReadAsStringAsync();
                                        var argumentResult = JsonConvert.DeserializeObject<Argument[]>(resultResponse);


                                        StringBuilder stringResults = new StringBuilder();
                                        foreach(var outArgument in userData.Operation.ArgumentDefinitionsOutput)
                                        {
                                            switch (outArgument.Type)
                                            {
                                                case ArgumentType.String:
                                                case ArgumentType.Integer:
                                                case ArgumentType.Double:
                                                    var argumentValue = argumentResult.FirstOrDefault(arg => outArgument.Name == arg.Name)?.Value;
                                                    stringResults.AppendLine($"{outArgument.Name}: {argumentValue}");
                                                    break;
                                                case ArgumentType.Image:

                                                    var argumentImageValue = argumentResult.FirstOrDefault(arg => outArgument.Name == arg.Name)?.Value;

                                                    string ImageUrl = $"https://localhost:44374/File/ResultImage?imageName={argumentImageValue}";

                                                    using (var imageResponse = await _httpClient.GetAsync(ImageUrl))
                                                    {
                                                        response.EnsureSuccessStatusCode();

                                                        var fileStream = await imageResponse.Content.ReadAsStreamAsync();

                                                        await botClient.SendPhotoAsync(
                                                            chatId: chatId,
                                                            photo: InputFile.FromStream(fileStream, argumentImageValue),
                                                            caption: outArgument.Name
                                                        );
                                                    }
                                                        
                                                    break;
                                            }
                                        }

                                        var stringMessage = stringResults.ToString();
                                        if (!string.IsNullOrEmpty(stringMessage))
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: stringMessage,
                                                cancellationToken: cancellationToken
                                            );
                                        }

                                        if (userData.Operation.Files.Any())
                                        {
                                            foreach (var keyvalueP in userData.Operation.Files)
                                            {
                                                var filePath = Path.Combine("Bot_Data", keyvalueP.Value);
                                                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                                            }
                                        }

                                        userData.Operation = null;
                                    }
                                }

                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"Invalid argument id.",
                                    cancellationToken: cancellationToken
                                );
                            }
                        }
                        break;
                }
            }
        }

    /*
     if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var messageText = update.Message.Text.ToLower();
            var chatId = update.Message.Chat.Id;

            switch (messageText)
            {
                case "/start":
                    await botClient.SendTextMessageAsync(chatId, "Welcome! Type /help to see available commands.", cancellationToken: cancellationToken);
                    break;

                case "/help":
                    await botClient.SendTextMessageAsync(chatId, "Available commands:\n/start - Start the bot\n/help - List commands\n/echo - Echo back text.", cancellationToken: cancellationToken);
                    break;

                case "/echo":
                    await botClient.SendTextMessageAsync(chatId, "Please type a message to echo back.", cancellationToken: cancellationToken);
                    break;

                default:
                    if (messageText.StartsWith("/echo "))
                    {
                        var echoMessage = messageText.Replace("/echo ", "");
                        await botClient.SendTextMessageAsync(chatId, echoMessage, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Unknown command. Type /help to see available commands.", cancellationToken: cancellationToken);
                    }
                    break;
            }
        }
    */
    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                throw exception;
            }
            catch(ApiRequestException ex)
            {
                Console.WriteLine($"Telegram API Error:\n[{ex.ErrorCode}]\n{ex.Message}");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return Task.CompletedTask;
        }

        private static readonly Dictionary<string, string> MimeTypes1
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "image/jpeg", ".jpg" },
            { "image/png", ".png" },
            { "image/gif", ".gif" },
            { "image/bmp", ".bmp" },
            { "application/pdf", ".pdf" },
            { "application/msword", ".doc" },
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" },
            { "application/vnd.ms-excel", ".xls" },
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx" },
            { "application/vnd.ms-powerpoint", ".ppt" },
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx" },
            { "text/plain", ".txt" },
            { "application/zip", ".zip" },
            { "application/json", ".json" }
        };

        private static string GetExtensionFromMimeType(string mimeType)
        {
            if (MimeTypes1.ContainsKey(mimeType)) return MimeTypes1[mimeType];

            return ".dat";
        }

        private static readonly Dictionary<string, string> MimeTypes
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".txt", "text/plain" },
            { ".zip", "application/zip" },
            { ".json", "application/json" }
        };

        public static string GetContentType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream"; // Default MIME type for unknown files
            }

            return MimeTypes.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";
        }
    }
}
