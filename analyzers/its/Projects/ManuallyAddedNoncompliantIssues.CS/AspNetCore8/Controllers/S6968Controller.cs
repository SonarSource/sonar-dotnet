using AspNetCore8.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore8.Controllers;

[ApiController]
public class S6968Controller : ControllerBase
{
    [HttpGet("foo")]
    public IActionResult ReturnsOkWithValue() // Noncompliant
        => Ok(new Foo());                     // Secondary
}
