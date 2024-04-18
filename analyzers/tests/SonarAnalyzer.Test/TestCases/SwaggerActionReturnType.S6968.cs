[ApiController]
public class NocompliantBaseline : ControllerBase
{
    private Foo foo = new();

    // For the implementation: If this seems too cumbersome, consider dropping it and documenting it as FN
    [HttpGet("foo")]
    public ObjectResult NewObjectResult() => // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for succesful responses}}
        //              ^^^^^^^^^^^^^^^
        new ObjectResult(42); // Secondary
    //  ^^^^^^^^^^^^^^^^^^^^

    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for succesful responses}}
        //               ^^^^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
    //         ^^^^^^^
    }

    [HttpGet("foo")]
    public IActionResult ReturnsMultipleValues(bool condition) // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for succesful responses}}
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
    public IActionResult ReturnsMultipleValuesTernary() // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for succesful responses}}
        //               ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        return true
            ? Ok(foo)           // Secondary
        //    ^^^^^^^
            : Accepted(foo);    // Secondary
        //    ^^^^^^^^^^^^^
    }

    [HttpGet("foo")]
    public IActionResult ReturnsMultipleValuesSwitch(int id) // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for succesful responses}}
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
    public IActionResult MarkedWithRoute() // Noncompliant {{Annotate this method with ProducesResponseType containing the return type for succesful responses}}
        //               ^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
    //         ^^^^^^^
    }

    [Route("foo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AnnotatedWithNoType() // Noncompliant {{Use the ProducesResponseType overload containing the return type for succesful responses}}
        //               ^^^^^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
        //     ^^^^^^^
    }

    [Route("foo")]
    [SwaggerResponse(200)]
    public IActionResult AnnotatedWithNoType2() // Noncompliant {{Use the ProducesResponseType overload containing the return type for succesful responses}}
        //               ^^^^^^^^^^^^^^^^^^^^
    {
        return Ok(foo); // Secondary
        //     ^^^^^^^
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
[ProducesResponseType(200)]
public class AnnotatedAtControllerLevelWithNoType : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() => // Noncompliant {{Use the ProducesResponseType overload containing the return type for succesful responses}}
    //                   ^^^^^^^^^^^^^^^^^^
        Ok(42); // Secondary
    //  ^^^^^^

}

public class Foo
{
    public int Bar { get; set; }
}
