using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace CodeFixCases
{
    [ApiController]
    public class Baseline : ControllerBase { }


    [ApiController]
    public class SimpleController : ControllerBase { } // Fixed

    [ApiController]
    public class CodeFixRespectsCommentsController : ControllerBase /* I'm a small comment and I wish to be respected */ { } // Fixed

    [ApiController]
    public class CodeFixRespectsCommentsAlsoHasInterfaceController : ControllerBase /* Ditto */, ITestInterface { } // Fixed

    [ApiController]
    public class ControllerWithInterface : ControllerBase, ITestInterface { } // Fixed

    public interface ITestInterface { }
}
