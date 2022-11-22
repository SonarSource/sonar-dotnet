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
            string part1 = "Access-Control";
            string part2 = "Allow-Origin";
            // The Access-Control-Allow-Origin response header indicates whether the response can be shared with requesting code from the given origin.
            Response.Headers.Add("""Access-Control-Allow-Origin""", """*"""); // Noncompliant
            Response.Headers.Add($$"""{{part1}}-{{part2}}""", "*"); // FN

            const string part3 = "Access-Control";
            const string part4 = "Allow-Origin";
            Response.Headers.Add($$"""{{part3}}-{{part4}}""", "*"); // FN
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
