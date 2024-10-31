using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using ImageProcessingApplication.Code;
using ImageProcessingApplication.Data;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

using Ninject;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;

[assembly: OwinStartup(typeof(ImageProcessingApplication.Code.Startup))]
namespace ImageProcessingApplication
{
    public class WebApiApplication : NinjectHttpApplication
    {
        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            // Register services with the kernel
            RegisterServices(kernel);
            return kernel;
        }

        private void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ApplicationDbContext>().ToSelf().InRequestScope();

            kernel.Bind<ApplicationUserManager>().ToMethod(context => HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>());
            //kernel.Bind<ApplicationSignInManager>().ToMethod(context => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>());
            kernel.Bind<IAuthenticationManager>().ToMethod(context => HttpContext.Current.GetOwinContext().Authentication);

            //kernel.Bind<ApplicationUserManager>().ToMethod(context => HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>());
            //kernel.Bind<ApplicationSignInManager>().ToSelf().InRequestScope();
        }
        protected override void OnApplicationStarted()
        {
            base.OnApplicationStarted();

            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            GlobalConfiguration.Configure(WebApiConfig.Register);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Use Ninject as the MVC dependency resolver
            //DependencyResolver.SetResolver(new NinjectDependencyResolver(Kernel));
        }
    }
}

//public class WebApiApplication : System.Web.HttpApplication
//{
//    protected void Application_Start(object sender, EventArgs e)
//    {
//        AreaRegistration.RegisterAllAreas();

//        GlobalConfiguration.Configure(WebApiConfig.Register);
//        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

//        RouteConfig.RegisterRoutes(RouteTable.Routes);
//        BundleConfig.RegisterBundles(BundleTable.Bundles);


//    }
//}
