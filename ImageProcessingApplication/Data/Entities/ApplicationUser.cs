using System.Collections.Generic;

using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDeleted { get; set; }
        public bool IsAnonymous { get; set; }

        public virtual ICollection<UserLimit> Limits { get; set; } = new List<UserLimit>();
    }
}