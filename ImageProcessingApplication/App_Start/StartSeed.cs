using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Common;

using ImageProcessingApplication.Code;
using ImageProcessingApplication.Data;
using ImageProcessingApplication.Data.Entities;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ImageProcessingApplication.App_Start
{
    public static class DatabaseSeeder
    {
        public static async Task SeedUsersAndRoles()
        {
            using (var context = new ApplicationDbContext())
            {
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                await SeedAdmin(userManager, roleManager);
                await SeedTelegram(userManager, roleManager);

                context.SaveChanges();
            }
        }

        private static async Task SeedAdmin(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Define a default admin user
            var defaultAdminEmail = SecretConstants.DefaultAdminEmail;
            var defaultAdminPassword = SecretConstants.DefaultAdminPassword;
            var adminUser = await userManager.FindByEmailAsync(defaultAdminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = defaultAdminEmail,
                    Email = defaultAdminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, defaultAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser.Id, "Admin");
                    await userManager.AddToRoleAsync(adminUser.Id, "User");
                }
            }
            else
            {
                // Optionally update user properties if necessary
                adminUser.UserName = defaultAdminEmail;
                adminUser.Email = defaultAdminEmail;
                await userManager.UpdateAsync(adminUser);
            }
        }

        private static async Task SeedTelegram(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            
            var roles = new[] { "Telegram", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Define a default admin user
            var email = Code.Constants.AnonymousUserEmailConstant;
            var password = SecretConstants.BotPassword;
            var name = SecretConstants.BotUserName;
            var telegramUser = await userManager.FindByNameAsync(name);

            if (telegramUser == null)
            {
                telegramUser = new ApplicationUser
                {
                    UserName = name,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(telegramUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(telegramUser.Id, "Telegram");
                    await userManager.AddToRoleAsync(telegramUser.Id, "User");
                }
            }
            else
            {
                // Optionally update user properties if necessary
                telegramUser.UserName = name;
                telegramUser.Email = email;
                await userManager.UpdateAsync(telegramUser);
            }
        }
    }
}