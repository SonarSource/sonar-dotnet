using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace NSonarQubeAnalyzer
{
    public class FileComments
    {
        public readonly IImmutableSet<int> NoSonar;
        public readonly IImmutableSet<int> NonBlank;

        public FileComments(IImmutableSet<int> noSonar, IImmutableSet<int> nonBlank)
        {
            this.NoSonar = noSonar;
            this.NonBlank = nonBlank;
        }
    }

    public class Metrics
    {
        private readonly string[] LINE_TERMINATORS = { "\r\n", "\n", "\r" };
        private SyntaxTree tree;

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

            foreach (SyntaxTrivia trivia in tree.GetRoot().DescendantTrivia())
            {
                if (!(ignoreHeaderComments && trivia.Token.GetPreviousToken().IsKind(SyntaxKind.None)) && IsComment(trivia))
                {
                    int lineNumber = tree.GetLineSpan(trivia.FullSpan).StartLinePosition.Line + 1;

                    foreach (string line in trivia.ToFullString().Split(LINE_TERMINATORS, StringSplitOptions.None))
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
            }

            return new FileComments(noSonar.ToImmutableHashSet(), nonBlank.ToImmutableHashSet());
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }

        public int Classes()
        {
            return tree.GetCompilationUnitRoot().DescendantNodes().Where(n => n.IsKind(SyntaxKind.ClassDeclaration)).Count();
        }

        public int Accessors()
        {
            return tree.GetCompilationUnitRoot()
                .DescendantNodes()
                .Where(
                    n =>
                        n.IsKind(SyntaxKind.GetAccessorDeclaration) ||
                        n.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                        n.IsKind(SyntaxKind.AddAccessorDeclaration) ||
                        n.IsKind(SyntaxKind.RemoveAccessorDeclaration))
                .Count();
        }

        public int Statements()
        {
            return tree.GetCompilationUnitRoot().DescendantNodes().Where(n => n is StatementSyntax && !n.IsKind(SyntaxKind.Block)).Count();
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
            return node.IsKind(SyntaxKind.ConstructorDeclaration) ||
                node.IsKind(SyntaxKind.DestructorDeclaration) ||
                node.IsKind(SyntaxKind.MethodDeclaration) ||
                node.IsKind(SyntaxKind.OperatorDeclaration) ||
                node.IsKind(SyntaxKind.GetAccessorDeclaration) ||
                node.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                node.IsKind(SyntaxKind.AddAccessorDeclaration) ||
                node.IsKind(SyntaxKind.RemoveAccessorDeclaration);
        }

        public int PublicApi()
        {
            return PublicApiNodes().Count();
        }

        public int PublicUndocumentedApi()
        {
            return PublicApiNodes()
                .Where(
                    n =>
                        !n.GetLeadingTrivia()
                        .Where(t => IsComment(t))
                        .Any())
                .Count();
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

                var isPublic = member.ChildTokens().Where(t => t.IsKind(SyntaxKind.PublicKeyword)).Any();

                if (isPublic)
                {
                    builder.Add(member);
                }

                if (isPublic || member.IsKind(SyntaxKind.NamespaceDeclaration))
                {
                    var children = member.ChildNodes().OfType<MemberDeclarationSyntax>();
                    foreach (var child in children)
                    {
                        toVisit.Push(child);
                    }
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
                .Where(
                    n =>
                        // TODO What about the null coalescing operator?
                        n.IsKind(SyntaxKind.IfStatement) ||
                        n.IsKind(SyntaxKind.SwitchStatement) ||
                        n.IsKind(SyntaxKind.LabeledStatement) ||
                        n.IsKind(SyntaxKind.WhileStatement) ||
                        n.IsKind(SyntaxKind.DoStatement) ||
                        n.IsKind(SyntaxKind.ForStatement) ||
                        n.IsKind(SyntaxKind.ForEachStatement) ||
                        n.IsKind(SyntaxKind.LogicalAndExpression) ||
                        n.IsKind(SyntaxKind.LogicalOrExpression) ||
                        n.IsKind(SyntaxKind.CaseSwitchLabel) ||
                        (n.IsKind(SyntaxKind.ReturnStatement) && !IsLastStatement(n)) ||
                        (IsFunction(n) && n.ChildNodes().Any(c => c.IsKind(SyntaxKind.Block))))
                .Count();
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

        public ImmutableArray<Tuple<string, int>> CopyPasteTokens()
        {
            var builder = ImmutableArray.CreateBuilder<Tuple<string, int>>();

            foreach (var token in tree.GetCompilationUnitRoot().DescendantTokens(n => !n.IsKind(SyntaxKind.UsingDirective)))
            {
                if (!token.IsKind(SyntaxKind.EndOfFileToken))
                {
                    var value = token.IsKind(SyntaxKind.StringLiteralToken) ? "\"\"" : token.ToString();
                    builder.Add(Tuple.Create(value, token.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
                }
            }

            return builder.ToImmutable();
        }
    }
}
