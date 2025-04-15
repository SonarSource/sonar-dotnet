using System;

namespace Tests.Diagnostics
{
    #region S3996 Property type
    class S3996_Foo
    {
        public virtual string Url { get; set; } // Noncompliant {{Change the 'Url' property type to 'System.Uri'.}}
//                     ^^^^^^
    }

    class S3996_Bar : S3996_Foo
    {
        public override string Url { get; set; } // Compliant
    }

    class S3996
    {
        string Url { get; set; } // Noncompliant {{Change the 'Url' property type to 'System.Uri'.}}
//      ^^^^^^
        string url // Noncompliant {{Change the 'url' property type to 'System.Uri'.}}
//      ^^^^^^
        {
            get;
            set;
        }

        string Url2 => ""; // Noncompliant {{Change the 'Url2' property type to 'System.Uri'.}}
//      ^^^^^^
        string FooUrlBar { get; set; } // Noncompliant {{Change the 'FooUrlBar' property type to 'System.Uri'.}}
        int ThisIsAnUrlProperty { get; set; } // Compliant


        // Urn
        string Urn { get; set; } // Noncompliant {{Change the 'Urn' property type to 'System.Uri'.}}
//      ^^^^^^
        string FooUrnBar{ get; set; } // Noncompliant {{Change the 'FooUrnBar' property type to 'System.Uri'.}}


        // Uri
        string Uri { get; set; } // Noncompliant {{Change the 'Uri' property type to 'System.Uri'.}}
//      ^^^^^^
        string FooUriBar{ get; set; } // Noncompliant {{Change the 'FooUriBar' property type to 'System.Uri'.}}

        string Urn2 // Noncompliant
        {
            get => "";
            set => throw new NotSupportedException();
        }


        // Test there are no false positives
        string TurnOff { get; set; } // Compliant
        string Urinal { get; set; } // Compliant
        string Hourly { get; set; } // Compliant
        string urifoo { get; set; } // Compliant
        string urlfoo { get; set; } // Compliant
        string urnfoo { get; set; } // Compliant
    }
    #endregion

    #region S3995 Method return type

    class S3995_Foo
    {
        public virtual string Uri() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
//                     ^^^^^^
    }

    class S3995_Bar : S3995_Foo
    {
        public override string Uri() => ""; // Compliant
    }

    class S3995
    {
        // Url
        string AUrlMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        string UrlMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        int ThisIsAnUrlMethod() => 1; // Compliant


        // Urn
        string AUrnMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        string UrnMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}


        // Uri
        string AUriMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        string UriMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}


        // Test there are no false positives
        string Pouring() => ""; // Compliant
        string Hurl() => ""; // Compliant
    }
    #endregion

    #region S3994 Parameter type or suitable overload available

    class S3994
    {
        public S3994(string uri) { } // Compliant
        public S3994(Uri uri) { } // Compliant

        public S3994(string uri, bool blah) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        private S3994(string uri, bool blah, bool foo) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        public string OverloadedMethod(string url) => OverloadedMethod(new Uri(url)); // Compliant
        public string OverloadedMethod(Uri url) => ""; // Compliant

        public string Method(Uri url) => "";
        public string MethodWithExtraParam(string url, bool somevalue) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        public string Method2(Uri url) => "";
        public string Method2WithOptionalParam(string url, bool foo = false) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        public string Method3(string url) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
        public string Method3Generic<T>(T url) => "";

        public string Method4Private(string url) => Method4Private(new Uri(url)); // Compliant
        private string Method4Private(Uri url) => ""; // Compliant

        public string ParamTest(string uri) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                              ^^^^^^
        public string ParamTest2(int url) => ""; // Compliant
        public string ParamTestOverload(string uriParam) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                      ^^^^^^
        public string ParamTestOverload(int uriParam) => ""; // Compliant
        public string MultipleParams(string uriParam, object urnParam, string urlParam, string andThisIsFine) => "";
//                                   ^^^^^^ Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                                                     ^^^^^^ Noncompliant@-1 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
    }
    #endregion

    #region S3997 Call System.Uri overload from method that accepts string url

    class S3997
    {
        public string OverloadedMethod(Uri url) => "";
        public string OverloadedMethod(string url) => OverloadedMethod(new Uri(url)); // Compliant

        public string OverloadedMethod2(Uri url) => "";
        public string OverloadedMethod2(string url)
        {
            return OverloadedMethod2(new Uri(url)); // Compliant
        }

        public string OverloadedMethod3(Uri url) => "";
        public string OverloadedMethod3(string url) // Compliant
        {
            OverloadedMethod3(new Uri(url));
            return "";
        }

        public string OverloadedMethod4(Uri url) => "";
        public string OverloadedMethod4(string url) // Noncompliant {{Refactor this method so it invokes the overload accepting a 'System.Uri' parameter.}}
        {
            return "foo";
        }

        public string OverloadedMethod5(Uri url) => "";
        public string OverloadedMethod5(string url) => ""; // Noncompliant {{Refactor this method so it invokes the overload accepting a 'System.Uri' parameter.}}
    }
    #endregion

    #region S4005 Call the overload that accepts System.Uri
    class S4005
    {
        static void Main()
        {
            var p = new S3994("www.sonarsource.com");
//                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}

            p.OverloadedMethod("www.sonarsource.com");
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}

            p.MethodWithExtraParam("www.sonarsource.com", true); // Compliant
            p.Method2WithOptionalParam("www.sonarsource.com"); // Compliant
            p.Method3("www.sonarsource.com"); // Compliant
            p.Method4Private("www.sonarsource.com"); // Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}

            Uri result;
            // Do not raise issues when using Uri class
            Uri.TryCreate("", UriKind.Absolute, out result); // Compliant

            result = new Uri("foo");
            result = new Uri("foo", true);
        }
    }

    #endregion

    #region Mix of rules
    class AllRules
    {
        string AllUrlWrong(string url, string uri, string urn) => "";
//      ^^^^^^ Noncompliant {{Change this return type to 'System.Uri'.}}
//                         ^^^^^^ Noncompliant@-1 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                     ^^^^^^ Noncompliant@-2 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                                 ^^^^^^ Noncompliant@-3 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        string GetTheUrl(string url) => "";
//      ^^^^^^ Noncompliant {{Change this return type to 'System.Uri'.}}
//             ^^^^^^^^^ Noncompliant@-1 {{Refactor this method so it invokes the overload accepting a 'System.Uri' parameter.}}

        string GetTheUrl(Uri url) => "";
//      ^^^^^^ Noncompliant {{Change this return type to 'System.Uri'.}}

    }
    #endregion
}
