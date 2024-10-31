using System;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class TimeHashStamp
    {
        public TimeHashStamp()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public DateTime StampTime { get; set; }
        public string HashStamp { get; set; }
    }
}