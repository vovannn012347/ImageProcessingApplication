using System.Configuration;
using System.Web.Http;

using ImageProcessingApplication.Data;

using Microsoft.Owin;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;

using Owin;

[assembly: OwinStartup(typeof(ImageProcessingApplication.Code.Startup))]

namespace ImageProcessingApplication.Code
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            /*
             //to not forget, sign in example
             $.ajax({
                url: '/api/token',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ username: 'user', password: 'password' }),
                success: function(response) {
                    // Set JWT token in cookie
                    document.cookie = "jwtToken=" + response.token;
                    // Redirect after login
                    window.location.href = '/Home/SecureAction'; 
                }
            });
                        
            //sign out sample
             function logout() {
                document.cookie = "jwtToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
                window.location.href = '/Home/Index';
            }
             */

            //token auth, solely for views
            app.Use(async (context, next) =>
            {
                var jwtToken = context.Request.Cookies["jwtToken"]; // Get JWT from cookie
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    context.Request.Headers.Add("Authorization", new[] { "Bearer " + jwtToken });
                }

                await next.Invoke();
            });

            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            //app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Configure JWT Bearer Authentication
            string siteName = ConfigurationManager.AppSettings[Constants.AppSettingKeys.Site];
            string jwtsecret = ConfigurationManager.AppSettings[Constants.AppSettingKeys.JwtSecret];

            var secret = TextEncodings.Base64Url.Decode(jwtsecret);

            var validAudiences = new[] { Constants.ClientTypes.Any, Constants.ClientTypes.Telegram };

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuer = siteName,
                    ValidateIssuer = true,

                    ValidAudiences = validAudiences,
                    ValidateAudience = true,

                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secret),
                    ValidateIssuerSigningKey = true,

                    ValidateLifetime = true
                }
            });
        }
    }
}