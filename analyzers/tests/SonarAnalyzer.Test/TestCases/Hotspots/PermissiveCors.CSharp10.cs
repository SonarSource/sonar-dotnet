using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace Net6Poc.PermissiveCors
{
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
}
