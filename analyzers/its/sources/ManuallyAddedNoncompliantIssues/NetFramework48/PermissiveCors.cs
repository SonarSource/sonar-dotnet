using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Net.Http.Headers;

namespace IntentionalFindings
{
    public class PermissiveCors : ApiController
    {
        [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-Custom-Header")] // Noncompliant (S5122) {{Make sure this permissive CORS policy is safe here.}}
        public string Get()
        {
            var response = HttpContext.Current.Response;

            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Noncompliant
            response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            response.AppendHeader(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant

            return string.Empty;
        }
    }
}
