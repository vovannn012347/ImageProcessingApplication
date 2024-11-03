using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Security.Claims;

using ImageProcessingApplication.Code;
using ImageProcessingApplication.Data.Entities;

using Microsoft.AspNet.Identity.Owin;
using ImageProcessingApplication.Models;
using Common;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using Common.Model.Areas;

namespace ImageProcessingApplication.Areas.Api.Controllers
{

    [RoutePrefix("api/user")]
    [Authorize]
    public class UserController : System.Web.Http.ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }


        public UserController()
        {
        }

        public UserController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await UserManager.FindAsync(model.Login, model.Password);
            if (user == null)
                return Unauthorized();

            var roles = await UserManager.GetRolesAsync(user.Id);

            string ip = IpHelper.GetClientIpAddress(Request);
            var token = TokenHelper.GenerateAuthenticatedToken(user.Id, user.UserName, ip, roles);
            //var renewtoken = TokenHelper.GenerateAuthenticatedRenewToken(user.Id, user.UserName, ip);
            return Ok(new TokenModel { Token = token });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("anonymous")]
        public async Task<IHttpActionResult> Login()
        {
            string ip = IpHelper.GetClientIpAddress(Request);

            var user = await UserManager.FindAsync(ip + '_' + Constants.AnonymousUserConstant, Constants.AnonymousUserConstantPassword);
            if (user == null)
            {
                var registeredUser = new ApplicationUser
                {
                    UserName = ip + '_' + Constants.AnonymousUserConstant,
                    Email = Constants.AnonymousUserConstant + '@' + Constants.AnonymousUserConstant,
                    IsAnonymous = true
                };

                var result = await UserManager.CreateAsync(registeredUser, Constants.AnonymousUserConstantPassword);
                if (!result.Succeeded)
                    return BadRequest(string.Concat(result.Errors));
            }

            var token = TokenHelper.GenerateAnonymousToken(ip);
            //var renewtoken = TokenHelper.GenerateAnonymousRenewToken(ip);
            return Ok(new TokenModel { Token = token });
        }

        [HttpPost]
        [Route("telegram")]
        [Authorize(Roles ="Telegram")]
        public async Task<IHttpActionResult> LoginTelegram(TelegramLoginModel model)
        {
            //var telegramToken = SecretConstants.BotToken; // Replace with your bot's token
            //var checkString = $"auth_date={model.AuthDate}\nid={model.Id}";
            //if (!string.IsNullOrEmpty(model.FirstName)) checkString += $"\nfirst_name={model.FirstName}";
            //if (!string.IsNullOrEmpty(model.LastName)) checkString += $"\nlast_name={model.LastName}";
            //if (!string.IsNullOrEmpty(model.PhotoUrl)) checkString += $"\nphoto_url={model.PhotoUrl}";
            //if (!string.IsNullOrEmpty(model.UserName)) checkString += $"\nusername={model.UserName}";

            //using (var sha256 = SHA256.Create())
            //{
            //    var secretKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(telegramToken));
            //    var computedHash = new HMACSHA256(secretKey).ComputeHash(Encoding.UTF8.GetBytes(checkString));
            //    var computedHashString = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

            //    if (computedHashString != model.Hash)
            //    {
            //        return BadRequest("Invalid Telegram login attempt.");
            //    }
            //}

            var user = await UserManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = Constants.AnonymousUserEmailConstant,
                    IsAnonymous = true
                };

                var result = await UserManager.CreateAsync(user);


                if (!result.Succeeded)
                {
                    return BadRequest(string.Concat(result.Errors));

                }
                else
                {
                    await UserManager.AddToRoleAsync(user.Id, "Telegram");
                    await UserManager.AddToRoleAsync(user.Id, "User");
                }
            }

            var token = TokenHelper.GenerateTelegramAuthenticatedToken(user.Id, user.UserName, model.Id.ToString(), new List<string> { "Telegram", "User" });
            return Ok(new TokenModel { Token = token });
        }

        //[HttpPost]
        //[Route("renew")]
        //public async Task<IHttpActionResult> Renew()
        //{
        //    string ip = IpHelper.GetClientIpAddress(Request);

        //    var user = await UserManager.FindAsync(ip + '_' + Constants.AnonymousUserConstant, Constants.AnonymousUserConstantPassword);
        //    if (user == null)
        //    {
        //        var registeredUser = new ApplicationUser
        //        {
        //            UserName = ip + '_' + Constants.AnonymousUserConstant,
        //            Email = Constants.AnonymousUserConstant + '@' + Constants.AnonymousUserConstant,
        //            IsAnonymous = true
        //        };

        //        var result = await UserManager.CreateAsync(registeredUser, Constants.AnonymousUserConstantPassword);
        //        if (!result.Succeeded)
        //            return BadRequest(string.Concat(result.Errors));
        //    }

        //    var token = TokenHelper.GenerateAnonymousToken(ip);
        //    return Ok(new { token = token });
        //}


        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> Profile()
        {
            var identity = User.Identity as ClaimsIdentity;
            
            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier) ?? identity?.FindFirst("sub");
            var userId = userIdClaim?.Value;

            if(string.IsNullOrEmpty(userId) || userId == Constants.AnonymousUserConstant)
            {
                var userIpClaim = identity?.FindFirst(Constants.ClaimTypes.IpAddress) ?? null;
                var userIp = userIpClaim?.Value;
                //add more logic for anonymous user limits

                return Ok(new { 
                    username = Constants.AnonymousUserConstant, 
                    email="", 
                    phone="" 
                });
            }

            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(
                new { 
                    username=user.UserName, 
                    email=user.Email, 
                    phone=user.PhoneNumber
                });
        }

        [HttpPut]
        [Route("")]
        public async Task<IHttpActionResult> UpdateUser([FromBody] UserProfileModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var identity = User.Identity as ClaimsIdentity;

            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier) ?? identity?.FindFirst("sub");
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId) || userId == Constants.AnonymousUserConstant)
            {
                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.AddModelError("Password", "Required");

                    return BadRequest(ModelState);
                }

                //making new user
                var userIpClaim = identity?.FindFirst(Constants.ClaimTypes.IpAddress) ?? null;
                var userIp = userIpClaim?.Value;

                var user = await UserManager.FindAsync(userIp + '_' + Constants.AnonymousUserConstant, Constants.AnonymousUserConstantPassword);
                
                if (user == null)
                {
                    var registeredUser = new ApplicationUser
                    {
                        UserName = userIp + '_' + Constants.AnonymousUserConstant,
                        Email = Constants.AnonymousUserConstant + '@' + Constants.AnonymousUserConstant,
                        IsAnonymous = true
                    };

                    var result1 = await UserManager.CreateAsync(registeredUser, Constants.AnonymousUserConstantPassword);
                    if (!result1.Succeeded)
                        return BadRequest(string.Concat(result1.Errors));

                    user = await UserManager.FindAsync(userIp + '_' + Constants.AnonymousUserConstant, Constants.AnonymousUserConstantPassword);
                }

                //TODO: addtional user modificationss
                user.UserName = model.UserName;
                if (string.IsNullOrEmpty(model.Email))
                {
                    user.Email = Constants.AnonymousUserConstant + '@' + Constants.AnonymousUserConstant;
                }
                else
                {
                    user.Email = model.Email;
                }

                user.IsAnonymous = false;
                user.IsDeleted = false;


                var result2 = await UserManager.ChangePasswordAsync(user.Id, Constants.AnonymousUserConstantPassword, model.Password);
                if (!result2.Succeeded)
                    return GetErrorResult(result2);

                var result3 = await UserManager.UpdateAsync(user);
                if (!result3.Succeeded)
                    return GetErrorResult(result3);

                var roles = await UserManager.GetRolesAsync(user.Id);
                var token = TokenHelper.GenerateAuthenticatedToken(user.Id, user.UserName, userIp, roles);

                return Ok(new { token = token });
            }
            else
            {
                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    //some wtf
                    return BadRequest();
                }

                if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.PreviousPassword))
                {
                    bool isPasswordValid = await UserManager.CheckPasswordAsync(user, model.PreviousPassword);
                    if (isPasswordValid)
                    {
                        await UserManager.ChangePasswordAsync(user.Id, model.PreviousPassword, model.Password);
                    }
                    else
                    {
                        ModelState.AddModelError("PreviousPassword", "Invalid");

                        return BadRequest(ModelState);
                    }
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.IsAnonymous = false;
                user.IsDeleted = false;

                var result = await UserManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return GetErrorResult(result);

                var userIpClaim = identity?.FindFirst(Constants.ClaimTypes.IpAddress) ?? null;
                var userIp = userIpClaim?.Value;

                var roles = await UserManager.GetRolesAsync(user.Id);
                var token = TokenHelper.GenerateAuthenticatedToken(user.Id, user.UserName, userIp, roles);

                return Ok(new { token = token });
            }
        }

        [HttpDelete]
        [Route("")]
        public async Task<IHttpActionResult> DeleteUser()
        {
            var identity = User.Identity as ClaimsIdentity;

            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier) ?? identity?.FindFirst("sub");
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId) || userId == Constants.AnonymousUserConstant)
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                //some wtf
                return BadRequest();
            }

            //TODO: other user modifications
            user.UserName = Constants.DeletedUserConstant + ' ' + user.UserName;
            user.Email = Constants.DeletedUserConstant + ' ' + user.Email;
            user.IsAnonymous = false;
            user.IsDeleted = true;

            await UserManager.UpdateAsync(user);
            await UserManager.RemovePasswordAsync(user.Id);

            return Ok();
        }

        private IHttpActionResult GetErrorResult(Microsoft.AspNet.Identity.IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                if (ModelState.IsValid)
                {
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
