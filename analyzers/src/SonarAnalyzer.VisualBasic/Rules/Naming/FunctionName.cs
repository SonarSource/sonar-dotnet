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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class FunctionName : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1542";
        private const string MessageFormat = "Rename {0} '{1}' to match the regular expression: '{2}'.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        [RuleParameter("format", PropertyType.String, "Regular expression used to check the function names against.", NamingHelper.PascalCasingPattern)]
        public string Pattern { get; set; } = NamingHelper.PascalCasingPattern;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodStatementSyntax)c.Node;
                    if (ShouldBeChecked(methodDeclaration, c.ContainingSymbol)
                        && !NamingHelper.IsRegexMatch(methodDeclaration.Identifier.ValueText, Pattern))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, methodDeclaration.Identifier.GetLocation(), "function", methodDeclaration.Identifier.ValueText, Pattern));
                    }
                },
                SyntaxKind.FunctionStatement);

            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodStatementSyntax)c.Node;
                    if (ShouldBeChecked(methodDeclaration, c.ContainingSymbol)
                        && !NamingHelper.IsRegexMatch(methodDeclaration.Identifier.ValueText, Pattern)
                        && !EventHandlerName.IsEventHandler(methodDeclaration, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, methodDeclaration.Identifier.GetLocation(), "procedure", methodDeclaration.Identifier.ValueText, Pattern));
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
                && methodSymbol.GetInterfaceMember() is { } interfaceMember
                && string.Equals(interfaceMember.Name, methodStatement.Identifier.ValueText, StringComparison.Ordinal);
        }
    }
}
