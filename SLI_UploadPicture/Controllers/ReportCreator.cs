using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SLI_UploadPicture.Models;
using System.Reflection.Metadata;

namespace SLI_UploadPicture.Controllers
{
    public class ReportCreator : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        // requires using Microsoft.Extensions.Configuration;
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        public ReportCreator(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Environment = hostingEnvironment;
            Configuration = configuration;
        }
        [HttpGet]
        public IActionResult Index(string DocumentReferenceId)
        {
            //HTTPrequest httpRequest = new HTTPrequest();
            //object resp = httpRequest.getResource(Configuration["FHIR_server"], "DocumentReference", DocumentReferenceId, Configuration["FHIRResponseType"]);

            HTTPrequest httpRequest = new HTTPrequest();
            var resp = httpRequest.getResource(Configuration["FHIR_server"], "DocumentReference", DocumentReferenceId, Configuration["FHIRResponseType"]);

            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject docRef = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<string> ImageList = new List<string>();
                foreach (JObject content in docRef["content"])
                {
                    ImageList.Add((content["attachment"]["url"].ToString()));
                }
                ViewBag.images = ImageList;
            }
            return View();
        }
    }
}
