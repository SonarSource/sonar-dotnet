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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Extensions
{
    internal static partial class SyntaxNodeExtensions
    {
        private static readonly ControlFlowGraphCache CfgCache = new();
        private static readonly SyntaxKind[] ParenthesizedNodeKinds = new[] { SyntaxKind.ParenthesizedExpression, SyntaxKindEx.ParenthesizedPattern };

        public static ControlFlowGraph CreateCfg(this SyntaxNode body, SemanticModel model, CancellationToken cancel) =>
            CfgCache.FindOrCreate(body is CompilationUnitSyntax ? body : body.Parent, model, cancel);

        public static bool ContainsConditionalConstructs(this SyntaxNode node) =>
            node != null &&
            node.DescendantNodes()
                .Any(descendant => descendant.IsAnyKind(SyntaxKind.IfStatement,
                    SyntaxKind.ConditionalExpression,
                    SyntaxKind.CoalesceExpression,
                    SyntaxKind.SwitchStatement,
                    SyntaxKindEx.SwitchExpression,
                    SyntaxKindEx.CoalesceAssignmentExpression));

        public static object FindConstantValue(this SyntaxNode node, SemanticModel semanticModel) =>
            new CSharpConstantValueFinder(semanticModel).FindConstant(node);

        public static string FindStringConstant(this SyntaxNode node, SemanticModel semanticModel) =>
            FindConstantValue(node, semanticModel) as string;

        public static bool IsPartOfBinaryNegationOrCondition(this SyntaxNode node)
        {
            if (!(node.Parent is MemberAccessExpressionSyntax))
            {
                return false;
            }

            var topNode = node.Parent.GetSelfOrTopParenthesizedExpression();
            if (topNode.Parent?.IsKind(SyntaxKind.BitwiseNotExpression) ?? false)
            {
                return true;
            }

            var current = topNode;
            while (!current.Parent?.IsAnyKind(SyntaxKind.BitwiseNotExpression,
                                              SyntaxKind.IfStatement,
                                              SyntaxKind.WhileStatement,
                                              SyntaxKind.ConditionalExpression,
                                              SyntaxKind.MethodDeclaration,
                                              SyntaxKind.SimpleLambdaExpression) ?? false)
            {
                current = current.Parent;
            }

            return current.Parent switch
            {
                IfStatementSyntax ifStatement => ifStatement.Condition == current,
                WhileStatementSyntax whileStatement => whileStatement.Condition == current,
                ConditionalExpressionSyntax condExpr => condExpr.Condition == current,
                _ => false
            };
        }

        public static string GetDeclarationTypeName(this SyntaxNode node) =>
            node.Kind() switch
            {
                SyntaxKind.ClassDeclaration => "class",
                SyntaxKind.ConstructorDeclaration => "constructor",
                SyntaxKind.DelegateDeclaration => "delegate",
                SyntaxKind.DestructorDeclaration => "destructor",
                SyntaxKind.EnumDeclaration => "enum",
                SyntaxKind.EnumMemberDeclaration => "enum",
                SyntaxKind.EventDeclaration => "event",
                SyntaxKind.EventFieldDeclaration => "event",
                SyntaxKind.FieldDeclaration => "field",
                SyntaxKind.IndexerDeclaration => "indexer",
                SyntaxKind.InterfaceDeclaration => "interface",
                SyntaxKindEx.LocalFunctionStatement => "local function",
                SyntaxKind.MethodDeclaration => "method",
                SyntaxKind.PropertyDeclaration => "property",
                SyntaxKindEx.RecordClassDeclaration => "record",
                SyntaxKindEx.RecordStructDeclaration => "record struct",
                SyntaxKind.StructDeclaration => "struct",
                _ => GetUnknownType(node.Kind())
            };

        // Extracts the expression body from an arrow-bodied syntax node.
        public static ArrowExpressionClauseSyntax ArrowExpressionBody(this SyntaxNode node) =>
            node switch
            {
                MethodDeclarationSyntax a => a.ExpressionBody,
                ConstructorDeclarationSyntax b => b.ExpressionBody(),
                OperatorDeclarationSyntax c => c.ExpressionBody,
                AccessorDeclarationSyntax d => d.ExpressionBody(),
                ConversionOperatorDeclarationSyntax e => e.ExpressionBody,
                _ => null
            };

        public static SyntaxNode RemoveParentheses(this SyntaxNode expression)
        {
            var current = expression;
            while (current is { } && current.IsAnyKind(ParenthesizedNodeKinds))
            {
                current = current.IsKind(SyntaxKindEx.ParenthesizedPattern)
                    ? ((ParenthesizedPatternSyntaxWrapper)current).Pattern
                    : ((ParenthesizedExpressionSyntax)current).Expression;
            }
            return current;
        }

        public static SyntaxNode WalkUpParentheses(this SyntaxNode node)
        {
            while (node is not null && node.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                node = node.Parent;
            }
            return node;
        }

        public static SyntaxToken? GetIdentifier(this SyntaxNode node) =>
            node switch
            {
                AliasQualifiedNameSyntax { Alias.Identifier: var identifier } => identifier,
                ArrayTypeSyntax { ElementType: { } elementType } => GetIdentifier(elementType),
                AttributeSyntax { Name: { } name } => GetIdentifier(name),
                BaseTypeDeclarationSyntax { Identifier: var identifier } => identifier,
                ConstructorDeclarationSyntax { Identifier: var identifier } => identifier,
                ConversionOperatorDeclarationSyntax { Type: { } type } => GetIdentifier(type),
                DelegateDeclarationSyntax { Identifier: var identifier } => identifier,
                DestructorDeclarationSyntax { Identifier: var identifier } => identifier,
                EnumMemberDeclarationSyntax { Identifier: var identifier } => identifier,
                IndexerDeclarationSyntax { ThisKeyword: var thisKeyword } => thisKeyword,
                InvocationExpressionSyntax
                {
                    Expression: not InvocationExpressionSyntax // We don't want to recurse into nested invocations like: fun()()
                } invocation => GetIdentifier(invocation.Expression),
                MethodDeclarationSyntax { Identifier: var identifier } => identifier,
                MemberBindingExpressionSyntax { Name.Identifier: var identifier } => identifier,
                MemberAccessExpressionSyntax { Name.Identifier: var identifier } => identifier,
                NamespaceDeclarationSyntax { Name: { } name } => GetIdentifier(name),
                NullableTypeSyntax { ElementType: { } elementType } => GetIdentifier(elementType),
                OperatorDeclarationSyntax { OperatorToken: var operatorToken } => operatorToken,
                ParameterSyntax { Identifier: var identifier } => identifier,
                PropertyDeclarationSyntax { Identifier: var identifier } => identifier,
                PointerTypeSyntax { ElementType: { } elementType } => GetIdentifier(elementType),
                PredefinedTypeSyntax { Keyword: var keyword } => keyword,
                QualifiedNameSyntax { Right.Identifier: var identifier } => identifier,
                SimpleNameSyntax { Identifier: var identifier } => identifier,
                TypeParameterConstraintClauseSyntax { Name.Identifier: var identifier } => identifier,
                TypeParameterSyntax { Identifier: var identifier } => identifier,
                UsingDirectiveSyntax { Alias.Name: { } name } => GetIdentifier(name),
                VariableDeclaratorSyntax { Identifier: var identifier } => identifier,
                SimpleBaseTypeSyntax { Type: { } type } => GetIdentifier(type),
                { } refType when RefTypeSyntaxWrapper.IsInstance(refType) => GetIdentifier(((RefTypeSyntaxWrapper)refType).Type),
                _ => null
            };

        /// <summary>
        /// Finds the syntactic complementing <see cref="SyntaxNode"/> of an assignment with tuples.
        /// <code>
        /// var (a, b) = (1, 2);      // if node is "a", "1" is returned and vice versa.
        /// (var a, var b) = (1, 2);  // if node is "2", "var b" is returned and vice versa.
        /// a = 1;                    // if node is "a", "1" is returned and vice versa.
        /// t = (1, 2);               // if node is "t", "(1, 2)" is returned, if node is "1", "null" is returned.
        /// </code>
        /// <paramref name="node"/> must be an <see cref="ArgumentSyntax"/> of a tuple or some variable designation of a <see cref="SyntaxKindEx.DeclarationExpression"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="SyntaxNode"/> on the other side of the assignment or <see langword="null"/> if <paramref name="node"/> is not
        /// a direct child of the assignment, not part of a tuple, not part of a designation, or no corresponding <see cref="SyntaxNode"/>
        /// can be found on the other side.
        /// </returns>
        public static SyntaxNode FindAssignmentComplement(this SyntaxNode node)
        {
            if (node is { Parent: AssignmentExpressionSyntax assigment })
            {
                return OtherSideOfAssignment(node, assigment);
            }
            // can be either outermost tuple, or DeclarationExpression if 'node' is SingleVariableDesignationExpression
            var outermostParenthesesExpression = node.AncestorsAndSelf()
                .TakeWhile(x => x.IsAnyKind(
                    SyntaxKind.Argument,
                    SyntaxKindEx.TupleExpression,
                    SyntaxKindEx.SingleVariableDesignation,
                    SyntaxKindEx.ParenthesizedVariableDesignation,
                    SyntaxKindEx.DiscardDesignation,
                    SyntaxKindEx.DeclarationExpression))
                .LastOrDefault(x => x.IsAnyKind(SyntaxKindEx.DeclarationExpression, SyntaxKindEx.TupleExpression));
            if ((TupleExpressionSyntaxWrapper.IsInstance(outermostParenthesesExpression) || DeclarationExpressionSyntaxWrapper.IsInstance(outermostParenthesesExpression))
                && outermostParenthesesExpression.Parent is AssignmentExpressionSyntax assignment)
            {
                var otherSide = OtherSideOfAssignment(outermostParenthesesExpression, assignment);
                if (TupleExpressionSyntaxWrapper.IsInstance(otherSide) || DeclarationExpressionSyntaxWrapper.IsInstance(otherSide))
                {
                    var stackFromNodeToOutermost = GetNestingPathFromNodeToOutermost(node);
                    return FindMatchingNestedNode(stackFromNodeToOutermost, otherSide);
                }
                else
                {
                    return null;
                }
            }

            return null;

            static ExpressionSyntax OtherSideOfAssignment(SyntaxNode oneSide, AssignmentExpressionSyntax assignment) =>
                assignment switch
                {
                    { Left: { } left, Right: { } right } when left.Equals(oneSide) => right,
                    { Left: { } left, Right: { } right } when right.Equals(oneSide) => left,
                    _ => null,
                };

            static Stack<PathPosition> GetNestingPathFromNodeToOutermost(SyntaxNode node)
            {
                Stack<PathPosition> pathFromNodeToTheTop = new();
                while (TupleExpressionSyntaxWrapper.IsInstance(node?.Parent)
                    || ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node?.Parent)
                    || DeclarationExpressionSyntaxWrapper.IsInstance(node?.Parent))
                {
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(node?.Parent) && node is { Parent.Parent: ArgumentSyntax { } argument })
                    {
                        node = argument;
                    }
                    node = node switch
                    {
                        ArgumentSyntax tupleArgument when TupleExpressionSyntaxWrapper.IsInstance(node.Parent) =>
                            PushPathPositionForTuple(pathFromNodeToTheTop, (TupleExpressionSyntaxWrapper)node.Parent, tupleArgument),
                        _ when VariableDesignationSyntaxWrapper.IsInstance(node) && ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(node.Parent) =>
                            PushPathPositionForParenthesizedDesignation(pathFromNodeToTheTop, (ParenthesizedVariableDesignationSyntaxWrapper)node.Parent, (VariableDesignationSyntaxWrapper)node),
                        _ => null,
                    };
                }
                return pathFromNodeToTheTop;
            }

            static SyntaxNode FindMatchingNestedNode(Stack<PathPosition> pathFromOutermostToGivenNode, SyntaxNode outermostParenthesesToMatch)
            {
                var matchedNestedNode = outermostParenthesesToMatch;
                while (matchedNestedNode is not null && pathFromOutermostToGivenNode.Count > 0)
                {
                    if (DeclarationExpressionSyntaxWrapper.IsInstance(matchedNestedNode))
                    {
                        matchedNestedNode = ((DeclarationExpressionSyntaxWrapper)matchedNestedNode).Designation;
                    }
                    var expectedPathPosition = pathFromOutermostToGivenNode.Pop();
                    matchedNestedNode = matchedNestedNode switch
                    {
                        _ when TupleExpressionSyntaxWrapper.IsInstance(matchedNestedNode) => StepDownInTuple((TupleExpressionSyntaxWrapper)matchedNestedNode, expectedPathPosition),
                        _ when ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(matchedNestedNode) =>
                            StepDownInParenthesizedVariableDesignation((ParenthesizedVariableDesignationSyntaxWrapper)matchedNestedNode, expectedPathPosition),
                        _ => null,
                    };
                }
                return matchedNestedNode;
            }

            static SyntaxNode PushPathPositionForTuple(Stack<PathPosition> pathPositions, TupleExpressionSyntaxWrapper tuple, ArgumentSyntax argument)
            {
                pathPositions.Push(new(tuple.Arguments.IndexOf(argument), tuple.Arguments.Count));
                return tuple.SyntaxNode.Parent;
            }

            static SyntaxNode PushPathPositionForParenthesizedDesignation(Stack<PathPosition> pathPositions,
                                                                         ParenthesizedVariableDesignationSyntaxWrapper parenthesizedDesignation,
                                                                         VariableDesignationSyntaxWrapper variable)
            {
                pathPositions.Push(new(parenthesizedDesignation.Variables.IndexOf(variable), parenthesizedDesignation.Variables.Count));
                return parenthesizedDesignation.SyntaxNode;
            }

            static SyntaxNode StepDownInParenthesizedVariableDesignation(ParenthesizedVariableDesignationSyntaxWrapper parenthesizedVariableDesignation, PathPosition expectedPathPosition) =>
                parenthesizedVariableDesignation.Variables.Count == expectedPathPosition.TupleLength
                    ? (SyntaxNode)parenthesizedVariableDesignation.Variables[expectedPathPosition.Index]
                    : null;

            static SyntaxNode StepDownInTuple(TupleExpressionSyntaxWrapper tupleExpression, PathPosition expectedPathPosition) =>
                tupleExpression.Arguments.Count == expectedPathPosition.TupleLength
                    ? (SyntaxNode)tupleExpression.Arguments[expectedPathPosition.Index].Expression
                    : null;
        }

        // This is a refactored version of internal Roslyn SyntaxNodeExtensions.IsInExpressionTree
        public static bool IsInExpressionTree(this SyntaxNode node, SemanticModel model)
        {
            return node.AncestorsAndSelf().Any(x => IsExpressionLambda(x) || IsExpressionSelectOrOrder(x) || IsExpressionQuery(x));

            bool IsExpressionLambda(SyntaxNode node) =>
                node is LambdaExpressionSyntax && model.GetTypeInfo(node).ConvertedType.DerivesFrom(KnownType.System_Linq_Expressions_Expression);

            bool IsExpressionSelectOrOrder(SyntaxNode node) =>
                node is SelectOrGroupClauseSyntax or OrderingSyntax && TakesExpressionTree(model.GetSymbolInfo(node));

            bool IsExpressionQuery(SyntaxNode node) =>
                node is QueryClauseSyntax queryClause
                && model.GetQueryClauseInfo(queryClause) is var info
                && (TakesExpressionTree(info.CastInfo) || TakesExpressionTree(info.OperationInfo));

            static bool TakesExpressionTree(SymbolInfo info)
            {
                var symbols = info.Symbol is null ? info.CandidateSymbols : ImmutableArray.Create(info.Symbol);
                return symbols.Any(x => x is IMethodSymbol method && method.Parameters.Length > 0 && method.Parameters[0].Type.DerivesFrom(KnownType.System_Linq_Expressions_Expression));
            }
        }

        /// <summary>
        /// Returns the left hand side of a conditional access expression. Returns c in case like a?.b?[0].c?.d.e?.f if d is passed.
        /// </summary>
        /// <remarks>Copied from <seealso href="https://github.com/dotnet/roslyn/blob/18f20b489/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L188">
        /// Roslyn SyntaxNodeExtensions</seealso></remarks>
        public static ConditionalAccessExpressionSyntax GetParentConditionalAccessExpression(this SyntaxNode node)
        {
            // Walk upwards based on the grammar/parser rules around ?. expressions (can be seen in
            // LanguageParser.ParseConsequenceSyntax).

            // These are the parts of the expression that the ?... expression can end with.  Specifically:
            //
            //  1.      x?.y.M()            // invocation
            //  2.      x?.y[...];          // element access
            //  3.      x?.y.z              // member access
            //  4.      x?.y                // member binding
            //  5.      x?[y]               // element binding
            var current = node;

            if ((current.IsParentKind(SyntaxKind.SimpleMemberAccessExpression, out MemberAccessExpressionSyntax memberAccess) && memberAccess.Name == current) ||
                (current.IsParentKind(SyntaxKind.MemberBindingExpression, out MemberBindingExpressionSyntax memberBinding) && memberBinding.Name == current))
            {
                current = current.Parent;
            }

            // Effectively, if we're on the RHS of the ? we have to walk up the RHS spine first until we hit the first
            // conditional access.
            while (current.IsAnyKind(
                SyntaxKind.InvocationExpression,
                SyntaxKind.ElementAccessExpression,
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.MemberBindingExpression,
                SyntaxKind.ElementBindingExpression,
                // Optional exclamations might follow the conditional operation. For example: a.b?.$$c!!!!()
                SyntaxKindEx.SuppressNullableWarningExpression) &&
                current.Parent is not ConditionalAccessExpressionSyntax)
            {
                current = current.Parent;
            }

            // Two cases we have to care about:
            //
            //      1. a?.b.$$c.d        and
            //      2. a?.b.$$c.d?.e...
            //
            // Note that `a?.b.$$c.d?.e.f?.g.h.i` falls into the same bucket as two.  i.e. the parts after `.e` are
            // lower in the tree and are not seen as we walk upwards.
            //
            //
            // To get the root ?. (the one after the `a`) we have to potentially consume the first ?. on the RHS of the
            // right spine (i.e. the one after `d`).  Once we do this, we then see if that itself is on the RHS of a
            // another conditional, and if so we hten return the one on the left.  i.e. for '2' this goes in this direction:
            //
            //      a?.b.$$c.d?.e           // it will do:
            //           ----->
            //       <---------
            //
            // Note that this only one CAE consumption on both sides.  GetRootConditionalAccessExpression can be used to
            // get the root parent in a case like:
            //
            //      x?.y?.z?.a?.b.$$c.d?.e.f?.g.h.i         // it will do:
            //                    ----->
            //                <---------
            //             <---
            //          <---
            //       <---
            if (current.IsParentKind(SyntaxKind.ConditionalAccessExpression, out ConditionalAccessExpressionSyntax conditional) &&
                conditional.Expression == current)
            {
                current = conditional;
            }

            if (current.IsParentKind(SyntaxKind.ConditionalAccessExpression, out conditional) &&
                conditional.WhenNotNull == current)
            {
                current = conditional;
            }

            return current as ConditionalAccessExpressionSyntax;
        }

        /// <summary>
        /// Call on the `.y` part of a `x?.y` to get the entire `x?.y` conditional access expression.  This also works
        /// when there are multiple chained conditional accesses.  For example, calling this on '.y' or '.z' in
        /// `x?.y?.z` will both return the full `x?.y?.z` node.  This can be used to effectively get 'out' of the RHS of
        /// a conditional access, and commonly represents the full standalone expression that can be operated on
        /// atomically.
        /// </summary>
        /// <remarks>Copied from Roslyn SyntaxNodeExtensions.</remarks>
        public static ConditionalAccessExpressionSyntax GetRootConditionalAccessExpression(this SyntaxNode node)
        {
            // Once we've walked up the entire RHS, now we continually walk up the conditional accesses until we're at
            // the root. For example, if we have `a?.b` and we're on the `.b`, this will give `a?.b`.  Similarly with
            // `a?.b?.c` if we're on either `.b` or `.c` this will result in `a?.b?.c` (i.e. the root of this CAE
            // sequence).
            var current = node.GetParentConditionalAccessExpression();
            while (current.IsParentKind(SyntaxKind.ConditionalAccessExpression, out ConditionalAccessExpressionSyntax conditional) &&
                conditional.WhenNotNull == current)
            {
                current = conditional;
            }

            return current;
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L347
        public static bool IsLeftSideOfAssignExpression(this SyntaxNode node)
            => node?.Parent is AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment &&
               assignment.Left == node;

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L43C1-L45C1
        public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
            => Microsoft.CodeAnalysis.CSharpExtensions.IsKind(node?.Parent, kind);

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L46
        public static bool IsParentKind<T>(this SyntaxNode node, SyntaxKind kind, out T result) where T : SyntaxNode
        {
            if (node?.Parent?.IsKind(kind) is true && node.Parent is T t)
            {
                result = t;
                return true;
            }
            result = null;
            return false;
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L351
        public static bool IsLeftSideOfAnyAssignExpression(this SyntaxNode node)
        {
            return node?.Parent != null &&
                node.Parent.IsAnyAssignExpression() &&
                ((AssignmentExpressionSyntax)node.Parent).Left == node;
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/SyntaxNodeExtensions.cs#L323
        public static bool IsAnyAssignExpression(this SyntaxNode node)
            => SyntaxFacts.IsAssignmentExpression(node.Kind());

        private static string GetUnknownType(SyntaxKind kind) =>

#if DEBUG

            throw new System.ArgumentException($"Unexpected type {kind}", nameof(kind));

#else

            "type";

#endif

        private readonly record struct PathPosition(int Index, int TupleLength);

        private sealed class ControlFlowGraphCache : ControlFlowGraphCacheBase
        {
            protected override bool IsLocalFunction(SyntaxNode node) =>
                node.IsKind(SyntaxKindEx.LocalFunctionStatement);

            protected override bool HasNestedCfg(SyntaxNode node) =>
                node.IsAnyKind(SyntaxKindEx.LocalFunctionStatement, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression, SyntaxKind.ParenthesizedLambdaExpression);
        }
    }
}
