using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SLI_UploadPicture.Models;

namespace SLI_UploadPicture.Controllers
{
    public class CommonController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        public CommonController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Environment = hostingEnvironment;
            Configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Upload FHIR Observation for Annotation 
        /// </summary>
        /// <returns>Fhir ObservationnAnnotation</returns>
        [HttpGet]
        public IActionResult GetResource([FromBody] GetResourceInformation ResourceInformation)
        {
            HTTPrequest httpRequest = new HTTPrequest();
            return (IActionResult)httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", ResourceInformation.resource, ResourceInformation.parameters, Configuration["FHIRResponseType"]);        
        }
    }
}
