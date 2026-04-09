/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Core.Extensions;

// Don't change the this parameter to (this IAnalysisContext context) because it would cause boxing
public static class ICompilationReportExtensions
{
    extension<TContext>(TContext context) where TContext : ICompilationReport
    {
        public void ReportIssue(DiagnosticDescriptor rule, SyntaxNode locationSyntax, params string[] messageArgs) =>
            context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, locationSyntax, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule, SyntaxToken locationToken, params string[] messageArgs) =>
            context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, locationToken, messageArgs);

        public void ReportIssue(DiagnosticDescriptor rule, Location location, params string[] messageArgs) =>
            context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, location, messageArgs);
    }
}
