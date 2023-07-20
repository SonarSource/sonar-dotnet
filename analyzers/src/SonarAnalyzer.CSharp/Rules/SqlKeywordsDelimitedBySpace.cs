/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
    public sealed class SqlKeywordsDelimitedBySpace : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2857";
        private const string MessageFormat = "Add a space before '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly IList<NameSyntax> SqlNamespaces = new List<NameSyntax>
        {
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("Microsoft", "EntityFrameworkCore"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("ServiceStack", "OrmLite"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "SqlClient"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "SQLite"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "SqlServerCe"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "Entity"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "Odbc"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "OracleClient"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("Microsoft", "Data", "SqlClient"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("Microsoft", "Data", "Sqlite"),
            SyntaxFactory.IdentifierName("Dapper"),
            SyntaxFactory.IdentifierName("NHibernate"),
            SyntaxFactory.IdentifierName("PetaPoco")
        };

        // We are interested in SQL keywords that start a query (so without "FROM", for example)
        private static readonly IList<string> SqlStartQueryKeywords = new List<string>()
        {
            "ALTER",
            "BULK INSERT",
            "CREATE",
            "DELETE",
            "DROP",
            "EXEC",
            "EXECUTE",
            "GRANT",
            "INSERT",
            "MERGE",
            "READTEXT",
            "SELECT",
            "TRUNCATE",
            "UPDATE",
            "UPDATETEXT",
            "WRITETEXT"
        };

        private static readonly int SqlKeywordMinSize = SqlStartQueryKeywords
            .Select(s => s.Length)
            .OrderBy(i => i)
            .First();

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var namespaceDeclaration = (BaseNamespaceDeclarationSyntaxWrapper)c.Node;
                    if (namespaceDeclaration.SyntaxNode.Parent is CompilationUnitSyntax compilationUnit
                        && (HasSqlNamespace(compilationUnit.Usings) || HasSqlNamespace(namespaceDeclaration.Usings)))
                    {
                        var visitor = new StringConcatenationWalker(c);
                        foreach (var member in namespaceDeclaration.Members)
                        {
                            visitor.SafeVisit(member);
                        }
                    }
                },
                SyntaxKind.NamespaceDeclaration,
                SyntaxKindEx.FileScopedNamespaceDeclaration);

        private static bool HasSqlNamespace(SyntaxList<UsingDirectiveSyntax> usings) =>
            usings.Select(x => x.Name)
                .Any(x => SqlNamespaces.Any(usingsNamespace => SyntaxFactory.AreEquivalent(x, usingsNamespace)));

        private sealed class StringConcatenationWalker : SafeCSharpSyntaxWalker
        {
            private readonly SonarSyntaxNodeReportingContext context;

            public StringConcatenationWalker(SonarSyntaxNodeReportingContext context) =>
                this.context = context;

            public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
            {
                if (TryGetConstantValuesOfInterpolatedStringExpression(node, out var stringParts))
                {
                    RaiseIssueIfSpaceDoesNotExistBetweenStrings(stringParts);
                }
                base.VisitInterpolatedStringExpression(node);
            }

            // The assumption is that in a chain of concatenations "a" + "b" + "c"
            // the AST form is
            //        +
            //       / \
            //      +  "c"
            //     / \
            //   "a" "b"
            // So we start from the lower-left node which should contain the SQL start keyword
            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.AddExpression)
                    // we do the analysis only if it's a SQL keyword on the left
                    && TryGetStringWrapper(node.Left, out var leftSide)
                    && TryGetStringWrapper(node.Right, out var rightSide)
                    && StartsWithSqlKeyword(leftSide.Text.Trim()))
                {
                    var strings = new List<StringWrapper>();
                    strings.Add(leftSide);
                    strings.Add(rightSide);
                    var onlyStringsInConcatenation = AddStringsToList(node, strings);
                    if (!onlyStringsInConcatenation)
                    {
                        return;
                    }

                    RaiseIssueIfSpaceDoesNotExistBetweenStrings(strings);
                }
                else
                {
                    Visit(node.Left);
                    Visit(node.Right);
                }
            }

            private void RaiseIssueIfSpaceDoesNotExistBetweenStrings(List<StringWrapper> stringWrappers)
            {
                for (var i = 0; i < stringWrappers.Count - 1; i++)
                {
                    var firstStringText = stringWrappers[i].Text;
                    var secondString = stringWrappers[i + 1];
                    var secondStringText = secondString.Text;
                    if (firstStringText.Length > 0
                        && IsAlphaNumericOrAt(firstStringText.Last())
                        && secondStringText.Length > 0
                        && IsAlphaNumericOrAt(secondStringText.First()))
                    {
                        var word = secondStringText.Split(' ').FirstOrDefault();
                        context.ReportIssue(CreateDiagnostic(Rule, secondString.Node.GetLocation(), word));
                    }
                }
            }

            private static bool TryGetStringWrapper(ExpressionSyntax expression, out StringWrapper stringWrapper)
            {
                if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    stringWrapper = new StringWrapper(literal, literal.Token.ValueText);
                    return true;
                }

                if (expression is InterpolatedStringExpressionSyntax interpolatedString)
                {
                    stringWrapper = new StringWrapper(interpolatedString, interpolatedString.GetContentsText());
                    return true;
                }

                stringWrapper = null;
                return false;
            }

            /**
             * Returns
             * - true if all the found elements are string literals.
             * - false if, inside the chain of binary expressions, some elements are not string literals or
             * some binary expressions are not additions.
             */
            private static bool AddStringsToList(BinaryExpressionSyntax node, List<StringWrapper> strings)
            {
                // this is the left-most node of a concatenation chain
                // collect all string literals
                var parent = node.Parent;
                while (parent is BinaryExpressionSyntax concatenation)
                {
                    if (concatenation.IsKind(SyntaxKind.AddExpression)
                        && TryGetStringWrapper(concatenation.Right, out var stringWrapper))
                    {
                        strings.Add(stringWrapper);
                    }
                    else
                    {
                        // we are in a binary expression, but it's not only of strings or not only concatenations
                        return false;
                    }
                    parent = parent.Parent;
                }
                return true;
            }

            private bool TryGetConstantValuesOfInterpolatedStringExpression(InterpolatedStringExpressionSyntax interpolatedStringExpression, out List<StringWrapper> parts)
            {
                parts = new List<StringWrapper>();
                foreach (var interpolatedStringContent in interpolatedStringExpression.Contents)
                {
                    if (interpolatedStringContent is InterpolationSyntax interpolation
                        && interpolation.Expression.FindConstantValue(context.SemanticModel) is string constantValue)
                    {
                        parts.Add(new StringWrapper(interpolatedStringContent, constantValue));
                    }
                    else if (interpolatedStringContent is InterpolatedStringTextSyntax interpolatedText)
                    {
                        parts.Add(new StringWrapper(interpolatedText, interpolatedText.TextToken.Text));
                    }
                    else
                    {
                        parts = null;
                        return false;
                    }
                }
                return true;
            }

            private static bool StartsWithSqlKeyword(string firstString) =>
                firstString.Length >= SqlKeywordMinSize
                && SqlStartQueryKeywords.Any(x => firstString.StartsWith(x, StringComparison.OrdinalIgnoreCase));

            /**
             * The '@' symbol is used for named parameters. The '{' and '}' symbols are used in string interpolations.
             * We ignore other non-alphanumeric characters (e.g. '>','=') to avoid false positives.
             */
            private static bool IsAlphaNumericOrAt(char c) =>
                char.IsLetterOrDigit(c) || c == '@' || c == '{' || c == '}';
        }

        private sealed class StringWrapper
        {
            public SyntaxNode Node { get; }
            public string Text { get; }

            internal StringWrapper(SyntaxNode node, string text)
            {
                Node = node;
                Text = text;
            }
        }
    }
}
