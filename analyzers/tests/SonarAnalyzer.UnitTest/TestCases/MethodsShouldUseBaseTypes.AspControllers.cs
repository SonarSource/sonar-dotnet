using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1
{
    [ApiController]
    [Route("api/controller")]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public string Get([FromQuery] Query query) // Compliant, it's a public method in a controller, see: https://github.com/SonarSource/sonar-dotnet/issues/5264
        {
            query.Foo();
            return "";
        }

        private void AMethod(Query query) // Noncompliant
        {
            query.Foo();
        }
    }

    public class QueryBase
    {
        public void Foo() { }
    }

    public class Query : QueryBase { }
}
