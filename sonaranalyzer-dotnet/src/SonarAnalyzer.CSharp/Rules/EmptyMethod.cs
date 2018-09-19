/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            Helpers.CSharp.GeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } = new []
        {
            SyntaxKind.MethodDeclaration
        };

        protected override void CheckMethod(SyntaxNodeAnalysisContext context)
        {
            var methodNode = (MethodDeclarationSyntax)context.Node;

            // No need to check for ExpressionBody as arrowed methods can't be empty
            if (methodNode.Body != null &&
                IsEmpty(methodNode.Body) &&
                !ShouldMethodBeExcluded(methodNode, context.SemanticModel))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodNode.Identifier.GetLocation()));
            }
        }

        private static bool ShouldMethodBeExcluded(MethodDeclarationSyntax methodNode, SemanticModel semanticModel)
        {
            if (methodNode.Modifiers.Any(SyntaxKind.VirtualKeyword))
            {
                return true;
            }

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
            if (methodSymbol != null &&
                methodSymbol.IsOverride &&
                methodSymbol.OverriddenMethod != null &&
                methodSymbol.OverriddenMethod.IsAbstract)
            {
                return true;
            }

            return methodNode.Modifiers.Any(SyntaxKind.OverrideKeyword) &&
                semanticModel.Compilation.IsTest();
        }

        private static bool IsEmpty(BlockSyntax node)
        {
            return !node.Statements.Any() && !ContainsComment(node);
        }

        private static bool ContainsComment(BlockSyntax node)
        {
            return ContainsComment(node.OpenBraceToken.TrailingTrivia) ||
                ContainsComment(node.CloseBraceToken.LeadingTrivia);
        }

        private static bool ContainsComment(SyntaxTriviaList trivias)
        {
            return trivias.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
        }
    }
}
