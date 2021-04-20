using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Net.Http.Headers;

namespace TestCases
{
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-Custom-Header")] // Noncompliant {{Make sure this permissive CORS policy is safe here.}}
    public class PermissiveCors : ApiController
    {
        [EnableCors("https:\\trustedwebsite.com", "*", "*", "X-Custom-Header")]
        public string Get()
        {
            var response = HttpContext.Current.Response;

            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Noncompliant
            response.Headers.Add("Access-Control-Allow-Origin", "https:\\trustedwebsite.com");
            response.Headers.Add("something else", "*");
            response.Headers.Add("Access-Control-Allow-Origin", new String('*', 1)); // FP

            response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "https:\\trustedwebsite.com");
            response.Headers.Add(HeaderNames.ContentLength, "*");

            response.AppendHeader("Access-Control-Allow-Origin", "*"); // Noncompliant
            response.AppendHeader("Access-Control-Allow-Origin", "https:\\trustedwebsite.com");
            response.AppendHeader("something else", "*");

            response.AppendHeader(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            response.AppendHeader(HeaderNames.AccessControlAllowOrigin, "https:\\trustedwebsite.com");
            response.AppendHeader(HeaderNames.ContentLength, "*");

            return string.Empty;
        }
    }
}
