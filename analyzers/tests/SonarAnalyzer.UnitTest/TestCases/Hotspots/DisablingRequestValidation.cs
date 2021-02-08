using System;
using System.Web.Mvc;

namespace Tests.Diagnostics
{
    [ValidateInput(false)] // Noncompliant {{Make sure disabling ASP.NET Request Validation feature is safe here.}}
    public class NonCompliantClass
    {
    }

    public class NonCompliantMethods // we don't care if it derives from controller
    {
        [ValidateInput(false)] // Noncompliant {{Make sure disabling ASP.NET Request Validation feature is safe here.}}
//       ^^^^^^^^^^^^^^^^^^^^
        public ActionResult Foo(string input)
        {
            return null;
        }

        [CLSCompliant(false)]
        [ValidateInput(false)] // Noncompliant
        public ActionResult WithTwoFalse(string input)
        {
            return Foo(input);
        }

        [HttpPost]
        [ValidateInput(false)] // Noncompliant
        [Obsolete]
        public ActionResult FooWithMoreAttributes(string input)
        {
            return Foo(input);
        }

        [ValidateInput(false)] // Noncompliant
        public ActionResult FooNoParam()
        {
            return null;
        }

        [ValidateInput(false)] // Noncompliant
        public void VoidFoo() { }

        [ValidateInput(false)] // Noncompliant
        private ActionResult ArrowFoo(string i) => null;
    }

    public class CompliantController : Controller
    {
        [ValidateInput(true)]
        public ActionResult Foo(string input)
        {
            return Foo(input);
        }

        public ActionResult Bar()
        {
            return null;
        }

        [HttpPost]
        public ActionResult Boo(AllowedHtml input) => null;

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.ValidateInput(true)]
        public ActionResult Qix(string input)
        {
            return Foo(input);
        }

        private ActionResult Quix(string i) => null;
    }

    [ValidateInput(true)]
    public class CompliantController2 : Controller
    {
    }

    public class AllowedHtml
    {
        [AllowHtml] // ok
        public string Prop { get; set; }
    }

    [Obsolete]
    public class MyObsoleteClass // for coverage
    {
    }

    [Obsolete("", false)]
    public class MyObsoleteClass2 // for coverage
    {
    }

    [CLSCompliant(false)] // for coverage
    public class ClassWithFalse
    {
    }

    public class Errors
    {
        [ValidateInput("foo")] // Error [CS1503] - cannot convert to bool
        public ActionResult Foo(string input)
        {
            return Foo(input);
        }

        [ValidateInput()] // Error [CS7036] - no arg given
        public ActionResult Bar(string input)
        {
            return Foo(input);
        }

        [ValidateInput(false, "foo")] // Error [CS1729] - ctor doesn't exist
        public ActionResult Baz(string input)
        {
            return Foo(input);
        }
    }
}

