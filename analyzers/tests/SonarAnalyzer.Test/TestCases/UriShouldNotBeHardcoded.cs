using System;
using System.IO;

namespace Tests.Diagnostics
{
    class Program
    {
        void InvalidCases(string s1, string s2)
        {
            var fileLiteral = "file://blah.txt"; // Noncompliant {{Refactor your code not to use hardcoded absolute paths or URIs.}}
//                            ^^^^^^^^^^^^^^^^^

            var webUri1 = "http://www.mywebsite.com"; // Noncompliant
            var webUri2 = "https://www.mywebsite.com"; // Noncompliant
            var webUri3 = "ftp://www.mywebsite.com"; // Noncompliant

            var windowsDrivePath1 = "c:\\blah\\blah\\blah.txt"; // Noncompliant
            var windowsDrivePath2 = "C:\\blah\\blah\\blah.txt"; // Noncompliant
            var windowsDrivePath3 = "C:/blah/blah/blah.txt"; // Noncompliant
            var windowsDrivePath4 = @"C:\blah\blah\blah.txt"; // Noncompliant
            var windowsDrivePath5 = @"C:\%foo%\Documents and Settings\file.txt"; // Noncompliant

            var windowsSharedDrivePath1 = @"\\my-network-drive\folder\file.txt"; // Noncompliant
            var windowsSharedDrivePath2 = @"\\my-network-drive\Documents and Settings\file.txt"; // Noncompliant
            var windowsSharedDrivePath3 = "\\\\my-network-drive\\folder\\file.txt"; // Noncompliant
            var windowsSharedDrivePath4 = @"\\my-network-drive\%foo%\file.txt"; // Noncompliant
            var windowsSharedDrivePath5 = @"\\my-network-drive/folder/file.txt"; // Noncompliant

            IComparable compatibleTypeUri = "ftp://www.mywebsite.com"; // Noncompliant

            var unixPath1 = "/my/other/folder"; // Compliant - we ignore unix paths by default
            var unixPath2 = "~/blah/blah/blah.txt"; // Compliant
            var unixPath3 = "~\\blah\\blah\\blah.txt"; // Compliant

            var concatWithDelimiterPath1 = s1 + "\\" + s2; // Noncompliant {{Remove this hardcoded path-delimiter.}}
//                                              ^^^^
            var concatWithDelimiterUri2 = s1 + @"\" + s2; // Noncompliant
            var concatWithDelimiterUri3 = s1 + "/" + s2; // Noncompliant
            var concatWithDelimiterUriOnLeft = "/" + s2; // Noncompliant

            var x = new Uri("C:/test.txt"); // Noncompliant
            new Uri(new Uri("../stuff"), ("C:/test.txt")); // Noncompliant
            File.OpenRead(@"\\drive\foo.csv"); // Noncompliant

            // We don't support non-English variable names. This is happening due to the way the rule checks against
            // a small set of predefined words that do not include translations.
            var unixChemin = "/my/other/folder"; // Compliant - we ignore unix paths by default
            var webChemin = "http://www.mywebsite.com"; // FN
            var windowsChemin = "c:\\blah\\blah\\blah.txt"; // FN

            var literalConcat = "http://" + "example.com"; // FN
            string GetPath() => "C:/test.txt"; // FN

            // The rule only checks the string literals that are [arguments in methods/constructors] or [assignment]
            bool ReturnStatement(string uri)
            {
                return uri is "\\my-network-drive\folder\file.txt"; // FN
            }


            bool ExpressionBody(string uri) =>
                uri is "\\my-network-drive\folder\file.txt"; // FN
        }

        void ValidCases(string s)
        {
            var windowsPathStartingWithVariable = "%AppData%\\Adobe";
            var windowsPathWithVariable = "%appdata%";

            var relativePath1 = "./my/folder";
            var relativePath2 = @".\my\folder";
            var relativePath3 = @"..\..\Documents";
            var relativePath4 = @"../../Documents";
            var file = "file.txt";

            var driveLetterPath = "C:";

            var concat1 = s + "\\" + s;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7815
class ReproFN_7815
{
    class MyClass
    {
        public string FilePath { get; set; }
    }

    void Method()
    {
        var myClass = new MyClass
        {
            FilePath = "/my/other/folder" // Compliant - we ignore unix paths by default
        };

        var myClass2 = new MyClass
        {
            FilePath = @"\\my-network-drive\folder\file.txt" // Noncompliant
        };

        var myClass3 = new MyClass
        {
            FilePath = "http://www.mywebsite.com" // Noncompliant
        };

        var myClass4 = new MyClass
        {
            FilePath = "c:\\blah\\blah\\blah.txt" // Noncompliant
        };
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8169
class Repro_8169
{
    void Method()
    {
        (string, string) uris1 = ("C:/test.txt", "C:/test.txt");       // FN
        (string uri, string) a = ("C:/test.txt", "C:/test.txt");       // FN, first
        (string uri, string uri2) a1 = ("C:/test.txt", "C:/test.txt"); // FN, first and second
        (string uri, string uri2) = ("C:/test.txt", "C:/test.txt");    // FN, first and second
        (string uri, string) uris = ("C:/test.txt", "C:/test.txt");    // FN, first and second
        (string b, (string uri, string c)) a2 = ("C:/test.txt", ("C:/test.txt", "C:/test.txt")); // FN, second
    }
}
