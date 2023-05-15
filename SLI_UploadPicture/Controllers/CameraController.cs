using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using Microsoft.Extensions.FileProviders;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.StaticFiles;
using SLI_UploadPicture.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Text;
using Renci.SshNet;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Http;

namespace SLI_UploadPicture.Controllers
{
    public class CameraController : Controller
    {
        // requires using Microsoft.Extensions.Configuration;
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        public CameraController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Environment = hostingEnvironment;
            Configuration = configuration;
        }
        [HttpGet]
        public IActionResult Capture(string serviceName)
        {
            //// Check token 
            //string authorizationToken = Request.Headers["Authorization"]; //eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJqdGkiOiJqd3QtdG9rZW4taWQiLCJpc3MiOiJodHRwOi8vMjAzLjY0Ljg0LjMzOjMzNDg0LyIsImF1ZCI6Imh0dHBzOi8vMjAzLjY0Ljg0LjE1MDo1ODQ0MyIsImlhdCI6MTY4MzM4MzkyNiwibmJmIjoxNjgzMzgzOTI2LCJleHAiOjE2ODMzODQ1MjYsInN1YiI6Imh0dHBzOi8vMjAzLjY0Ljg0LjE1MDo1ODQ0My9yNS9maGlyL1ByYWN0aXRpb25lclJvbGUvYWYwNTVjYjYtYmQwNi00Y2U0LTk5NjctOTBkNGU5ZjI1MDQ0Iiwic2NvcGUiOiJbe3VybDpodHRwOi8vMjAzLjY0Ljg0LjMyOjk4NzYvZmhpci9CdW5kbGUvNzI1fV0ifQ.uD9wYh9oRaAiNM-aWbUHtcdOcBoXb3Gpd_a70Xoq71k
            //if (CheckToken(authorizationToken)) { 
                foreach (string key in HttpContext.Request.Cookies.Keys)
                {
                    HttpContext.Response.Cookies.Append(key, "", new Microsoft.AspNetCore.Http.CookieOptions
                    { Expires = DateTime.Now.AddDays(-1), });
                }
                //Create new web of searcing patient
                //Get practitionerrole from portal and serach its practitioner role on repository fhir based on the practitioner identifier and practitioner, and organization
                //Get patient on fhir repository
                // create personnalPortalIDs and PersonnalIDs cookie
                JObject PersonnalPortalIDs = new JObject(), role = new JObject(), PersonnalIDs = new JObject(), patient = new JObject();
                role["resourceType"] = "PractitionerRole";
                role["id"] = "af055cb6-bd06-4ce4-9967-90d4e9f25044";
                role["identifier"] = "YuliHospital_Practitioner01";
                role["name"] = "Dr. Carolina SpKK";
                role["organizationId"] = "4af8db0c-50d1-406e-a134-d2972c79f194";
                role["organizationName"] = "Yuli Hospital";
                PersonnalPortalIDs["role"] = role;

                patient["resourceType"] = "Patient";
                patient["id"] = "88df8172-9f58-4a0f-bcca-bcdcd5ea6485";
                patient["identifier"] = "YuliHospital_Patient01";
                patient["name"] = "Will Smith";
                patient["organizationId"] = "4af8db0c-50d1-406e-a134-d2972c79f194";
                patient["organizationName"] = "Yuli Hospital";
                PersonnalIDs["role"] = role;
                PersonnalIDs["patient"] = patient;

                string PersonnalPortalIDsjsonstr = PersonnalPortalIDs.ToString(Newtonsoft.Json.Formatting.None);
                HttpContext.Response.Cookies.Append("PersonnalPortalIDs", PersonnalPortalIDsjsonstr, new Microsoft.AspNetCore.Http.CookieOptions
                { Expires = DateTime.Now.AddHours(1), });
                string PersonnalIDsjsonstr = PersonnalIDs.ToString(Newtonsoft.Json.Formatting.None);
                HttpContext.Response.Cookies.Append("PersonnalIDs", PersonnalIDsjsonstr, new Microsoft.AspNetCore.Http.CookieOptions
                { Expires = DateTime.Now.AddHours(1), });

                if (serviceName == "encounterHospital")
                {
                    ViewBag.Title = "Encounter Report Creator - Upload Picture";
                    ViewBag.Author = "Practitioner ID";
                    return View();
                }
                else
                {
                    ViewBag.Title = "Skin Lesion Image Record Creator - Upload Picture";
                    ViewBag.Author = "Uploader ID";
                    return View("UploadImageforPHR");
                }
            //}
            //else
            //{
            //    return View("../Shared/error");
            //}
        }

