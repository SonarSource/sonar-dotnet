using System;
using System.Web.Mvc;

namespace Tests.Diagnostics
{
    public class FooBarController1 : Controller
    {
        [HttpPost]
        [ValidateInput(false)] // Noncompliant
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

        [ValidateInput(false)] // Noncompliant - no HttpPost
        public ActionResult Purchase4(string input)
        {
            return Foo(input);
        }
    }

    public class FooBarController1
    {
        [HttpPost]
        [ValidateInput(false)] // Noncompliant - class doesn't inherit from Controller
        public ActionResult Purchase1(string input)
        {
            return Foo(input);
        }
    }
}
