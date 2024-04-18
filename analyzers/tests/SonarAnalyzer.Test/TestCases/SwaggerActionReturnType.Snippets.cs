[DataTestMethod]
[DataRow("""Ok(bar)""")]
[DataRow("""Created("uri", bar)""")]
[DataRow("""Created(new Uri("uri"), bar)""")]

[DataRow("""CreatedAtAction("actionName", bar)""")]
[DataRow("""CreatedAtAction("actionName", null, bar)""")]
[DataRow("""CreatedAtAction("actionName", "controllerName", null, bar)""")]

[DataRow("""CreatedAtRoute("routeName", bar)""")]
[DataRow("""CreatedAtRoute("default(object)", bar)""")]
[DataRow("""CreatedAtRoute("routeName", null, bar)""")]

[DataRow("""Accepted(bar)""")]
[DataRow("""Accepted("uri", bar)""")]
[DataRow("""Accepted(new Uri("uri"), bar)""")]

[DataRow("""AcceptedAtAction("actionName", bar)""")]
[DataRow("""AcceptedAtAction("actionName", "controllerName", bar)""")]
[DataRow("""AcceptedAtAction("actionName", "controllerName", null, bar)""")]

[DataRow("""AccteptedAtRoute("routeName", null, foo)""")]
[DataRow("""AccteptedAtRoute(default(object), foo)""")]
public void RuleName_IActionResult(string invocation)
{
    builder
        .AddSnippet($$"""
        using System;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Builder;
        using Swashbuckle.AspNetCore.Annotations;
        using Microsoft.AspNetCore.Http.HttpResults;

        class Program
        {
            static void Main(string[] args)
            {
                WebApplication app = null;
                app.UseSwagger(); // Necessary for the rule to raise
            }
        }

        [ApiController]
        public class Foo : Controller
        {
            [HttpGet("a")]
            public IActionResult Method() =>    // Noncompliant
                {{invocation}};                 // Secondary

            private Bar bar = new();
        }

        public class Bar {}
        """)
        .Verify();

}

[DataTestMethod]
[DataRow("""Results.Ok(bar)""")]
[DataRow("""Results.Ok((object) bar)""")]
[DataRow("""Results.Ok(bar)""")]

[DataRow("""Results.Created("uri", bar)""")]
[DataRow("""Results.Created("uri", (object) bar)""")]
[DataRow("""Results.Created(new Uri("uri"), bar)""")]
[DataRow("""Results.Created(new Uri("uri"), (object) bar)""")]

[DataRow("""Results.CreatedAtRoute(value: (object) bar)""")]
[DataRow("""Results.CreatedAtRoute("", null, (object) bar)""")]
[DataRow("""Results.CreatedAtRoute(value: bar)""")]
[DataRow("""Results.CreatedAtRoute("", null, bar)""")]

[DataRow("""Results.Accepted("uri", bar)""")]
[DataRow("""Results.Accepted("uri", (object) bar)""")]

[DataRow("""Results.AcceptedAtRoute(value: (object) bar)""")]
[DataRow("""Results.AcceptedAtRoute("", null, (object) bar)""")]
[DataRow("""Results.AcceptedAtRoute(value: bar)""")]
[DataRow("""Results.AcceptedAtRoute("", null, bar)""")]
public void RuleName_IResult(string invocation)
{
    builder
        .AddSnippet($$"""
        using System;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Builder;
        using Swashbuckle.AspNetCore.Annotations;
        using Microsoft.AspNetCore.Http.HttpResults;

        class Program
        {
            static void Main(string[] args)
            {
                WebApplication app = null;
                app.UseSwagger(); // Necessary for the rule to raise
            }
        }

        [ApiController]
        public class Foo : Controller
        {
            [HttpGet("a")]
            public IResult Method() =>    // Noncompliant
                {{invocation}};           // Secondary

            private Bar bar = new();
        }

        public class Bar {}
        """)
        .Verify();
}

[TestMethod]
public void ApiConventionType_AssemblyLevel()
{
    builder
        .AddSnippet("""
        using System;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Mvc;
        using Swashbuckle.AspNetCore.Annotations;
        using Microsoft.AspNetCore.Http.HttpResults;

        [assembly: ApiConventionType(typeof(DefaultApiConventions))]
        namespace MyNameSpace;

        class Program
        {
            static void Main(string[] args)
            {
                WebApplication app = null;
                app.UseSwagger(); // Necessary for the rule to raise
            }
        }

        [ApiController]
        public class Foo : Controller
        {
            [HttpGet("a")]
            public IActionResult Method() => Ok(bar); // Compliant
            private Bar bar = new();
        }

        public class Bar {}
        """)
        .Verify();
}