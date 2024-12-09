/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.AnalysisContext;

public static class ICompilationReportExtensions
{
    public static void ReportIssue<T>(this T context, GeneratedCodeRecognizer generatedCodeRecognizer,
                            DiagnosticDescriptor rule,
                            SyntaxNode locationSyntax,
                            params string[] messageArgs) where T : ICompilationReport =>
        context.ReportIssue(generatedCodeRecognizer, rule, locationSyntax.GetLocation(), messageArgs);

    public static void ReportIssue<T>(this T context, GeneratedCodeRecognizer generatedCodeRecognizer,
                            DiagnosticDescriptor rule,
                            SyntaxToken locationToken,
                            params string[] messageArgs) where T : ICompilationReport =>
        context.ReportIssue(generatedCodeRecognizer, rule, locationToken.GetLocation(), messageArgs);

    public static void ReportIssue<T>(this T context, GeneratedCodeRecognizer generatedCodeRecognizer,
                            DiagnosticDescriptor rule,
                            Location location,
                            params string[] messageArgs) where T : ICompilationReport =>
        context.ReportIssue(generatedCodeRecognizer, rule, location, [], messageArgs);
}
