using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

class ChildAttribute : ApiControllerAttribute { }
class GrandChildAttribute : ChildAttribute { }

public class ParentController : Controller { }
public class ParentControllerBase : ControllerBase { }

public class ChildController : ParentController { }
public class ChildControllerBase : ParentControllerBase { }

namespace SimpleCases
{
    [ApiController]
    public class Baseline : Controller { }  // Noncompliant {{Inherit from ControllerBase instead of Controller.}}
    //                      ^^^^^^^^^^

    [ApiController]
    public class ControllerWithInterface : Controller, ITestInterface { } // Noncompliant
    //                                     ^^^^^^^^^^

    [ApiController]
    public class ChildOfBase : ControllerBase { }       // Compliant

    public class WithoutAttribute : Controller { }      // Compliant

    public class PocoController { }                     // Compliant

    [ApiController]
    public class PocoWithApiAttribute { }               // Compliant

    [Controller]
    public class PocoWithControllerAttribute { }        // Compliant

    [ApiController]
    [Controller]
    public class PocoWithBothAttributes { }             // Compliant, this is a very rare case, found <10 hits for [Controller] on SG

    [ApiController]
    [NonController]
    public class NotAController : Controller { }        // Compliant, [NonController] is excluded

    [ApiController]
    internal class Internal : Controller { }            // Compliant, only raises at public methods

    public interface ITestInterface { }
}

namespace SpecialAttributeUsages
{
    [type: ApiController]
    public class WithType : Controller { }          // Noncompliant
    //                      ^^^^^^^^^^
    [ApiControllerAttribute]
    public class WithSuffix : Controller { }        // Noncompliant
    //                        ^^^^^^^^^^
    [ApiController()]
    public class WithParentheses : Controller { }   // Noncompliant
    //                             ^^^^^^^^^^
    [type: ApiControllerAttribute()]
    public class Everything : Controller { }        // Noncompliant
    //                        ^^^^^^^^^^
}

namespace Inheritance
{
    [ApiController]
    public class Child : ParentController { }            // Compliant, we only check direct inheritance from "Controller"

    [ApiController]
    public class GrandChild : ChildController { }        // Compliant

    [ApiController]
    public class NoInheritance { }                       // Compliant
}

namespace CustomAttribute
{
    [ChildAttribute]
    public class UsesChildAttribute : Controller { }                 // Noncompliant
    //                                ^^^^^^^^^^

    [GrandChildAttribute]
    public class UsesGrandChildAttribute : Controller { }            // Noncompliant
    //                                     ^^^^^^^^^^

    [ChildAttribute]
    public class UsesChildAttributeBase : ControllerBase { }         // Compliant

    [GrandChildAttribute]
    public class UsesGrandChildAttributeBase : ControllerBase { }    // Compliant
}

namespace Partial
{
    [ApiController]
    public partial class Partial { }
    public partial class Partial : Controller { }                  // Noncompliant
    //                             ^^^^^^^^^^

    [ApiController]
    public partial class PartialBase { }
    public partial class PartialBase : ControllerBase { }          // Compliant

    [ApiController]
    public partial class PartialDoubleInheritance : Controller { } // Noncompliant
    //                                              ^^^^^^^^^^

    public partial class PartialDoubleInheritance : Controller { } // Noncompliant
    //                                              ^^^^^^^^^^

}

namespace Nested
{
    public class Outer
    {
        [ApiController]
        public class Inner : Controller { }   // Compliant, is in nested class - not accessible from the user.
    }
}

namespace MemberUsages
{
    [ApiController]
    public class OverrideViewInvocation : Controller    // Compliant
    {
        public object Foo() => this.View();             // overrides and uses View
        public override ViewResult View() => null;
    }

    [ApiController]
    public class FakeViewInvocation : Controller        // FN, it's not an actual view
    {
        public object Foo() => this.View();             // hides View, does not override it
        public ViewResult View() => null;
    }

    [ApiController]
    public class BaseViewInvocation : Controller        // Compliant
    {
        public object Foo() => base.View();             // uses Controller.View
        public ViewResult View() => null;
    }

    [ApiController]
    public partial class Partial { }
    public partial class Partial : Controller { }       // Compliant
    public partial class Partial
    {
        public object Foo() => View();
    }

    [ApiController]
    public class MemberReference : Controller               // Compliant
    {
        public Func<ViewResult> NotCalled() => this.View;   // nothing is invoked, but the dependency is used.
    }

    [ApiController]
    public class NameOf : Controller          // Compliant
    {
        public object Foo() => nameof(View); // same as above
    }

    [ApiController]
    public class PassByName : Controller                // Compliant
    {
        public void Foo() => ExpectsAction(View);
        public void ExpectsAction(Func<ViewResult> func)
        {
            // here func could be invoked or not.
        }
    }

    [ApiController]
    public class PassByLambda : Controller              // Compliant
    {
        public void Foo() => ExpectsAction(() => View());
        public void ExpectsAction(Func<ViewResult> func)
        {
            // here func could be invoked or not.
        }
    }
}
