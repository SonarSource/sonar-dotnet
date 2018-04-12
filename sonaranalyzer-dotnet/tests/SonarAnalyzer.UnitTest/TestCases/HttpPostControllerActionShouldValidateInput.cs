using System;
using System.Web.Mvc;

namespace Tests.Diagnostics
{
    public class FooBarController1 : Controller
    {
        [HttpPost]
        [ValidateInput(false)] // Noncompliant {{Enable validation on this 'ValidateInput' attribute.}}
//       ^^^^^^^^^^^^^^^^^^^^
        public ActionResult Purchase1(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput(true)] // Compliant - not false
        public ActionResult Purchase2(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput()] // Compliant - not false
        public ActionResult Purchase3(string input)
        {
            return Foo(input);
        }

        [ValidateInput(false)] // Noncompliant - even when there is no HttpPost
        public ActionResult Purchase4(string input)
        {
            return Foo(input);
        }
    }

    public class FooBarController2
    {
        [HttpPost]
        [ValidateInput(false)] // Noncompliant - even when class doesn't inherit from Controller
        public ActionResult Purchase1(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput("foo")] // Compliant - unrecognized argument
        public ActionResult Purchase2(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput()] // Compliant - no argument
        public ActionResult Purchase3(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput(false, "foo")] // Compliant - more than 1 argument
        public ActionResult Purchase3(string input)
        {
            return Foo(input);
        }
    }
}
