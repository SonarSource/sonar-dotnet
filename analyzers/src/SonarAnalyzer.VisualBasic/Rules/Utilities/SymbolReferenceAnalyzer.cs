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

namespace SonarAnalyzer.VisualBasic.Rules
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
