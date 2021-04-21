namespace TestCases_Controller
{
    using System;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using Microsoft.Net.Http.Headers;

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
            response.Headers.Add("Access-Control-Allow-Origin", new String('*', 1)); // FN

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

namespace TestCases_MvcFilter
{
    using System.Web.Mvc;
    using Microsoft.Net.Http.Headers;

    public class AllowCrossSiteAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*"); // Noncompliant
            context.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "https:\\trustedwebsite.com");
            context.RequestContext.HttpContext.Response.AddHeader("something else", "*");

            context.RequestContext.HttpContext.Response.AddHeader(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            context.RequestContext.HttpContext.Response.AddHeader(HeaderNames.AccessControlAllowOrigin, "https:\\trustedwebsite.com");
            context.RequestContext.HttpContext.Response.AddHeader(HeaderNames.ContentLength, "*");

            base.OnActionExecuting(context);
        }
    }
}

namespace TestCases_WebApiFilter
{
    using System.Web.Http.Filters;
    using Microsoft.Net.Http.Headers;

    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*"); // Noncompliant
            context.Response.Headers.Add("Access-Control-Allow-Origin", "https:\\trustedwebsite.com");
            context.Response.Headers.Add("something else", "*");

            context.Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            context.Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "https:\\trustedwebsite.com");
            context.Response.Headers.Add(HeaderNames.ContentLength, "*");

            base.OnActionExecuted(context);
        }
    }
}
