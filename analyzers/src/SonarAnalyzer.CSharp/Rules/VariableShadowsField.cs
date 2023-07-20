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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VariableShadowsField : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1117";
        private const string MessageFormat = "Rename '{0}' which hides the {1} with the same name.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    var declaration = (ForEachStatementSyntax)c.Node;

                    var variableSymbol = c.SemanticModel.GetDeclaredSymbol(declaration);
                    if (variableSymbol == null)
                    {
                        return;
                    }

                    var members = GetMembers(variableSymbol.ContainingType);

                    ReportOnVariableMatchingField(c, members, declaration.Identifier);
                },
                SyntaxKind.ForEachStatement);

            context.RegisterNodeAction(c => ProcessVariableDeclaration(c, ((LocalDeclarationStatementSyntax)c.Node).Declaration), SyntaxKind.LocalDeclarationStatement);
            context.RegisterNodeAction(c => ProcessVariableDeclaration(c, ((ForStatementSyntax)c.Node).Declaration), SyntaxKind.ForStatement);
            context.RegisterNodeAction(c => ProcessVariableDeclaration(c, ((UsingStatementSyntax)c.Node).Declaration), SyntaxKind.UsingStatement);
            context.RegisterNodeAction(c => ProcessVariableDeclaration(c, ((FixedStatementSyntax)c.Node).Declaration), SyntaxKind.FixedStatement);

            context.RegisterNodeAction(c => ProcessVariableDesignation(c, ((DeclarationExpressionSyntaxWrapper)c.Node).Designation), SyntaxKindEx.DeclarationExpression);
            context.RegisterNodeAction(c => ProcessVariableDesignation(c, ((RecursivePatternSyntaxWrapper)c.Node).Designation), SyntaxKindEx.RecursivePattern);
            context.RegisterNodeAction(c => ProcessVariableDesignation(c, ((VarPatternSyntaxWrapper)c.Node).Designation), SyntaxKindEx.VarPattern);
            context.RegisterNodeAction(c => ProcessVariableDesignation(c, ((DeclarationPatternSyntaxWrapper)c.Node).Designation), SyntaxKindEx.DeclarationPattern);
            context.RegisterNodeAction(c => ProcessVariableDesignation(c, ((ListPatternSyntaxWrapper)c.Node).Designation), SyntaxKindEx.ListPattern);
        }

        private static void ProcessVariableDesignation(SonarSyntaxNodeReportingContext context, VariableDesignationSyntaxWrapper variableDesignation)
        {
            if (variableDesignation.AllVariables() is { Length: > 0 } variables
                && context.ContainingSymbol.ContainingType is { } containingType
                && GetMembers(containingType) is var members)
            {
                foreach (var variable in variables)
                {
                    ReportOnVariableMatchingField(context, members, variable.Identifier);
                }
            }
        }

        private static void ProcessVariableDeclaration(SonarSyntaxNodeReportingContext context, VariableDeclarationSyntax variableDeclaration)
        {
            if (variableDeclaration is { Variables: { Count: > 0 } variables }
                && context.ContainingSymbol.ContainingType is { } containingType
                && GetMembers(containingType) is var members)
            {
                foreach (var variable in variables)
                {
                    ReportOnVariableMatchingField(context, members, variable.Identifier);
                }
            }
        }

        private static void ReportOnVariableMatchingField(SonarSyntaxNodeReportingContext context, IEnumerable<ISymbol> members, SyntaxToken identifier)
        {
            if (members.FirstOrDefault(m => m.Name == identifier.ValueText) is { } matchingMember)
            {
                context.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation(), identifier.Text, matchingMember is IFieldSymbol ? "field" : "property"));
            }
        }

        private static List<ISymbol> GetMembers(INamespaceOrTypeSymbol classSymbol) =>
            classSymbol.GetMembers()
                       .Where(member => member is IFieldSymbol or IPropertySymbol)
                       .ToList();
    }
}
