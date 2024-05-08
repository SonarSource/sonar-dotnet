using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("SomeRoute")]
public class C(HttpClient client) : ControllerBase;

[ApiController]
[Route("SomeRoute")]
public class D(HttpClient client) : C(new HttpClient())     // Compliant
{
    public D() : this(new HttpClient()) { }                 // Compliant
    private HttpClient Client { get => new HttpClient(); }  // Noncompliant
}

[ApiController]
[Route("SomeRoute")]
public class E : C
{
    public E() : base(new HttpClient()) { }                 // Compliant
}
