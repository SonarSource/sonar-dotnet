using System;
using System.IO;
using System.Collections.Generic;

class PrimaryConstructor(
    string ctorParamUri = "file://blah.txt", // Noncompliant
    string ctorParam = "file://blah.txt") // Compliant
{
    void Method(
        string methodParamUri = "file://blah.txt", // Noncompliant
        string methodParam = "file://blah.txt") // Compliant
    {
        var lambda = (string lambdaParamUri = "file://blah.txt") => lambdaParamUri; // Noncompliant
        var lambda2 = (string lambdaParam = "file://blah.txt") => lambdaParam; // Compliant

        var lambdaMultipleNoncompliances = (string paramUri1 = "file://blah.txt", string paramUri2 = "file://blah.txt") => paramUri1;
        //                                                     ^^^^^^^^^^^^^^^^^
        //                                                                                           ^^^^^^^^^^^^^^^^^@-1
    }
}

struct PrimaryConstructorStruct(
    string ctorParamUri = "file://blah.txt", // Noncompliant
    string ctorParam = "file://blah.txt") // Compliant
{
}

record class PrimaryConstructorRecordClass(
    string ctorParamUri = "file://blah.txt", // Noncompliant
    string ctorParam = "file://blah.txt") // Compliant
{
}

record struct PrimaryConstructorRecordStruct(
    string ctorParamUri = "file://blah.txt", // Noncompliant
    string ctorParam = "file://blah.txt") // Compliant
{
}

// https://github.com/SonarSource/sonar-dotnet/pull/8146
class Repro_8146
{
    void Method()
    {
        IList<string> uris1 = new[] { "C:/test.txt" }; // Noncompliant
        IList<string> uris2 = ["C:/test.txt"]; // Noncompliant
        IList<string> uris3 = new List<string> { "file://blah.txt" }; // Noncompliant
        var uris4 = new string[1] { "C:/test.txt" }; // Noncompliant
        var uris5 = new[] { "C:/test.txt" }; // Noncompliant
        string[] uris6 = ["C:/test.txt"]; // Noncompliant
        string[][] urisMatrix1 = [["C:/test.txt"]]; // Noncompliant
        IDictionary<string, string> urisDict = new Dictionary<string, string> { ["a"] = "C:/test.txt" }; // FN
        IDictionary<string, string> urisDict2 = new Dictionary<string, string> { ["urisDict2"] = "C:/test.txt" }; // FN

        IList<string> paths = new[] { "c:\\blah.txt" }; // Noncompliant
        IList<string> files = new[] { "file://blah.txt" }; // Noncompliant
        IList<string> urls = new[] { "http://www.mywebsite.com" }; // Noncompliant
        IList<string> urns = new[] { "http://bar.html" }; // Noncompliant
    }
}

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

public class CSharp13
{
    public void EschapeChar()
    {
        var fileLiteral = "file://blah\e.txt"; // Noncompliant {{Refactor your code not to use hardcoded absolute paths or URIs.}}
//                        ^^^^^^^^^^^^^^^^^^^

        var webUri1 = "http://www.mywebsite.com\e"; // Noncompliant
        var windowsDrivePath1 = "c:\\blah\\blah\\bla\eh.txt"; // Noncompliant
        var windowsSharedDrivePath1 = """\\my-n\eetwork-drive\folder\file.txt"""; // Noncompliant
        var x = new Uri("C:/t\eest.txt"); // Noncompliant

        var unixPath1 = "/my/other/fold\eer"; // Compliant - we ignore unix paths by default
        var unixPath2 = "~/blah/blah/blah\e.txt"; // Compliant
        var unixPath3 = "~\\blah\\blah\\blah.\etxt"; // Compliant
        var windowsPathStartingWithVariable = "%AppData%\\Adobe\e";
    }
}
