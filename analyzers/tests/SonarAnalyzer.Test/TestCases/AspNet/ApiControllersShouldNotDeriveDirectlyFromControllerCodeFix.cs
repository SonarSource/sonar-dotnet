using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace CodeFixCases
{
    [ApiController]
    public class Baseline : ControllerBase { }


    [ApiController]
    public class SimpleController : Controller { } // Noncompliant

    [ApiController]
    public class CodeFixRespectsCommentsController : Controller /* I'm a small comment and I wish to be respected */ { } // Noncompliant

    [ApiController]
    public class CodeFixRespectsCommentsAlsoHasInterfaceController : Controller /* Ditto */, ITestInterface { } // Noncompliant

    [ApiController]
    public class ControllerWithInterface : Controller, ITestInterface { } // Noncompliant

    public interface ITestInterface { }
}
