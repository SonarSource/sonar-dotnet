/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NumberPatternShouldBeRegular : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3937";
        private const string MessageFormat = "Review this number; its irregular pattern indicates an error.";

        private const char Underscore = '_';
        private const char Dot = '.';
        private const int NotFound = -1;

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    if (!c.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp7))
                    {
                        return;
                    }

                    var literal = (LiteralExpressionSyntax)c.Node;

                    if (HasIrregularPattern(literal.Token.Text))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, literal.GetLocation()));
                    }
                },
                SyntaxKind.NumericLiteralExpression);
        }

        /// <remarks>internal for test purposes.</remarks>
        internal static bool HasIrregularPattern(string numericToken)
        {
            var split = StripNumericPreAndSuffix(numericToken).Split(Dot);

            // ignore multiple dots.
            if (split.Length > 2)
            {
                return false;
            }

            var groupLengthsLeftFromDot = split[0].Split(Underscore).Select(g => g.Length).ToArray();

            if (HasIrregularGroupLengths(groupLengthsLeftFromDot))
            {
                return true;
            }

            // no dot, so done.
            if (split.Length == 1)
            {
                return false;
            }

            // reverse, as for right from the dot, the last (instead of the first)
            // group length is allowed to be shorter than the group length.
            var groupLengthsRightFromDot = split[1].Split(Underscore).Select(g => g.Length).Reverse().ToArray();

            return HasIrregularGroupLengths(groupLengthsRightFromDot);
        }

        private static string StripNumericPreAndSuffix(string numericToken)
        {
            var length = numericToken.Length;

            // hexadecimal and binary prefixes (0xFFFF_23_AB, 0b1110_1101)
            if (numericToken.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ||
                numericToken.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
            {
                return numericToken.Substring(2, length - 2);
            }

            // Scientific notation (1.23E8)
            var exponentMarker = numericToken.IndexOf("E", StringComparison.InvariantCultureIgnoreCase);
            if (exponentMarker != NotFound)
            {
                return numericToken.Substring(0, exponentMarker);
            }

            // UL and LU suffix.
            if (numericToken.EndsWith("UL", StringComparison.OrdinalIgnoreCase) ||
                numericToken.EndsWith("LU", StringComparison.OrdinalIgnoreCase))
            {
                return numericToken.Substring(0, length - 2);
            }
            // single suffixes
            if ("LDFUMldfum".IndexOf(numericToken[numericToken.Length - 1]) != NotFound)
            {
                return numericToken.Substring(0, length - 1);
            }

            return numericToken;
        }

        private static bool HasIrregularGroupLengths(int[] groupLengths)
        {
            if (groupLengths.Length < 2)
            {
                return false;
            }

            // the first group is allowed to contain less digits that the other ones,
            // so take the expected length from the second group.
            var groupLength = groupLengths[1];

            // we consider groups of 1 digit irregular.
            // first should not be bigger.
            if (groupLength < 2 || groupLengths[0] > groupLength)
            {
                return true;
            }

            return groupLengths.Skip(1).Any(l => l != groupLength);
        }
    }
}
