namespace TestCases_Controller
{
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using Microsoft.Net.Http.Headers;

    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-Custom-Header")] // Noncompliant (S5122) {{Make sure this permissive CORS policy is safe here.}}
    public class PermissiveCors : ApiController
    {
        [EnableCors("https:\\trustedwebsite.com", "*", "*", "X-Custom-Header")]
        public string Get()
        {
            var response = HttpContext.Current.Response;

            response.Headers.Add("Access-Control-Allow-Origin", "*"); // Noncompliant
            response.Headers.Add("Access-Control-Allow-Origin", "https:\\trustedwebsite.com");
            response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant

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
            context.RequestContext.HttpContext.Response.AddHeader(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            context.RequestContext.HttpContext.Response.AddHeader(HeaderNames.AccessControlAllowOrigin, "https:\\trustedwebsite.com");

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
            context.Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            context.Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "https:\\trustedwebsite.com");

            base.OnActionExecuted(context);
        }
    }
}
