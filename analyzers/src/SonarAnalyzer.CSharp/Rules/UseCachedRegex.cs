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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCachedRegex : SonarDiagnosticAnalyzer<SyntaxKind>
{
    private const int MinCtorParamsCount = 1;
    private const int MaxCtorParamsCount = 1;
    private const string RegexName = "Regex";
    private const string DiagnosticId = "S6614";

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override string MessageFormat => $"{RegexName} instances should be cached";

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
                if (CanBeCorrectObjectCreation(c.Node)
                    && IsWithinContext(c.Node, CorrectContextSyntaxKinds, out SyntaxNode kindContext)
                    && IsCorrectObjectType(c.Node, c.SemanticModel)
                    && IsArgumentConstantOrReadOnly(Language.Syntax.ArgumentExpressions(c.Node).FirstOrDefault(), c.SemanticModel)
                    && IsNoncompliant(c.Node, c.SemanticModel, kindContext))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            },
            Language.SyntaxKind.ObjectCreationExpressions);

    private bool CanBeCorrectObjectCreation(SyntaxNode objectCreation) =>
        (objectCreation.NameIs(RegexName)
         || ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(objectCreation))
        && HasRightArgumentCount(objectCreation);

    private bool HasRightArgumentCount(SyntaxNode objectCreation) =>
        Language.Syntax.ArgumentExpressions(objectCreation).Count() is >= MinCtorParamsCount and <= MaxCtorParamsCount;

    private static bool IsWithinContext<TSyntaxNode>(SyntaxNode node, SyntaxKind expectedContext, out TSyntaxNode contextNode) where TSyntaxNode : SyntaxNode =>
        IsWithinContext(node, new HashSet<SyntaxKind> { expectedContext }, out contextNode);

    private static bool IsWithinContext<TSyntaxNode>(SyntaxNode node, ISet<SyntaxKind> expectedContextList, out TSyntaxNode contextNode)
        where TSyntaxNode : SyntaxNode
    {
        while (!node.IsKind(SyntaxKind.CompilationUnit) && !node.IsAnyKind(expectedContextList))
        {
            node = node.Parent;
        }

        contextNode = !node.IsKind(SyntaxKind.CompilationUnit) ? (TSyntaxNode)node : null;

        return !node.IsKind(SyntaxKind.CompilationUnit);
    }

    private static bool IsCorrectObjectType(SyntaxNode objectCreation, SemanticModel model) =>
        model.GetTypeInfo(objectCreation).Type.Is(KnownType.System_Text_RegularExpressions_Regex);

    private static bool IsArgumentConstantOrReadOnly(SyntaxNode argument, SemanticModel model) =>
        argument is not null
        && (model.GetConstantValue(argument).HasValue
            || model.GetSymbolInfo(argument).Symbol is IFieldSymbol { IsReadOnly: true });

    private bool IsNoncompliant(SyntaxNode node, SemanticModel model, SyntaxNode context) =>
        node.Parent switch
        {
            EqualsValueClauseSyntax { Parent.Parent.Parent: LocalDeclarationStatementSyntax } => true,
            AssignmentExpressionSyntax assignment when IsWithinContext(assignment, SyntaxKind.CoalesceExpression, out BinaryExpressionSyntax coalesce) =>
                IsNoncompliantCondition(coalesce.Left, assignment.Left, model),
            AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) =>
                IsNoncompliantSimpleAssignment(assignment, model, context)
                && (!IsWithinContext(assignment, SyntaxKind.IfStatement, out IfStatementSyntax ifStatement)
                    || IsNoncompliantCondition(ifStatement.Condition, assignment.Left, model)
                    || !IsConditionCheckingForNull(ifStatement.Condition)),
            AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKindEx.CoalesceAssignmentExpression) =>
                !IsSymbolFieldOrProperty(assignment.Left, model),
            BinaryExpressionSyntax { Parent: AssignmentExpressionSyntax assignment } coalesce when coalesce.IsKind(SyntaxKind.CoalesceExpression) =>
                IsNoncompliant(coalesce, model, context)
                || IsNoncompliantCondition(coalesce.Left, assignment.Left, model),
            BinaryExpressionSyntax coalesce when coalesce.IsKind(SyntaxKind.CoalesceExpression) => true,
            ConditionalExpressionSyntax { Parent: AssignmentExpressionSyntax assignment, Condition: { } condition } conditional =>
                IsNoncompliant(conditional, model, context)
                || IsNoncompliantCondition(condition, assignment.Left, model)
                || IsNoncompliantBranches(conditional, assignment.Left),
            ArgumentSyntax => true,
            _ => false
        };

    private bool IsNoncompliantSimpleAssignment(AssignmentExpressionSyntax assignment, SemanticModel model, SyntaxNode context)
    {
        var symbol = model.GetSymbolInfo(assignment.Left).Symbol;

        return symbol is IParameterSymbol or ILocalSymbol
               || (assignment.Right.IsAnyKind(Language.SyntaxKind.ObjectCreationExpressions)
                   && symbol is IFieldSymbol { IsReadOnly: false } or IPropertySymbol { IsReadOnly: false }
                   && !context.IsKind(SyntaxKind.ConstructorDeclaration));
    }

    private static bool IsNoncompliantCondition(ExpressionSyntax condition, ExpressionSyntax left, SemanticModel model) =>
        condition switch
        {
            BinaryExpressionSyntax expression =>
                (!left.NameIs(expression.Left.GetName())
                 && !left.NameIs(expression.Right.GetName()))
                || (!expression.Left.IsKind(SyntaxKind.NullLiteralExpression)
                    && !expression.Right.IsKind(SyntaxKind.NullLiteralExpression)),
            _ when IsPatternExpressionSyntaxWrapper.IsInstance(condition) =>
                !((IsPatternExpressionSyntaxWrapper)condition).Expression.NameIs(left.GetName())
                || IsNoncompliantPatternCondition(((IsPatternExpressionSyntaxWrapper)condition).Pattern, model),
            ObjectCreationExpressionSyntax => true,
            IdentifierNameSyntax identifier =>
                !identifier.NameIs(left.GetName())
                || !IsSymbolFieldOrProperty(identifier, model),
            _ => false,
        };

    private static bool IsNoncompliantPatternCondition(PatternSyntaxWrapper pattern, SemanticModel model) =>
        pattern.SyntaxNode switch
        {
            SyntaxNode node when ConstantPatternSyntaxWrapper.IsInstance(node) =>
                !((ConstantPatternSyntaxWrapper)node).Expression.IsKind(SyntaxKind.NullLiteralExpression)
                && !model.GetTypeInfo(((ConstantPatternSyntaxWrapper)node).Expression).Type.Is(KnownType.System_Text_RegularExpressions_Regex),
            SyntaxNode node when UnaryPatternSyntaxWrapper.IsInstance(node) =>
                IsNoncompliantNotPatternCondition(((UnaryPatternSyntaxWrapper)node).Pattern, model),
            SyntaxNode node when DeclarationPatternSyntaxWrapper.IsInstance(node) =>
                !model.GetTypeInfo(((DeclarationPatternSyntaxWrapper)node).Type).Type.Is(KnownType.System_Text_RegularExpressions_Regex),
            SyntaxNode node when RecursivePatternSyntaxWrapper.IsInstance(node) => true,
            _ => false
        };

    private static bool IsNoncompliantNotPatternCondition(PatternSyntaxWrapper pattern, SemanticModel model) =>
        pattern.SyntaxNode switch
        {
            SyntaxNode node when RecursivePatternSyntaxWrapper.IsInstance(node) =>
                ((RecursivePatternSyntaxWrapper)node).PropertyPatternClause.Subpatterns.Count > 0,
            _ => IsNoncompliantPatternCondition(pattern, model)
        };

    private static bool IsNoncompliantBranches(ConditionalExpressionSyntax conditional, ExpressionSyntax left)
    {
        var isCheckingNull = IsConditionCheckingForNull(conditional.Condition);

        return (!isCheckingNull
                && (!conditional.WhenTrue.NameIs(left.GetName())
                    || !conditional.WhenFalse.IsKind(SyntaxKind.ObjectCreationExpression)))
               || (isCheckingNull
                   && (!conditional.WhenTrue.IsKind(SyntaxKind.ObjectCreationExpression)
                       || !conditional.WhenFalse.NameIs(left.GetName())));
    }

    private static bool IsConditionCheckingForNull(ExpressionSyntax condition) =>
        condition switch
        {
            BinaryExpressionSyntax expression when condition.IsKind(SyntaxKind.EqualsExpression) =>
                expression.Left.IsKind(SyntaxKind.NullLiteralExpression) || expression.Right.IsKind(SyntaxKind.NullLiteralExpression),
            _ when IsPatternExpressionSyntaxWrapper.IsInstance(condition) =>
                IsPatternCheckingForNull(((IsPatternExpressionSyntaxWrapper)condition).Pattern),
            _ => false,
        };

    private static bool IsPatternCheckingForNull(PatternSyntaxWrapper pattern) =>
        pattern.SyntaxNode switch
        {
            SyntaxNode node when ConstantPatternSyntaxWrapper.IsInstance(node) =>
                ((ConstantPatternSyntaxWrapper)node).Expression.IsKind(SyntaxKind.NullLiteralExpression)
                || !((ConstantPatternSyntaxWrapper)node).Expression.NameIs(RegexName),
            SyntaxNode node when RecursivePatternSyntaxWrapper.IsInstance(node) => false,
            SyntaxNode node when DeclarationPatternSyntaxWrapper.IsInstance(node) =>
                !((DeclarationPatternSyntaxWrapper)node).Type.NameIs(RegexName),
            SyntaxNode node when UnaryPatternSyntaxWrapper.IsInstance(node) =>
                !IsPatternCheckingForNull(((UnaryPatternSyntaxWrapper)node).Pattern),
            _ => false,
        };

    private static bool IsSymbolFieldOrProperty(SyntaxNode identifier, SemanticModel model) => model.GetSymbolInfo(identifier).Symbol is IFieldSymbol or IPropertySymbol;
}
