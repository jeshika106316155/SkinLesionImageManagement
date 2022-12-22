using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
//using System.Web.Mvc;

namespace SLI_UploadPicture.Models
{
    public class HTTPrequest
    {
        public object getResource(string fhirUrl, string ResourceName, string Parameter, string FHIRResponseType) //, Action <object> CallbackFunc
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl + ResourceName + Parameter);
                requestHttp.ContentType = "application/json";
                requestHttp.Method = "GET";
                var response = (HttpWebResponse)requestHttp.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        JObject resultJson = JObject.Parse(result);

                        //var id = resultJson["id"];
                        //if (CallbackFunc != null)
                        //{
                        //    CallbackFunc(resultJson);
                        //}
                        return resultJson;
                    }
                }
                else
                {
                    //reqMessage = "Error upload to FHIR Server!"; 
                    return JObject.Parse("{'total':0;'message':'Error upload to FHIR Server!'}");
                }
            }
            catch (Exception e)
            {
                return JObject.Parse("{'total':0;'message':'Error request to FHIR Server!'}");
            }
            return JObject.Parse("{'total':0;'message':'Error request to FHIR Server!'}");
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult postResource(string fhirUrl, string ResourceName, JObject body, string token, Func<JObject,string,object> CallbackFunction)
        {
            dynamic errmsg = new JObject();

            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var requestHttp = (HttpWebRequest)WebRequest.Create(fhirUrl + ResourceName);
                requestHttp.ContentType = "application/json";
                requestHttp.Method = "POST";
                requestHttp.Headers["Authorization"] = token;
                string postBody = body.ToString();
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postBody);
                using (Stream reqStream = requestHttp.GetRequestStream())
                {
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }
                string er="";
                HttpWebResponse response ;
                try {   response = (HttpWebResponse)requestHttp.GetResponse();
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();
                            JObject resultJson = JObject.Parse(result);
                            return new OkObjectResult(result);
                            //CallbackFunction(resultJson, token);
                            //return callbackResult;
                        }
                    }
                }
                catch (Exception e) { er = e.Message; return new BadRequestObjectResult(errmsg); }
            }
            catch (Exception e)
            {
                errmsg.Message = e.Message;
                return new BadRequestObjectResult(errmsg);
            }
            errmsg.Message = "error";
            return new BadRequestObjectResult(errmsg);
        }
    }
}