        private bool CheckToken(string authorizationToken)
        {
            byte[] data = Convert.FromBase64String(authorizationToken);
            string decodedString = Encoding.UTF8.GetString(data);
            return true;
        }

        [HttpPost]
        public IActionResult CaptureImg(string encounterId)
        {
            dynamic errmsg = new JObject();
            var files = HttpContext.Request.Form.Files;
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        // Getting Filename  
                        var fileName = file.FileName;
                        // Unique filename "Guid"  
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());
                        // Getting Extension  
                        var fileExtension = Path.GetExtension(fileName);
                        // Concating filename + fileExtension (unique filename)  
                        var newFileName = string.Concat(myUniqueFileName, fileExtension);
                        //  Generating Path to store photo   
                        //var filepath = Path.Combine(Environment.WebRootPath, "CameraPhotos", encounterId);// + $@"\{newFileName}";

                        //if (!string.IsNullOrEmpty(filepath))
                        //{
                        //    // Storing Image in Folder  
                        //    StoreInFolder(file, filepath, newFileName);
                        //    Console.Write("FilePath is exist.\n");
                        //}
                        try
                        {
                            String host = Configuration["sftp_Host"];//@"59.126.145.136";
                            String username = Configuration["sftp_username"];//@"linux1";
                            String password = Configuration["sftp_password"];//@"a0933475910!@#";
                            int port = int.Parse(Configuration["sftp_port"]);//15322;
                            string destinationpath = Configuration["sftp_imgUrl"] + encounterId;
                            Console.Write("Store on ftp.\n");
                            using (SftpClient client = new SftpClient(host, port, username, password))
                            {
                                client.Connect();
                                if (client.Exists(destinationpath))
                                {
                                    Console.Write("Directory already exists.\n");
                                }
                                else
                                {
                                    client.CreateDirectory(destinationpath);
                                    Console.Write("CreateDirectory on ftp.\n");
                                }

                                client.ChangeDirectory(destinationpath);
                                using (var uplfileStream = file.OpenReadStream())
                                {
                                    client.BufferSize = 4 * 1024;
                                    client.UploadFile(uplfileStream, newFileName);
                                }
                                Console.Write("Upload at on ftp.\n");
                                client.Disconnect();
                            }
                        }
                        catch (Exception e)
                        {
                            errmsg.Message = e.Message;
                            return Json(errmsg);
                        }
                    }
                }
                return Json(true);
            }
            else
            {
                return Json(false);
            }

        }
        [HttpPost]
        public IActionResult DeleteImage(string filePath)
        {
            try
            {
                String host = Configuration["sftp_Host"];//@"59.126.145.136";
                String username = Configuration["sftp_username"];//@"linux1";
                String password = Configuration["sftp_password"];//@"a0933475910!@#";
                int port = int.Parse(Configuration["sftp_port"]);//15322;
                string destinationpath = Configuration["sftp_imgUrl"] + filePath;

                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();
                    if (client.Exists(destinationpath))
                    {
                        client.DeleteFile(destinationpath);
                        return Json(true);
                    }
                    else
                    {
                        Console.WriteLine("File not found");
                        return Json(false);
                    }
                    client.Disconnect();
                }
            }
            catch (Exception)
            {
                throw;
                return Json(false);
            }

            //try
            //{
            //    var fullPath = Environment.WebRootPath+filePath;
            //    // Check if file exists with its full path    
            //    if (System.IO.File.Exists(fullPath))
            //    {
            //        // If file found, delete it    
            //        System.IO.File.Delete(fullPath);
            //        Console.WriteLine("File deleted.");
            //        return Json(true);
            //    }
            //    else { Console.WriteLine("File not found");
            //        return Json(false);
            //    }
            //}
            //catch (IOException ioExp)
            //{
            //    Console.WriteLine(ioExp.Message);
            //    return Json(false);
            //}
        }
        /// <summary>  
        /// Saving captured image into Folder.  
        /// </summary>  
        /// <param name="file"></param>  
        /// <param name="fileName"></param>  
        private void StoreInFolder(IFormFile file, string fileName, string newFileName)
        {
            System.IO.Directory.CreateDirectory(fileName);
            using (FileStream fs = System.IO.File.Create(fileName + $@"\{newFileName}"))
            {
                file.CopyTo(fs);
                fs.Flush();
            }
        }
        public List<string> ImageList { get; set; }

        /// <summary>
        /// List all image in Encounter-image folder
        /// </summary>
        /// <returns>Encounter images</returns>
        [HttpPost]
        public List<string> GetCameraPhotos(string encounterId)
        {
            ImageList = new List<string>();

            try
            {
                String host = Configuration["sftp_Host"];//@"59.126.145.136";
                String username = Configuration["sftp_username"];//@"linux1";
                String password = Configuration["sftp_password"];//@"a0933475910!@#";
                int port = int.Parse(Configuration["sftp_port"]);//15322;
                string dirName = Configuration["sftp_imgUrl"] + encounterId;

                using (SftpClient client = new SftpClient(host, port, username, password))
                {
                    client.Connect();

                    if(dirName!=null)
                    {
                        foreach (var entry in client.ListDirectory(dirName))
                        {
                            if (!entry.IsDirectory)
                            { ImageList.Add(entry.Name); }
                        }
                    }
                    client.Disconnect();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return ImageList;

            //var provider = new PhysicalFileProvider(Environment.WebRootPath);
            //var contents = provider.GetDirectoryContents(Path.Combine("CameraPhotos", encounterId));
            //var objFiles = contents.OrderBy(m => m.LastModified);

            //ImageList = new List<string>();
            //foreach (var item in objFiles.ToList())
            //{
            //    ImageList.Add(item.Name);
            //}
            //return ImageList;
        }

        /// <summary>
        /// List all image in Encounter-image folder
        /// </summary>
        /// <returns>Encounter images</returns>
        [HttpPost]
        public IActionResult UploadDocumentReference([FromBody] EncounterInformation encounterInfo)
        {
            //var provider = new PhysicalFileProvider(Environment.WebRootPath);
            //var contents = provider.GetDirectoryContents(Path.Combine("CameraPhotos", encounterInfo.encounterID));
            //var objFiles = contents.OrderBy(m => m.LastModified);

            List<string> Images= GetCameraPhotos(encounterInfo.encounterID);

            JObject docRefJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["DocumentReference_forimg_path"]));
            JArray content = new JArray(),author = new JArray(), context = new JArray(), organization = new JArray();

            docRefJson["identifier"][0]["value"] = Convert.ToString(Guid.NewGuid());
            context.Add(new JObject { { "reference", "Encounter/" + encounterInfo.encounterID } });
            docRefJson["context"] = context;
            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            docRefJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
            author.Add(new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] }, { "display", PersonnalIDscookies["role"]["name"] } });
            docRefJson["author"] = author;
            organization.Add(new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] }, { "display", PersonnalIDscookies["role"]["organizationName"] } });
            docRefJson["custodian"] = organization;
            //docRefJson["subject"]["reference"] = "Patient/"+encounterInfo.patientID;

            docRefJson["date"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");
            foreach (var item in Images)
            {
                JObject attachmentJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["atachmentdata_path"]));
                string contentType = "";
                attachmentJson["attachment"]["contentType"] = (!new FileExtensionContentTypeProvider().TryGetContentType(item, out contentType)) ? "application/octet-stream" : contentType;
                attachmentJson["attachment"]["url"] = Configuration["http_SLIUrl"]+ encounterInfo.encounterID + "/" +item;
                attachmentJson["attachment"]["title"] = item;
                content.Add(JToken.FromObject(attachmentJson));
            }
            docRefJson["content"] = content;
            if (encounterInfo.serviceName!= "encounterHospital")
            {
                docRefJson["practiceSetting"] = "";
                docRefJson["custodian"] = "";
            }
            JObject callBackParams = new JObject();
            callBackParams["cookieName"] = "DocrefIDs";
            callBackParams["rowslength"] = "-1";
            HTTPrequest httpRequest = new HTTPrequest();
            var respdocument = new object();
            var respdocref =  httpRequest.postResource(Configuration["Repository_gateway_fhir"], "DocumentReference", docRefJson, "Bearer 123", recordDocref, callBackParams);
            return UploadDokumentSkinLesion(JObject.Parse((respdocref as OkObjectResult).Value.ToString()), "0"); 
        }
        /// <summary>
        /// Upload encounter for recording image and condition by person
        /// </summary>
        /// <returns>Encounter id</returns>
        [HttpPost]
        public IActionResult UploadEncounter()
        {
            JObject encounterJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["encounter_recordImagebySelfUse_path"]));

            encounterJson["id"] = Convert.ToString(Guid.NewGuid());
            HTTPrequest httpRequest = new HTTPrequest();
            return httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Encounter", encounterJson, "Bearer 123", null,null);
        }
        public IActionResult UploadDokumentSkinLesion(JObject callBackParams, string rowNum)
        {
            HTTPrequest httpRequest = new HTTPrequest();
            JObject DocumentJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["documentbundleskinlesion_path"]));
            JArray subject = new JArray(), docentry = new JArray(), compentry = new JArray(), author = new JArray(), docentrycomfirst = new JArray();

            // Upload composition resource
            JObject CompositionJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["composition_path"]));
            CompositionJson["date"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");

            // Map all personel information cookies to composition
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            subject.Add(new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } });
            CompositionJson["subject"] = subject;
            DocumentJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } }; 
            author.Add(new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] }, { "display", PersonnalIDscookies["role"]["name"] } });
            CompositionJson["author"] = author;
            DocumentJson["author"] = author;
            CompositionJson["custodian"] = new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] }, { "display", PersonnalIDscookies["role"]["organizationName"] } };
            DocumentJson["custodian"] = new JObject { { "reference", "Organization/" + PersonnalIDscookies["role"]["organizationId"] }, { "display", PersonnalIDscookies["role"]["organizationName"] } };
            
            // Add all personel information cookies to composition and document entry
            string[] resources = { (PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"]), ("Organization/" + PersonnalIDscookies["role"]["organizationId"]), (PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"]) };
            foreach (string x in resources)
            {
                compentry.Add(new JObject { { "reference", x } });
                var respPersonnal = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", x, "", Configuration["FHIRResponseType"]);
                if (respPersonnal.GetType().Equals(typeof(OkObjectResult)))
                {
                    JObject respDocrefIDJson = JObject.Parse((respPersonnal as OkObjectResult).Value.ToString());
                    docentry.Add(new JObject { { "fullUrl", Configuration["FHIR_server_portal"] + x }, { "resource", respDocrefIDJson } });
                }
            }

            // Read all document reference of image
            compentry.Add(new JObject { { "reference", "DocumentReference/" + callBackParams["id"] } });
            var respDocref = httpRequest.getResource(Configuration["Repository_gateway_fhir"], "Bearer 123", "DocumentReference/", (string)callBackParams["id"], Configuration["FHIRResponseType"]);
            if (respDocref.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject respDocrefIDJson = JObject.Parse((respDocref as OkObjectResult).Value.ToString());
                docentry.Add(new JObject { { "fullUrl", Configuration["FHIR_server_portal"] + "DocumentReference/" + (string)callBackParams["id"] },{ "resource", respDocrefIDJson } });
            } 

            CompositionJson["section"][0]["entry"] = compentry;
            // Add the diagnostic report and composition to document
            var respcomp = httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Composition", CompositionJson, "Bearer 123", null, null);
            var respcompJson = JObject.Parse(((respcomp as OkObjectResult).Value).ToString()); ;
            docentrycomfirst.Add(new JObject { { "fullUrl", Configuration["FHIR_server_portal"] + "Composition/" + (string)respcompJson["id"] }, { "resource", respcompJson } });
            foreach (JObject x in docentry)
            {
                docentrycomfirst.Add(x);
            }

            DocumentJson["entry"] = docentrycomfirst;
            var bundledocresp = httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Bundle", DocumentJson, "Bearer 123", null, null);
            var docrefindex = IndexReportSkinLesion(JObject.Parse((bundledocresp as OkObjectResult).Value.ToString()), "0");
            if ((docrefindex as OkObjectResult).StatusCode == 200)
            {
                return bundledocresp;
            }
            dynamic errmsg = new JObject();
            errmsg.Message = "Error post document bundle!";
            return new BadRequestObjectResult(errmsg);
        }
        public IActionResult IndexReportSkinLesion(JObject callBackParams, string rowNum)
        {
            JArray author = new JArray();
            HTTPrequest httpRequest = new HTTPrequest();
            var bundledocumentresultJson = callBackParams;
            JObject DocRefJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["DocumentReferenceforIndex_path"]));
            DocRefJson["date"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");
            DocRefJson["type"]["coding"][0]["code"] = "skinlesion.image.document";
            DocRefJson["type"]["coding"][0]["display"] = "Skin lesion image document";

            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            string patidentifier = (string)PersonnalIDscookies["patient"]["identifier"];
            var respPatientPortal = httpRequest.getResource(Configuration["FHIR_server_portal"], "Bearer 123", "Patient", ("?identifier=" + (string)PersonnalIDscookies["patient"]["identifier"]), Configuration["FHIRResponseType"]);
            if (respPatientPortal.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject PatientportalJson = JObject.Parse((respPatientPortal as OkObjectResult).Value.ToString());
                DocRefJson["subject"] = new JObject { { "reference", (string)PatientportalJson["entry"][0]["resource"]["resourceType"] + "/" + PatientportalJson["entry"][0]["resource"]["id"] }, { "display", (string)PatientportalJson["entry"][0]["resource"]["name"][0]["text"] } };
            }

            string PersonnalPortalIDs = HttpContext.Request.Cookies["PersonnalPortalIDs"];
            JObject PersonnalPortalIDscookies = JObject.Parse((string)PersonnalPortalIDs);
            author.Add(new JObject { { "reference", (string)PersonnalPortalIDscookies["role"]["resourceType"] +"/"+ PersonnalPortalIDscookies["role"]["id"] }, { "display", (string)PersonnalPortalIDscookies["role"]["name"] } });
            DocRefJson["author"] = author;
            DocRefJson["custodian"] = new JObject { { "reference", "Organization/" + PersonnalPortalIDscookies["role"]["organizationId"] }, { "display", (string)PersonnalPortalIDscookies["role"]["organizationName"] } };
            DocRefJson["content"][0]["attachment"]["url"] = Configuration["Repository_gateway_fhir"] + bundledocumentresultJson["resourceType"] + "/" + bundledocumentresultJson["id"];
            DocRefJson["content"][0]["attachment"]["title"] = Configuration["Repository_gateway_fhir"] + bundledocumentresultJson["resourceType"] + "/" + bundledocumentresultJson["id"];

            return httpRequest.postResource(Configuration["FHIR_server_portal"], "DocumentReference", DocRefJson, "Bearer 123", null, null);
        }
        public IActionResult recordDocref(JObject callBackParams, string rowNum)
        {
            var resultJson = JObject.Parse((string)callBackParams["result"]);
            string annotationIDs = HttpContext.Request.Cookies[(string)callBackParams["cookieName"]];
            JObject annotationIdscookies;
            if (annotationIDs == null)
            {
                annotationIdscookies = new JObject();
            }
            else
            {
                annotationIdscookies = JObject.Parse((string)annotationIDs);
            }
            annotationIdscookies.Add(new JProperty((string)callBackParams["rowslength"], (string)resultJson["id"]));
            string annotationIDsjsonstr = annotationIdscookies.ToString(Newtonsoft.Json.Formatting.None);
            HttpContext.Response.Cookies.Append("DocrefIDs", annotationIDsjsonstr, new Microsoft.AspNetCore.Http.CookieOptions
            { Expires = DateTime.Now.AddHours(1), });
            return new OkObjectResult((string)callBackParams["result"]);
        }
    }
}
