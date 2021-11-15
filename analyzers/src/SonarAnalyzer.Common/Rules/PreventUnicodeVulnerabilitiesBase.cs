using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class PreventUnicodeVulnerabilitiesBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1940";
        internal const string MessageFormat = "Vulnerable character U+{0:x} detected";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    foreach (var point in c.Tree.GetText().Lines
                        .SelectMany(line => CodePoint.Parse(line.ToString(), line.Start))
                        .Where(point => IsVulnerability(point.Utf32)))
                    {
                        c.ReportIssue(
                            Diagnostic.Create(
                                SupportedDiagnostics[0],
                                c.Tree.GetLocation(point.TextSpan),
                                point.Utf32));
                    }
                });
        }

        internal static bool IsVulnerability(char ch)
            => IsSuspiciousCategory(char.GetUnicodeCategory(ch))
            && !InAllowedRange(ch);

        internal static bool IsVulnerability(int utf32)
            => IsSuspiciousCategory(utf32)
            && !InAllowedRange(utf32);

        private static bool IsSuspiciousCategory(int utf32)
        {
            var str = char.ConvertFromUtf32(utf32);
            return str.Any(ch => IsSuspiciousCategory(char.GetUnicodeCategory(ch)));
        }

        private static bool IsSuspiciousCategory(UnicodeCategory cat)
            => cat == UnicodeCategory.Control
            || cat == UnicodeCategory.Format
            || cat == UnicodeCategory.Surrogate
            || cat == UnicodeCategory.OtherNotAssigned;

        private static bool InAllowedRange(int utf32)
            => Arabic.Contains(utf32)
            || ArabicExtendedA.Contains(utf32)
            || ArabicExtendedB.Contains(utf32)
            || ArabicPresentationFormsA.Contains(utf32)
            || ArabicPresentationFormsB.Contains(utf32)
            || RumiNumeralSymbols.Contains(utf32)
            || IndicSiyaqNumbers.Contains(utf32)
            || OttomanSiyaqNumbers.Contains(utf32)
            || ArabicMathematicalAlphabeticSymbols.Contains(utf32)
            || CJKUnifiedIdeographs.Contains(utf32)
            || CJKUnifiedIdeographsExtensionA.Contains(utf32)
            || CJKUnifiedIdeographsExtensionB.Contains(utf32)
            || CJKUnifiedIdeographsExtensionC.Contains(utf32)
            || CJKUnifiedIdeographsExtensionD.Contains(utf32)
            || CJKUnifiedIdeographsExtensionE.Contains(utf32)
            || CJKUnifiedIdeographsExtensionF.Contains(utf32)
            || CyrillicExtendedC.Contains(utf32);

        private static readonly Range Arabic = new Range(0x00600, 0x006FF);
        private static readonly Range ArabicSupplement = new Range(0x00750, 0x0077F);
        private static readonly Range ArabicExtendedA = new Range(0x008A0, 0x008FF);
        private static readonly Range ArabicExtendedB = new Range(0x00870, 0x0089F);
        private static readonly Range ArabicPresentationFormsA = new Range(0x0FB50, 0x0FDFF);
        private static readonly Range ArabicPresentationFormsB = new Range(0x0FE70, 0x0FEFF);
        private static readonly Range RumiNumeralSymbols = new Range(0x10E60, 0x10E7F);
        private static readonly Range IndicSiyaqNumbers = new Range(0x1EC70, 0x1ECBF);
        private static readonly Range OttomanSiyaqNumbers = new Range(0x1ED00, 0x1ED4F);
        private static readonly Range ArabicMathematicalAlphabeticSymbols = new Range(0x1EE00, 0x1EEFF);
        private static readonly Range CJKUnifiedIdeographs = new Range(0x04E00, 0X09FEF);
        private static readonly Range CJKUnifiedIdeographsExtensionA = new Range(0x03400, 0X04DBF);
        private static readonly Range CJKUnifiedIdeographsExtensionB = new Range(0x20000, 0X2A6DF);
        private static readonly Range CJKUnifiedIdeographsExtensionC = new Range(0x2A700, 0X2B73F);
        private static readonly Range CJKUnifiedIdeographsExtensionD = new Range(0x2B740, 0X2B81F);
        private static readonly Range CJKUnifiedIdeographsExtensionE = new Range(0x2B820, 0X2CEAF);
        private static readonly Range CJKUnifiedIdeographsExtensionF = new Range(0x2CEB0, 0X2EBEF);
        private static readonly Range CyrillicExtendedC = new Range(0x01C80, 0x01C8F);

        private readonly struct Range
        {
            public Range(int start, int end)
            {
                Start = start;
                End = end;
            }
            public int Start { get; }
            public int End { get; }
            public bool Contains(int utf32) => utf32 >= Start && utf32 <= End;
        }

        private readonly struct CodePoint
        {
            public CodePoint(int utf32, int index)
            {
                Utf32 = utf32;
                Index = index;
            }
            public int Utf32 { get; }
            public int Index { get; }
            public TextSpan TextSpan => TextSpan.FromBounds(Index, Index + 1);

            public static IEnumerable<CodePoint> Parse(string str, int offset)
            {
                var codePoints = new List<CodePoint>(str.Length + 8);
                var index = 0;
                while (index < str.Length)
                {
                    var utf32 = char.ConvertToUtf32(str, index);
                    var size = char.IsHighSurrogate(str[index]) ? 2 : 1;
                    codePoints.Add(new CodePoint(utf32, index + offset));
                    offset -= size - 1;
                    index += size;
                }
                return codePoints;
            }
        }
    }
}
