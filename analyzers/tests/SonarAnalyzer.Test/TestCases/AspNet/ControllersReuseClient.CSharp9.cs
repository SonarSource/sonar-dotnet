using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("Hello")]
public class SomeController : ControllerBase
{
    [HttpGet("foo")]
    public async Task<string> Foo()
    {
        HttpClient client = new();  // Noncompliant
        return "bar";
    }
}
