using System;
using System.Web.Mvc;

namespace Tests.Diagnostics
{
    public class CompliantController : Controller
    {
        [HttpPost] // Compliant - input is validated
        [ValidateInput(true)]
        public ActionResult Foo1(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Compliant - no input to validate
        public ActionResult Foo2()
        {
            return null;
        }

        [ValidateInput(false)] // Compliant - no HttpPostAttribute
        public ActionResult Foo3(string input)
        {
            return null;
        }

        [System.Web.Mvc.HttpPost] // Compliant - input is validated
        [System.Web.Mvc.ValidateInput(true)]
        public ActionResult Foo4(string input)
        {
            return Foo(input);
        }

        private ActionResult Foo(string i) => null;
    }

    public class NonCompliantController : Controller
    {
        [HttpPost] // Noncompliant {{Enable input validation for this HttpPost method.}}
//       ^^^^^^^^
        [ValidateInput(false)]
        public ActionResult Foo1(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Noncompliant
        public ActionResult Foo2(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Noncompliant
        [ValidateInput("foo")] // Error [CS1503] - cannot convert
        public ActionResult Foo3(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Noncompliant
        [ValidateInput()] // Error [CS7036] - no arg given
        public ActionResult Foo4(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Noncompliant
        [ValidateInput(false, "foo")] // Error [CS1729] - ctor doesn't exist
        public ActionResult Foo5(string input)
        {
            return Foo(input);
        }

        private ActionResult Foo(string i) => null;
    }

    public class MoreNonCompliantController
    {
        [HttpPost] // Noncompliant - even when class doesn't derived from Controller
        public ActionResult Foo1(string input)
        {
            return Foo(input);
        }

        private ActionResult Foo(string i) => null;
    }
}
