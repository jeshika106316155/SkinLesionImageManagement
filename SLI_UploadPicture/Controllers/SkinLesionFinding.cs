using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SLI_UploadPicture.Models;
using System.Web.Helpers;

namespace SLI_UploadPicture.Controllers
{
    public class SkinLesionFinding : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        public SkinLesionFinding(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Environment = hostingEnvironment;
            Configuration = configuration;
        }
        public IActionResult Index(string annotationnum, string findingID)
        {
            string annotationIDs = HttpContext.Request.Cookies["AnnotationIDs"];
            JObject annotationIdscookies;
            if (annotationIDs != null)
            {
                annotationIdscookies = JObject.Parse((string)annotationIDs);
                string annotatioID = (string)annotationIdscookies[annotationnum];

                // Add cookie of targeted annotation ID
                HttpContext.Response.Cookies.Append("targetedAnnotationId", annotatioID, new Microsoft.AspNetCore.Http.CookieOptions
                { Expires = DateTime.Now.AddHours(1), });
                ViewBag.annotationnum = annotationnum;
            }
            HTTPrequest httpRequest = new HTTPrequest();
            
            var resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/ca1dc86c-c762-4eb3-aaa3-04acea091ece", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> skintype = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    skintype.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.skintype = JsonConvert.SerializeObject(skintype, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/7c050998-6d9a-4332-b41b-17f951fc5088", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> bodysite = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    bodysite.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.bodysite = JsonConvert.SerializeObject(bodysite, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/37610548-9669-4199-916c-7a151fc015bd", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> arrangement = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    arrangement.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.arrangement = JsonConvert.SerializeObject(arrangement, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/d28ce8fd-fd07-48bd-b37d-d0610f5771a8", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> border = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    border.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.border = JsonConvert.SerializeObject(border, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/85b27c7f-feb5-4c38-a758-c4a2821591e7", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> color = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    color.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.color = JsonConvert.SerializeObject(color, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/bfc9d3ca-240d-4575-9e84-f383b51e6c8e", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> primarymorphology = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    primarymorphology.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.primarymorphology = JsonConvert.SerializeObject(primarymorphology, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/191503f2-ba04-46d5-8b71-ae243333cee6", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> secondarychange = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    secondarychange.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.secondarychange = JsonConvert.SerializeObject(secondarychange, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/ee91548f-3b4e-47bb-955f-1b1c0d59628c", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> shapeconfiguration = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    shapeconfiguration.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.shapeconfiguration = JsonConvert.SerializeObject(shapeconfiguration, Formatting.Indented);
            }
            resp = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "ValueSet", "/525967e6-eadc-4fe0-8ebb-f2987826a508", Configuration["FHIRResponseType"]);
            if (resp.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject valueset = JObject.Parse((resp as OkObjectResult).Value.ToString());
                List<JObject> reactionpattern = new List<JObject>();

                foreach (JObject code in valueset["compose"]["include"][0]["concept"])
                {
                    reactionpattern.Add(new JObject(new JProperty("code", code["code"]), new JProperty("display", code["display"])));
                }
                ViewBag.reactionpattern = JsonConvert.SerializeObject(reactionpattern, Formatting.Indented);
            }
            if (findingID!=null)
            {
                // Get FHIR  finding and put on viewbag
                resp = httpRequest.getResource(Configuration["Repository_gateway_fhir"], "Bearer 123", "Observation/", findingID, Configuration["FHIRResponseType"]);
                if (resp.GetType().Equals(typeof(OkObjectResult)))
                {
                    JObject findingresp = JObject.Parse((resp as OkObjectResult).Value.ToString());
                    JObject components = new JObject();

                    foreach (JObject component in findingresp["component"])
                    {
                        string compcode = ((string)component["code"]["coding"][0]["code"]).Replace(".","_" );
                        if (compcode == "skinlesion_length" || compcode == "skinlesion_width" || compcode == "skinlesion_depth")
                        {
                            components.Add(((string)component["code"]["coding"][0]["code"]), (component["valueQuantity"]["value"]));
                        }
                        else if (compcode == "PersonalHxmelanoma" || compcode == "FamilyHxmelanoma" || compcode == "skinlesion_evolution")
                        {
                            components.Add(((string)component["code"]["coding"][0]["code"]), (component["valueBoolean"]));
                        }
                        else
                        {
                            components.Add(((string)component["code"]["coding"][0]["code"]), (component["valueCodeableConcept"]["coding"][0]["code"]));
                        }
                    }
                    components.Add("Location", (findingresp["bodySite"]["coding"][0]["code"]));
                    ViewBag.components = JsonConvert.SerializeObject(components, Formatting.Indented);
                }
                ViewBag.viewFinding = true;
            }
            else { ViewBag.viewFinding = false; }
            return View();
        }
        /// <summary>
        /// Upload FHIR Observation for Finding 
        /// </summary>
        /// <returns>Fhir ObservationnFinding</returns>
        [HttpPost]
        public IActionResult UploadFindingObservation([FromBody] FindingInformation FindingInfo)
        {
            JObject findingObsJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["findingObservation_path"]));
            
