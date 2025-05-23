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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Extensions;

public static class SyntaxNodeExtensionsCSharp
{
    private static readonly ControlFlowGraphCache CfgCache = new();
    private static readonly SyntaxKind[] ParenthesizedNodeKinds = [SyntaxKind.ParenthesizedExpression, SyntaxKindEx.ParenthesizedPattern];

    private static readonly SyntaxKind[] EnclosingScopeSyntaxKinds = [
        SyntaxKind.AddAccessorDeclaration,
        SyntaxKind.AnonymousMethodExpression,
        SyntaxKind.BaseConstructorInitializer,
        SyntaxKind.CompilationUnit,
        SyntaxKind.ConstructorDeclaration,
        SyntaxKind.ConversionOperatorDeclaration,
        SyntaxKind.DestructorDeclaration,
        SyntaxKind.EnumMemberDeclaration,
        SyntaxKind.FieldDeclaration,
        SyntaxKind.GetAccessorDeclaration,
        SyntaxKind.GroupClause,
        SyntaxKindEx.InitAccessorDeclaration,
        SyntaxKind.JoinClause,
        SyntaxKind.LetClause,
        SyntaxKindEx.LocalFunctionStatement,
        SyntaxKind.MethodDeclaration,
        SyntaxKind.OrderByClause,
        SyntaxKind.OperatorDeclaration,
        SyntaxKind.Parameter,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKindEx.PrimaryConstructorBaseType,
        SyntaxKind.PropertyDeclaration,
        SyntaxKind.RemoveAccessorDeclaration,
        SyntaxKind.QueryContinuation,
        SyntaxKind.SelectClause,
        SyntaxKind.SetAccessorDeclaration,
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.ThisConstructorInitializer,
        SyntaxKind.WhereClause];

    private static readonly SyntaxKind[] NegationOrConditionEnclosingSyntaxKinds = [
        SyntaxKind.AnonymousMethodExpression,
        SyntaxKind.BitwiseNotExpression,
        SyntaxKind.ConditionalExpression,
        SyntaxKind.IfStatement,
        SyntaxKind.MethodDeclaration,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.WhileStatement];

    public static ControlFlowGraph CreateCfg(this SyntaxNode node, SemanticModel model, CancellationToken cancel) =>
        CfgCache.FindOrCreate(node, model, cancel);

    public static bool ContainsConditionalConstructs(this SyntaxNode node) =>
        node is not null && node.DescendantNodes().Any(x => x.Kind() is SyntaxKind.IfStatement
                or SyntaxKind.ConditionalExpression
                or SyntaxKind.CoalesceExpression
                or SyntaxKind.SwitchStatement
                or SyntaxKindEx.SwitchExpression
                or SyntaxKindEx.CoalesceAssignmentExpression);

    public static object FindConstantValue(this SyntaxNode node, SemanticModel semanticModel) =>
        new CSharpConstantValueFinder(semanticModel).FindConstant(node);

    public static string FindStringConstant(this SyntaxNode node, SemanticModel semanticModel) =>
        FindConstantValue(node, semanticModel) as string;

