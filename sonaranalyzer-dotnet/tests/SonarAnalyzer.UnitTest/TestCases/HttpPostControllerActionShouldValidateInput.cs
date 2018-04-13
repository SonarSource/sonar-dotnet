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
        }

        [ValidateInput(false)] // Compliant - no HttpPostAttribute
        public ActionResult Foo3(string input)
        {
        }

        [System.Web.Mvc.HttpPost] // Compliant - input is validated
        [System.Web.Mvc.ValidateInput(true)]
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

        [HttpPost] // Noncompliant
        [ValidateInput("foo")]
        public ActionResult Foo3(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Noncompliant
        [ValidateInput()]
        public ActionResult Foo4(string input)
        {
            return Foo(input);
        }

        [HttpPost] // Noncompliant
        [ValidateInput(false, "foo")]
        public ActionResult Foo5(string input)
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
