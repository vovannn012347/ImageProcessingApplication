using System;
using System.Collections.Generic;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class OperationOrder
    {
        public OperationOrder()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public string ParamsMapping { get; set; }

        public string NextOperationId { get; set; }
        public virtual ProcessOperation NextOperation { get; set; }
        public string PreviousOperationId { get; set; }
        public virtual ProcessOperation PreviousOperation { get; set; }
        

    }
}