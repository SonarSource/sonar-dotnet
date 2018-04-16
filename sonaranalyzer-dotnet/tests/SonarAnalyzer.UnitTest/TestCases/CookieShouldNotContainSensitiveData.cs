using System;
using System.Web;

namespace Tests.Diagnostics
{
    class Program
    {
        HttpCookie field1 = new HttpCookie("c"); // Compliant
        HttpCookie field2;

        HttpCookie Property1 { get; set; } = new HttpCookie("c"); // Compliant
        HttpCookie Property2 { get; set; }

        void CtorSetsAllowedValue()
        {
            // none
        }

        void CtorSetsNotAllowedValue()
        {
            new HttpCookie("c", "value"); // Noncompliant {{If the data stored in this cookie is sensitive, it should be moved to the user session.}}
        }

        void InitializerSetsAllowedValue()
        {
            new HttpCookie("c") { }; // Compliant
        }

        void InitializerSetsNotAllowedValue()
        {
            new HttpCookie("c") { Value = "value" }; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }

        void PropertySetsNotAllowedValue()
        {
            var c = new HttpCookie("c");
            c.Value = "value"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^
            c["key"] = "value"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            c.Values.Add("key", "value"); // Compliant, FN

            field1.Value = "value"; // Noncompliant
            this.field1.Value = "value"; // Noncompliant
            field1["key"] = "value"; // Noncompliant
            this.field1["key"] = "value"; // Noncompliant

            Property1.Value = "value"; // Noncompliant
            this.Property1.Value = "value"; // Noncompliant
            Property1["key"] = "value"; // Noncompliant
            this.Property1["key"] = "value"; // Noncompliant
        }

        void PropertySetsAllowedValue(bool foo)
        {
            // none
        }
    }
}
