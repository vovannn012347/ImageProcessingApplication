using System;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class ImageFile
    {
        public ImageFile()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public int FriendlyName { get; set; }
        public int FilePath { get; set; }
        public DateTime TimeDue { get; set; }
    }
}