using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
public class SomeController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private HttpClient clientField = new HttpClient();

    [HttpGet("foo")]
    public async Task<string> Foo()
    {
        using var clientA = new HttpClient();                     // Noncompliant
        //                  ^^^^^^^^^^^^^^^^
        await clientA.GetStringAsync("");

        clientField ??= new HttpClient();                         // Compliant
        using var pooledClient = _clientFactory.CreateClient();   // Compliant
        _ = true switch { true => new HttpClient() };             // Compliant
        return "bar";
    }
}