            JArray performer = new JArray(), component = new JArray(), author = new JArray(), organization = new JArray();
            findingObsJson["effectiveDateTime"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");
            findingObsJson["derivedFrom"][0]["reference"] = "Observation/"+ HttpContext.Request.Cookies["targetedAnnotationId"]; 
            findingObsJson["identifier"][0]["value"] = FindingInfo.lesionname; 
            findingObsJson["bodySite"]["coding"][0]["code"] = FindingInfo.Location;

            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            findingObsJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
            performer.Add(new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] },
            { "display", PersonnalIDscookies["role"]["organizationName"] }});
            findingObsJson["performer"] = performer;

            string a = JsonConvert.SerializeObject(FindingInfo);
            dynamic findingInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(a);
            foreach (var x in findingInfo)
            {
                if (x.Key != "Location" && x.Key != "lesionname")
                {
                    JObject obsComponentcodeJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_code_path"]));
                    obsComponentcodeJson["code"]["coding"][0]["system"] = "https://203.64.84.150:58443/r5/fhir/CodeSystem/8f414151-bf5f-46ce-94fe-f96d9e867d29\",";
                    obsComponentcodeJson["code"]["coding"][0]["code"] = (x.Key).Replace("_", "."); ;

                    if (x.Key == "skinlesion_length" || x.Key == "skinlesion_width" || x.Key == "skinlesion_depth")
                    {
                        JObject obsComponentvqJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_valueQuantity_path"]));
                        obsComponentvqJson["value"] = x.Value;
                        obsComponentcodeJson.Add("valueQuantity", obsComponentvqJson);
                    }
                    else if (x.Key == "PersonalHxmelanoma" || x.Key == "FamilyHxmelanoma" || x.Key == "skinlesion_evolution")
                    {
                        obsComponentcodeJson.Add("valueBoolean", x.Value);
                    }
                    else
                    {
                        JObject obsComponentvccJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_valueCodeableConcept_path"]));
                        obsComponentvccJson["coding"][0]["system"] = "https://203.64.84.150:58443/r5/fhir/CodeSystem/8f414151-bf5f-46ce-94fe-f96d9e867d29\",";
                        obsComponentvccJson["coding"][0]["code"] = x.Value;
                        obsComponentcodeJson.Add("valueCodeableConcept", obsComponentvccJson);
                    }
                    component.Add(obsComponentcodeJson);
                }
            }
            findingObsJson["component"] = component;

            JObject callBackParams = new JObject();
            callBackParams["rowslength"] = ViewBag.annotationID;
            callBackParams["cookieName"] = "FindingIDs";

            HTTPrequest httpRequest = new HTTPrequest();
            return httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Observation", findingObsJson, "Bearer 123", recordFinding, callBackParams);
        }
        /// <summary>
        /// Upload FHIR Observation for Finding 
        /// </summary>
        /// <returns>Fhir ObservationnFinding</returns>
        [HttpPut]
        public IActionResult UpdateFindingObservation([FromBody] FindingInformation FindingInfo)
        {
            JObject findingObsJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["findingObservation_path"]));

            JArray performer = new JArray(), component = new JArray(), author = new JArray(), organization = new JArray();
            findingObsJson.Add("id", FindingInfo.findingID);
            findingObsJson["effectiveDateTime"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");
            findingObsJson["derivedFrom"][0]["reference"] = "Observation/" + HttpContext.Request.Cookies["targetedAnnotationId"];
            findingObsJson["identifier"][0]["value"] = FindingInfo.lesionname;
            findingObsJson["bodySite"]["coding"][0]["code"] = FindingInfo.Location;

            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            findingObsJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
            performer.Add(new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] },{ "display", PersonnalIDscookies["role"]["organizationName"] }});
            findingObsJson["performer"] = performer;

            string a = JsonConvert.SerializeObject(FindingInfo);
            dynamic findingInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(a);
            foreach (var x in findingInfo)
            {
                if (x.Key != "Location" && x.Key != "lesionname")
                {
                    JObject obsComponentcodeJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_code_path"]));
                    obsComponentcodeJson["code"]["coding"][0]["system"] = "https://203.64.84.150:58443/r5/fhir/CodeSystem/8f414151-bf5f-46ce-94fe-f96d9e867d29\",";
                    obsComponentcodeJson["code"]["coding"][0]["code"] = (x.Key).Replace("_", "."); 

                    if (x.Key == "skinlesion_length" || x.Key == "skinlesion_width" || x.Key == "skinlesion_depth")
                    {
                        JObject obsComponentvqJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_valueQuantity_path"]));
                        obsComponentvqJson["value"] = x.Value;
                        obsComponentcodeJson.Add("valueQuantity", obsComponentvqJson);
                    }
                    else if (x.Key == "PersonalHxmelanoma" || x.Key == "FamilyHxmelanoma" || x.Key == "skinlesion_evolution")
                    {
                        obsComponentcodeJson.Add("valueBoolean", x.Value);
                    }
                    else
                    {
                        JObject obsComponentvccJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_valueCodeableConcept_path"]));
                        obsComponentvccJson["coding"][0]["system"] = "https://203.64.84.150:58443/r5/fhir/CodeSystem/8f414151-bf5f-46ce-94fe-f96d9e867d29\",";
                        obsComponentvccJson["coding"][0]["code"] = x.Value;
                        obsComponentcodeJson.Add("valueCodeableConcept", obsComponentvccJson);
                    }
                    component.Add(obsComponentcodeJson);
                }
            }
            findingObsJson["component"] = component;

            HTTPrequest httpRequest = new HTTPrequest();
            return httpRequest.putResource(Configuration["Repository_gateway_fhir"], ("Observation/"+ FindingInfo.findingID.ToString()), findingObsJson, "Bearer 123", null, null);
        }
        public IActionResult recordFinding(JObject callBackParams, string rowNum)
        {
            var resultJson = JObject.Parse((string)callBackParams["result"]);
            string FindingIDs = HttpContext.Request.Cookies[(string)callBackParams["cookieName"]];
            JObject FindingIDscookies;
            if (FindingIDs == null)
            {
                FindingIDscookies = new JObject();
            }
            else
            {
                FindingIDscookies = JObject.Parse((string)FindingIDs);
            }
            FindingIDscookies.Add(HttpContext.Request.Cookies["targetedAnnotationId"], (string)resultJson["id"]);
            string FindingIDsjsonstr = FindingIDscookies.ToString(Newtonsoft.Json.Formatting.None);
            HttpContext.Response.Cookies.Append((string)callBackParams["cookieName"], FindingIDsjsonstr, new Microsoft.AspNetCore.Http.CookieOptions
            { Expires = DateTime.Now.AddHours(1), });
            return new OkObjectResult((string)callBackParams["result"]);
        }
    }
}
