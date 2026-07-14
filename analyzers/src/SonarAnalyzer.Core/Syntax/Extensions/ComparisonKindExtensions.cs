/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class ComparisonKindExtensions
{
    extension(ComparisonKind comparison)
    {
        public ComparisonKind Mirror() =>
            comparison switch
            {
                ComparisonKind.GreaterThan => ComparisonKind.LessThan,
                ComparisonKind.GreaterThanOrEqual => ComparisonKind.LessThanOrEqual,
                ComparisonKind.LessThan => ComparisonKind.GreaterThan,
                ComparisonKind.LessThanOrEqual => ComparisonKind.GreaterThanOrEqual,
                _ => comparison,
            };

        public string ToDisplayString(AnalyzerLanguage language)
        {
            if (language != AnalyzerLanguage.CSharp && language != AnalyzerLanguage.VisualBasic)
            {
                throw new NotSupportedException($"Language {language} is not supported.");
            }
            return comparison switch
            {
                ComparisonKind.Equals when language == AnalyzerLanguage.CSharp => "==",
                ComparisonKind.Equals => "=",
                ComparisonKind.NotEquals when language == AnalyzerLanguage.CSharp => "!=",
                ComparisonKind.NotEquals => "<>",
                ComparisonKind.LessThan => "<",
                ComparisonKind.LessThanOrEqual => "<=",
                ComparisonKind.GreaterThan => ">",
                ComparisonKind.GreaterThanOrEqual => ">=",
                _ => throw new InvalidOperationException(),
            };
        }

        public CountComparisonResult Compare(int count) =>
            comparison switch
            {
                ComparisonKind.Equals => Equals(count),
                ComparisonKind.NotEquals => NotEquals(count),
                ComparisonKind.GreaterThanOrEqual => GreaterThanOrEqual(count),
                ComparisonKind.GreaterThan => GreaterThan(count),
                ComparisonKind.LessThan => LessThan(count),
                ComparisonKind.LessThanOrEqual => LessThanOrEqual(count),
                _ => CountComparisonResult.None,
            };
    }

    private static CountComparisonResult Equals(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysFalse, CountComparisonResult.Empty);

    private static CountComparisonResult NotEquals(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysTrue, CountComparisonResult.NotEmpty);

    private static CountComparisonResult GreaterThan(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysTrue, CountComparisonResult.NotEmpty);

    private static CountComparisonResult GreaterThanOrEqual(int count) =>
        Check(count, 1, CountComparisonResult.AlwaysTrue, CountComparisonResult.NotEmpty);

    private static CountComparisonResult LessThan(int count) =>
        Check(count, 1, CountComparisonResult.AlwaysFalse, CountComparisonResult.Empty);

    private static CountComparisonResult LessThanOrEqual(int count) =>
        Check(count, 0, CountComparisonResult.AlwaysFalse, CountComparisonResult.Empty);

    private static CountComparisonResult Check(int count, int threshold, CountComparisonResult belowThreshold, CountComparisonResult onThreshold)
    {
        if (count == threshold)
        {
            return onThreshold;
        }
        else
        {
            return count < threshold ? belowThreshold : CountComparisonResult.SizeDepedendent;
        }
    }
}
