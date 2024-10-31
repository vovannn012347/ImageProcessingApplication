using System;
using System.Security.Claims;
using System.Web.Mvc;

using ImageProcessingApplication.Code;

using Microsoft.Owin;

namespace ImageProcessingApplication.Controllers
{
    //currently stubs only
    public class UserController : Controller
    {
        public ActionResult Login()
        {
            ViewBag.Title = "Sign in";
            var identity = User.Identity as ClaimsIdentity;

            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier) ?? identity?.FindFirst("sub");
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId) || userId == Constants.AnonymousUserConstant)
            {
                return View();
            }

            return Redirect("/Home/Index");
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Sign up";
            var identity = User.Identity as ClaimsIdentity;

            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier) ?? identity?.FindFirst("sub");
            var userId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(userId) || userId == Constants.AnonymousUserConstant)
            {
                return View();
            }

            return Redirect("/Home/Index");
        }

        public ActionResult UserInfo()
        {
            ViewBag.Title = "Profile";

            return View();
        }

        public ActionResult Logout()
        {
            ViewBag.Title = "Logout";

            Response.Cookies.Add(new System.Web.HttpCookie("jwtToken", "")
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            });
            Response.Cookies.Add(new System.Web.HttpCookie("jwtToken_expiration", "")
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            });

            return Redirect("/Home/Index");
        }
    }
}
