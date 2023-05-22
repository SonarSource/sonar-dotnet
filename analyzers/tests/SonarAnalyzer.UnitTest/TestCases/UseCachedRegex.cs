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

    private Regex backingRegex;

    public Regex PropertyCachedRegexWithBackingField
    {
        get
        {
            if (this.backingRegex == null)
            {
                this.backingRegex = new Regex("^[a-zA-Z]$"); // Compliant
            }

            return this.backingRegex;
        }
    }

    public Regex PropertyCachedRegexWithBackingFieldCoalesce =>
        this.backingRegex ?? (this.backingRegex = new Regex("^[a-zA-Z]$")); // Compliant

    // See https://github.com/dotnet/csharplang/issues/140#issuecomment-284895933
    // Not sure if this compile in C# 12
    // public Regex CSharp12SemiAutoPropertyCachedRegex
    // {
    //     get
    //     {
    //         if (field == null)
    //         {
    //             field = new Regex("^[a-zA-Z]$"); // Compliant
    //         }
    //         return field;
    //     }
    // }

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
        PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Compliant
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

        PropertyCachedRegex = PropertyCachedRegex == null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Compliant
        PropertyCachedRegex = PropertyCachedRegex != null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        PropertyCachedRegex = null == PropertyCachedRegex ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Compliant
        PropertyCachedRegex = null != PropertyCachedRegex ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        PropertyCachedRegex = PropertyCachedRegex is null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Compliant

        MutableCachedRegex = MutableCachedRegex == null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Compliant
        MutableCachedRegex = MutableCachedRegex != null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex = null == MutableCachedRegex ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Compliant
        MutableCachedRegex = null == MutableCachedRegex ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex = MutableCachedRegex is null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Compliant

        StaticMutableCachedRegex = StaticMutableCachedRegex == null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Compliant
        StaticMutableCachedRegex = StaticMutableCachedRegex != null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex = null == StaticMutableCachedRegex ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Compliant
        StaticMutableCachedRegex = null == StaticMutableCachedRegex ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex = StaticMutableCachedRegex is null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Compliant

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

        UseRegex(MutableCachedRegex ?? (MutableCachedRegex = new Regex("^[a-zA-Z]$"))); // Compliant
        UseRegex(StaticMutableCachedRegex ?? (StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"))); // Compliant
        UseRegex(PropertyCachedRegex ?? (PropertyCachedRegex = new Regex("^[a-zA-Z]$"))); // Compliant
    }

    void Noncompliant()
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

        MutableCachedRegex = myRegex == null ? new Regex("^[a-zA-Z]$") : myRegex; // Noncompliant
        myRegex = true ? new Regex("^[a-zA-Z]$") : false ? new Regex("^[0-9]$") : new Regex("^[a-zA-Z0-9]$"); // Noncompliant
        MutableCachedRegex = MutableCachedRegex != null ? MutableCachedRegex : false ? new Regex("^[a-zA-Z]$") : new Regex("^[0-9]$"); // Noncompliant, FP
        MutableCachedRegex = new Regex("^[a-zA-Z]$") ?? MutableCachedRegex; // Noncompliant

        PropertyCachedRegex = PropertyCachedRegex != null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Noncompliant
        PropertyCachedRegex = PropertyCachedRegex == null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = null != PropertyCachedRegex ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Noncompliant
        PropertyCachedRegex = null == PropertyCachedRegex ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = PropertyCachedRegex is null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant

        MutableCachedRegex = MutableCachedRegex == null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant
        MutableCachedRegex = MutableCachedRegex != null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        MutableCachedRegex = null == MutableCachedRegex ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant
        MutableCachedRegex = null == MutableCachedRegex ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        MutableCachedRegex = MutableCachedRegex is null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant

        StaticMutableCachedRegex = StaticMutableCachedRegex == null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant
        StaticMutableCachedRegex = StaticMutableCachedRegex != null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex = null == StaticMutableCachedRegex ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant
        StaticMutableCachedRegex = null == StaticMutableCachedRegex ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex = StaticMutableCachedRegex is null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant


        if (myRegex == null)
        {
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
            //                   ^^^^^^^^^^^^^^^^^^^^^^^
        }

        if (MutableCachedRegex == null)
        {
            myRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
            //        ^^^^^^^^^^^^^^^^^^^^^^^
        }

        if (null == myRegex)
        {
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
            //                   ^^^^^^^^^^^^^^^^^^^^^^^
        }

        if (null == MutableCachedRegex)
        {
            myRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
            //        ^^^^^^^^^^^^^^^^^^^^^^^
        }

        if (myRegex is null)
        {
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
            //                   ^^^^^^^^^^^^^^^^^^^^^^^
        }

        if (MutableCachedRegex is null)
        {
            myRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
            //        ^^^^^^^^^^^^^^^^^^^^^^^
        }

        if (myRegex is null)
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        //                       ^^^^^^^^^^^^^^^^^^^^^^^

        if (MutableCachedRegex is null)
            myRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        //            ^^^^^^^^^^^^^^^^^^^^^^^

        MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant

        UseRegex(MutableCachedRegex ?? new Regex("^[a-zA-Z]$")); // Noncompliant
        //                             ^^^^^^^^^^^^^^^^^^^^^^^
        UseRegex(StaticMutableCachedRegex ?? new Regex("^[a-zA-Z]$")); // Noncompliant
        //                                   ^^^^^^^^^^^^^^^^^^^^^^^
        UseRegex(PropertyCachedRegex ?? new Regex("^[a-zA-Z]$")); // Noncompliant
        //                              ^^^^^^^^^^^^^^^^^^^^^^^

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
