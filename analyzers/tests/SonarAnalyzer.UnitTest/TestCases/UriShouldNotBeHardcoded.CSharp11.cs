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
    }

    public void SpanMatch(Span<char> span, ReadOnlySpan<char> readonlySpan, string simpleString)
    {
        var URI = span is """\\my-network-drive\folder\file.txt"""             // Noncompliant
                || readonlySpan is """\\my-network-drive\folder\file.txt"""    // Noncompliant
                || simpleString is @"\\my-network-drive/folder/file.txt";      // Noncompliant
    }

    public void ListPattern(string[] uris)
    {
        bool pathFlag = uris is ["""\\my-network-drive\folder\file.txt"""]; // Noncompliant
    }

    public void Utf8()
    {
        var utf8uri = "file://blah.txt"u8; // Noncompliant
        var utf8 = "file://blah.txt"u8;
        var utf8uriRaw = """file://blah.txt"""u8; // Noncompliant
        var utf8Raw = """file://blah.txt"""u8;
    }
}
