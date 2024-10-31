using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using ImageProcessingApplication.Data.Entities;

namespace ImageProcessingApplication.Code
{
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager
            (ApplicationUserManager userManager, 
            IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        // Override the method to configure the sign-in settings
        public override async Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            // Add additional claims if necessary
            return identity;
        }

        //public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        //{
        //    return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        //}
    }
}