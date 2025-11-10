/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Syntax.Extensions;

internal static class SyntaxNodeExtensions
{
    public static bool IsInDebugBlock(this SyntaxNode node) =>
        node.ActiveConditionalCompilationSections().Contains("DEBUG");

    public static bool IsInConditionalDebug(this SyntaxNode node, SemanticModel model)
    {
        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>() is { } containingMethod
            ? model.GetDeclaredSymbol(containingMethod)
            : null;
        return method.IsConditionalDebugMethod();
    }

    /// <summary>
    /// Returns a list of the names of #if [NAME] sections that the specified
    /// node is contained in.
    /// </summary>
    /// <remarks>
    /// Note: currently we only handle directives with simple identifiers e.g. #if FOO, #elif FOO
    /// We don't handle logical operators e.g. #if !DEBUG, and we don't handle cases like
    /// #if !DEBUG ... #else... :DEBUG must be true in the else case.
    /// </remarks>
    public static IEnumerable<string> ActiveConditionalCompilationSections(this SyntaxNode node)
    {
        var directives = CollectPrecedingDirectiveSyntax(node);
        if (directives.Count == 0)
        {
            return [];
        }

        var activeDirectives = new Stack<BranchingDirectiveTriviaSyntax>();
        foreach (var directive in directives)
        {
            switch (directive.RawKind)
            {
                case (int)SyntaxKind.IfDirectiveTrivia:
                    activeDirectives.Push((BranchingDirectiveTriviaSyntax)directive);
                    break;
                case (int)SyntaxKind.ElseDirectiveTrivia:
                case (int)SyntaxKind.ElifDirectiveTrivia:
                    // If we hit an if or elif then that effective acts as an "end" for the previous if/elif block -> pop it
                    SafePop(activeDirectives);
                    activeDirectives.Push((BranchingDirectiveTriviaSyntax)directive);
                    break;
                case (int)SyntaxKind.EndIfDirectiveTrivia:
                    SafePop(activeDirectives);
                    break;
                default:
                    Debug.Fail($"Unexpected token type: {directive.Kind()}");
                    break;
            }
        }
        Debug.Assert(activeDirectives.All(x => x.IsActive), "Not all of the collected directives were active");
        Debug.Assert(activeDirectives.All(x => x.BranchTaken), "Not all of the collected directives were for the branch that was taken");
        return activeDirectives.Select(FindDirectiveName).WhereNotNull().ToHashSet();
    }

    private static string FindDirectiveName(BranchingDirectiveTriviaSyntax directiveTriviaSyntax) =>
        directiveTriviaSyntax is ConditionalDirectiveTriviaSyntax conditionalDirective && conditionalDirective.Condition is IdentifierNameSyntax identifierName
            ? identifierName.Identifier.ValueText
            : null;

    private static void SafePop(Stack<BranchingDirectiveTriviaSyntax> stack)
    {
        if (stack.Count > 0)    // This should never be empty
        {
            stack.Pop();
        }
    }

    private static IList<DirectiveTriviaSyntax> CollectPrecedingDirectiveSyntax(SyntaxNode node)
    {
        var walker = new BranchingDirectiveCollector(node);
        return walker.SafeVisit(node.SyntaxTree.GetRoot()) ? walker.CollectedDirectives : [];
    }

    /// <summary>
    /// Collects all of the #if, #else, #elsif and #endif directives occuring in the
    /// syntax tree up to the specified node
    /// </summary>
    private sealed class BranchingDirectiveCollector : SafeCSharpSyntaxWalker
    {
        private readonly SyntaxNode terminatingNode;
        private bool found;

        public List<DirectiveTriviaSyntax> CollectedDirectives { get; } = new();

        public BranchingDirectiveCollector(SyntaxNode terminatingNode) : base(SyntaxWalkerDepth.StructuredTrivia) =>
            this.terminatingNode = terminatingNode;

        public override void Visit(SyntaxNode node)
        {
            // Stop traversing once we've walked down to the terminating node
            if (found)
            {
                return;
            }

            if (node == terminatingNode)
            {
                VisitTerminatingNodeLeadingTrivia();
                found = true;
            }
            else
            {
                base.Visit(node);
            }
        }

        public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            AddDirective(node);
            base.VisitIfDirectiveTrivia(node);
        }

        public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
        {
            AddDirective(node);
            base.VisitElseDirectiveTrivia(node);
        }

        public override void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
        {
            AddDirective(node);
            base.VisitElifDirectiveTrivia(node);
        }

        public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
        {
            AddDirective(node);
            base.VisitEndIfDirectiveTrivia(node);
        }

        private void AddDirective(DirectiveTriviaSyntax node)
        {
            if (node.IsActive)
            {
                CollectedDirectives.Add(node);
            }
        }

        private void VisitTerminatingNodeLeadingTrivia()
        {
            // Special case: the leading trivia of the terminating node could contain directives. However, we won't have processed
            // these yet, as they are treated as children of the node even though they appear before it in the text
            if (terminatingNode.HasLeadingTrivia)
            {
                VisitLeadingTrivia(terminatingNode.GetFirstToken(includeZeroWidth: true));
            }
        }
    }
}
