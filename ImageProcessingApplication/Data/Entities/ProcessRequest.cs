using System;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class ProcessRequest
    {
        public ProcessRequest()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public string ProcessOperation { get; set; }
        public DateTime CreateTime { get; set; }
        public string ProcessState { get; set; }
        public string ProcessResultJson { get; set; }

        public string ParentRequestId { get; set; }
        public virtual CompositeProcessRequest ParentRequest { get; set; }

    }
}