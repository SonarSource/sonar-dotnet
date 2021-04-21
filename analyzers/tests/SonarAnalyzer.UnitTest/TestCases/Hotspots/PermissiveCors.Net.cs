using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Tests.Diagnostics
{
    [ApiController]
    [Route("[controller]")]
    public class CorsEnabledManualAddedHeadersController : Controller
    {
        private const string Star = "*";

        [HttpGet]
        public void Index(string header, string headerValue)
        {
            // The Access-Control-Allow-Origin response header indicates whether the response can be shared with requesting code from the given origin.
            Response.Headers.Add("Access-Control-Allow-Origin", "*"); // Noncompliant
            Response.Headers.Add("Access-Control-Allow-Origin", Star); // Noncompliant
            Response.Headers.Add("Access-Control-Allow-Origin", "https://trustedwebsite.com");
            Response.Headers.Add("Access-Control-Allow-Origin", new [] {"https://trustedwebsite.com", "*"}); // Noncompliant
            Response.Headers.Add("Access-Control-Allow-Origin", "*****");
            Response.Headers.Add("OtherString", "*");
            Response.Headers.Add(HeaderNames.Age, "*");
            Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, "https://trustedwebsite.com");
            Response.Headers.Add(header, "*");
            Response.Headers.Add(HeaderNames.AccessControlAllowOrigin, headerValue);

            Response.Headers.Append("Access-Control-Allow-Origin", "*"); // Noncompliant
            Response.Headers.Append("Access-Control-Allow-Origin", "https://trustedwebsite.com");

            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, "*"); // Noncompliant
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, Star); // Noncompliant
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, "https://trustedwebsite.com");
            Response.Headers.Append("OtherString", "*");
            Response.Headers.Append(HeaderNames.Age, "*");

            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, new StringValues("*")); // Noncompliant
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, new StringValues(Star)); // Noncompliant
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, new StringValues("https://trustedwebsite.com"));
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, new StringValues(new [] {"*", "https://trustedwebsite.com"})); // Noncompliant

            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, "*, https://trustedwebsite.com"); // FN
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, $"{Star}, https://trustedwebsite.com"); // FN
        }
    }

    public class Setup
    {
        private const string Star = "*";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("*"); // Noncompliant
                    builder.WithOrigins(Star); // Noncompliant
                    builder.WithOrigins("*", "*", "*"); // Noncompliant
                });

                options.AddPolicy("EnableAllPolicy", builder =>
                {
                    builder.WithOrigins("https://trustedwebsite.com", "*"); // Noncompliant
                });

                options.AddPolicy("OtherPolicy", builder =>
                {
                    builder.AllowAnyOrigin(); // Noncompliant
                });

                options.AddPolicy("Safe", builder =>
                {
                    builder.WithOrigins("https://trustedwebsite.com", "https://anothertrustedwebsite.com");
                });

                options.AddPolicy("MyAllowSubdomainPolicy", builder =>
                {
                    builder.WithOrigins("https://*.example.com")
                           .SetIsOriginAllowedToAllowWildcardSubdomains();
                });

                var unsafeBuilder = new CorsPolicyBuilder("*"); // Noncompliant
                options.AddPolicy("UnsafeBuilder", unsafeBuilder.Build());

                var safeBuilder = new CorsPolicyBuilder("https://trustedwebsite.com");
                options.AddPolicy("SafeBuilder", safeBuilder.Build());

                CorsPolicyBuilder builder = new ("*"); // FN

                builder = new CorsPolicyBuilder { };
            });

            services.AddControllers();
        }
    }
}
