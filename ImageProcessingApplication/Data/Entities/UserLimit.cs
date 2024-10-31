using System;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class UserLimit
    {
        public UserLimit()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }
        public int OperationLimit { get; set; }
        public int MaximumLimit { get; set; }
        public DateTime RenewTime { get; set; }


        // Foreign key to link the Limit to a User
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }


    }
}