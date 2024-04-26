using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
public class CompliantBaseline : Controller
{
    private Foo foo = new();

    [HttpGet("foo")]
    public IActionResult ReturnsNoValue() => Ok();

    [HttpGet("foo")]
    public IActionResult NotSuccessfulResult() => BadRequest(foo);

    [HttpGet("foo")]
    [Produces(typeof(Foo))]
    public IActionResult HasProducesTypeOf() => Ok(foo);

    [HttpGet("foo")]
    [Produces<Foo>()]
    public IActionResult HasProducesGeneric() => Ok(foo);

    [HttpGet("foo")]
    [ProducesResponseType(typeof(Foo), StatusCodes.Status200OK)]
    public IActionResult HasProducesResponseTypeTypeOf() => Ok(foo);

    [HttpGet("foo")]
    [ProducesResponseType(typeof(Foo), StatusCodes.Status200OK, "application/json")]
    public IActionResult HasProducesResponseTypeTypeOf2() => Ok(foo);

    [HttpGet("foo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public IActionResult HasProducesResponseTypeAsParameter() => Ok(foo);

    [HttpGet("foo")]
    [ProducesResponseType<Foo>(StatusCodes.Status200OK)]
    public IActionResult HasProducesResponseTypeGeneric() => Ok(foo);

    [HttpGet("foo")]
    [ProducesResponseType<Foo>(StatusCodes.Status200OK, "application/json")]
    public IActionResult HasProducesResponseTypeGeneric2() => Ok(foo);

    [HttpGet("foo")]
    [SwaggerResponse(StatusCodes.Status200OK, "", typeof(Foo), "application/json")]
    public IActionResult HasSwaggerResponse() => Ok(foo);

    [HttpGet("foo")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Foo))]
    public IActionResult HasSwaggerResponse2() => Ok(foo);

    [HttpGet("foo")]
    [SwaggerResponse(StatusCodes.Status200OK, type: typeof(Foo))]
    public IResult IResult_HasAnnotation() => Results.Ok(foo);

    [HttpGet("foo")]
    public ActionResult<Foo> TypedResponse1() => Ok(foo);

    [HttpGet("foo")]
    public Foo TypedResponse2() => foo;

    [HttpGet("foo")]
    public Results<NotFound, Ok<Foo>> TypedResponse3() =>
        foo == null
            ? TypedResults.NotFound()
            : TypedResults.Ok(foo);

    [HttpGet("foo")]
    public async Task<Foo> TypedResponse4()
    {
        await Task.Delay(42);
        return foo;
    }

    // For implementation: I think if the type is specified at least once, we should not raise for simplicity, even if there is an http code mismatch.
    [Route("foo")]
    [ProducesResponseType<Foo>(StatusCodes.Status201Created)]
    public IActionResult AnnotatedForWrongStatusCode()
    {
        return Ok(foo); // This raises API1000, so the user will find out that the status code in the attribute is wrong.
    }

    [HttpGet("foo")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult IgnoreApiTrue() => Ok(42);
}

[ApiController]
public class NocompliantBaseline : ControllerBase
{
    private Foo foo = new();

    // For the implementation: If this seems too cumbersome, consider dropping it and documenting it as FN
    [HttpGet("foo")]
    public ObjectResult NewObjectResult() => // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
        //              ^^^^^^^^^^^^^^^
        new ObjectResult(42); // Secondary
    //  ^^^^^^^^^^^^^^^^^^^^

    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
    //         ^^^^^^^
    }

    [HttpGet("foo")]
    public IActionResult ReturnsMultipleValues(bool condition) // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^^^^^^^
    {
        if (condition)
        {
            return Ok(foo);         // Secondary
    //             ^^^^^^^
        }
        else
        {
            return Accepted(foo);   // Secondary
    //             ^^^^^^^^^^^^^
        }
    }

    [HttpGet("foo")]
    public IActionResult ReturnsMultipleValuesTernary() // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        return true
            ? Ok(foo)           // Secondary
        //    ^^^^^^^
            : Accepted(foo);    // Secondary
        //    ^^^^^^^^^^^^^
    }

    [HttpGet("foo")]
    public IActionResult ReturnsMultipleValuesSwitch(int id) // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        return id switch
        {
            1 => Ok(foo), // Secondary
            //   ^^^^^^^
            2 => BadRequest(),
            3 => Accepted(foo), // Secondary
            //   ^^^^^^^^^^^^^
            4 => NotFound(),
            5 => Created("", foo), // Secondary
            //   ^^^^^^^^^^^^^^^^
        };
    }

    [Route("foo")]
    public IActionResult MarkedWithRoute() // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
    //         ^^^^^^^
    }

    [HttpGet("foo")]
    [Produces("text/plain")]
    public IActionResult HasProducesTypeOf() => // Noncompliant
        Ok(foo);                                // Secondary

    [Route("foo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AnnotatedWithNoType() // Noncompliant {{Use the ProducesResponseType overload containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
        //     ^^^^^^^
    }

    [Route("foo")]
    [SwaggerResponse(200)]
    public IActionResult AnnotatedWithNoType2() // Noncompliant {{Use the ProducesResponseType overload containing the return type for successful responses.}}
        //               ^^^^^^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
        //     ^^^^^^^
    }

    [HttpGet("foo")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public IActionResult IgnoreApiFalse() => // Noncompliant
        Ok(42);                              // Secondary

    [HttpGet("foo")]
    public IActionResult UnusedSuccessResponse() // Noncompliant - FP, the created success response is not returned
    {
        var x = Ok(42);                          // Secondary
        return null;
    }
}

public class NotApiController : Controller
{
    [HttpGet("foo")]
    public IActionResult Foo() => Ok(new Foo());
}

[NonController]
[ApiController]
public class NotAController : Controller
{
    [HttpGet("foo")]
    public IActionResult Foo() => Ok(new Foo());
}

[ApiController]
public class NotPublicAction : Controller
{
    [HttpGet("foo")]
    internal IActionResult Foo() => Ok(new Foo());
}

[ApiController]
public class UsesApiConventionMethod : Controller
{
    [HttpGet("foo")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    public IActionResult Foo() => Ok(new Foo());
}

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
public class UsesApiConventionType : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult Foo() => Ok(new Foo());
}

[ApiController]
[ProducesResponseType<int>(StatusCodes.Status200OK)]
public class AnnotatedAtControllerLevel : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() => Ok(42);
}

[ApiController]
[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
public class AnnotatedAtControllerLevelWithTypeOf : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() => Ok(42);
}

[ApiController]
[ProducesResponseType(200)]
public class AnnotatedAtControllerLevelWithNoType : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() => // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
    //                   ^^^^^^^^^^^^^^^^^^
        Ok(42); // Secondary
    //  ^^^^^^

}

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class IgnoreApiTrueController : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() => Ok(42);
}

[ApiController]
[SwaggerResponse(StatusCodes.Status200OK, type: typeof(Foo))]
public class SwaggerResponseController : Controller
{
    [HttpGet("foo")]
    public IActionResult Foo() => Ok(new Foo());
}

[ApiController]
[ApiExplorerSettings(IgnoreApi = false)]
public class IgnoreApiFalseController : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() => // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for successful responses.}}
    //                   ^^^^^^^^^^^^^^^^^^
        Ok(42); // Secondary
    //  ^^^^^^
}

public class Foo
{
    public int Bar { get; set; }
}