    public static bool IsPartOfBinaryNegationOrCondition(this SyntaxNode node)
    {
        if (node.Parent is not MemberAccessExpressionSyntax)
        {
            return false;
        }

        var topNode = node.Parent.GetSelfOrTopParenthesizedExpression();
        if (topNode.Parent?.IsKind(SyntaxKind.BitwiseNotExpression) ?? false)
        {
            return true;
        }

        var current = topNode;
        while (current.Parent is not null && !NegationOrConditionEnclosingSyntaxKinds.Contains(current.Parent.Kind()))
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
            SyntaxKindEx.RecordDeclaration => "record",
            SyntaxKindEx.RecordStructDeclaration => "record struct",
            SyntaxKind.StructDeclaration => "struct",
#if DEBUG
            _ => throw new UnexpectedValueException("node.Kind()", node.Kind().ToString())
#else
            _ => "type"
#endif
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
            ArgumentSyntax { NameColon.Name.Identifier: var identifier } => identifier,
            ArrayTypeSyntax { ElementType: { } elementType } => GetIdentifier(elementType),
            AttributeArgumentSyntax { NameColon.Name.Identifier: var identifier } => identifier,
            AttributeArgumentSyntax { NameEquals.Name.Identifier: var identifier } => identifier,
            AttributeSyntax { Name: { } name } => GetIdentifier(name),
            BaseTypeDeclarationSyntax { Identifier: var identifier } => identifier,
            ConditionalAccessExpressionSyntax { WhenNotNull: var rightSide } => GetIdentifier(rightSide),
            ConstructorDeclarationSyntax { Identifier: var identifier } => identifier,
            ConstructorInitializerSyntax { ThisOrBaseKeyword: var keyword } => keyword,
            ConversionOperatorDeclarationSyntax { Type: { } type } => GetIdentifier(type),
            DelegateDeclarationSyntax { Identifier: var identifier } => identifier,
            DestructorDeclarationSyntax { Identifier: var identifier } => identifier,
            EnumMemberDeclarationSyntax { Identifier: var identifier } => identifier,
            EventDeclarationSyntax { Identifier: var identifier } => identifier,
            IndexerDeclarationSyntax { ThisKeyword: var thisKeyword } => thisKeyword,
            InvocationExpressionSyntax
            {
                Expression: not InvocationExpressionSyntax // We don't want to recurse into nested invocations like: fun()()
            } invocation => GetIdentifier(invocation.Expression),
            MemberAccessExpressionSyntax { Name.Identifier: var identifier } => identifier,
            MemberBindingExpressionSyntax { Name.Identifier: var identifier } => identifier,
            MethodDeclarationSyntax { Identifier: var identifier } => identifier,
            NameColonSyntax nameColon => nameColon.Name.Identifier,
            NamespaceDeclarationSyntax { Name: { } name } => GetIdentifier(name),
            NullableTypeSyntax { ElementType: { } elementType } => GetIdentifier(elementType),
            ObjectCreationExpressionSyntax { Type: var type } => GetIdentifier(type),
            OperatorDeclarationSyntax { OperatorToken: var operatorToken } => operatorToken,
            ParameterSyntax { Identifier: var identifier } => identifier,
            ParenthesizedExpressionSyntax { Expression: { } expression } => GetIdentifier(expression),
            PropertyDeclarationSyntax { Identifier: var identifier } => identifier,
            PointerTypeSyntax { ElementType: { } elementType } => GetIdentifier(elementType),
            PredefinedTypeSyntax { Keyword: var keyword } => keyword,
            QualifiedNameSyntax { Right.Identifier: var identifier } => identifier,
            SimpleBaseTypeSyntax { Type: { } type } => GetIdentifier(type),
            SimpleNameSyntax { Identifier: var identifier } => identifier,
            TypeParameterConstraintClauseSyntax { Name.Identifier: var identifier } => identifier,
            TypeParameterSyntax { Identifier: var identifier } => identifier,
            PrefixUnaryExpressionSyntax { Operand: { } operand } => GetIdentifier(operand),
            PostfixUnaryExpressionSyntax { Operand: { } operand } => GetIdentifier(operand),
            UsingDirectiveSyntax { Alias.Name: { } name } => GetIdentifier(name),
            VariableDeclaratorSyntax { Identifier: var identifier } => identifier,
            { } implicitNew when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(implicitNew) => ((ImplicitObjectCreationExpressionSyntaxWrapper)implicitNew).NewKeyword,
            { } fileScoped when FileScopedNamespaceDeclarationSyntaxWrapper.IsInstance(fileScoped)
                && ((FileScopedNamespaceDeclarationSyntaxWrapper)fileScoped).Name is { } name => GetIdentifier(name),
            { } primary when PrimaryConstructorBaseTypeSyntaxWrapper.IsInstance(primary)
                && ((PrimaryConstructorBaseTypeSyntaxWrapper)primary).Type is { } type => GetIdentifier(type),
            { } refType when RefTypeSyntaxWrapper.IsInstance(refType) => GetIdentifier(((RefTypeSyntaxWrapper)refType).Type),
            { } subPattern when SubpatternSyntaxWrapper.IsInstance(subPattern) => GetIdentifier(((SubpatternSyntaxWrapper)subPattern).ExpressionColon.Expression),
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

    // based on Type="BaseArgumentListSyntax" in https://github.com/dotnet/roslyn/blob/main/src/Compilers/CSharp/Portable/Syntax/Syntax.xml
    public static BaseArgumentListSyntax ArgumentList(this SyntaxNode node) =>
        node switch
        {
            ObjectCreationExpressionSyntax creation => creation.ArgumentList,
            InvocationExpressionSyntax invocation => invocation.ArgumentList,
            ConstructorInitializerSyntax constructorInitializer => constructorInitializer.ArgumentList,
            ElementAccessExpressionSyntax x => x.ArgumentList,
            ElementBindingExpressionSyntax x => x.ArgumentList,
            null => null,
            _ when PrimaryConstructorBaseTypeSyntaxWrapper.IsInstance(node) => ((PrimaryConstructorBaseTypeSyntaxWrapper)node).ArgumentList,
            _ when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(node) => ((ImplicitObjectCreationExpressionSyntaxWrapper)node).ArgumentList,
            _ => throw new InvalidOperationException($"The {nameof(node)} of kind {node.Kind()} does not have an {nameof(ArgumentList)}."),
        };

    public static ParameterListSyntax ParameterList(this SyntaxNode node) =>
        node switch
        {
            BaseMethodDeclarationSyntax method => method.ParameterList,
            TypeDeclarationSyntax type => type.ParameterList(),
            { RawKind: (int)SyntaxKindEx.LocalFunctionStatement } localFunction => ((LocalFunctionStatementSyntaxWrapper)localFunction).ParameterList,
            ParenthesizedLambdaExpressionSyntax lambda => lambda.ParameterList,
            AnonymousMethodExpressionSyntax anonymous => anonymous.ParameterList,
            DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.ParameterList,
            _ => default,
        };

    public static BlockSyntax GetBody(this SyntaxNode node) =>
        node switch
        {
            BaseMethodDeclarationSyntax method => method.Body,
            AccessorDeclarationSyntax accessor => accessor.Body,
            _ when LocalFunctionStatementSyntaxWrapper.IsInstance(node) => ((LocalFunctionStatementSyntaxWrapper)node).Body,
            _ => null,
        };

    public static SyntaxNode GetInitializer(this SyntaxNode node) =>
        node switch
        {
            VariableDeclaratorSyntax { Initializer: { } initializer } => initializer,
            PropertyDeclarationSyntax { Initializer: { } initializer } => initializer,
            _ => null
        };

    public static SyntaxTokenList GetModifiers(this SyntaxNode node) =>
        node switch
        {
            AccessorDeclarationSyntax accessor => accessor.Modifiers,
            MemberDeclarationSyntax member => member.Modifiers(),
            _ => default,
        };

    public static bool IsTrue(this SyntaxNode node) =>
        node switch
        {
            { RawKind: (int)SyntaxKind.TrueLiteralExpression } => true, // true
            { RawKind: (int)SyntaxKind.LogicalNotExpression } => IsFalse(((PrefixUnaryExpressionSyntax)node).Operand), // !false
            { RawKind: (int)SyntaxKindEx.ConstantPattern } => IsTrue(((ConstantPatternSyntaxWrapper)node).Expression), // is true
            { RawKind: (int)SyntaxKindEx.NotPattern } => IsFalse(((UnaryPatternSyntaxWrapper)node).Pattern), // is not false
            { RawKind: (int)SyntaxKind.ParenthesizedExpression } => IsTrue(((ParenthesizedExpressionSyntax)node).Expression), // (true)
            { RawKind: (int)SyntaxKindEx.ParenthesizedPattern } => IsTrue(((ParenthesizedPatternSyntaxWrapper)node).Pattern), // is (true)
            _ => false,
        };

    public static bool IsFalse(this SyntaxNode node) =>
        node switch
        {
            { RawKind: (int)SyntaxKind.FalseLiteralExpression } => true, // false
            { RawKind: (int)SyntaxKind.LogicalNotExpression } => IsTrue(((PrefixUnaryExpressionSyntax)node).Operand), // !true
            { RawKind: (int)SyntaxKindEx.ConstantPattern } => IsFalse(((ConstantPatternSyntaxWrapper)node).Expression), // is false
            { RawKind: (int)SyntaxKindEx.NotPattern } => IsTrue(((UnaryPatternSyntaxWrapper)node).Pattern), // is not true
            { RawKind: (int)SyntaxKind.ParenthesizedExpression } => IsFalse(((ParenthesizedExpressionSyntax)node).Expression), // (false)
            { RawKind: (int)SyntaxKindEx.ParenthesizedPattern } => IsFalse(((ParenthesizedPatternSyntaxWrapper)node).Pattern), // is (false)
            _ => false,
        };

    public static SyntaxNode EnclosingScope(this SyntaxNode node) =>
        node.AncestorsAndSelf().FirstOrDefault(x => x.IsAnyKind(EnclosingScopeSyntaxKinds));

    private readonly record struct PathPosition(int Index, int TupleLength);

    private sealed class ControlFlowGraphCache : ControlFlowGraphCacheBase
    {
        protected override bool IsLocalFunction(SyntaxNode node) =>
            node.IsKind(SyntaxKindEx.LocalFunctionStatement);

        protected override bool HasNestedCfg(SyntaxNode node) =>
            node.Kind() is SyntaxKindEx.LocalFunctionStatement
                or SyntaxKind.SimpleLambdaExpression
                or SyntaxKind.AnonymousMethodExpression
                or SyntaxKind.ParenthesizedLambdaExpression;
    }
}
