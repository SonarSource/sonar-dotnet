/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public static void ReportIssue<TContext>(this TContext context, DiagnosticDescriptor rule, SyntaxNode locationSyntax, params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, locationSyntax, messageArgs);

    public static void ReportIssue<TContext>(this TContext context, DiagnosticDescriptor rule, SyntaxToken locationToken, params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, locationToken, messageArgs);

    public static void ReportIssue<TContext>(this TContext context, DiagnosticDescriptor rule, Location location, params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, location, messageArgs);
}
