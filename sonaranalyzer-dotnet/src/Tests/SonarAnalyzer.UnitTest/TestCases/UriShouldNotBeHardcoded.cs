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

            var unixPath1 = "/my/other/folder"; // Noncompliant
            var unixPath2 = "~/blah/blah/blah.txt"; // Noncompliant
            var unixPath3 = "~\\blah\\blah\\blah.txt"; // Noncompliant

            var concatWithDelimiterPath1 = s1 + "\\" + s2; // Noncompliant {{Remove this hardcoded path-delimiter.}}
//                                              ^^^^
            var concatWithDelimiterUri2 = s1 + @"\" + s2; // Noncompliant
            var concatWithDelimiterUri3 = s1 + "/" + s2; // Noncompliant

            var x = new Uri("C:/test.txt"); // Noncompliant
            new Uri(new Uri(), ("C:/test.txt")); // Noncompliant
            File.OpenRead(@"\\drive\foo.csv"); // Noncompliant
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