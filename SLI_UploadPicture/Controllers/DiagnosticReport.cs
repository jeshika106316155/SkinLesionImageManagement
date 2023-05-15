using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SLI_UploadPicture.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection.Metadata.Ecma335;

namespace SLI_UploadPicture.Controllers
{
    public class DiagnosticReport : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        public DiagnosticReport(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Environment = hostingEnvironment;
            Configuration = configuration;
        }
        public IActionResult Index()
        {
            HTTPrequest httpRequest = new HTTPrequest();
            var resp = httpRequest.getResource(Configuration["FHIR_server"], "Bearer 123", "ValueSet", "/7968c486-836f-41fa-8dbf-bcc50e5cd8d1", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> skindiseases = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    skindiseases.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.skindiseasescode = JsonConvert.SerializeObject(skindiseases, Formatting.Indented);
            }
            return View();
        }
        /// <summary>
        /// Upload FHIR Observation for Finding 
        /// </summary>
        /// <returns>Fhir ObservationnFinding</returns>
        [HttpPost]
        public IActionResult UploadReportSkinLesion([FromBody] DiagnosticReportInformation DiagnosticReportInfo)
        {
            JObject DReportObsJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["diagnosticReport_path"]));

            JArray performer = new JArray(), resultinterpreter = new JArray(), result = new JArray(), author = new JArray(), organization = new JArray();
            DReportObsJson["effectiveDateTime"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");
            DReportObsJson["issued"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");
            DReportObsJson["encounter"]["reference"] = "Encounter/09962243-8315-4c1e-90bf-a36d3b95bd1a";

            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            DReportObsJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
            performer.Add(new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] },
            { "display", PersonnalIDscookies["role"]["organizationName"] }});
            DReportObsJson["performer"] = performer;
            resultinterpreter.Add(new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] },
            { "display", PersonnalIDscookies["role"]["name"] }});
            DReportObsJson["resultsInterpreter"] = resultinterpreter;
            

            // Read finding cookies and record in the result component
            string findingIDs = HttpContext.Request.Cookies["FindingIDs"];
            JObject findingIDscookies = JObject.Parse((string)findingIDs);

            foreach (var x in findingIDscookies)
            {
                result.Add(new JObject { { "reference", "Observation/"+x.Value } });
            }
            DReportObsJson["result"] = result;
            // Read all document reference of image
            string DocrefIDs = HttpContext.Request.Cookies["DocrefIDs"];
            JObject DocrefIDscookies = JObject.Parse((string)DocrefIDs);

            // Get docref list each content attachement to media
            JArray medias = CreateMedia("DocumentReference/" + DocrefIDscookies["-1"]);
            DReportObsJson["media"] =  medias[1];
            DReportObsJson["conclusionCode"][0]["coding"][0]["code"] = DiagnosticReportInfo.skin_diseases;
            DReportObsJson["conclusion"] = DiagnosticReportInfo.conclusion;

            JObject callBackParams = new JObject();
            callBackParams["mediaentry"] = medias[0];
            HTTPrequest httpRequest = new HTTPrequest();
            return httpRequest.postResource(Configuration["Repository_gateway_fhir"], "DiagnosticReport", DReportObsJson, "Bearer 123", UploadDokumentSkinLesion, callBackParams);
        }

        private JArray CreateMedia(string documentReference)
        {
            JArray mediasdoc = new JArray(), media_documentndreport = new JArray(), mediadrreport = new JArray();
            HTTPrequest httpRequest = new HTTPrequest();
            var respPersonnal = httpRequest.getResource(Configuration["Repository_gateway_fhir"] + documentReference, "Bearer 123", "", "", Configuration["FHIRResponseType"]);
            if (respPersonnal.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject respDocrefIDJson = JObject.Parse((respPersonnal as OkObjectResult).Value.ToString());
                foreach (JObject x in respDocrefIDJson["content"])
                {
                    // create Media 
                    JObject MediaJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["media_path"]));

                    JArray operators = new JArray(), content = new JArray(), result = new JArray(), author = new JArray(), organization = new JArray();
                    MediaJson["createdDateTime"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");

                    // Add all personel information cookies 
                    string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
                    JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
                    MediaJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
                    operators.Add(new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] },{ "display", PersonnalIDscookies["role"]["organizationName"] }});
                    MediaJson["operators"] = operators;
                    content.Add(new JObject { { "contentType", "image/jpeg" },
                        { "url", (Configuration["Repository_gateway_media"]+x["attachment"]["url"]) },{ "title", (x["attachment"]["url"]) }});
                    MediaJson["content"] = content;

                    JObject callBackParams = new JObject();
                    var respmedia = httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Media", MediaJson, "Bearer 123", null, null);
                    JObject respmediaJson = JObject.Parse((respmedia as OkObjectResult).Value.ToString());
                    mediasdoc.Add(respmediaJson);
                    JObject link = new JObject();
                    link["reference"] = (string)respmediaJson["id"];
                    mediadrreport.Add(new JObject{ {"link", link } });
                }
            }
            media_documentndreport.Add(new JObject { { "entry", mediasdoc } });
            media_documentndreport.Add(new JObject { { "content", mediadrreport } });
            return media_documentndreport;
        }

        public IActionResult UploadDokumentSkinLesion(JObject callBackParams, string rowNum)
        {
            HTTPrequest httpRequest = new HTTPrequest();
            JObject DocumentJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["documentbundleskinlesion_path"]));
            JArray subject = new JArray(), docentry = new JArray(), compentry = new JArray(), author = new JArray(), custodian = new JArray();
            docentry = new JArray(callBackParams["mediaentry"]); 
            
            // Upload composition resource
            JObject CompositionJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["composition_path"])); 
            CompositionJson["date"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");

            // Map all personel information cookies to composition
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            subject.Add(new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } });
            CompositionJson["subject"] = subject;
            DocumentJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
            author.Add(new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] }, { "display", PersonnalIDscookies["role"]["organizationName"] } });
            CompositionJson["author"] = author;
            DocumentJson["author"] = author;
            CompositionJson["custodian"] = new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] }, { "display", PersonnalIDscookies["role"]["name"] } };
            DocumentJson["custodian"] = new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] }, { "display", PersonnalIDscookies["role"]["name"] } };
            
            // Add all personel information cookies to composition and document entry
            string[] resources = { (PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"]),("Organization/" + PersonnalIDscookies["role"]["organizationId"]),(PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"])  };
            foreach (string x in resources)
            {
                compentry.Add(new JObject { { "reference",x } });
                var respPersonnal = httpRequest.getResource(Configuration["FHIR_server"], "Bearer 123", x, "", Configuration["FHIRResponseType"]);
                if (respPersonnal.GetType().Equals(typeof(OkObjectResult)))
                {
                    JObject respDocrefIDJson = JObject.Parse((respPersonnal as OkObjectResult).Value.ToString());
                    docentry.Add(respDocrefIDJson);
                }
            }

            // Read all document reference of image
            string DocrefIDs = HttpContext.Request.Cookies["DocrefIDs"];
            JObject DocrefIDscookies = JObject.Parse((string)DocrefIDs);
            foreach (var x in DocrefIDscookies)
            {
                compentry.Add(new JObject { { "reference", "DocumentReference/" + x.Value } });
                var respDocref = httpRequest.getResource(Configuration["Repository_gateway_fhir"], "Bearer 123", "DocumentReference/", (string)x.Value, Configuration["FHIRResponseType"]);
                if (respDocref.GetType().Equals(typeof(OkObjectResult)))
                {
                    JObject respDocrefIDJson = JObject.Parse((respDocref as OkObjectResult).Value.ToString());
                    docentry.Add(respDocrefIDJson);
                }
            }

            // Add all annotation cookies
            string annotationIDs = HttpContext.Request.Cookies["AnnotationIDs"];
            JObject annotationIDscookies = JObject.Parse((string)annotationIDs);
            foreach (var x in annotationIDscookies)
            {
                compentry.Add(new JObject { { "reference", "Observation/" + x.Value } });
                var respAnnotation = httpRequest.getResource(Configuration["Repository_gateway_fhir"], "Bearer 123", "Observation/", (string)x.Value, Configuration["FHIRResponseType"]);
                if (respAnnotation.GetType().Equals(typeof(OkObjectResult)))
                {
                    JObject respAnnotationJson = JObject.Parse((respAnnotation as OkObjectResult).Value.ToString());
                    docentry.Add(respAnnotationJson);
                }
            }
            // Read finding cookies and record in the result component
            string findingIDs = HttpContext.Request.Cookies["FindingIDs"];
            JObject findingIDscookies = JObject.Parse((string)findingIDs);
            foreach (var x in findingIDscookies)
            {
                compentry.Add(new JObject { { "reference", "Observation/" + x.Value } });
                var respFinding = httpRequest.getResource(Configuration["Repository_gateway_fhir"], "Bearer 123", "Observation/", (string)x.Value, Configuration["FHIRResponseType"]);
                if (respFinding.GetType().Equals(typeof(OkObjectResult)))
                {
                    JObject respFindingJson = JObject.Parse((respFinding as OkObjectResult).Value.ToString());
                    docentry.Add(respFindingJson);
                }
            }
            CompositionJson["section"][0]["entry"] = compentry;
            // Add the diagnostic report and composition to document
            JObject respDReportJson = JObject.Parse((callBackParams["result"].ToString() ));
            var respcomp = httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Composition", CompositionJson, "Bearer 123", null, null);
            var resultDreportJson = JObject.Parse((string)callBackParams["result"]);
            docentry.Add(respDReportJson);
            docentry.Add(resultDreportJson );
            DocumentJson["content"] = docentry;

            return httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Bundle", DocumentJson, "Bearer 123", IndexReportSkinLesion, callBackParams);
        }
        public IActionResult IndexReportSkinLesion(JObject callBackParams, string rowNum)
        {
            JArray author = new JArray();
            HTTPrequest httpRequest = new HTTPrequest();
            var bundledocumentresultJson = JObject.Parse((string)callBackParams["result"]);
            JObject DocRefJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["DocumentReferenceforIndex_path"]));
            DocRefJson["date"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");

            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            var respPatientPortal = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "Patient", "?identifier=" + PersonnalIDscookies["patient"]["identifier"], Configuration["FHIRResponseType"]);
            if (respPatientPortal.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject PatientportalJson = JObject.Parse((respPatientPortal as OkObjectResult).Value.ToString());
                DocRefJson["subject"] = new JObject { { "reference", (string)PatientportalJson["entry"][0]["resource"]["resourceType"] +"/"+ PatientportalJson["entry"][0]["resource"]["id"] }, { "display", (string)PatientportalJson["entry"][0]["resource"]["name"][0]["value"] } };
            }

            string PersonnalPortalIDs = HttpContext.Request.Cookies["PersonnalPortalIDs"];
            JObject PersonnalPortalIDscookies = JObject.Parse((string)PersonnalPortalIDs);
            author.Add(new JObject { { "reference", (string)PersonnalPortalIDscookies["role"]["resourceType"] +"/"+ PersonnalPortalIDscookies["role"]["id"] }, { "display", (string)PersonnalPortalIDscookies["role"]["name"] } });
            DocRefJson["author"] = author;
            DocRefJson["custodian"] = new JObject { { "reference","Organization/" + PersonnalPortalIDscookies["role"]["organizationId"] }, { "display", (string)PersonnalPortalIDscookies["role"]["organizationName"] } };
            DocRefJson["content"][0]["attachment"]["url"] = Configuration["Repository_gateway_fhir"]+ bundledocumentresultJson["resourceType"] + bundledocumentresultJson["id"];
            DocRefJson["content"][0]["attachment"]["title"] = Configuration["Repository_gateway_fhir"] + bundledocumentresultJson["id"];

            return httpRequest.postResource(Configuration["FHIR_server_portal"], "DocumentReference", DocRefJson, "Bearer 123", null, null);
        }
    }
}
