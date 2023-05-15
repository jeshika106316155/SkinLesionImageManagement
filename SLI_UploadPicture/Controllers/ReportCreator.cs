using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Renci.SshNet.Security;
using RestSharp;
using SLI_UploadPicture.Models;
using System;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace SLI_UploadPicture.Controllers
{
    public class ReportCreator : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly IWebHostEnvironment Environment;
        static string key = "H707-tzuchiuniversity-masterprogram-110";//"H707tcumi110";
        public ReportCreator(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            Environment = hostingEnvironment;
            Configuration = configuration;
        }
        [HttpGet]
        public IActionResult Index(string DocumentBundle)
        {
            // Check token 
            
            string authorizationToken = Request.Headers["Authorization"]; //eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJqdGkiOiJqd3QtdG9rZW4taWQiLCJpc3MiOiJodHRwOi8vMjAzLjY0Ljg0LjMzOjMzNDg0LyIsImF1ZCI6Imh0dHBzOi8vMjAzLjY0Ljg0LjE1MDo1ODQ0MyIsImlhdCI6MTY4MzM4MzkyNiwibmJmIjoxNjgzMzgzOTI2LCJleHAiOjE2ODMzODQ1MjYsInN1YiI6Imh0dHBzOi8vMjAzLjY0Ljg0LjE1MDo1ODQ0My9yNS9maGlyL1ByYWN0aXRpb25lclJvbGUvYWYwNTVjYjYtYmQwNi00Y2U0LTk5NjctOTBkNGU5ZjI1MDQ0Iiwic2NvcGUiOiJbe3VybDpodHRwOi8vMjAzLjY0Ljg0LjMyOjk4NzYvZmhpci9CdW5kbGUvNzI1fV0ifQ.uD9wYh9oRaAiNM-aWbUHtcdOcBoXb3Gpd_a70Xoq71k
            authorizationToken = GenerateToken(key,"123","345");
            if (ValidateToken(authorizationToken))
            {
                HTTPrequest httpRequest = new HTTPrequest();
            var docref = getResourceinDocumentbundle(DocumentBundle);
            JObject respondocrefimage = JObject.Parse((docref as OkObjectResult).Value.ToString());
            
            //delete cookie and create new
            foreach (string key in HttpContext.Request.Cookies.Keys)
            {
                HttpContext.Response.Cookies.Append(key, "", new Microsoft.AspNetCore.Http.CookieOptions
                { Expires = DateTime.Now.AddDays(-1), });
            }
            JObject Docrefcookie = new JObject();
            Docrefcookie["-1"] = respondocrefimage["id"];
            string docrefIDsjsonstr = Docrefcookie.ToString(Newtonsoft.Json.Formatting.None);
            HttpContext.Response.Cookies.Append("DocrefIDs", docrefIDsjsonstr, new Microsoft.AspNetCore.Http.CookieOptions
            { Expires = DateTime.Now.AddHours(1), });
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

            if (docref.GetType().Equals(typeof(OkObjectResult)))
            {
                JObject docRef = JObject.Parse((docref as OkObjectResult).Value.ToString());
                List<string> ImageList = new List<string>();
                foreach (JObject content in docRef["content"])
                {
                    ImageList.Add((content["attachment"]["url"].ToString()));
                }
                ViewBag.images = ImageList;
            }
            return View();
            }
            else
            {
                return View("../Shared/error");
            }
        }
        private static bool ValidateToken(string authToken)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = "http://203.64.84.32:9876/",
                ValidIssuer = "http://203.64.84.33:33484/",
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(hmac.Key)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(authToken, validationParameters, out var validToken);
                //var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp")).Value;
                var scope = (principal.FindFirst(c => c.Type.ToLower() == "scope")).Value;
                JArray scopes = new JArray(scope);

                if (scope == null ) throw new Exception("404 - Authorization failed - Invalid Scope");
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }
            //if (AuthenticationHeaderValue.TryParse(authToken, out var headerValue))
            //{
            //    // we have a valid AuthenticationHeaderValue that has the following details:

            //    var scheme = headerValue.Scheme;
            //    var parameter = headerValue.Parameter;
            //    var handler = new JwtSecurityTokenHandler();
            //    var jwtSecurityToken = handler.ReadJwtToken(parameter);
            //    //byte[] data = Convert.FromBase64String(parameter);
            //    //string decodedString = Encoding.UTF8.GetString(data);

            //    // scheme will be "Bearer"
            //    // parmameter will be the token itself.

            //    var validationParameters = GetValidationParameters();

            //    SecurityToken validatedToken;
            //    try
            //    {
            //        var principal = handler.ValidateToken(parameter, validationParameters, out validatedToken);
            //        var scope = principal.FindFirst(c => c.Type.ToLower() == "scope" && (c.Value.ToLower() == "url:http://203.64.84.32:9876/fhir/Bundle/725"));
            //        if (scope == null) throw new Exception("404 - Authorization failed - Invalid Scope");
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        return false;

            //    }
            //}
            //return false;

        }
            public static string GenerateToken(string key, string a1, string a2)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var token = new JwtSecurityToken(
                claims: new Claim[]
                {
            new Claim(JwtRegisteredClaimNames.Iss, "http://203.64.84.33:33484/"),
            new Claim(JwtRegisteredClaimNames.Aud, "https://203.64.84.150:58443"),
            new Claim(JwtRegisteredClaimNames.Sub, a1),
            new Claim("a", a2),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                },
                //notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(DateTime.Now.AddMinutes(1)).DateTime,
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static TokenValidationParameters GetValidationParameters()
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                //ValidateIssuerSigningKey = true,
                //ValidIssuer = "http://203.64.84.33:33484/",
                //ValidAudience = "https://203.64.84.150:58443",
                IssuerSigningKey = new SymmetricSecurityKey(hmac.Key) // The same key as the one that generate the token
            };
        }
        /// <summary>
        /// Upload FHIR Observation for Annotation 
        /// </summary>
        /// <returns>Fhir ObservationnAnnotation</returns>
        [HttpPost]
        public IActionResult UploadAnnotationObservation([FromBody] AnnotationInformation annotationInfo)
        {
            JObject annoObsJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["annotationObservation_path"]));
            JObject obsComponentcodeJson = JObject.Parse(System.IO.File.ReadAllText(Configuration["observationComponent_code_path"]));
            JArray performer = new JArray(), component = new JArray(), content = new JArray(), organization = new JArray();
            HTTPrequest httpRequest = new HTTPrequest();

            annoObsJson["effectiveDateTime"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ssszzz");

            // Create single document reference of image
            string DocrefIDs = HttpContext.Request.Cookies["DocrefIDs"];
            JObject DocrefIDscookies = JObject.Parse((string)DocrefIDs);
            var respDocrefstudy = httpRequest.getResource(Configuration["Repository_gateway_fhir"], "Bearer 123", "DocumentReference/", (string)DocrefIDscookies["0"], Configuration["FHIRResponseType"]);
            JObject Docrefsingle = JObject.Parse((respDocrefstudy as OkObjectResult).Value.ToString());  
            content.Add(new JObject { 
                { "attachment", (new JObject {{"url", annotationInfo.imageURL } } ) } 
            });
            Docrefsingle["content"] = content;
            JObject callBackParams = new JObject();
            callBackParams["rowslength"] = annotationInfo.rowslength;
            callBackParams["cookieName"] = "DocrefIDs";
            var singleDocrefResp = httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Observation", annoObsJson, "Bearer 123", recordAnnotation, callBackParams);
            JObject respDocrefIDJson = JObject.Parse((singleDocrefResp as OkObjectResult).Value.ToString());
            annoObsJson["derivedFrom"][0]["reference"] = respDocrefIDJson["resourceType"]+"/"+ respDocrefIDJson["id"];


            // Add all personel information cookies
            string PersonnalIDs = HttpContext.Request.Cookies["PersonnalIDs"];
            JObject PersonnalIDscookies = JObject.Parse((string)PersonnalIDs);
            annoObsJson["subject"] = new JObject { { "reference", PersonnalIDscookies["patient"]["resourceType"] + "/" + PersonnalIDscookies["patient"]["id"] }, { "display", PersonnalIDscookies["patient"]["name"] } };
            performer.Add(new JObject { { "reference", PersonnalIDscookies["role"]["resourceType"] + "/" + PersonnalIDscookies["role"]["id"] } });
            annoObsJson["performer"] = performer;

            obsComponentcodeJson["code"]["coding"][0]["system"] = "https://203.64.84.150:58443/r5/fhir/CodeSystem/8f414151-bf5f-46ce-94fe-f96d9e867d29";
            obsComponentcodeJson["code"]["coding"][0]["code"] = "annotation.svg";
            obsComponentcodeJson["code"]["coding"][0]["display"] = "SVG Annotation";
            obsComponentcodeJson.Add("valueString", annotationInfo.svgInput );
            component.Add(obsComponentcodeJson);
            annoObsJson["component"] = component;

            callBackParams = new JObject();
            callBackParams["rowslength"] = annotationInfo.rowslength;
            callBackParams["cookieName"] = "AnnotationIDs";

            return httpRequest.postResource(Configuration["Repository_gateway_fhir"], "Observation", annoObsJson, "Bearer 123", recordAnnotation,callBackParams);
        }
        public IActionResult recordAnnotation(JObject callBackParams, string rowNum)
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
                annotationIdscookies= JObject.Parse((string)annotationIDs);
            }
            annotationIdscookies.Add(new JProperty((string)callBackParams["rowslength"], (string)resultJson["id"]));
            string annotationIDsjsonstr = annotationIdscookies.ToString(Newtonsoft.Json.Formatting.None);
            HttpContext.Response.Cookies.Append((string)callBackParams["cookieName"], annotationIDsjsonstr, new Microsoft.AspNetCore.Http.CookieOptions
            { Expires = DateTime.Now.AddHours(1), });
            return new OkObjectResult((string)callBackParams["result"]); 
        }
        public IActionResult getResourceinDocumentbundle(string documentbundle)
        {
            HTTPrequest httpRequest = new HTTPrequest();
            var respbundle = httpRequest.getResource(documentbundle, "Bearer 123", "", "", Configuration["FHIRResponseType"]);
            JObject respFindingJson = JObject.Parse((respbundle as OkObjectResult).Value.ToString());
            foreach (JObject x in respFindingJson["entry"])
            {
                JObject respresource = (JObject)x["resource"];
                if ((string)respresource["resourceType"] == "DocumentReference")
                {
                    return new OkObjectResult(respresource); ;
                }
            }
            dynamic errmsg = new JObject();
            errmsg.Message = "DocumentReference image is not exist!";
            return new BadRequestObjectResult(errmsg);
        }
    }
}
