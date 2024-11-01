using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ProcessorAlgorithm;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    public class ChatState
    {
        public List<RequestedOperations[]> Operations { get; set; } = new List<RequestedOperations[]>();
        public string loginJwtToken { get; set; }

    }

    public class RequestedOperations
    {
        public List<Argument> Arguments { get; set; }
    }

    public class TelegramBot
    {
        private readonly TelegramBotClient _botClient;
        private CancellationTokenSource cts;

        public TelegramBot(string token)
        {
            _botClient = new TelegramBotClient(token);
        }

        public void StartReceiving()
        {
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

        private static readonly Dictionary<long, ChatState> UserDatabase = new Dictionary<long, ChatState>();


        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //if (update.Type == UpdateType.Message && update.Message != null && update.Message.Type == MessageType.Text)
            //{
            //    var chatId = update.Message.Chat.Id;
            //    var messageText = update.Message.Text;

            //    Console.WriteLine($"Received a message from {chatId}: {messageText}");

            //    await botClient.SendTextMessageAsync(
            //        chatId: chatId,
            //        text: "Hello! You said:\n" + messageText,
            //        cancellationToken: cancellationToken
            //    );
            //}

            if (update.Type == UpdateType.Message && update.Message != null && update.Message.Type == MessageType.Text)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;

                // Get user details
                var userId = update.Message.From.Id;
                var username = update.Message.From.Username ?? "No Username";
                var firstName = update.Message.From.FirstName;
                var lastName = update.Message.From.LastName ?? string.Empty;

                //Console.WriteLine($"Received a message from {firstName} {lastName} (Username: {username}, User ID: {userId}): {messageText}");

                bool somecriteria = false;
                if (somecriteria)
                {
                    //if (!UserDatabase.ContainsKey(chatId))
                    //{
                    //    UserDatabase[chatId] = new ChatState
                    //    {
                    //    };


                    //}


                    //await _botClient.SendTextMessageAsync(
                    //    chatId: chatId,
                    //    text: message
                    //);
                }

                // Reply to the user
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Hello, {firstName}({userId})! Your message is {messageText}.",
                    cancellationToken: cancellationToken
                );

                // Optional: Store user information
                // You can save userId, username, firstName, and lastName to a database or file
            }
        }

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
    }
}
