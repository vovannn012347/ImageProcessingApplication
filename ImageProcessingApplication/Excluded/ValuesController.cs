using System.Collections.Generic;
using System.Web.Http;

namespace ImageProcessingApplication.Controllers
{
    public class ValuesController : ApiController
    {
        // todo: add this for REST stuff in routes 
        // Enable attribute routing
        // routes.MapMvcAttributeRoutes();

        [HttpGet]
        [Route("")]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        [Route("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        [Route("")]
        public void Post([FromBody] string value)
        {
        }

        [HttpPut]
        [Route("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [HttpDelete()]
        [Route("{id}")]
        public void Delete(int id)
        {
        }
    }
}
