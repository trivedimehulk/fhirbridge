using Hl7.Fhir.Model;
using Microsoft.Practices.Unity;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Service;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Hl7.Fhir.Rest;
using Spark.Core;
using Spark.Infrastructure;
using System.Net.Http;
using System.Dynamic;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Net.Http.Headers;

namespace Spark.Controllers
{
    [RoutePrefix("fhir"), EnableCors("*", "*", "*", "*")]
    [RouteDataValuesOnly]
    public class FhirController : ApiController
    {

        readonly IFhirService _fhirService;

        [InjectionConstructor]
        public FhirController(IFhirService fhirService)
        {
            // This will be a (injected) constructor parameter in ASP.vNext.
            _fhirService = fhirService;
        }

        [InjectionConstructor]
        public FhirController()
        {
            //todo
        }

        [HttpGet, Route("{type}/{id}")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> Read(string type, string id)
        {
            //mt - defaulting it to xml
            string ContentType = "application/xml";
            var responseString = string.Empty;
            HttpStatusCode _respCode = HttpStatusCode.OK;

            try
            {
                ConditionalHeaderParameters parameters = new ConditionalHeaderParameters(Request);
                Key key = Key.Create(type, id);
                var headers = Request.Headers;
                string fhir_host = String.Empty;
                
                

                //fetch the fhir host from header - this will decide where to route the call
                if (headers.Contains("fhir_host") && headers.GetValues("fhir_host").First().Length > 0)
                {
                    fhir_host = headers.GetValues("fhir_host").First();
                    // clean up / from end
                    if (fhir_host.EndsWith("/") == true)
                    {
                        fhir_host = fhir_host.Substring(0, fhir_host.Length - 1);
                    }

                    //fetch the accept header type from request - in which format client expects response
                    if (headers.Contains("Content_Type"))//default is xml
                    {
                        ContentType = headers.GetValues("Content_Type").First();
                    }


                    HttpClient client = new HttpClient();

                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue(ContentType));//ACCEPT header


                    //if fhir host is https, then enable ssl
                    if (fhir_host.Contains("https") == true)
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    try
                    {
                        responseString = await client.GetStringAsync(fhir_host + "/" + type + "/" + id);
                    }
                    catch (Exception ex)
                    {
                        //wrap and send the exception to client
                        _respCode = HttpStatusCode.InternalServerError;
                        ContentType = "text/plain";
                        responseString = "Error while calling FHIR endpoint -> Original error " + ex.Message + "|Detailed Stack" + ex.StackTrace;
                    }
                }
                else
                {
                    _respCode = HttpStatusCode.NotAcceptable;
                    //the host is blank - fail the request
                    ContentType = "text/plain";
                    responseString = "No FHIR host url specified. Please supply a FHIR host";
                }
               
            }
            catch (Exception ex)
            {
                //wrap and send the exception to client
                _respCode = HttpStatusCode.InternalServerError;
                ContentType = "text/plain";
                responseString = "System error -> " + ex.Message + "|Detailed Stack" + ex.StackTrace;
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(responseString, Encoding.UTF8, ContentType)
                , StatusCode = _respCode
            };

        }

        [HttpGet, Route("{type}/{id}/_history/{vid}")]
        public FhirResponse VRead(string type, string id, string vid)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPut, Route("{type}/{id?}")]
        public FhirResponse Update(string type, Resource resource, string id = null)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, Route("{type}")]
        public FhirResponse Create(string type, Resource resource)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpDelete, Route("{type}/{id}")]
        public FhirResponse Delete(string type, string id)

        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpDelete, Route("{type}")]
        public FhirResponse ConditionalDelete(string type)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpGet, Route("{type}/{id}/_history")]
        public FhirResponse History(string type, string id)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        // ============= Validate
        [HttpPost, Route("{type}/{id}/$validate")]
        public FhirResponse Validate(string type, string id, Resource resource)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, Route("{type}/$validate")]
        public FhirResponse Validate(string type, Resource resource)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        //// ============= Type Level Interactions

        [HttpGet, Route("{type}")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> Search(string type)
        {
            string ContentType = "application/xml";
            var responseString = string.Empty;
            HttpStatusCode _respCode = HttpStatusCode.OK;

            try
            {
                int start = Request.GetIntParameter(FhirParameter.SNAPSHOT_INDEX) ?? 0;
                var searchparams = Request.GetSearchParams();
                string _searchParams = Request.RequestUri.Query;
                var headers = Request.Headers;
                string fhir_host = String.Empty;
                

                //fetch the fhir host from header - this will decide where to route the call
                if (headers.Contains("fhir_host") && headers.GetValues("fhir_host").First().Length > 0)
                {
                    fhir_host = headers.GetValues("fhir_host").First();


                    // clean up / from end
                    if (fhir_host.EndsWith("/") == true)
                    {
                        fhir_host = fhir_host.Substring(0, fhir_host.Length - 1);
                    }

                    //fetch the accept header type from request - in which format client expects response
                    if (headers.Contains("Content_Type"))//default is xml
                    {
                        ContentType = headers.GetValues("Content_Type").First();
                    }


                    HttpClient client = new HttpClient();

                    client.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue(ContentType));//ACCEPT header


                    //if fhir host is https, then enable ssl
                    if (fhir_host.Contains("https") == true)
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    }

                    try
                    {
                        responseString = await client.GetStringAsync(fhir_host + "/" + type + _searchParams);
                    }
                    catch (Exception ex)
                    {
                        _respCode = HttpStatusCode.InternalServerError;
                        //wrap and send the exception to client
                        ContentType = "text/plain";
                        responseString = "Error while calling FHIR endpoint -> Original error " + ex.Message + "|Detailed Stack" + ex.StackTrace;
                    }
                }
                else
                {
                    _respCode = HttpStatusCode.NotAcceptable;
                    //the host is blank - fail the request
                    ContentType = "text/plain";
                    responseString = "No FHIR host url specified. Please supply a FHIR host";
                }
            }
            catch (Exception ex)
            {
                _respCode = HttpStatusCode.InternalServerError;
                //wrap and send the exception to client
                ContentType = "text/plain";
                responseString = "System error -> " + ex.Message + "|Detailed Stack" + ex.StackTrace;
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(responseString, Encoding.UTF8, ContentType)
                , StatusCode = _respCode
            };
        }

        [HttpPost, HttpGet, Route("{type}/_search")]
        public FhirResponse SearchWithOperator(string type)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpGet, Route("{type}/_history")]
        public FhirResponse History(string type)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        //// ============= Whole System Interactions

        [HttpGet, Route("metadata")]
        public FhirResponse Metadata()
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpOptions, Route("")]
        public FhirResponse Options()
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, Route("")]
        public FhirResponse Transaction(Bundle bundle)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, Route("Mailbox")]
        public FhirResponse Mailbox(Bundle document)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpGet, Route("_history")]
        public FhirResponse History()
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpGet, Route("_snapshot")]
        public FhirResponse Snapshot()
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        //// Operations

        [HttpPost, Route("${operation}")]
        public FhirResponse ServerOperation(string operation)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, Route("{type}/{id}/${operation}")]
        public FhirResponse InstanceOperation(string type, string id, string operation, Parameters parameters)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, HttpGet, Route("{type}/{id}/$everything")]
        public FhirResponse Everything(string type, string id = null)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, HttpGet, Route("{type}/$everything")]
        public FhirResponse Everything(string type)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

        [HttpPost, HttpGet, Route("Composition/{id}/$document")]
        public FhirResponse Document(string id)
        {
            return new FhirResponse(HttpStatusCode.NotImplemented) { };
        }

    }

}
