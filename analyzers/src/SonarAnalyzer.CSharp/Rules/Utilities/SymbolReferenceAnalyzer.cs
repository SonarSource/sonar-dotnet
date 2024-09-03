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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SymbolReferenceAnalyzer : SymbolReferenceAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

        protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
            token.GetBindableParent();

        protected override ReferenceInfo[] CreateDeclarationReferenceInfo(SyntaxNode node, SemanticModel model) =>
            node switch
            {
                BaseTypeDeclarationSyntax typeDeclaration => new[] { CreateDeclarationReferenceInfo(node, typeDeclaration.Identifier, model) },
                VariableDeclarationSyntax variableDeclaration => CreateDeclarationReferenceInfo(variableDeclaration, model),
                MethodDeclarationSyntax methodDeclaration => new[] { CreateDeclarationReferenceInfo(node, methodDeclaration.Identifier, model) },
                ParameterSyntax parameterSyntax => new[] { CreateDeclarationReferenceInfo(node, parameterSyntax.Identifier, model) },
                LocalDeclarationStatementSyntax localDeclarationStatement => CreateDeclarationReferenceInfo(localDeclarationStatement.Declaration, model),
                PropertyDeclarationSyntax propertyDeclaration => new[] { CreateDeclarationReferenceInfo(node, propertyDeclaration.Identifier, model) },
                TypeParameterSyntax typeParameterSyntax => new[] { CreateDeclarationReferenceInfo(node, typeParameterSyntax.Identifier, model) },
                var localFunction when LocalFunctionStatementSyntaxWrapper.IsInstance(localFunction) =>
                    new[] { CreateDeclarationReferenceInfo(node, ((LocalFunctionStatementSyntaxWrapper)localFunction).Identifier, model) },
                var singleVariableDesignation when SingleVariableDesignationSyntaxWrapper.IsInstance(singleVariableDesignation) =>
                    new[] { CreateDeclarationReferenceInfo(node, ((SingleVariableDesignationSyntaxWrapper)singleVariableDesignation).Identifier, model) },
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

            public readonly List<SyntaxNode> Declarations = new();

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
}
