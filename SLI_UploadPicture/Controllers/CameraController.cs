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
        }

        [HttpPost]
        public IActionResult CaptureImg(string encounterId)
        {
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
                        var filepath = Path.Combine(Environment.WebRootPath, "CameraPhotos", encounterId);// + $@"\{newFileName}";

                        if (!string.IsNullOrEmpty(filepath))
                        {
                            // Storing Image in Folder  
                            StoreInFolder(file, filepath, newFileName);
                        }
                        try
                        {
                            String host = Configuration["sftp_Host"];//@"59.126.145.136";
                            String username = Configuration["sftp_username"];//@"linux1";
                            String password = Configuration["sftp_password"];//@"a0933475910!@#";
                            int port = int.Parse(Configuration["sftp_port"]);//15322;
                            string destinationpath = Configuration["sftp_imgUrl"] + encounterId;
                            
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
                                }

                                client.ChangeDirectory(destinationpath);
                                using (var uplfileStream = file.OpenReadStream())
                                {
                                    client.BufferSize = 4 * 1024;
                                    client.UploadFile(uplfileStream, newFileName);
                                }
                                client.Disconnect();
                            }
                        }
                        catch (Exception)
                        {
                            throw;
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

                    foreach (var entry in client.ListDirectory(dirName))
                    {
                        if (!entry.IsDirectory)
                        { ImageList.Add(entry.Name); }
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
            author.Add(new JObject { { "reference", encounterInfo.uploaderID } });
            organization.Add(new JObject { { "reference", "Organization/" + encounterInfo.organizationID } });
            docRefJson["custodian"] = organization;
            docRefJson["context"]= context;
            docRefJson["subject"]["reference"] = "Patient/"+encounterInfo.patientID;
            docRefJson["author"] = author;
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

            HTTPrequest httpRequest = new HTTPrequest();
            return httpRequest.postResource(Configuration["FHIR_server"], "DocumentReference", docRefJson, "", null);
        }
        /// <summary>
        /// Upload encounter for recording image and condition by person
        /// </summary>
        /// <returns>Encounter id</returns>
        [HttpPost]
        public IActionResult UploadEncounter()
        {
            JObject encounterJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["encounter_recordImagebySelfUse_path"]));
            JArray content = new JArray(), author = new JArray(), context = new JArray();

            encounterJson["id"] = Convert.ToString(Guid.NewGuid());
            HTTPrequest httpRequest = new HTTPrequest();
            return httpRequest.postResource(Configuration["FHIR_server"], "Encounter", encounterJson, "", null);
        }
    }
}
