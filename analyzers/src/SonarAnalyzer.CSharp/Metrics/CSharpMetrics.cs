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

namespace SonarAnalyzer.Metrics.CSharp
{
    public class CSharpMetrics : MetricsBase
    {
        private readonly Lazy<ImmutableArray<int>> lazyExecutableLines;

        public override ImmutableArray<int> ExecutableLines =>
            lazyExecutableLines.Value;

        public CSharpMetrics(SyntaxTree tree, SemanticModel semanticModel) : base(tree)
        {
            if (tree.GetRoot().Language != LanguageNames.CSharp)
            {
                throw new ArgumentException(InitializationErrorTextPattern, nameof(tree));
            }

            lazyExecutableLines = new Lazy<ImmutableArray<int>>(() =>
                CSharpExecutableLinesMetric.GetLineNumbers(tree, semanticModel));
        }

        protected override int ComputeCognitiveComplexity(SyntaxNode node) =>
            CSharpCognitiveComplexityMetric.GetComplexity(node).Complexity;

        public override int ComputeCyclomaticComplexity(SyntaxNode node) =>
            CSharpCyclomaticComplexityMetric.GetComplexity(node).Complexity;

        protected override bool IsClass(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKindEx.RecordDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKindEx.RecordStructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                    return IsInSameFile(node.GetLocation().GetMappedLineSpan());

                default:
                    return false;
            }
        }

        protected override bool IsCommentTrivia(SyntaxTrivia trivia) =>
            trivia.IsComment();

        protected override bool IsEndOfFile(SyntaxToken token) =>
            token.IsKind(SyntaxKind.EndOfFileToken);

        protected override bool IsFunction(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKindEx.LocalFunctionStatement:
                    return true;

                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).ExpressionBody != null;

                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    return ((BaseMethodDeclarationSyntax)node).HasBodyOrExpressionBody(); // Non-abstract, non-interface methods
                case SyntaxKind.MethodDeclaration:
                    var methodDeclaration = (BaseMethodDeclarationSyntax)node;
                    return methodDeclaration.HasBodyOrExpressionBody() // Non-abstract, non-interface methods
                        && IsInSameFile(methodDeclaration.GetLocation().GetMappedLineSpan()); // Excluding razor functions that are not mapped
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKindEx.InitAccessorDeclaration:
                    var accessor = (AccessorDeclarationSyntax)node;
                    if (accessor.HasBodyOrExpressionBody())
                    {
                        return true;
                    }

                    if (!accessor.Parent.Parent.IsAnyKind(SyntaxKind.PropertyDeclaration, SyntaxKind.EventDeclaration))
                    {
                        // Unexpected
                        return false;
                    }

                    var basePropertyNode = (BasePropertyDeclarationSyntax)accessor.Parent.Parent;

                    if (basePropertyNode.Modifiers.Any(SyntaxKind.AbstractKeyword))
                    {
                        return false;
                    }

                    return !basePropertyNode.Parent.IsKind(SyntaxKind.InterfaceDeclaration);

                default:
                    return false;
            }
        }

        protected override bool IsNoneToken(SyntaxToken token) =>
            token.IsKind(SyntaxKind.None);

        protected override bool IsStatement(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.BreakStatement:
                case SyntaxKind.CheckedStatement:
                case SyntaxKind.ContinueStatement:
                case SyntaxKind.DoStatement:
                case SyntaxKind.EmptyStatement:
                case SyntaxKind.ExpressionStatement:
                case SyntaxKind.FixedStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKindEx.ForEachVariableStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.GlobalStatement:
                case SyntaxKind.GotoCaseStatement:
                case SyntaxKind.GotoDefaultStatement:
                case SyntaxKind.GotoStatement:
                case SyntaxKind.IfStatement:
                case SyntaxKind.LabeledStatement:
                case SyntaxKind.LocalDeclarationStatement:
                case SyntaxKindEx.LocalFunctionStatement:
                case SyntaxKind.LockStatement:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.SwitchStatement:
                case SyntaxKind.ThrowStatement:
                case SyntaxKind.TryStatement:
                case SyntaxKind.UncheckedStatement:
                case SyntaxKind.UnsafeStatement:
                case SyntaxKind.UsingStatement:
                case SyntaxKind.WhileStatement:
                case SyntaxKind.YieldBreakStatement:
                case SyntaxKind.YieldReturnStatement:
                    return IsInSameFile(node.GetLocation().GetMappedLineSpan()); // Excluding razor statements that are not mapped
                case SyntaxKind.Block:
                    return false;

                default:
                    return node is StatementSyntax
                               ? throw new InvalidOperationException($"{node.Kind()} is statement and it isn't handled.")
                               : false;
            }
        }
    }
}
