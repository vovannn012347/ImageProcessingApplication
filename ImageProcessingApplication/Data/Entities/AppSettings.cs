using System;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class AppSettings
    {
        public AppSettings()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public string ImageSavePath { get; set; }
        public int DefaultOperationLimit { get; set; }
        public int UserMaxLimit { get; set; }
        public TimeSpan LimitRenewTime { get; set; }
    }
}