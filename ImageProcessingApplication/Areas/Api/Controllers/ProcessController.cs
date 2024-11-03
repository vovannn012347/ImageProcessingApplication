using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using ImageProcessingApplication.Code;

using Microsoft.AspNet.Identity.Owin;
using ImageProcessingApplication.Data;
using ProcessorAlgorithm;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Common.Model;

namespace ImageProcessingApplication.Areas.Api.Controllers
{
    [RoutePrefix("api/process")]
    [Authorize]
    public class ProcessController : System.Web.Http.ApiController
    {
        private ApplicationDbContext _dbContext;
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ProcessController()
        {
        }

        public ProcessController(
            ApplicationUserManager userManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }
        

        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProcesses()
        {
            /*
            var processes = _dbContext.ProcessOperations.ToList()
            .Select(p => new ProcessReturn
            {
                ProcessId = p.Id,
                CodeName = p.CodeName,
                FriendlyName = p.FriendlyName,
                InputParams = JsonConvert.DeserializeObject<ArgumentDefinition[]>(p.InputParams),
                OutputParams = JsonConvert.DeserializeObject<ArgumentDefinition[]>(p.OutputParams)
            });

            return Ok(processes);
            */

            //return sobel edge detection processing data

            return Ok(new List<ProcessReturn> { new ProcessReturn
                {
                    ProcessId = "sobel_processing_guid",
                    CodeName = "sobel_edge",
                    FriendlyName = "Sobel Edge Detection",
                    InputParams = new ArgumentDefinition[] {
                        new ArgumentDefinition{
                            Name = "input_img",
                            Type = ArgumentType.Image
                        }
                    },
                    OutputParams = new ArgumentDefinition[] {
                        new ArgumentDefinition{
                            Name = "output_img",
                            Type = ArgumentType.Image
                        }
                    }
                }});
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetProcess(string id)
        {
            //return ONLY sobel edge detection processing data
            if(id == "sobel_processing_guid")
            {
                return Ok(new ProcessReturn
                {
                    ProcessId = "sobel_processing_guid",
                    CodeName = "sobel_edge",
                    FriendlyName = "Sobel Edge Detection",
                    InputParams = new ArgumentDefinition[] { 
                        new ArgumentDefinition{
                            Name = "input_img",
                            Type = ArgumentType.Image
                        }
                    },
                    OutputParams = new ArgumentDefinition[] {
                        new ArgumentDefinition{
                            Name = "output_img",
                            Type = ArgumentType.Image
                        }
                    }
                });
            }

            return BadRequest("Not Found");

            /*
            var process =
                _dbContext.ProcessOperations
                .FirstOrDefault(i => i.Id == id);

            if (process != null)
                return Ok(new ProcessReturn
                {
                    ProcessId = process.Id,
                    CodeName = process.CodeName,
                    FriendlyName = process.FriendlyName,
                    InputParams = JsonConvert.DeserializeObject<ArgumentDefinition[]>(process.InputParams),
                    OutputParams = JsonConvert.DeserializeObject<ArgumentDefinition[]>(process.OutputParams)
                });

            return BadRequest("Not Found");*/
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IHttpActionResult> StartProcessing(string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            if (id == "sobel_processing_guid")
            {
                //here should be algorithm selection from database
                //i dont have time for it right now
                //so will be using handwritten stubs
                //TODO: read from database
                var inputArguments =
                    new ArgumentDefinition[] {
                        new ArgumentDefinition{
                            Name = "input_img",
                            Type = ArgumentType.Image
                        }
                    };

                var outputArguments =
                    new ArgumentDefinition[] {
                        new ArgumentDefinition{
                            Name = "output_img",
                            Type = ArgumentType.Image
                        }
                    };

                string inputUploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Processing", "Uploads");
                if (!Directory.Exists(inputUploadPath))
                    Directory.CreateDirectory(inputUploadPath);

                List<Argument> arguments = new List<Argument>();

                if (inputArguments.NeedFileSave())
                {
                    //string root = HttpContext.Current.Server.MapPath(inputUploadPath);
                    var provider = new MultipartFormDataStreamProvider(inputUploadPath);

                    try
                    {
                        // Read the multipart content
                        await Request.Content.ReadAsMultipartAsync(provider);


                        foreach(var inputArg in inputArguments)
                        {
                            if (provider.FormData.AllKeys.Contains(inputArg.Name))
                            {
                                return BadRequest(inputArg.Name + " is required");
                            }
                        }

                        foreach (var inputArg in inputArguments)
                        {
                            if (inputArg.Type == ArgumentType.Image)
                            {
                                var fileData = provider.FileData
                                   .FirstOrDefault(fd => fd.Headers.ContentDisposition.Name?.Trim('"') == inputArg.Name);

                                if (fileData == null)
                                {
                                    return BadRequest(inputArg.Name + "File is required");
                                }

                                // Get the original file name
                                string originalFileName = fileData.Headers.ContentDisposition.FileName.Trim('"');
                                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(originalFileName);
                                string newFilePath = Path.Combine(inputUploadPath, newFileName);

                                // Move the uploaded file to the new location with the new name
                                File.Move(fileData.LocalFileName, newFilePath);

                                arguments.Add(new Argument
                                {
                                    Name = inputArg.Name,
                                    Value = newFilePath
                                });

                            }
                            else
                            {
                                arguments.Add(new Argument
                                {
                                    Name = inputArg.Name,
                                    Value = provider.FormData[inputArg.Name]
                                });
                            }
                        }



                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
                }

                string algorithmResultsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Algorithms", "Results");
                if (!Directory.Exists(algorithmResultsDirectory))
                    Directory.CreateDirectory(algorithmResultsDirectory);

                //todo: use process isolation,
                //sandboxdomain does not work as i need to load additional assemblies and it is fucked in that way

                
                var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Algorithms", "SobelEdgeDetection3.dll");
                /*var assemblyDllsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

                PermissionSet permissionSet = new PermissionSet(PermissionState.None);
                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                permissionSet.AddPermission(new UIPermission(PermissionState.Unrestricted));//you motherfucker
                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, AppDomain.CurrentDomain.BaseDirectory));
                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, assemblyDllsPath));

                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, inputUploadPath));
                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.PathDiscovery, algorithmResultsDirectory));
                permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, assemblyPath));
                permissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));

                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                var location = Path.GetDirectoryName(executingAssembly.Location);

                var applicationDir = "bin";
                var algorithmsDir = Path.Combine("bin", "Algorithms");
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string algorithmsDirectory = Path.Combine(baseDirectory, "bin", "Algorithms");

                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                    PrivateBinPath = applicationDir + ";" + algorithmsDir// Set the base path for the new AppDomain
                };
                AppDomain sandboxedDomain = AppDomain.CreateDomain("SandboxedDomain", AppDomain.CurrentDomain.Evidence, setup, permissionSet);

                try
                {
                    sandboxedDomain.AssemblyResolve += DllResolver.PluginAppDomain_LoadAssembly;
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }*/


                DllResolver.Touch();
                var resultWork = await DoStuff(assemblyPath, algorithmResultsDirectory, arguments);

                return resultWork;
                //finally
                //{
                //    // Unload the sandboxed AppDomain
                //    AppDomain.Unload(sandboxedDomain);
                //}

            }

            return BadRequest("Not Found");
        }

        private async Task<IHttpActionResult> DoStuff(string assemblyPath, string algorithmResultsDirectory, List<Argument> arguments)
        {
            Argument[] result = null;
            try
            {
                Type baseType = typeof(ProcessAlgorithm); // Replace with your type name

                //var opencv = Path.Combine(Path.GetDirectoryName(assemblyPath), "OpenCvSharp.dll");

                //Assembly loadedAssembly = Assembly.LoadFrom(opencv);// AppDomain.CurrentDomain.Load("OpenCvSharp");
                //var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                //loadedAssembly = AppDomain.CurrentDomain.Load("OpenCvSharpExtern");
                //domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                var loadedAssembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(assemblyPath));
                var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                //TODO: move this to algorithm upload

                //Assembly loadedAssembly = Assembly.LoadFile(assemblyPath);
                //Assembly loadedAssembly = Assembly.LoadFrom(assemblyPath);

                //var assemblyName = AssemblyName.GetAssemblyName(assemblyPath).FullName;
                //sandboxedDomain.ExecuteAssembly(assemblyPath);
                //Assembly loadedAssembly = sandboxedDomain.Load(AssemblyName.GetAssemblyName(assemblyPath));

                //var assemblyName = loadedAssembly.GetName();
                //Assembly loadedAssembly = sandboxedDomain.Load(File.ReadAllBytes(assemblyPath));

                // Find all types that inherit from the specified base type
                var derivedTypes =
                    loadedAssembly.GetTypes()
                    .Where(type => baseType.IsAssignableFrom(type) && type != baseType && !type.IsAbstract);

                if (derivedTypes.Count() != 1)
                    throw new ArgumentException("Must be single executing class");

                var derivedType = derivedTypes.FirstOrDefault();
                var fullyQualifiedName = derivedTypes.FirstOrDefault().FullName;

                ProcessAlgorithm processorClass = (ProcessAlgorithm)Activator.CreateInstance(derivedType);

                //ProcessAlgorithm sandboxedInstance = (ProcessAlgorithm)sandboxedDomain.CreateInstanceAndUnwrap(loadedAssembly.FullName, fullyQualifiedName);

                result = await processorClass.Process(algorithmResultsDirectory, arguments.ToArray());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(result);
        }


        /*
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <title>Upload Model with Files</title>
        </head>
        <body>
            <form id="uploadForm">
                <div>
                    <label>Title:</label>
                    <input type="text" name="Title" id="title" required />
                </div>
                <br />
                <div>
                    <label>Description:</label>
                    <textarea name="Description" id="description" required></textarea>
                </div>
                <br />
                <div>
                    <label>Select files:</label>
                    <input type="file" id="multipleFiles" name="multipleFiles" multiple required />
                </div>
                <br />
                <div>
                    <label>Select files:</label>
                    <input type="file" id="files" name="files" required />
                </div>
                <button type="button" onclick="uploadData()">Upload</button>
            </form>

            <script>
                async function uploadData() {
                    const form = document.getElementById('uploadForm');
                    const formData = new FormData();

                    // Add form fields
                    formData.append("Title", document.getElementById("title").value);
                    formData.append("Description", document.getElementById("description").value);

                    // Add files
                    const files = document.getElementById("files").files;
                    for (let i = 0; i < files.length; i++) {
                        formData.append("Files", files[i]); // "Files" matches the parameter name in the controller
                    }

                    try {
                        const response = await fetch('/api/fileupload/upload', {
                            method: 'POST',
                            body: formData
                        });

                        if (response.ok) {
                            const result = await response.json();
                            alert("Upload successful: " + JSON.stringify(result));
                        } else {
                            alert("Upload failed.");
                        }
                    } catch (error) {
                        console.error("Error:", error);
                        alert("An error occurred during upload.");
                    }
                }
            </script>
        </body>
        </html>
        */
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> AddProcess()
        {
            /*
            //lets assume that input is correct
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Unsupported media type.");
            }

            //var name = HttpContext.Current.Request.Form["name"];
            //var friendly_name = HttpContext.Current.Request.Form["friendly_name"];
            //var metadata = HttpContext.Current.Request.Files["metadata"];

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // Initialize lists to hold file information
            var singleFile = new byte[0];
            var multipleFiles = new List<byte[]>();
            string description = null;

            // Process each part of the request
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('"');
                var buffer = await file.ReadAsByteArrayAsync();

                if (file.Headers.ContentDisposition.Name.Trim('"') == "singleFile")
                {
                    singleFile = buffer; // Store single file
                }
                else if (file.Headers.ContentDisposition.Name.Trim('"') == "multipleFiles")
                {
                    multipleFiles.Add(buffer); // Store multiple files
                }
            }



            // Get uploaded files
            var files = HttpContext.Current.Request.Files;
            var savedFilePaths = new List<string>();

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file != null && file.ContentLength > 0)
                {
                    var uploadPath = HttpContext.Current.Server.MapPath("~/uploads");
                    Directory.CreateDirectory(uploadPath); // Ensure directory exists
                    var filePath = Path.Combine(uploadPath, file.FileName);
                    file.SaveAs(filePath);
                    savedFilePaths.Add(filePath); // Store saved file paths if needed
                }
            }

            // Return a result with details about the uploaded files
            return Ok(new { success = true, savedFilePaths });

            */
            return Ok();
        }

        [HttpPut]
        [Route("list")]
        public async Task<IHttpActionResult> UpdateProcess()
        {
            return Ok();
        }

    }

    public static class DllResolver
    {
        static string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        static string algorithmsDirectory = Path.Combine(baseDirectory, "bin", "Algorithms");
        static string binDirectory = Directory.GetParent(algorithmsDirectory).FullName;// Path.Combine(baseDirectory, "bin", "Algorithms"); 
        static DllResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public static void Touch() { }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyname = new AssemblyName(args.Name).Name;

            var assemblyFileName = Path.Combine(algorithmsDirectory, assemblyname + ".dll");
            if (!File.Exists(assemblyFileName))
            {
                assemblyFileName = Path.Combine(binDirectory, assemblyname + ".dll");
                if (!File.Exists(assemblyFileName))
                    return null;
            }
            return Assembly.LoadFrom(assemblyFileName);
        }
    }

}
