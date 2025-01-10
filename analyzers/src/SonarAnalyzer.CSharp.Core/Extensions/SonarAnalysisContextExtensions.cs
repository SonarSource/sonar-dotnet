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

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class SonarAnalysisContextExtensions
{
    public static void RegisterNodeAction(this SonarAnalysisContext context, Action<SonarSyntaxNodeReportingContext> action, params SyntaxKind[] syntaxKinds) =>
        context.RegisterNodeAction(CSharpGeneratedCodeRecognizer.Instance, action, syntaxKinds);

    public static void RegisterNodeAction(this SonarParametrizedAnalysisContext context, Action<SonarSyntaxNodeReportingContext> action, params SyntaxKind[] syntaxKinds) =>
        context.RegisterNodeAction(CSharpGeneratedCodeRecognizer.Instance, action, syntaxKinds);

    public static void RegisterNodeAction(this SonarCompilationStartAnalysisContext context, Action<SonarSyntaxNodeReportingContext> action, params SyntaxKind[] syntaxKinds) =>
        context.RegisterNodeAction(CSharpGeneratedCodeRecognizer.Instance, action, syntaxKinds);

    public static void RegisterTreeAction(this SonarAnalysisContext context, Action<SonarSyntaxTreeReportingContext> action) =>
        context.RegisterTreeAction(CSharpGeneratedCodeRecognizer.Instance, action);

    public static void RegisterTreeAction(this SonarParametrizedAnalysisContext context, Action<SonarSyntaxTreeReportingContext> action) =>
        context.RegisterTreeAction(CSharpGeneratedCodeRecognizer.Instance, action);

    public static void RegisterSemanticModelAction(this SonarParametrizedAnalysisContext context, Action<SonarSemanticModelReportingContext> action) =>
        context.RegisterSemanticModelAction(CSharpGeneratedCodeRecognizer.Instance, action);

    public static void RegisterSemanticModelAction(this SonarAnalysisContext context, Action<SonarSemanticModelReportingContext> action) =>
        context.RegisterSemanticModelAction(CSharpGeneratedCodeRecognizer.Instance, action);

    public static void RegisterCodeBlockStartAction(this SonarAnalysisContext context, Action<SonarCodeBlockStartAnalysisContext<SyntaxKind>> action) =>
        context.RegisterCodeBlockStartAction(CSharpGeneratedCodeRecognizer.Instance, action);

    public static void ReportIssue<TContext>(this TContext context, DiagnosticDescriptor rule, SyntaxNode locationSyntax, params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, rule, locationSyntax, messageArgs);

    public static void ReportIssue<TContext>(this TContext context, DiagnosticDescriptor rule, SyntaxToken locationToken, params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, rule, locationToken, messageArgs);

    public static void ReportIssue<TContext>(this TContext context, DiagnosticDescriptor rule, Location location, params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, rule, location, messageArgs);

    public static void ReportIssue<TContext>(this TContext context,
                                             DiagnosticDescriptor rule,
                                             Location primaryLocation,
                                             IEnumerable<SecondaryLocation> secondaryLocations,
                                             params string[] messageArgs) where TContext : ICompilationReport =>
        context.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, rule, primaryLocation, secondaryLocations, messageArgs);
}
