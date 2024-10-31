using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace ImageProcessingApplication.Controllers
{
    public class FileController : Controller
    {
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

        private static string GetContentType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream"; // Default MIME type for unknown files
            }

            return MimeTypes.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";
        }

        public ActionResult Image(string imageName)
        {
            var filePath = Path.Combine(Server.MapPath("~/Content/Images"), imageName);

            if (System.IO.File.Exists(filePath))
            {
                return File(filePath, GetContentType(imageName));
            }

            return HttpNotFound();
        }

        public ActionResult ResultImage(string imageName)
        {
            var fileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Algorithms", "Results");
            var filePath =  Path.Combine(fileDir, imageName);

            if (System.IO.File.Exists(filePath))
            {
                return File(filePath, GetContentType(imageName));
            }

            return HttpNotFound();
        }
    }
}
