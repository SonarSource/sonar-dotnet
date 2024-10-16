using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using CPB = Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder;
using SV = Microsoft.Extensions.Primitives.StringValues;


internal class TestCases
{
    public void Bar(IEnumerable<int> collection)
    {
        [EnableCors()] int Get() => 1; // Compliant - we don't know what default policy is

        _ = collection.Select([EnableCors("policyName")] (x) => x + 1);

        Action a = [EnableCors("*")] () => { }; // Compliant - `*`, in this case, is the name of the policy

        Action x = true
                       ? ([EnableCors] () => { })
                       : [EnableCors("*")] () => { };

        Call([EnableCors] (x) => { });
    }

    private void Call(Action<int> action) => action(1);
    
    [ApiController]
    [Route("[controller]")]
    public class ConstantInterpolatedStringController : Controller
    {
        [HttpGet]
        public void Index()
        {
            const string constAccessControl = "Access-Control";
            const string constAllowOrigin = "Allow-Origin";
            Response.Headers.Add($"{constAccessControl}-{constAllowOrigin}", "*"); // Noncompliant

            const string constString = "Access-Control-Allow-Origin";
            Response.Headers.Add(constString, "*"); // FN

            const string interpolatedString = $"{constAccessControl}-{constAllowOrigin}";
            Response.Headers.Add(interpolatedString, "*"); // FN
        }
    }
}

[ApiController]
[Route("[controller]")]
public class CorsEnabledManualAddedHeadersController : Controller
{
    [HttpGet]
    public void Index(string header, string headerValue)
    {
        string accessControl = "Access-Control";
        string allowOrigin = "Allow-Origin";
        // The Access-Control-Allow-Origin response header indicates whether the response can be shared with requesting code from the given origin.
        Response.Headers.Add("""Access-Control-Allow-Origin""", """*"""); // Noncompliant
        Response.Headers.Add($$"""{{accessControl}}-{{allowOrigin}}""", "*"); // FN (at the moment we validate only constant string)

        const string constAccessControl = "Access-Control";
        const string constAllowOrigin = "Allow-Origin";
        Response.Headers.Add($$"""{{constAccessControl}}-{{constAllowOrigin}}""", "*"); // Noncompliant

        const string RawStar = """*""";
        Response.Headers.Add("Access-Control-Allow-Origin", RawStar); // Noncompliant
        Response.Headers.Add($"""{constAccessControl}-{constAllowOrigin}""", RawStar); // Noncompliant

        const string constRawAccessControl = """Access-Control""";
        const string constRawAllowOrigin = """Allow-Origin""";
        Response.Headers.Add($"""{constRawAccessControl}-{constRawAllowOrigin}""", """*"""); // Noncompliant
    }
}

internal class TestCases2
{
    [GenericAttribute<int>("*")] // Compliant - "*" is the policy name in this case
    public void A() { }

    [GenericAttribute<int>]
    public void B() { }

    [EnableCors()]
    public void C() { }

    [EnableCors("*")]
    public void D() { }
}

public class GenericAttribute<T> : EnableCorsAttribute
{
    public GenericAttribute() : base() { }

    public GenericAttribute(string policyName) : base(policyName) { }
}

partial class Partial
{
    private const string Star = "*";

    public partial IServiceCollection A { get; }
}

partial class Partial
{
    public partial IServiceCollection A => new ServiceCollection().AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins("*"); // Noncompliant
            builder.WithOrigins(Star); // Noncompliant
            builder.WithOrigins("*", "*", "*"); // Noncompliant
        });
    });
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

            var unsafeBuilderWithAlias = new CPB("*"); // FN - for performance reasons type alias is not supported
            options.AddPolicy("UnsafeBuilderWithAlias", unsafeBuilderWithAlias.Build());

            var safeBuilder = new CorsPolicyBuilder("https://trustedwebsite.com");
            options.AddPolicy("SafeBuilder", safeBuilder.Build());

            CorsPolicyBuilder builder = new("*");     // Noncompliant

            builder = new CorsPolicyBuilder { };
        });

        services.AddControllers();
    }
}

namespace NetTests
{
    [ApiController]
    [Route("[controller]")]
    public class CorsEnabledManualAddedHeadersController : Controller
    {
        private const string Star = "*";
        private string NonConstStar = "*";

        [HttpGet]
        public void Index(string header, string headerValue)
        {
            // The Access-Control-Allow-Origin response header indicates whether the response can be shared with requesting code from the given origin.
            Response.Headers.Add("Access-Control-Allow-Origin", "*"); // Noncompliant
            Response.Headers.Add("Access-Control-Allow-Origin", Star); // Noncompliant
            Response.Headers.Add("Access-Control-Allow-Origin", "https://trustedwebsite.com");
            Response.Headers.Add("Access-Control-Allow-Origin", new[] { "https://trustedwebsite.com", "*" }); // Noncompliant
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
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, new StringValues(new[] { "*", "https://trustedwebsite.com" })); // Noncompliant

            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, new SV("*")); // FN - for performance reasons type alias is not supported
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, "*, https://trustedwebsite.com"); // FN
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, $"{Star}, https://trustedwebsite.com"); // FN

            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, $"{Star}"); // Noncompliant
            Response.Headers.Append(HeaderNames.AccessControlAllowOrigin, $"{NonConstStar}"); // FN (at the moment we validate only constant string)
        }
    }
}
