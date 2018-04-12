using System;
using System.Web.Mvc;

namespace Tests.Diagnostics
{
    public class CompliantController : Controller
    {
        [HttpPost]
        [ValidateInput(true)] // Compliant - not false
        public ActionResult Foo1(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput("foo")] // Compliant - unrecognized argument
        public ActionResult Foo2(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput()] // Compliant - no argument
        public ActionResult Foo3(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput(false, "foo")] // Compliant - more than 1 argument
        public ActionResult Foo4(string input)
        {
            return Foo(input);
        }
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
    }

    public class MoreNonCompliantController
    {
        [HttpPost] // Noncompliant - even when class doesn't derived from Controller
        public ActionResult Foo1(string input)
        {
            return Foo(input);
        }
    }
}
