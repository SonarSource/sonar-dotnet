using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.PermissiveCors
{
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

    internal class TestCases
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
}
