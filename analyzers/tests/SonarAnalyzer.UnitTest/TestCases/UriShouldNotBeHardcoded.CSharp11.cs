using System;
using System.IO;

public class TestCases
{
    public void RawStrings()
    {
        var fileLiteral = """file://blah.txt"""; // Noncompliant {{Refactor your code not to use hardcoded absolute paths or URIs.}}
//                        ^^^^^^^^^^^^^^^^^^^^^

        var webUri1 = """http://www.mywebsite.com"""; // Noncompliant
        var windowsDrivePath1 = """c:\\blah\\blah\\blah.txt"""; // Noncompliant
        var windowsSharedDrivePath1 = """\\my-network-drive\folder\file.txt"""; // Noncompliant
        var x = new Uri("""C:/test.txt"""); // Noncompliant

        var unixPath1 = """/my/other/folder"""; // Compliant - we ignore unix paths by default
        var unixPath2 = """~/blah/blah/blah.txt"""; // Compliant
        var unixPath3 = """~\\blah\\blah\\blah.txt"""; // Compliant
        var windowsPathStartingWithVariable = """%AppData%\\Adobe""";

        var webChemin = """http://www.mywebsite.com"""; // FN
        var windowsChemin = """c:\\blah\\blah\\blah.txt"""; // FN
    }

    public void SpanMatch(Span<char> span, ReadOnlySpan<char> readonlySpan, string simpleString)
    {
        var a = span is """\\my-network-drive\folder\file.txt"""               // FN
                || readonlySpan is """\\my-network-drive\folder\file.txt"""    // FN
                || simpleString is @"\\my-network-drive/folder/file.txt";      // FN

        var URI = span is """\\my-network-drive\folder\file.txt"""             // Noncompliant
                || readonlySpan is """\\my-network-drive\folder\file.txt"""    // Noncompliant
                || simpleString is @"\\my-network-drive/folder/file.txt";      // Noncompliant
    }

    public bool ListPattern(string[] uris) =>
        uris is ["""\\my-network-drive\folder\file.txt"""]; // FN
}
