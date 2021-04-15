using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using CSharpVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion;
using VBVersion = Microsoft.CodeAnalysis.VisualBasic.LanguageVersion;

namespace SonarAnalyzer.Rules.Tests.Framework
{
    public sealed class LanguageVersionInfo
    {
        private const string NonSplittingDot = "·";

        public static readonly LanguageVersionInfo CSharp6 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(6, 0), new CSharpParseOptions(CSharpVersion.CSharp6));
        public static readonly LanguageVersionInfo CSharp7 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(7, 0), new CSharpParseOptions(CSharpVersion.CSharp7));
        public static readonly LanguageVersionInfo CSharp7_1 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(7, 1), new CSharpParseOptions(CSharpVersion.CSharp7_1));
        public static readonly LanguageVersionInfo CSharp7_2 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(7, 2), new CSharpParseOptions(CSharpVersion.CSharp7_2));
        public static readonly LanguageVersionInfo CSharp7_3 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(7, 3), new CSharpParseOptions(CSharpVersion.CSharp7_3));
        public static readonly LanguageVersionInfo CSharp8 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(8, 0), new CSharpParseOptions(CSharpVersion.CSharp8));
        public static readonly LanguageVersionInfo CSharp9 = new LanguageVersionInfo(LanguageNames.CSharp, new Version(9, 0), new CSharpParseOptions(CSharpVersion.CSharp9));

        public static readonly LanguageVersionInfo VisualBasic12 = new LanguageVersionInfo(LanguageNames.VisualBasic, new Version(12, 0), new VisualBasicParseOptions(VBVersion.VisualBasic12));
        public static readonly LanguageVersionInfo VisualBasic14 = new LanguageVersionInfo(LanguageNames.VisualBasic, new Version(14, 0), new VisualBasicParseOptions(VBVersion.VisualBasic14));
        public static readonly LanguageVersionInfo VisualBasic15 = new LanguageVersionInfo(LanguageNames.VisualBasic, new Version(15, 0), new VisualBasicParseOptions(VBVersion.VisualBasic15));
        public static readonly LanguageVersionInfo VisualBasic15_3 = new LanguageVersionInfo(LanguageNames.VisualBasic, new Version(15, 3), new VisualBasicParseOptions(VBVersion.VisualBasic15_3));
        public static readonly LanguageVersionInfo VisualBasic15_5 = new LanguageVersionInfo(LanguageNames.VisualBasic, new Version(15, 5), new VisualBasicParseOptions(VBVersion.VisualBasic15_5));

        public static IEnumerable<LanguageVersionInfo> CSharp(Func<LanguageVersionInfo, bool> predicate = null) =>
            new[] { CSharp6, CSharp7, CSharp7_1, CSharp7_2, CSharp7_3, CSharp8, CSharp9 }
            .Where(predicate ?? (language => true));
        public static IEnumerable<LanguageVersionInfo> VisualBasic(Func<LanguageVersionInfo, bool> predicate = null) =>
            new[] { VisualBasic12, VisualBasic14, VisualBasic15, VisualBasic15_3, VisualBasic15_5 }
            .Where(predicate ?? (language => true));

        private LanguageVersionInfo(string language, Version version, ParseOptions optionss)
        {
            Language = language;
            Version = version;
            Options = optionss;
        }

        public string Language { get; }
        public Version Version { get; }
        public ParseOptions Options { get; }

        public string DisplayName
        {
            get
            {
                var display = Language == LanguageNames.VisualBasic ? "VB" : Language;
                display += $" {Version.Major}";
                if (Version.Minor > 0)
                {
                    display += $"{NonSplittingDot}{Version.Minor}";
                }
                return display;
            }
        }

        public override string ToString() => $"{Language} {Version}";

        public static IEnumerable<LanguageVersionInfo> Select(LanguageVersions versions) =>
            versions switch
            {
                LanguageVersions.BeforeCSharp7 => CSharp(language => language.Version < CSharp7.Version),
                LanguageVersions.BeforeCSharp8 => CSharp(language => language.Version < CSharp8.Version),
                LanguageVersions.BeforeCSharp9 => CSharp(language => language.Version < CSharp9.Version),

                LanguageVersions.FromCSharp6 => CSharp(language => language.Version >= CSharp6.Version),
                LanguageVersions.FromCSharp7 => CSharp(language => language.Version >= CSharp7.Version),
                LanguageVersions.FromCSharp7_1 => CSharp(language => language.Version >= CSharp7_1.Version),
                LanguageVersions.FromCSharp7_2 => CSharp(language => language.Version >= CSharp7_2.Version),
                LanguageVersions.FromCSharp8 => CSharp(language => language.Version >= CSharp8.Version),

                LanguageVersions.CSharp9 or
                LanguageVersions.FromCSharp9 => CSharp(language => language.Version >= CSharp9.Version),

                LanguageVersions.FromVisualBasic12 => VisualBasic(language => language.Version >= VisualBasic12.Version),
                LanguageVersions.FromVisualBasic14 => VisualBasic(language => language.Version >= VisualBasic14.Version),
                LanguageVersions.FromVisualBasic15 => VisualBasic(language => language.Version >= VisualBasic15.Version),

                _ => null,
            };

        public static IEnumerable<LanguageVersionInfo> Parse(string str)
        {
            Enum.TryParse<LanguageVersions>(str?.Replace(".", "_").Trim(), out var versions);
            return Select(versions);
        }
    }
}
