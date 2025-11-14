/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class FunctionName : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1542";
    private const string MessageFormat = "Rename {0} '{1}' to match the regular expression: '{2}'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, false);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    [RuleParameter("format", PropertyType.String, "Regular expression used to check the function names against.", NamingPatterns.PascalCasingPattern)]
    public string Pattern { get; set; } = NamingPatterns.PascalCasingPattern;

    protected override void Initialize(SonarParametrizedAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var methodDeclaration = (MethodStatementSyntax)c.Node;
                if (ShouldBeChecked(methodDeclaration, c.ContainingSymbol)
                    && !NamingPatterns.IsRegexMatch(methodDeclaration.Identifier.ValueText, Pattern))
                {
                    c.ReportIssue(Rule, methodDeclaration.Identifier, "function", methodDeclaration.Identifier.ValueText, Pattern);
                }
            },
            SyntaxKind.FunctionStatement);

        context.RegisterNodeAction(
            c =>
            {
                var methodDeclaration = (MethodStatementSyntax)c.Node;
                if (ShouldBeChecked(methodDeclaration, c.ContainingSymbol)
                    && !NamingPatterns.IsRegexMatch(methodDeclaration.Identifier.ValueText, Pattern)
                    && !EventHandlerName.IsEventHandler(methodDeclaration, c.Model))
                {
                    c.ReportIssue(Rule, methodDeclaration.Identifier, "procedure", methodDeclaration.Identifier.ValueText, Pattern);
                }
            },
            SyntaxKind.SubStatement);

        static bool ShouldBeChecked(MethodStatementSyntax methodStatement, ISymbol declaredSymbol) =>
            !declaredSymbol.IsOverride
            && !IsExternImport(declaredSymbol)
            && !ImplementsSingleMethodWithoutOverride(methodStatement, declaredSymbol);

        static bool IsExternImport(ISymbol methodSymbol) =>
            methodSymbol.IsExtern && methodSymbol.IsStatic && methodSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_DllImportAttribute);

        static bool ImplementsSingleMethodWithoutOverride(MethodStatementSyntax methodStatement, ISymbol methodSymbol) =>
            methodStatement.ImplementsClause is { } implementsClause
            && implementsClause.InterfaceMembers.Count == 1
            && methodSymbol.InterfaceMembers().FirstOrDefault() is { } interfaceMember
            && string.Equals(interfaceMember.Name, methodStatement.Identifier.ValueText, StringComparison.Ordinal);
    }
}
