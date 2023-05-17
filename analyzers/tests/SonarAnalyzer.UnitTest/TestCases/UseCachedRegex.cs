using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

public class UseCachedRegex
{
    const string IMMUTABLE_REGEX_PATTERN = "^[a-zA-Z]$";
    readonly string READONLY_REGEX_PATTERN = "^[a-zA-Z]$";

    readonly Regex CachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
    readonly Regex CachedRegexConstPattern = new Regex(IMMUTABLE_REGEX_PATTERN); // Compliant
    public Regex ReadOnlyPropertyCachedRegex { get; } = new Regex(IMMUTABLE_REGEX_PATTERN); // Compliant
    public Regex PropertyCachedRegex { get; set; } = new Regex(IMMUTABLE_REGEX_PATTERN); // Compliant

    readonly static Regex StaticCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant

    Regex MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
    static Regex StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
    string mutableRegexPattern = "^[a-zA-Z]$";

    private readonly List<Regex> NestedCachedRegex = new List<Regex>
    {
        new Regex("^[a-zA-Z]$") // Compliant
    };

    public UseCachedRegex()
    {
        var localRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        CachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
        ReadOnlyPropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
    }

    void UseRegex(Regex regex)
    {
        regex = new Regex("^[a-zA-Z]$"); // Noncompliant
        //      ^^^^^^^^^^^^^^^^^^^^^^^
        regex = regex ?? new Regex("^[a-zA-Z]$"); // Noncompliant
        //               ^^^^^^^^^^^^^^^^^^^^^^^
    }

    void Compliant(string input)
    {
        var myRegex = new Regex($"^.+{input}.+$"); // Compliant
        myRegex = new Regex($"^.+" + input + ".+$"); // Compliant
        myRegex = new Regex(mutableRegexPattern); // Compliant
        PropertyCachedRegex = PropertyCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex = MutableCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex = StaticMutableCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant

        if (MutableCachedRegex == null)
        {
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
        }

        if (MutableCachedRegex == null)
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant

        if (null == MutableCachedRegex)
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant

        if (MutableCachedRegex is null)
        {
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
        }

        if (StaticMutableCachedRegex == null)
        {
            StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
        }

        if (PropertyCachedRegex == null)
        {
            PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
        }

        UseRegex(MutableCachedRegex ?? new Regex("^[a-zA-Z]$")); // Compliant
        UseRegex(StaticMutableCachedRegex ?? new Regex("^[a-zA-Z]$")); // Compliant
        UseRegex(PropertyCachedRegex ?? new Regex("^[a-zA-Z]$")); // Compliant
    }

    void Noncompliant(string inputString)
    {
        var myRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        //            ^^^^^^^^^^^^^^^^^^^^^^^

        myRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^

        myRegex = new Regex(IMMUTABLE_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex = new Regex(READONLY_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex = myRegex ?? new Regex("^[a-zA-Z]$"); // Noncompliant
        //                   ^^^^^^^^^^^^^^^^^^^^^^^

        MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant

        UseRegex(new Regex("^[a-zA-Z]$")); // Noncompliant
        //       ^^^^^^^^^^^^^^^^^^^^^^^
    }
}

public class UseCachedRegexFP
{
    private static readonly Dictionary<RegexOptions, Dictionary<string, Regex>> regexCache =
        new Dictionary<RegexOptions, Dictionary<string, Regex>>();

    // Taken from https://github.com/PowerShell/PowerShell/blob/ef0af95/src/System.Management.Automation/engine/lang/parserutils.cs#L1386-L1404
    internal static Regex NewRegex(string patternString, RegexOptions options)
    {
        if (regexCache.TryGetValue(options, out var subordinateRegexCache))
        {
            subordinateRegexCache = new Dictionary<string, Regex>(StringComparer.Ordinal);
            regexCache.Add(options, subordinateRegexCache);
        }

        if (subordinateRegexCache.TryGetValue(patternString, out Regex result))
        {
            return result;
        }
        else
        {
            var regex = new Regex(patternString, options); // FP?
            subordinateRegexCache.Add(patternString, regex);
            return regex;
        }
    }
}
