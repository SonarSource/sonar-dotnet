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

        private static readonly SyntaxKind[] TypesWithPrimaryConstructorDeclarations =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKindEx.RecordStructDeclaration
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c => Process(c, GetDeclarationOrDesignation(c.Node)),
                SyntaxKind.LocalDeclarationStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.UsingStatement,
                SyntaxKind.FixedStatement,
                SyntaxKindEx.DeclarationExpression,
                SyntaxKindEx.RecursivePattern,
                SyntaxKindEx.VarPattern,
                SyntaxKindEx.DeclarationPattern,
                SyntaxKindEx.ListPattern);
            context.RegisterNodeAction(c => Process(c, c.Node),
                SyntaxKind.ForEachStatement);
        }

        private static SyntaxNode GetDeclarationOrDesignation(SyntaxNode node) =>
            node switch
            {
                LocalDeclarationStatementSyntax localDeclaration => localDeclaration.Declaration,
                ForStatementSyntax forStatement => forStatement.Declaration,
                UsingStatementSyntax usingStatement => usingStatement.Declaration,
                FixedStatementSyntax fixedStatement => fixedStatement.Declaration,
                _ when DeclarationExpressionSyntaxWrapper.IsInstance(node) => ((DeclarationExpressionSyntaxWrapper)node).Designation,
                _ when RecursivePatternSyntaxWrapper.IsInstance(node) => ((RecursivePatternSyntaxWrapper)node).Designation,
                _ when VarPatternSyntaxWrapper.IsInstance(node) => ((VarPatternSyntaxWrapper)node).Designation,
                _ when DeclarationPatternSyntaxWrapper.IsInstance(node) => ((DeclarationPatternSyntaxWrapper)node).Designation,
                _ when ListPatternSyntaxWrapper.IsInstance(node) => ((ListPatternSyntaxWrapper)node).Designation,
                _ => null
            };

        private static void Process(SonarSyntaxNodeReportingContext context, SyntaxNode node)
        {
            if (ExtractIdentifiers(node) is { Count: > 0 } identifiers
                && GetContextSymbols(context) is var members)
            {
                foreach (var identifier in identifiers)
                {
                    ReportOnVariableMatchingField(context, members, identifier);
                }
            }
        }

        private static List<SyntaxToken> ExtractIdentifiers(SyntaxNode node) =>
            node switch
            {
                VariableDeclarationSyntax variableDeclaration => variableDeclaration.Variables.Select(x => x.Identifier).ToList(),
                ForEachStatementSyntax foreachStatement => new() { foreachStatement.Identifier },
                _ when VariableDesignationSyntaxWrapper.IsInstance(node) => ((VariableDesignationSyntaxWrapper)node).AllVariables().Select(x => x.Identifier).ToList(),
                _ => new()
            };

        private static List<ISymbol> GetContextSymbols(SonarSyntaxNodeReportingContext context)
        {
            var symbols = context.ContainingSymbol.ContainingSymbol.GetSymbolType().GetMembers();
            var primaryCtor = symbols.FirstOrDefault(x => x is IMethodSymbol && IsPrimaryCtor(x));
            var primaryCtorParameters = primaryCtor?.GetParameters();
            var relevantSymbols = new List<ISymbol>();
            foreach (var relevant in symbols.Where(x => x is IPropertySymbol or IFieldSymbol))
            {
                if (primaryCtorParameters is not null
                    && primaryCtorParameters.Any()
                    && TryGetAssignedValue(relevant, out var assignedValue)
                    && primaryCtorParameters.Any(x => x.Name == assignedValue))
                {
                    primaryCtorParameters = primaryCtorParameters.Where(x => x.Name != assignedValue).ToImmutableArray();
                }
                else
                {
                    relevantSymbols.Add(relevant);
                }
            }
            return primaryCtor is null ? relevantSymbols : relevantSymbols.Concat(primaryCtorParameters).ToList();
        }

        private static bool TryGetAssignedValue(ISymbol symbol, out string assignedValue)
        {
            assignedValue = string.Empty;
            if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is { } syntax)
            {
                if (syntax is VariableDeclaratorSyntax variableDeclarator && variableDeclarator.Initializer is { Value: IdentifierNameSyntax { } fieldValue })
                {
                    assignedValue = fieldValue.Identifier.ValueText;
                    return true;
                }
                else if (syntax is PropertyDeclarationSyntax propertyDeclaration && propertyDeclaration.Initializer is { Value: IdentifierNameSyntax { } propertyValue })
                {
                    assignedValue = propertyValue.Identifier.ValueText;
                    return true;
                }
            }
            return false;
        }

        private static bool IsPrimaryCtor(ISymbol methodSymbol) =>
            methodSymbol.IsConstructor()
            && methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is { } syntax
            && TypesWithPrimaryConstructorDeclarations.Contains(syntax.Kind());

        private static void ReportOnVariableMatchingField(SonarSyntaxNodeReportingContext context, IEnumerable<ISymbol> members, SyntaxToken identifier)
        {
            if (members.FirstOrDefault(x => x.Name == identifier.ValueText) is { } matchingMember)
            {
                context.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.Text, GetSymbolName(matchingMember)));
            }
        }

        private static string GetSymbolName(ISymbol symbol) =>
            symbol switch
            {
                IFieldSymbol => "field",
                IPropertySymbol => "property",
                IParameterSymbol => "primary constructor parameter",
                _ => string.Empty
            };
    }
}
