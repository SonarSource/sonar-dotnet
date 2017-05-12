using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public static void FetchResource(string uriString) { }
        public static void FetchResource(Uri uri) { }

        public static void FetchResource2(Uri uri, bool whatever) { }
        public static void FetchResource2(string stringUri, bool whatever) { }

        public static void FetchResource3(object stringUri, bool whatever) { }

        public static void FetchResource4(string stringUri, bool whatever) { }
//                                        ^^^^^^ Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
        public static void FetchResource4(object stringUri, bool whatever) { }

        public static void FetchResource5(Uri uri, Uri url) { }
        public static void FetchResource5(string stringUri, string stringUrl) { }


        public static void FetchResource6(Uri uri, Uri url) { }
        public static void FetchResource6(string stringUri, string stringUrl, bool extraParam) { } // Noncompliant
                                                                                                   // Noncompliant@-1

        static void Main()
        {
            FetchResource("www.sonarsource.com");
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}
            FetchResource(new Uri("www.sonarsource.com"));

            FetchResource2("www.sonarsource.com", true);
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}
            FetchResource2(new Uri("www.sonarsource.com"), true);

            FetchResource3("www.sonarsource.com", true);

            FetchResource4("www.sonarsource.com", true);

            FetchResource5("www.sonarsource.com", "www.sonarqube.com"); // Noncompliant

            FetchResource6("www.sonarsource.com", "www.sonarqube.com");

            Uri result;
            // Note there is an overload Uri.TryCreate(Uri, UriKind, out Uri), which should not be flagged
            Uri.TryCreate("", UriKind.Absolute, out result);
            Uri.TryCreate(new object(), UriKind.Absolute, out result);

            result = new Uri("foo");
            result = new Uri("foo", true);
        }
    }
}
