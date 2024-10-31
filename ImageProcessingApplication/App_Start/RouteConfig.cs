using System.Web.Mvc;
using System.Web.Routing;

namespace ImageProcessingApplication
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            //tis important
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //REST attribute routing for areas
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}


