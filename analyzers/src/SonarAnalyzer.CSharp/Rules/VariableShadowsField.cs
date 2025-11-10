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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VariableShadowsField : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1117";
        private const string MessageFormat = "Rename '{0}' which hides the {1} with the same name.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c => Process(c, GetDeclarationOrDesignation(c.Node)),
                SyntaxKind.LocalDeclarationStatement,
                SyntaxKind.ForStatement,
                SyntaxKind.UsingStatement,
                SyntaxKind.FixedStatement,
                SyntaxKindEx.DeclarationExpression,
                SyntaxKindEx.RecursivePattern,
                SyntaxKindEx.VarPattern,
                SyntaxKindEx.DeclarationPattern,
                SyntaxKindEx.ListPattern,
                SyntaxKind.ForEachStatement);

        private static SyntaxNode GetDeclarationOrDesignation(SyntaxNode node) =>
            node switch
            {
                LocalDeclarationStatementSyntax localDeclaration => localDeclaration.Declaration,
                ForStatementSyntax forStatement => forStatement.Declaration,
                UsingStatementSyntax usingStatement => usingStatement.Declaration,
                FixedStatementSyntax fixedStatement => fixedStatement.Declaration,
                ForEachStatementSyntax forEachStatement => forEachStatement,
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
            var members = context.ContainingSymbol.ContainingType.GetMembers();
            var primaryConstructorParameters = members.FirstOrDefault(x => x.IsPrimaryConstructor())?.GetParameters();
            var fieldsAndProperties = members.Where(x => x is IPropertySymbol or IFieldSymbol).ToList();
            return primaryConstructorParameters is null ? fieldsAndProperties : fieldsAndProperties.Concat(primaryConstructorParameters).ToList();
        }

        private static void ReportOnVariableMatchingField(SonarSyntaxNodeReportingContext context, IEnumerable<ISymbol> members, SyntaxToken identifier)
        {
            if (members.FirstOrDefault(x => x.Name == identifier.ValueText
                && (x.IsStatic || !identifier.Parent.EnclosingScope().GetModifiers().Any(x => x.Kind() == SyntaxKind.StaticKeyword))) is { } matchingMember)
            {
                context.ReportIssue(Rule, identifier, identifier.Text, GetSymbolName(matchingMember));
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
