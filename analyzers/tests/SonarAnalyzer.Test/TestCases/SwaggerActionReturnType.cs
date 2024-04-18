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
    [ProducesResponseType(typeof(Foo), StatusCodes.Status200OK)]
    public IActionResult HasProducesResponseTypeTypeOf() => Ok(foo);

    [HttpGet("foo")]
    [ProducesResponseType(typeof(Foo), StatusCodes.Status200OK, "application/json")]
    public IActionResult HasProducesResponseTypeTypeOf2() => Ok(foo);

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
}

public class Foo
{
    public int Bar { get; set; }
}
