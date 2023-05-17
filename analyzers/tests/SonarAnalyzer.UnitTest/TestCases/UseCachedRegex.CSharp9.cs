using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class UseCachedRegex
{
    const string IMMUTABLE_REGEX_PATTERN = "^[a-zA-Z]$";
    readonly string READONLY_REGEX_PATTERN = "^[a-zA-Z]$";

    Regex MutableCachedRegex = new Regex("^[a-zA-Z]$"); // ???
    static Regex StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"); // ???
    string mutableRegexPattern = "^[a-zA-Z]$";

    void Compliant(string input)
    {
        Regex myRegex = new ($"^.+{input}.+$"); // Compliant
        myRegex = new ($"^.+" + input + ".+$"); // Compliant
        myRegex = new(mutableRegexPattern); // Compliant
    }

    void Noncompliant()
    {
        Regex myRegex = new ("^[a-zA-Z]$"); // Noncompliant
        //            ^^^^^^^^^^^^^^^^^^^^^^^

        myRegex = new ("^[a-zA-Z]$"); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^

        myRegex = new (IMMUTABLE_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex = new (READONLY_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        MutableCachedRegex = new ("^[a-zA-Z]$"); // ??? (Update mutable cached regex)
        StaticMutableCachedRegex = new ("^[a-zA-Z]$"); // ??? (Update mutable cached regex)

        UseRegex(new ("^[a-zA-Z]$")); // Noncompliant
        //       ^^^^^^^^^^^^^^^^^^^^^^^

        void UseRegex(Regex regex) { }
    }
}

public partial class UseCachedRegexFP
{
#if NET7_0_OR_GREATER
    [GeneratedRegex("^[a-zA-Z]$")]
    private static partial Regex CreateCachedRegex();
#else
    private static Regex CreateCachedRegex() => new Regex("^[a-zA-Z]$", RegexOptions.Compiled); // FP
#endif
}
