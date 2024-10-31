using ImageProcessingApplication.Data;
using ImageProcessingApplication.Data.Entities;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageProcessingApplication.Code
{
    public class CustomUserValidator : UserValidator<ApplicationUser>
    {
        public CustomUserValidator(UserManager<ApplicationUser, string> manager)
            : base(manager)
        {
        }

        public override async Task<IdentityResult> ValidateAsync(ApplicationUser item)
        {
            IdentityResult result; 
            if(RequireUniqueEmail && item.Email == Constants.AnonymousUserEmailConstant)
            {
                RequireUniqueEmail = false;
                result = await base.ValidateAsync(item);
                RequireUniqueEmail = true;
            }
            else
            {
                result = await base.ValidateAsync(item);
            }

            return result;
        }
    }
        
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }
        
        public static ApplicationUserManager Create(
            IdentityFactoryOptions<ApplicationUserManager> options, 
            IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            // Configure validation logic for usernames
            manager.UserValidator = new CustomUserValidator(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure password validation
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            return manager;
        }
    }
}