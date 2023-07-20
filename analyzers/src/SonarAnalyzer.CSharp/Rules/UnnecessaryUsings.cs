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
    public sealed class UnnecessaryUsings : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1128";
        private const string MessageFormat = "Remove this unnecessary 'using'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    // When using top level statements, we are called twice for the same compilation unit. The second call has the containing symbol kind equal to `Method`.
                    if (c.ContainingSymbol.Kind == SymbolKind.Method)
                    {
                        return;
                    }

                    var compilationUnit = (CompilationUnitSyntax)c.Node;
                    var simpleNamespaces = compilationUnit.Usings.Where(usingDirective => usingDirective.Alias == null).ToList();
                    var globalUsingDirectives = simpleNamespaces.Select(x => new EquivalentNameSyntax(x.Name)).ToImmutableHashSet();

                    var visitor = new CSharpRemovableUsingWalker(c, globalUsingDirectives, null);
                    VisitContent(visitor, compilationUnit.Members, c.Node.DescendantTrivia());
                    foreach (var attribute in compilationUnit.AttributeLists)
                    {
                        visitor.SafeVisit(attribute);
                    }

                    CheckUnnecessaryUsings(c, simpleNamespaces, visitor.NecessaryNamespaces);
                },
                SyntaxKind.CompilationUnit);

        private static void VisitContent(ISafeSyntaxWalker visitor, SyntaxList<MemberDeclarationSyntax> members, IEnumerable<SyntaxTrivia> trivias)
        {
            var comments = trivias.Where(trivia => trivia.IsAnyKind(SyntaxKind.SingleLineDocumentationCommentTrivia, SyntaxKind.MultiLineDocumentationCommentTrivia));

            foreach (var member in members)
            {
                visitor.SafeVisit(member);
            }
            foreach (var comment in comments.Where(x => x.HasStructure))
            {
                visitor.SafeVisit(comment.GetStructure());
            }
        }

        private static void CheckUnnecessaryUsings(SonarSyntaxNodeReportingContext context, IEnumerable<UsingDirectiveSyntax> usingDirectives, ISet<INamespaceSymbol> necessaryNamespaces)
        {
            foreach (var usingDirective in usingDirectives)
            {
                // This will create some FNs but will kill noise from FPs.
                // For more info see issue: https://github.com/SonarSource/sonar-dotnet/issues/5946
                if (usingDirective.GetFirstToken().IsKind(SyntaxKind.GlobalKeyword))
                {
                    return;
                }
                if (context.SemanticModel.GetSymbolInfo(usingDirective.Name).Symbol is INamespaceSymbol namespaceSymbol
                    && !necessaryNamespaces.Any(usedNamespace => usedNamespace.IsSameNamespace(namespaceSymbol)))
                {
                    context.ReportIssue(CreateDiagnostic(Rule, usingDirective.GetLocation()));
                }
            }
        }

        private sealed class CSharpRemovableUsingWalker : SafeCSharpSyntaxWalker
        {
            public readonly HashSet<INamespaceSymbol> NecessaryNamespaces = new();

            private readonly SonarSyntaxNodeReportingContext context;
            private readonly IImmutableSet<EquivalentNameSyntax> usingDirectivesFromParent;
            private readonly INamespaceSymbol currentNamespace;
            private bool linqQueryVisited;

            public CSharpRemovableUsingWalker(SonarSyntaxNodeReportingContext context, IImmutableSet<EquivalentNameSyntax> usingDirectivesFromParent, INamespaceSymbol currentNamespace)
            {
                this.context = context;
                this.usingDirectivesFromParent = usingDirectivesFromParent;
                this.currentNamespace = currentNamespace;
            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                VisitNamespace(node, node.Usings, node.Name, node.Members);
            }

            public override void VisitInitializerExpression(InitializerExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.CollectionInitializerExpression))
                {
                    foreach (var addExpression in node.Expressions)
                    {
                        VisitSymbol(context.SemanticModel.GetCollectionInitializerSymbolInfo(addExpression).Symbol);
                    }
                }
                base.VisitInitializerExpression(node);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node) =>
                VisitNameNode(node);

            public override void VisitGenericName(GenericNameSyntax node)
            {
                VisitNameNode(node);
                base.VisitGenericName(node);
            }

            public override void VisitAwaitExpression(AwaitExpressionSyntax node)
            {
                VisitSymbol(context.SemanticModel.GetAwaitExpressionInfo(node).GetAwaiterMethod);
                base.VisitAwaitExpression(node);
            }

            /// <summary>
            /// LINQ Query Syntax do not use symbols from the 'System.Linq' namespace directly, but the using directive is
            /// still necessary to use the Query Syntax form.
            /// </summary>
            public override void VisitQueryExpression(QueryExpressionSyntax node)
            {
                if (!linqQueryVisited && TryGetSystemLinkNamespace(out var systemLinqNamespaceSymbol))
                {
                    NecessaryNamespaces.Add(systemLinqNamespaceSymbol);
                }
                linqQueryVisited = true;
                base.VisitQueryExpression(node);
            }

            public override void Visit(SyntaxNode node)
            {
                if (node.IsKind(SyntaxKindEx.FileScopedNamespaceDeclaration))
                {
                    var fileScopedNamespace = (FileScopedNamespaceDeclarationSyntaxWrapper)node;
                    VisitNamespace(node, fileScopedNamespace.Usings, fileScopedNamespace.Name, fileScopedNamespace.Members);
                }
                if (node.IsKind(SyntaxKindEx.ParenthesizedVariableDesignation)) // Tuple deconstruction declaration
                {
                    NecessaryNamespaces.Add(context.Compilation.GetSpecialType(SpecialType.System_Object).ContainingNamespace);
                }
                base.Visit(node);
            }

            private void VisitNamespace(SyntaxNode node, SyntaxList<UsingDirectiveSyntax> usings, NameSyntax name, SyntaxList<MemberDeclarationSyntax> members)
            {
                var simpleNamespaces = usings.Where(usingDirective => usingDirective.Alias == null).ToList();
                var newUsingDirectives = new HashSet<EquivalentNameSyntax>();
                newUsingDirectives.UnionWith(usingDirectivesFromParent);
                newUsingDirectives.UnionWith(simpleNamespaces.Select(x => new EquivalentNameSyntax(x.Name)));

                // We visit the namespace declaration with the updated set of parent 'usings', this is needed in case of nested namespaces
                var visitingNamespace = context.SemanticModel.GetSymbolInfo(name).Symbol as INamespaceSymbol;
                var visitor = new CSharpRemovableUsingWalker(context, newUsingDirectives.ToImmutableHashSet(), visitingNamespace);

                VisitContent(visitor, members, node.DescendantTrivia());
                CheckUnnecessaryUsings(context, simpleNamespaces, visitor.NecessaryNamespaces);

                NecessaryNamespaces.UnionWith(visitor.NecessaryNamespaces);
            }

            private bool TryGetSystemLinkNamespace(out INamespaceSymbol systemLinqNamespace)
            {
                foreach (var usingDirective in usingDirectivesFromParent)
                {
                    if (context.SemanticModel.GetSymbolInfo(usingDirective.Name).Symbol is INamespaceSymbol namespaceSymbol
                        && namespaceSymbol.ToDisplayString() == "System.Linq")
                    {
                        systemLinqNamespace = namespaceSymbol;
                        return true;
                    }
                }
                systemLinqNamespace = null;
                return false;
            }

            /// <summary>
            /// We check the symbol of each name node found in the code. If the containing namespace of the symbol is
            /// neither the current namespace or one of its parent, it is then added to the necessary namespace set, as
            /// importing that namespace is indeed necessary.
            /// </summary>
            private void VisitNameNode(ExpressionSyntax node) =>
                VisitSymbol(context.SemanticModel.GetSymbolInfo(node).Symbol);

            private void VisitSymbol(ISymbol symbol)
            {
                if (symbol != null
                    && symbol.ContainingNamespace is INamespaceSymbol namespaceSymbol
                    && (currentNamespace == null || !namespaceSymbol.IsSameOrAncestorOf(currentNamespace)))
                {
                    NecessaryNamespaces.Add(namespaceSymbol);
                }
            }
        }
    }

    internal sealed class EquivalentNameSyntax : IEquatable<EquivalentNameSyntax>
    {
        public NameSyntax Name { get; private set; }

        public EquivalentNameSyntax(NameSyntax name) =>
            Name = name;

        public override int GetHashCode() =>
            Name.ToString().GetHashCode();

        public override bool Equals(object obj) =>
            obj is EquivalentNameSyntax equivalentName && Equals(equivalentName);

        public bool Equals(EquivalentNameSyntax other) =>
            other != null && CSharpEquivalenceChecker.AreEquivalent(Name, other.Name);
    }
}
