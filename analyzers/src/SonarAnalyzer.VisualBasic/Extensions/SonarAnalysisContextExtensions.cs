/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Extensions;

public static class SonarAnalysisContextExtensions
{
    public static void RegisterNodeAction(this SonarAnalysisContext context, Action<SonarSyntaxNodeReportingContext> action, params SyntaxKind[] syntaxKinds) =>
        context.RegisterNodeAction(VisualBasicGeneratedCodeRecognizer.Instance, action, syntaxKinds);

    public static void RegisterNodeAction(this SonarParametrizedAnalysisContext context, Action<SonarSyntaxNodeReportingContext> action, params SyntaxKind[] syntaxKinds) =>
        context.RegisterNodeAction(VisualBasicGeneratedCodeRecognizer.Instance, action, syntaxKinds);

    public static void RegisterNodeAction(this SonarCompilationStartAnalysisContext context, Action<SonarSyntaxNodeReportingContext> action, params SyntaxKind[] syntaxKinds) =>
        context.RegisterNodeAction(VisualBasicGeneratedCodeRecognizer.Instance, action, syntaxKinds);

    public static void RegisterTreeAction(this SonarAnalysisContext context, Action<SonarSyntaxTreeReportingContext> action) =>
        context.RegisterTreeAction(VisualBasicGeneratedCodeRecognizer.Instance, action);

    public static void RegisterTreeAction(this SonarParametrizedAnalysisContext context, Action<SonarSyntaxTreeReportingContext> action) =>
        context.RegisterTreeAction(VisualBasicGeneratedCodeRecognizer.Instance, action);

    public static void RegisterSemanticModelAction(this SonarAnalysisContext context, Action<SonarSemanticModelReportingContext> action) =>
        context.RegisterSemanticModelAction(VisualBasicGeneratedCodeRecognizer.Instance, action);

    public static void RegisterSemanticModelAction(this SonarParametrizedAnalysisContext context, Action<SonarSemanticModelReportingContext> action) =>
        context.RegisterSemanticModelAction(VisualBasicGeneratedCodeRecognizer.Instance, action);

    public static void RegisterCodeBlockStartAction(this SonarAnalysisContext context, Action<SonarCodeBlockStartAnalysisContext<SyntaxKind>> action) =>
        context.RegisterCodeBlockStartAction(VisualBasicGeneratedCodeRecognizer.Instance, action);

    public static void ReportIssue(this SonarCompilationReportingContext context, DiagnosticDescriptor rule, Location location, params string[] messageArgs) =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, location, messageArgs);

    public static void ReportIssue(this SonarSymbolReportingContext context, DiagnosticDescriptor rule, SyntaxNode locationSyntax, params string[] messageArgs) =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, locationSyntax, messageArgs);

    public static void ReportIssue(this SonarSymbolReportingContext context, DiagnosticDescriptor rule, SyntaxToken locationToken, params string[] messageArgs) =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, locationToken, messageArgs);

    public static void ReportIssue(this SonarSymbolReportingContext context, DiagnosticDescriptor rule, Location location, params string[] messageArgs) =>
        context.ReportIssue(VisualBasicGeneratedCodeRecognizer.Instance, rule, location, messageArgs);
}
