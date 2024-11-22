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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TooManyParameters : TooManyParametersBase<SyntaxKind, ParameterListSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        private static readonly ImmutableDictionary<SyntaxKind, string> NodeToDeclarationName = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.ConstructorDeclaration, "Constructor" },
            { SyntaxKind.StructDeclaration, "Constructor" },
            { SyntaxKind.ClassDeclaration, "Constructor" },
            { SyntaxKind.MethodDeclaration, "Method" },
            { SyntaxKind.DelegateDeclaration, "Delegate" },
            { SyntaxKind.AnonymousMethodExpression, "Delegate" },
            { SyntaxKind.ParenthesizedLambdaExpression, "Lambda" },
            { SyntaxKind.SimpleLambdaExpression, "Lambda" },
            { SyntaxKindEx.LocalFunctionStatement, "Local function" }
        }.ToImmutableDictionary();

        protected override string UserFriendlyNameForNode(SyntaxNode node) =>
            NodeToDeclarationName[node.Kind()];

        protected override int CountParameters(ParameterListSyntax parameterList) =>
            parameterList.Parameters.Count;

        protected override bool CanBeChanged(SyntaxNode node, SemanticModel semanticModel) =>
            NodeToDeclarationName.ContainsKey(node.Kind()) && VerifyCanBeChangedBySymbol(node, semanticModel);

        protected override int BaseParameterCount(SyntaxNode node) =>
            node switch
            {
                ConstructorDeclarationSyntax ctorDeclaration => ctorDeclaration.Initializer?.ArgumentList?.Arguments.Count ?? 0,
                ClassDeclarationSyntax classDeclaration => RetrieveBasePrimaryConstructorArguments(classDeclaration),
                _ => 0,
            };

        protected override bool IsExtern(SyntaxNode node) =>
            node is BaseMethodDeclarationSyntax { } methodDeclaration && methodDeclaration.IsExtern();

        private static int RetrieveBasePrimaryConstructorArguments(ClassDeclarationSyntax node)
        {
            var type = node.BaseList?.Types.FirstOrDefault();
            return PrimaryConstructorBaseTypeSyntaxWrapper.IsInstance(type)
                ? ((PrimaryConstructorBaseTypeSyntaxWrapper)type).ArgumentList.Arguments.Count
                : 0;
        }
    }
}
