using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Baseline
{
    [ApiController]
    public class NoncompliantController : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "Hi";                // Noncompliant {{REST API controller actions should be annotated with the appropriate HTTP verb attribute.}}
        //            ^^^^^^

        public int FooPost([FromBody] string id) =>    // Noncompliant
            StatusCodes.Status200OK;
    }

    [ApiController]
    public class CompliantController : ControllerBase
    {
        [HttpGet("foo")]
        public string Get() => "Hi";

        [HttpPut("foo")]
        public int Put([FromBody] string id) =>
            StatusCodes.Status200OK;

        [HttpPost("foo")]
        public int Post([FromBody] string id) =>
            StatusCodes.Status200OK;

        [HttpDelete("foo")]
        public int Delete([FromBody] string id) =>
            StatusCodes.Status200OK;

        [HttpPatch("foo")]
        public int Patch([FromBody] string id) =>
            StatusCodes.Status200OK;

        [HttpHead("foo")]
        public int Head([FromBody] string id) =>
            StatusCodes.Status200OK;

        [Route("foo")]
        [HttpOptions]
        public int Options([FromBody] string id) =>
            StatusCodes.Status200OK;

        [AcceptVerbs("GET", "POST")]
        [Route("test")]
        public int AcceptVerbs([FromBody] string id) =>
            StatusCodes.Status200OK;

        [Route("Error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public int IgnoresApi([FromBody] string id) =>
            StatusCodes.Status200OK;
    }
}

namespace CustomHttpMethods
{
    [ApiController]
    public class CompliantController : ControllerBase
    {
        [Route("foo")]
        [MyHttpGet]
        public int MyGet(string id) =>
            StatusCodes.Status200OK;

        [Route("foo")]
        [MyHttpMethod]
        public int MyMethod() =>
            StatusCodes.Status200OK;
    }

    public class MyHttpGetAttribute : HttpGetAttribute { }

    public class MyHttpMethodAttribute : HttpMethodAttribute
    {
        public MyHttpMethodAttribute() : base(null) { }
    }
}

namespace ApiExplorer
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ExcludedFromOpenApiController : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "Hi";

        public int FooPost([FromBody] string id) =>
            StatusCodes.Status200OK;
    }

    [ApiController]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class MarkedWithApiExplorerButIgnoreApiSpecifiedAsFalse : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "hi";    // Noncompliant
    }

    [ApiController]
    [ApiExplorerSettings]
    public class MarkedWithApiExplorerButIgnoreApiUnspecified : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "hi";    // Noncompliant
    }

    [ApiController]
    [ApiExplorerSettings(GroupName = "group")]
    public class MarkedWithApiExplorerButIgnoreApiUnspecified2 : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "hi";    // Noncompliant
    }

    namespace CustomApiExplorer
    {
        class MyApiExplorerSettingsAttribute : ApiExplorerSettingsAttribute { }

        [ApiController]
        [MyApiExplorerSettings(IgnoreApi = true)]
        public class MarkedWithMyApiExplorer : ControllerBase
        {
            [Route("foo")]
            public string FooGet() => "hi";
        }

        [ApiController]
        [MyApiExplorerSettings(IgnoreApi = false)]
        public class MarkedWithMyApiExplorerButIgnoreApiSpecifiedAsFalse : ControllerBase
        {
            [Route("foo")]
            public string FooGet() => "hi"; // Noncompliant
        }

        [ApiController]
        [MyApiExplorerSettings()]
        public class MarkedWithMyApiExplorerButIgnoreApiUnspecified : ControllerBase
        {
            [Route("foo")]
            public string FooGet() => "hi"; // Noncompliant
        }
    }
}

namespace Visibility
{
    [ApiController]
    internal class NotPublicClass : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "Hi";
    }

    [ApiController]
    public class NotPublicMethod : ControllerBase
    {
        [Route("foo")]
        protected string FooGet() => "Hi";
    }
}

namespace ControllerAnnotations
{
    [ApiController]
    [Controller]
    public class AnnotatedAsController
    {
        [Route("foo")]
        public string FooGet() => "hi";    // Noncompliant
    }

    [NonController]
    [ApiController]
    public class NotAController : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "Hi";
    }

    [ApiController]
    [Controller]
    [NonController]
    public class AnnotatedAsControllerAndNonController
    {
        [Route("foo")]
        public string FooGet() => "hi";
    }

    public class NotApiController : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "hi";
    }

    [Controller]
    public class NotApiController2
    {
        [Route("foo")]
        public string FooGet() => "hi";
    }

    [MyApiController]
    public class AnnotatedWithCustomApiController : ControllerBase
    {
        [Route("foo")]
        public string FooGet() => "hi";   // Noncompliant
    }

    class MyApiControllerAttribute : ApiControllerAttribute { }
}

namespace Inheritance
{
    [ApiController]
    public class NotDerivingController
    {
        [Route("foo")]
        public string FooGet() => "hi";
    }

    [ApiController]
    public class DerivingController : Controller
    {
        [Route("foo")]
        public string Foo() => "hi"; // Noncompliant

        public string Get() => "hi"; // Noncompliant
    }

    [ApiController]
    public class DerivingMyController : MyController
    {
        [Route("foo")]
        public string Foo() => "hi"; // Noncompliant

        public string Get() => "hi"; // Noncompliant
    }

    public class MyController : Controller { }

    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        [Route("foo")]
        public virtual string BaseHasNoMethod() => "hi"; // Noncompliant

        [HttpGet("foo")]
        public virtual string BaseHasMethod() => "hi";

        [AcceptVerbs("GET")]
        public virtual string BaseHasAcceptVerbs() => "hi";

        [ApiExplorerSettings()]
        public virtual string BaseHasApiExplorerSettingsWithoutIgnoring() => "hi"; // Noncompliant

        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual string BaseHasApiExplorerSettingsIgnoring() => "hi";

        public abstract string BaseAbstractMethod();
    }

    public class ImplementationNoBehaviorChangeController : BaseController
    {
        public override string BaseHasNoMethod() => "hi"; // Noncompliant

        public override string BaseHasMethod() => "hi";

        public override string BaseHasAcceptVerbs() => "hi";

        public override string BaseHasApiExplorerSettingsWithoutIgnoring() => "hi"; // Noncompliant

        public override string BaseHasApiExplorerSettingsIgnoring() => "hi";

        public override string BaseAbstractMethod() => "hi"; // Noncompliant
    }

    public class ImplementationBehaviorChangedController : BaseController
    {
        [HttpGet]
        public override string BaseHasNoMethod() => "hi";

        public override string BaseHasAcceptVerbs() => "hi";

        [ApiExplorerSettings(IgnoreApi = true)]
        public override string BaseHasApiExplorerSettingsWithoutIgnoring() => "hi";

        [ApiExplorerSettings(IgnoreApi = false)]
        public override string BaseHasApiExplorerSettingsIgnoring() => "hi"; // Noncompliant

        [HttpGet]
        public override string BaseAbstractMethod() => "hi";
    }
}
