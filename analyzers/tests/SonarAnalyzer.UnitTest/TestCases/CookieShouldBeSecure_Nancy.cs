using Nancy.Cookies;
using System;

namespace Tests.Diagnostics
{
    class Program
    {
        private bool trueField = true;
        private bool falseField = false;

        void CtorSetsAllowedValues()
        {
            new NancyCookie("name", "value", true, true);
            new NancyCookie("name", "value", false, true);
            new NancyCookie("name", "value", false, trueField);
            new NancyCookie("name", "value", secure: true, httpOnly: false);
            new NancyCookie(httpOnly: false, name: "name", secure: true, value: "value");
        }

        void CtorSetsDisallowedValues(string name, string value, DateTime? expires, string domain, string path)
        {
            new NancyCookie("name", "secure");                      // Noncompliant
            new NancyCookie("name", "httpOnly");                    // Noncompliant
            new NancyCookie("name", "value", DateTime.Now);         // Noncompliant
            new NancyCookie("name", "value", true);                 // Noncompliant
            new NancyCookie("name", "value", false);                // Noncompliant
            new NancyCookie("name", "value", false, false);         // Noncompliant
            new NancyCookie("name", "value", false, falseField);    // Noncompliant
            new NancyCookie("name", "value", true, false);          // Noncompliant
            new NancyCookie("name", "value", false, false, DateTime.Now);       // Noncompliant
            new NancyCookie("name", "value", true, false, DateTime.Now);        // Noncompliant
            new NancyCookie("name", "value", secure: false, httpOnly: true);    // Noncompliant
            new NancyCookie(httpOnly: true, name: "name", secure: false, value: "value");       // Noncompliant
            new NancyCookie(name, value) { Expires = expires, Domain = domain, Path = path };   // Noncompliant
        }
    }
}
