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

namespace SonarAnalyzer.Core.AnalysisContext;

// Don't change the this parameter to (this IAnalysisContext context) because it would cause boxing
public static class ICompilationReportExtensions
{
    extension<T>(T context) where T : ICompilationReport
    {
        public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer,
                                DiagnosticDescriptor rule,
                                SyntaxNode locationSyntax,
                                params string[] messageArgs) =>
            context.ReportIssue(generatedCodeRecognizer, rule, locationSyntax.GetLocation(), messageArgs);

        public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer,
                                DiagnosticDescriptor rule,
                                SyntaxToken locationToken,
                                params string[] messageArgs) =>
            context.ReportIssue(generatedCodeRecognizer, rule, locationToken.GetLocation(), messageArgs);

        public void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer,
                                DiagnosticDescriptor rule,
                                Location location,
                                params string[] messageArgs) =>
            context.ReportIssue(generatedCodeRecognizer, rule, location, [], messageArgs);
    }
}
