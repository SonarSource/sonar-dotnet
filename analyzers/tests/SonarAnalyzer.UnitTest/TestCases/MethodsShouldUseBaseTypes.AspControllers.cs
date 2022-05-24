using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1
{
    [ApiController]
    [Route("api/controller")]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public string Get([FromQuery] Query query) // Noncompliant
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
