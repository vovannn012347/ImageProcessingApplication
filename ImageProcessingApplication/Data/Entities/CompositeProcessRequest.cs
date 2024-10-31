using System;
using System.Collections.Generic;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class CompositeProcessRequest
    {
        public CompositeProcessRequest()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public string ProcessOperation { get; set; }
        public DateTime CreateTime { get; set; }
        public string ProcessState { get; set; }
        public string ProcessHash { get; set; }
        public string ProcessResultJson { get; set; }

        public virtual ICollection<ProcessRequest> ChildRequests { get; set; } = new List<ProcessRequest>();
    }
}