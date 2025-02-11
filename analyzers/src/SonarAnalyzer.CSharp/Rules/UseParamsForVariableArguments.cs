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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UseParamsForVariableArguments : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4061";
        private const string MessageFormat = "Use the 'params' keyword instead of '__arglist'.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    if (c.Node.ParameterList() is { Parameters: { Count: > 0 and var count } parameters }
                        && parameters[count - 1].Identifier.IsKind(SyntaxKind.ArgListKeyword)
                        && CheckModifiers(c.Node)
                        && c.Node.GetIdentifier() is { IsMissing: false } identifier
                        && MethodSymbol(c.Node, c.Model) is { } methodSymbol
                        && !methodSymbol.IsOverride
                        && methodSymbol.IsPubliclyAccessible()
                        && methodSymbol.GetInterfaceMember() == null)
                    {
                        c.ReportIssue(Rule, identifier);
                    }
                },
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

        private static IMethodSymbol MethodSymbol(SyntaxNode node, SemanticModel semanticModel) =>
            node is TypeDeclarationSyntax type
                ? type.PrimaryConstructor(semanticModel)
                : semanticModel.GetDeclaredSymbol(node) as IMethodSymbol;

        private static bool CheckModifiers(SyntaxNode node) =>
            node is not BaseMethodDeclarationSyntax method
            || !method.Modifiers.Any(SyntaxKind.ExternKeyword);
    }
}
