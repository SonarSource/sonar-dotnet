using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Testcases
{
    void Simple()
    {
        var str = "hello";

        str.StartsWith("x"); // Noncompliant {{"StartsWith" overloads that take a "char" should be used}}
        //  ^^^^^^^^^^
        str.EndsWith("x"); // Noncompliant {{"EndsWith" overloads that take a "char" should be used}}
        //  ^^^^^^^^

        str.StartsWith('x'); // Compliant
        str.EndsWith('x'); // Compliant

        str.StartsWith("xx"); // Compliant (length > 1)
        str.EndsWith("xx"); // Compliant (length > 1)

        var hString = "x";
        str.StartsWith(hString); // Compliant, only raise on literals
        str.StartsWith(hString); // Compliant, only raise on literals

        var hChar = 'x';
        str.StartsWith(hChar); // Compliant
        str.EndsWith(hChar); // Compliant

        const string hConst = "x";
        str.StartsWith(hConst); // Compliant
        str.EndsWith(hConst); // Compliant

        str.StartsWith(hConst.ToLower()); // Compliant
        str.StartsWith(hConst.ToString()); // Compliant

        str.StartsWith(hString, StringComparison.CurrentCulture); // Compliant
        str.EndsWith(hConst, StringComparison.CurrentCulture); // Compliant 
        str.StartsWith("x", true, CultureInfo.CurrentCulture); // Compliant
        str.EndsWith("x", true, CultureInfo.CurrentCulture); // Compliant

        const int five = 5;
        str.StartsWith(five.ToString()); // Compliant
        str.EndsWith(five.ToString()); // Compliant

        var list = new List<string> { "hey" };
        list.First().StartsWith("x"); // Noncompliant
        list.Any(x => x.EndsWith("x")); // Noncompliant
    }

    void Edgecases()
    {
        new List<int> { 42 }.Select(x => x.ToString()).Last().Trim().EndsWith("x"); // Noncompliant
        (true ? "hey" : "hello").StartsWith("x"); // Noncompliant
        ("hey" ?? "hello").EndsWith("x"); // Noncompliant
        (true ? "hey" : ("hello" ?? "heya")).StartsWith("x"); // Noncompliant
        (true ? "hey" : ("hello" ?? "heya")).StartsWith("x", StringComparison.CurrentCulture); // Compliant
    }

    void Chaining()
    {
        var str = "hello";

        GetString().StartsWith("x"); // Noncompliant
        GetString().EndsWith("x", StringComparison.InvariantCultureIgnoreCase); // Compliant

        MutateString(MutateString(MutateString(GetString()))).StartsWith("x"); // Noncompliant
        MutateString(MutateString(MutateString(GetString()))).EndsWith("x", true, CultureInfo.CurrentCulture); // Compliant

        str.Trim().PadLeft(42).PadRight(42).ToLower().StartsWith("x"); // Noncompliant
        str.Trim().PadLeft(42).PadRight(42).ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim().PadLeft(42).PadRight(42)?.ToLower().StartsWith("x"); // Noncompliant
        str.Trim().PadLeft(42).PadRight(42)?.ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42).ToLower().StartsWith("x"); // Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42).ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42)?.ToLower().StartsWith("x"); // Noncompliant
        str.Trim().PadLeft(42)?.PadRight(42)?.ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42).ToLower().StartsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42).ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42)?.ToLower().StartsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42).PadRight(42)?.ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42).ToLower().StartsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42).ToLower()?.EndsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42)?.ToLower().StartsWith("x"); // Noncompliant
        str.Trim()?.PadLeft(42)?.PadRight(42)?.ToLower()?.EndsWith("x"); // Noncompliant
        //                                                ^^^^^^^^
    }

    static string GetString() => "42";
    static string MutateString(string str) => "42";

    void NotCalledOnString()
    {
        var fake = new FakeString();
        fake.StartsWith("x"); // Compliant
        fake.EndsWith("x"); // Compliant
        fake.StartsWith('x'); // Compliant
        fake.EndsWith('x'); // Compliant

        fake.StartsWith(); // Compliant
        fake.EndsWith(); // Compliant
    }

    class FakeString
    {
        public bool StartsWith(string value) => true;
        public bool EndsWith(string value) => false;

        public bool StartsWith(char value) => true;
        public bool EndsWith(char value) => false;

        public bool StartsWith() => true;
        public bool EndsWith() => false;
    }
}
