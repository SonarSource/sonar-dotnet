using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace CodeFixCases
{
    [ApiController]
    public class Baseline : ControllerBase { }


    [ApiController]
    public class SimpleController: Controller { } // Noncompliant

    [ApiController]
    public class ControllerWithInterface : Controller, ITestInterface { } // Noncompliant

    public interface ITestInterface { }
}
