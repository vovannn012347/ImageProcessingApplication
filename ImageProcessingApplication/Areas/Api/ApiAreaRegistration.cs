using System.Web.Http;
using System.Web.Mvc;

namespace ImageProcessingApplication.Areas.Api
{
    public class ApiAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "Api";

        public override void RegisterArea(AreaRegistrationContext context)
        {
        }
    }
}