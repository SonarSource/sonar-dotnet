using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SonarQube.Analyzers
{
    public class FileComments
    {
        public readonly IImmutableSet<int> NoSonar;
        public readonly IImmutableSet<int> NonBlank;

        public FileComments(IImmutableSet<int> noSonar, IImmutableSet<int> nonBlank)
        {
            NoSonar = noSonar;
            NonBlank = nonBlank;
        }
    }

    public class Metrics
    {
        private readonly string[] lineTerminators = { "\r\n", "\n", "\r" };
        private readonly SyntaxTree tree;

        public Metrics(SyntaxTree tree)
        {
            this.tree = tree;
        }

        public int Lines()
        {
            return tree.GetLineSpan(TextSpan.FromBounds(tree.Length, tree.Length)).EndLinePosition.Line + 1;
        }

        public IImmutableSet<int> LinesOfCode()
        {
            return tree.GetCompilationUnitRoot()
                .DescendantTokens()
                .Where(t => !t.IsKind(SyntaxKind.EndOfFileToken))
                .SelectMany(
                    t =>
                    {
                        var start = t.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        var end = t.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
                        return Enumerable.Range(start, end - start + 1);
                    })
                .ToImmutableHashSet();
        }

        public FileComments Comments(bool ignoreHeaderComments)
        {
            var noSonar = ImmutableHashSet.CreateBuilder<int>();
            var nonBlank = ImmutableHashSet.CreateBuilder<int>();

            foreach (var trivia in tree.GetRoot().DescendantTrivia())
            {
                if (!IsComment(trivia) ||
                    ignoreHeaderComments && trivia.Token.GetPreviousToken().IsKind(SyntaxKind.None))
                {
                    continue;
                }

                var lineNumber = tree.GetLineSpan(trivia.FullSpan).StartLinePosition.Line + 1;

                foreach (var line in trivia.ToFullString().Split(lineTerminators, StringSplitOptions.None))
                {
                    if (line.Contains("NOSONAR"))
                    {
                        nonBlank.Remove(lineNumber);
                        noSonar.Add(lineNumber);
                    }
                    else if (line.Any(char.IsLetter) && !noSonar.Contains(lineNumber))
                    {
                        nonBlank.Add(lineNumber);
                    }

                    lineNumber++;
                }
            }

            return new FileComments(noSonar.ToImmutableHashSet(), nonBlank.ToImmutableHashSet());
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return TriviaKinds.Contains(trivia.Kind());
        }

        public int Classes()
        {
            return tree.GetCompilationUnitRoot().DescendantNodes().Count(n => n.IsKind(SyntaxKind.ClassDeclaration));
        }

        public int Accessors()
        {
            return tree.GetCompilationUnitRoot()
                .DescendantNodes()
                .Count(
                    n => AccessorKinds.Contains(n.Kind()));
        }

        public int Statements()
        {
            return tree.GetCompilationUnitRoot()
                .DescendantNodes()
                .Count(n => n is StatementSyntax && !n.IsKind(SyntaxKind.Block));
        }

        public int Functions()
        {
            return FunctionNodes().Count();
        }

        public IEnumerable<SyntaxNode> FunctionNodes()
        {
            return tree.GetCompilationUnitRoot()
                .DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.Block) && IsFunction(n.Parent));
        }

        private static bool IsFunction(SyntaxNode node)
        {
            return FunctionKinds.Contains(node.Kind());
        }

        

        public int PublicApi()
        {
            return PublicApiNodes().Count();
        }

        public int PublicUndocumentedApi()
        {
            return PublicApiNodes()
                .Count(n =>
                    !n.GetLeadingTrivia()
                        .Any(IsComment));
        }

        private IEnumerable<SyntaxNode> PublicApiNodes()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

            var toVisit = new Stack<SyntaxNode>();
            var visited = new HashSet<SyntaxNode>();

            foreach (var member in tree.GetCompilationUnitRoot().Members)
            {
                toVisit.Push(member);
            }

            while (toVisit.Any())
            {
                var member = toVisit.Pop();
                visited.Add(member);

                var isPublic = member.ChildTokens().Any(t => t.IsKind(SyntaxKind.PublicKeyword));

                if (isPublic)
                {
                    builder.Add(member);
                }

                if (!isPublic && !member.IsKind(SyntaxKind.NamespaceDeclaration))
                {
                    continue;
                }

                var children = member.ChildNodes().OfType<MemberDeclarationSyntax>();
                foreach (var child in children)
                {
                    toVisit.Push(child);
                }
            }

            return builder.ToImmutable();
        }

        public int Complexity()
        {
            return Complexity(tree.GetCompilationUnitRoot());
        }

        public static int Complexity(SyntaxNode node)
        {
            return node
                .DescendantNodesAndSelf()
                .Count(
                    n =>
                        // TODO What about the null coalescing operator?
                        ComplexityIncreasingKinds.Contains(n.Kind()) ||
                        IsReturnButNotLast(n) ||
                        IsFunctionAndHasBlock(n));
        }

        private static bool IsFunctionAndHasBlock(SyntaxNode n)
        {
            return IsFunction(n) && n.ChildNodes().Any(c => c.IsKind(SyntaxKind.Block));
        }

        private static bool IsReturnButNotLast(SyntaxNode n)
        {
            return n.IsKind(SyntaxKind.ReturnStatement) && !IsLastStatement(n);
        }

        // TODO What about lambdas?
        private static bool IsLastStatement(SyntaxNode node)
        {
            var nextToken = node.GetLastToken().GetNextToken();
            return nextToken.IsKind(SyntaxKind.CloseBraceToken) &&
                nextToken.Parent.IsKind(SyntaxKind.Block) &&
                IsFunction(nextToken.Parent.Parent);
        }

        public Distribution FunctionComplexityDistribution()
        {
            var distribution = new Distribution(1, 2, 4, 6, 8, 10, 12);
            foreach (var node in FunctionNodes())
            {
                distribution.Add(Complexity(node));
            }
            return distribution;
        }

        #region Kind lists

        private static IEnumerable<SyntaxKind> TriviaKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.SingleLineCommentTrivia,
                    SyntaxKind.MultiLineCommentTrivia, 
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    SyntaxKind.MultiLineDocumentationCommentTrivia
                };
            }
        }

        private static IEnumerable<SyntaxKind> AccessorKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.GetAccessorDeclaration,
                    SyntaxKind.SetAccessorDeclaration, 
                    SyntaxKind.AddAccessorDeclaration,
                    SyntaxKind.RemoveAccessorDeclaration
                };
            }
        }
        private static IEnumerable<SyntaxKind> FunctionKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.ConstructorDeclaration,
                    SyntaxKind.DestructorDeclaration, 
                    SyntaxKind.MethodDeclaration,
                    SyntaxKind.OperatorDeclaration,
                    SyntaxKind.GetAccessorDeclaration,
                    SyntaxKind.SetAccessorDeclaration,
                    SyntaxKind.AddAccessorDeclaration,
                    SyntaxKind.RemoveAccessorDeclaration
                };
            }
        }

        private static IEnumerable<SyntaxKind> ComplexityIncreasingKinds
        {
            get
            {
                return new[]
                {
                    SyntaxKind.IfStatement,
                    SyntaxKind.SwitchStatement, 
                    SyntaxKind.LabeledStatement,
                    SyntaxKind.WhileStatement,
                    SyntaxKind.DoStatement,
                    SyntaxKind.ForStatement,
                    SyntaxKind.ForEachStatement,
                    SyntaxKind.LogicalAndExpression,
                    SyntaxKind.LogicalOrExpression,
                    SyntaxKind.CaseSwitchLabel
                };
            }
        }

        #endregion
    }
}
