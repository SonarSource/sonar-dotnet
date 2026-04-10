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
    public static ComparisonKind Mirror(this ComparisonKind comparison) =>
        comparison switch
        {
            ComparisonKind.GreaterThan => ComparisonKind.LessThan,
            ComparisonKind.GreaterThanOrEqual => ComparisonKind.LessThanOrEqual,
            ComparisonKind.LessThan => ComparisonKind.GreaterThan,
            ComparisonKind.LessThanOrEqual => ComparisonKind.GreaterThanOrEqual,
            _ => comparison,
        };

    public static string ToDisplayString(this ComparisonKind kind, AnalyzerLanguage language)
    {
        if (language != AnalyzerLanguage.CSharp && language != AnalyzerLanguage.VisualBasic)
        {
            throw new NotSupportedException($"Language {language} is not supported.");
        }
        return kind switch
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
}
