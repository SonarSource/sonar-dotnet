using Nancy.Cookies;
using System;

namespace Tests.Diagnostics
{
    class Program
    {
        private bool trueField = true;
        private bool falseField = false;

        void CtorSetsAllowedValues(bool arg)
        {
            new NancyCookie("name", "value", true);
            new NancyCookie("name", "value", trueField);
            new NancyCookie("name", "value", true, false);
            new NancyCookie("name", "value", true, true);
            new NancyCookie("name", "value", true, false, DateTime.Now);
            new NancyCookie("name", "value", secure: false, httpOnly: true);
            new NancyCookie(httpOnly: true, name: "name", secure: false, value: "value");
        }

        void CtorSetsDisallowedValues(string name, string value, DateTime? expires, string domain, string path, bool arg)
        {
            var falseVariable = false;
            new NancyCookie("name", "value");                   // Noncompliant
            new NancyCookie("name", "httpOnly");                // Noncompliant
            new NancyCookie("name", "value", DateTime.Now);     // Noncompliant
            new NancyCookie("name", "value", false);            // Noncompliant
            new NancyCookie("name", "value", arg);              // Noncompliant, it's not known to be true, so we raise
            new NancyCookie("name", "value", falseField);       // Noncompliant
            new NancyCookie("name", "value", falseVariable);    // Noncompliant
            new NancyCookie("name", "value", false, false);     // Noncompliant
            new NancyCookie("name", "value", false, true);      // Noncompliant
            new NancyCookie("name", "value", false, false, DateTime.Now);       // Noncompliant
            new NancyCookie("name", "value", secure: true, httpOnly: false);    // Noncompliant
            new NancyCookie(httpOnly: false, name: "name", secure: true, value: "value");       // Noncompliant
            new NancyCookie(name, value) { Expires = expires, Domain = domain, Path = path };   // Noncompliant
        }
    }
}
