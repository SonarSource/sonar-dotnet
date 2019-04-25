using System;
using System.Collections.Specialized;
using System.Web;

namespace Tests.Diagnostics
{
    class Program
    {
        HttpCookie field1 = new HttpCookie("c"); // Compliant, value is not set
        HttpCookie field2 = new HttpCookie("c", ""); // Noncompliant
        HttpCookie field3;

        HttpCookie Property1 { get; set; } = new HttpCookie("c"); // Compliant, value is not set
        HttpCookie Property2 { get; set; } = new HttpCookie("c", ""); // Noncompliant
        HttpCookie Property3 { get; set; }

        void CtorSetsValue()
        {
            HttpCookie cookie;
            cookie = new HttpCookie("c"); // Compliant, value is not set
            cookie = new HttpCookie("c", ""); // Noncompliant {{Make sure that this cookie is written safely.}}

            cookie = new HttpCookie("c") { }; // Compliant
            cookie = new HttpCookie("c") { Value = "" }; // Noncompliant
//                                         ^^^^^
        }

        void Value_Vaues_Write()
        {
            var c = new HttpCookie("c");

            c.Value = ""; // Noncompliant
//          ^^^^^^^
            c["key"] = ""; // Noncompliant
//          ^^^^^^^^

            c.Values[""] = ""; // Noncompliant
            c.Values.Add("key", "value"); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            // operations on generic NameValueCollection objects do not raise any issue
            var nvc = new NameValueCollection { { "a", "1" }, { "b", "2" } };
            nvc["a"] = "2";
            nvc.Add("c", "2");

            // setting HttpCookie.Value on fields
            field1.Value = "value"; // Noncompliant
            this.field1.Value = "value"; // Noncompliant
            field1["key"] = "value"; // Noncompliant
            this.field1["key"] = "value"; // Noncompliant

            // setting HttpCookie.Value on properties
            Property1.Value = "value"; // Noncompliant
            this.Property1.Value = "value"; // Noncompliant
            Property1["key"] = "value"; // Noncompliant
            this.Property1["key"] = "value"; // Noncompliant
        }

        void Value_Values_Read(HttpCookie cookie)
        {
            string value;
            value = cookie.Value; // Compliant
            value = cookie[""]; // Compliant
            value = cookie.Values[""]; // Compliant
            value = cookie.Values[""]; // Compliant

            if (cookie.Value != "") // Compliant
            {
                Console.Write(cookie.Value); // Compliant
            }
        }

        void Request_Response_Cookies(HttpRequest request, HttpResponse response)
        {
            request.Cookies[""].Value = ""; // Noncompliant
            request.Cookies[0].Value = ""; // Noncompliant
            request.Cookies[0][""] = ""; // Noncompliant
            request.Cookies[0].Values[""] = ""; // Noncompliant

            response.Cookies[""].Value = ""; // Noncompliant
            response.Cookies[0].Value = ""; // Noncompliant
            response.Cookies[0][""] = ""; // Noncompliant
            response.Cookies[0].Values[""] = ""; // Noncompliant
        }
    }
}
