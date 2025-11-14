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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SymbolReferenceAnalyzer : SymbolReferenceAnalyzerBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

    protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
        token.GetBindableParent();

    protected override ReferenceInfo[] CreateDeclarationReferenceInfo(SyntaxNode node, SemanticModel model) =>
        node switch
        {
            BaseTypeDeclarationSyntax typeDeclaration => [CreateDeclarationReferenceInfo(node, typeDeclaration.Identifier, model)],
            VariableDeclarationSyntax variableDeclaration => CreateDeclarationReferenceInfo(variableDeclaration, model),
            MethodDeclarationSyntax methodDeclaration => [CreateDeclarationReferenceInfo(node, methodDeclaration.Identifier, model)],
            ParameterSyntax parameterSyntax => [CreateDeclarationReferenceInfo(node, parameterSyntax.Identifier, model)],
            LocalDeclarationStatementSyntax localDeclarationStatement => CreateDeclarationReferenceInfo(localDeclarationStatement.Declaration, model),
            PropertyDeclarationSyntax propertyDeclaration => [CreateDeclarationReferenceInfo(node, propertyDeclaration.Identifier, model)],
            TypeParameterSyntax typeParameterSyntax => [CreateDeclarationReferenceInfo(node, typeParameterSyntax.Identifier, model)],
            var localFunction when LocalFunctionStatementSyntaxWrapper.IsInstance(localFunction) =>
                [CreateDeclarationReferenceInfo(node, ((LocalFunctionStatementSyntaxWrapper)localFunction).Identifier, model)],
            var singleVariableDesignation when SingleVariableDesignationSyntaxWrapper.IsInstance(singleVariableDesignation) =>
                [CreateDeclarationReferenceInfo(node, ((SingleVariableDesignationSyntaxWrapper)singleVariableDesignation).Identifier, model)],
            _ => null
        };

    protected override IList<SyntaxNode> GetDeclarations(SyntaxNode node)
    {
        var walker = new DeclarationsFinder();
        walker.SafeVisit(node);
        return walker.Declarations;
    }

    private static ReferenceInfo[] CreateDeclarationReferenceInfo(VariableDeclarationSyntax declaration, SemanticModel model) =>
        declaration.Variables.Select(x => CreateDeclarationReferenceInfo(x, x.Identifier, model)).ToArray();

    private static ReferenceInfo CreateDeclarationReferenceInfo(SyntaxNode node, SyntaxToken identifier, SemanticModel model) =>
        new(node, identifier, model.GetDeclaredSymbol(node), true);

    private sealed class DeclarationsFinder : SafeCSharpSyntaxWalker
    {
        public readonly List<SyntaxNode> Declarations = [];

        private readonly ISet<ushort> declarationKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.LocalDeclarationStatement,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.Parameter,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.TypeParameter,
            SyntaxKind.VariableDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKindEx.SingleVariableDesignation
        }.Cast<ushort>().ToHashSet();

        public override void Visit(SyntaxNode node)
        {
            if (declarationKinds.Contains((ushort)node.RawKind))
            {
                Declarations.Add(node);
            }
            base.Visit(node);
        }
    }
}
