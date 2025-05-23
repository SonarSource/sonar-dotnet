﻿/*
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class SymbolReferenceAnalyzer : SymbolReferenceAnalyzerBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;

        protected override SyntaxNode GetBindableParent(SyntaxToken token) =>
            token.GetBindableParent();

        protected override ReferenceInfo[] CreateDeclarationReferenceInfo(SyntaxNode node, SemanticModel model) =>
            node switch
            {
                TypeStatementSyntax typeStatement => new[] { CreateDeclarationReferenceInfo(node, typeStatement.Identifier, model) },
                MethodStatementSyntax methodStatement => new[] { CreateDeclarationReferenceInfo(node, methodStatement.Identifier, model) },
                EventStatementSyntax eventStatement => new[] { CreateDeclarationReferenceInfo(node, eventStatement.Identifier, model) },
                FieldDeclarationSyntax fieldDeclaration => CreateDeclarationReferenceInfo(fieldDeclaration, model),
                VariableDeclaratorSyntax variableDeclarator => CreateDeclarationReferenceInfo(variableDeclarator, model),
                ParameterSyntax parameter => new[] { CreateDeclarationReferenceInfo(node, parameter.Identifier.Identifier, model) },
                LocalDeclarationStatementSyntax localDeclaration => CreateDeclarationReferenceInfo(localDeclaration, model),
                PropertyStatementSyntax propertyStatement => new[] { CreateDeclarationReferenceInfo(node, propertyStatement.Identifier, model) },
                TypeParameterSyntax typeParameter => new[] { CreateDeclarationReferenceInfo(node, typeParameter.Identifier, model) },
                _ => null
            };

        protected override IList<SyntaxNode> GetDeclarations(SyntaxNode node)
        {
            var walker = new DeclarationsFinder();
            walker.SafeVisit(node);
            return walker.Declarations;
        }

        private static ReferenceInfo[] CreateDeclarationReferenceInfo(LocalDeclarationStatementSyntax declaration, SemanticModel model) =>
            declaration.Declarators.SelectMany(x => CreateDeclarationReferenceInfo(x, model)).ToArray();

        private static ReferenceInfo[] CreateDeclarationReferenceInfo(FieldDeclarationSyntax fieldDeclaration, SemanticModel model) =>
            fieldDeclaration.Declarators.SelectMany(x => CreateDeclarationReferenceInfo(x, model)).ToArray();

        private static ReferenceInfo[] CreateDeclarationReferenceInfo(VariableDeclaratorSyntax variableDeclarator, SemanticModel model) =>
            variableDeclarator.Names.Select(x => CreateDeclarationReferenceInfo(x, x.Identifier, model)).ToArray();

        private static ReferenceInfo CreateDeclarationReferenceInfo(SyntaxNode node, SyntaxToken identifier, SemanticModel model) =>
            new(node, identifier, model.GetDeclaredSymbol(node), true);

        private sealed class DeclarationsFinder : SafeVisualBasicSyntaxWalker
        {
            private readonly ISet<ushort> declarationKinds = new HashSet<SyntaxKind>
            {
                SyntaxKind.ClassStatement,
                SyntaxKind.EnumStatement,
                SyntaxKind.EventStatement,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.FunctionStatement,
                SyntaxKind.InterfaceStatement,
                SyntaxKind.LocalDeclarationStatement,
                SyntaxKind.Parameter,
                SyntaxKind.PropertyStatement,
                SyntaxKind.StructureStatement,
                SyntaxKind.SubStatement,
                SyntaxKind.TypeParameter,
                SyntaxKind.VariableDeclarator
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
