using System;
using System.Collections.Generic;

namespace ImageProcessingApplication.Data.Entities
{
    public class ProcessOperation
    {
        public ProcessOperation()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public string CodeName { get; set; } // for search
        public string FriendlyName { get; set; }
        public string FilePath { get; set; }//directory where files are stored
        public string[] Files { get; set; }
        public string Metadata { get; set; }

        //derived, filled from metadata
        public string InputParams { get; set; }
        public string OutputParams { get; set; }
        public string OperationsOrder { get; set; } //unused for now

        public string CreatorUserId { get; set; }
        public virtual ApplicationUser CreatorUser { get; set; }

        public virtual ICollection<OperationOrder> NextOperations { get; set; } = new List<OperationOrder>();
        public virtual ICollection<OperationOrder> PreviousOperations { get; set; } = new List<OperationOrder>();
    }
}