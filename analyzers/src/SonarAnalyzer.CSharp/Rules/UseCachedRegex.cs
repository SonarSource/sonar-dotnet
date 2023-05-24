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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCachedRegex : SonarDiagnosticAnalyzer<SyntaxKind>
{
    private const string DiagnosticId = "S6614";

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override string MessageFormat => $"{nameof(Regex)} instances should be cached";

    private static readonly HashSet<SyntaxKind> CorrectContextSyntaxKinds = new()
    {
        SyntaxKind.MethodDeclaration,
        SyntaxKind.ConstructorDeclaration,
        SyntaxKind.DestructorDeclaration,
        SyntaxKind.GetAccessorDeclaration,
        SyntaxKind.SetAccessorDeclaration,
        SyntaxKindEx.LocalFunctionStatement,
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.AnonymousMethodExpression
    };

    public UseCachedRegex() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var objectCreation = c.Node as ObjectCreationExpressionSyntax;
                ImplicitObjectCreationExpressionSyntaxWrapper? implicitObjectCreation = objectCreation is null ? (ImplicitObjectCreationExpressionSyntaxWrapper)c.Node : null;

                if (IsCorrectName(c.Node)
                    && IsWithinCorrectContext(c.Node, out var kindContext)
                    && IsCorrectObjectType(c.Node, c.SemanticModel)
                    && IsArgumentConstantOrReadOnly(objectCreation?.ArgumentList ?? implicitObjectCreation?.ArgumentList, c.SemanticModel)
                    && !IsCompliantAssignment(c.Node, c.SemanticModel, kindContext))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            SyntaxKind.ObjectCreationExpression,
            SyntaxKindEx.ImplicitObjectCreationExpression);

    private static bool IsCorrectName(SyntaxNode node) =>
        node.NameIs(nameof(Regex)) || node is { RawKind: (int)SyntaxKindEx.ImplicitObjectCreationExpression }; // We check the TypeInfo in IsCorrectObjectType

    private static bool IsWithinCorrectContext(SyntaxNode node, out SyntaxKind context)
    {
        context = SyntaxKind.None;
        while (!node.IsKind(SyntaxKind.CompilationUnit))
        {
            if (node.Parent.IsAnyKind(CorrectContextSyntaxKinds))
            {
                context = (SyntaxKind)node.Parent.RawKind;
                return true;
            }

            node = node.Parent;
        }

        return false;
    }

    private static bool IsCorrectObjectType(SyntaxNode objectCreation, SemanticModel model) =>
        model.GetTypeInfo(objectCreation).Type.Is(KnownType.System_Text_RegularExpressions_Regex);

    private static bool IsArgumentConstantOrReadOnly(ArgumentListSyntax argumentList, SemanticModel model) =>
        argumentList?.Arguments[0].Expression is { } expression
        && (model.GetConstantValue(expression).HasValue
            || model.GetSymbolInfo(expression).Symbol is IFieldSymbol { IsReadOnly: true });

    private static bool IsCompliantAssignment(SyntaxNode objectCreation, SemanticModel model, SyntaxKind context) =>
        objectCreation.Parent switch
        {
            AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) => IsCompliantSimpleAssignment(assignment, model, context),
            AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKindEx.CoalesceAssignmentExpression) =>
                IsSymbolFieldOrProperty(assignment.Left, model),
            BinaryExpressionSyntax { Parent: AssignmentExpressionSyntax assignment } coalesce
                when coalesce.IsKind(SyntaxKind.CoalesceExpression) && assignment.IsAnyKind(SyntaxKind.SimpleAssignmentExpression, SyntaxKindEx.CoalesceAssignmentExpression) =>
                coalesce.Left.GetName() == assignment.Left.GetName() && IsSymbolFieldOrProperty(assignment.Left, model),
            ConditionalExpressionSyntax { Parent: AssignmentExpressionSyntax assignment, Condition: { } condition } conditional
                when assignment.IsAnyKind(SyntaxKind.SimpleAssignmentExpression, SyntaxKindEx.CoalesceAssignmentExpression) =>
                IsConditionInvolveAssigned(condition, assignment)
                && IsCompliantConditionalResultBranches(conditional, condition, assignment),
            _ => false
        };

    private static bool IsSymbolFieldOrProperty(SyntaxNode identifier, SemanticModel model) => model.GetSymbolInfo(identifier).Symbol is IFieldSymbol or IPropertySymbol;

    private static bool IsCompliantSimpleAssignment(AssignmentExpressionSyntax assignment, SemanticModel model, SyntaxKind context)
    {
        var symbol = model.GetSymbolInfo(assignment.Left).Symbol;

        return symbol is IFieldSymbol or IPropertySymbol
               && (symbol is IFieldSymbol { IsReadOnly: true } or IPropertySymbol { IsReadOnly: true }
                   || context == SyntaxKind.ConstructorDeclaration
                   || IsAssignmentWithinIfStatement(assignment, model)
                   || IsAssignmentWithinCoalesceExpressionAsFunctionArgument(assignment));
    }

    private static bool IsCompliantConditionalResultBranches(ConditionalExpressionSyntax conditional, ExpressionSyntax condition, AssignmentExpressionSyntax assignment)
    {
        return condition switch
        {
            BinaryExpressionSyntax when condition.IsAnyKind(SyntaxKind.EqualsExpression) => CheckCorrectBranchResult(conditional.WhenTrue, conditional.WhenFalse),
            BinaryExpressionSyntax when condition.IsAnyKind(SyntaxKind.NotEqualsExpression) => CheckCorrectBranchResult(conditional.WhenFalse, conditional.WhenTrue),
            { RawKind: (int)SyntaxKindEx.IsPatternExpression } =>
                (IsPatternExpressionSyntaxWrapper)condition switch
                {
                    { Pattern.SyntaxNode.RawKind: (int)SyntaxKindEx.ConstantPattern } => CheckCorrectBranchResult(conditional.WhenTrue, conditional.WhenFalse),
                    { Pattern.SyntaxNode.RawKind: (int)SyntaxKindEx.NotPattern } => CheckCorrectBranchResult(conditional.WhenFalse, conditional.WhenTrue),
                    _ => false
                },
            _ => false
        };

        bool CheckCorrectBranchResult(ExpressionSyntax first, ExpressionSyntax second) =>
            first is ObjectCreationExpressionSyntax && second.GetName() == assignment.Left.GetName();
    }

    private static bool IsConditionInvolveAssigned(ExpressionSyntax condition, AssignmentExpressionSyntax assignment) =>
        condition switch
        {
            BinaryExpressionSyntax binaryCondition when condition.IsAnyKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression) =>
                ChecksForNullOnAssigned(assignment, binaryCondition.Left, binaryCondition.Right) || ChecksForNullOnAssigned(assignment, binaryCondition.Right, binaryCondition.Left),
            { RawKind: (int)SyntaxKindEx.IsPatternExpression } =>
                (IsPatternExpressionSyntaxWrapper)condition switch
                {
                    { Expression: { } expression, Pattern: { SyntaxNode.RawKind: (int)SyntaxKindEx.ConstantPattern } pattern } =>
                        ChecksForNullOnAssigned(assignment, expression, ((ConstantPatternSyntaxWrapper)pattern).Expression),
                    { Expression: { } expression, Pattern: { SyntaxNode.RawKind: (int)SyntaxKindEx.NotPattern } notPattern } =>
                        (UnaryPatternSyntaxWrapper)notPattern is { Pattern: { SyntaxNode.RawKind: (int)SyntaxKindEx.ConstantPattern } pattern }
                        && ChecksForNullOnAssigned(assignment, expression, ((ConstantPatternSyntaxWrapper)pattern).Expression),
                    _ => false
                },
            _ => false
        };

    private static bool ChecksForNullOnAssigned(AssignmentExpressionSyntax assignmentExpression, SyntaxNode first, SyntaxNode second) =>
        assignmentExpression.Left.GetName() == first.GetName() && second is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

    private static bool IsAssignmentWithinIfStatement(AssignmentExpressionSyntax assignment, SemanticModel model)
    {
        SyntaxNode node = assignment;
        while (!node.IsAnyKind(SyntaxKind.CompilationUnit, SyntaxKind.IfStatement))
        {
            node = node.Parent;
        }

        return node is IfStatementSyntax { Condition: { } condition }
               && condition switch
               {
                   BinaryExpressionSyntax binaryExpression  => IsValidBinaryCondition(binaryExpression),
                   { RawKind: (int)SyntaxKindEx.IsPatternExpression } => IsValidPatternCondition(condition),
                   _ => false,
               };

        bool IsValidBinaryCondition(BinaryExpressionSyntax binaryExpression) =>
            binaryExpression.IsKind(SyntaxKind.EqualsExpression)
            && binaryExpression.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken)
            && (ChecksForNullOnAssigned(assignment, binaryExpression.Left, binaryExpression.Right)
                || ChecksForNullOnAssigned(assignment, binaryExpression.Right, binaryExpression.Left))
            && IsSymbolFieldOrProperty(assignment.Left, model);

        bool IsValidPatternCondition(ExpressionSyntax isPattern) =>
            (IsPatternExpressionSyntaxWrapper)isPattern is { Expression: { } expression, Pattern: { SyntaxNode.RawKind: (int)SyntaxKindEx.ConstantPattern } pattern }
            && ChecksForNullOnAssigned(assignment, expression, ((ConstantPatternSyntaxWrapper)pattern).Expression)
            && IsSymbolFieldOrProperty(assignment.Left, model);
    }

    private static bool IsAssignmentWithinCoalesceExpressionAsFunctionArgument(AssignmentExpressionSyntax assignment) =>
        assignment.Parent is ParenthesizedExpressionSyntax { Parent: BinaryExpressionSyntax coalesce }
        && coalesce.IsKind(SyntaxKind.CoalesceExpression);
}
