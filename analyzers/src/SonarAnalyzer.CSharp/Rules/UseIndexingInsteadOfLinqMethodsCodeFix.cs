/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SonarAnalyzer.CSharp.Rules;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class UseIndexingInsteadOfLinqMethodsCodeFix : SonarCodeFix
{
    private const string Title = "Use indexing";

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UseIndexingInsteadOfLinqMethods.DiagnosticId);

    /// <inheritdoc />
    protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        if (context.FindNode(root).FirstAncestorOrSelf<InvocationExpressionSyntax>() is { } expression
            && await Index(expression, expression.ArgumentList.Arguments, context).ConfigureAwait(false) is { } index)
        {
            context.RegisterCodeFix(
                Title,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(Replace(root, expression, Args(index)))),
                context.Diagnostics);
        }
    }

    private static BracketedArgumentListSyntax Args(ExpressionSyntax index) =>
        BracketedArgumentList(SingletonSeparatedList(Argument(index)));

    private static SyntaxNode Replace(SyntaxNode root, InvocationExpressionSyntax expression, BracketedArgumentListSyntax arguments) =>
        expression.Parent is ConditionalAccessExpressionSyntax { Expression: var receiver } && IsArrayCreation(receiver)
            ? root.ReplaceNodes(
                [expression, receiver],
                (original, _) => Parenthesize(original, expression, receiver, arguments))
            : root.ReplaceNode(expression, Change(expression, arguments).WithTriviaFrom(expression));

    private static ExpressionSyntax Change(InvocationExpressionSyntax expression, BracketedArgumentListSyntax arguments) =>
        expression.Expression is MemberAccessExpressionSyntax member
            ? ElementAccessExpression(Parenthesize(member.Expression), arguments)
            : ElementBindingExpression(arguments);

    private static ExpressionSyntax Parenthesize(
        ExpressionSyntax original,
        InvocationExpressionSyntax expression,
        ExpressionSyntax receiver,
        BracketedArgumentListSyntax arguments) =>
        original == expression
            ? Change(expression, arguments).WithTriviaFrom(expression)
            : ParenthesizedExpression(receiver.WithoutTrivia()).WithTriviaFrom(receiver);

    private static ExpressionSyntax Parenthesize(ExpressionSyntax member) =>
        IsArrayCreation(member) ? ParenthesizedExpression(member) : member;

    private static async Task<ExpressionSyntax> Index(SyntaxNode expression, SeparatedSyntaxList<ArgumentSyntax> args, SonarCodeFixContext context) =>
        expression.GetName() switch
        {
            nameof(Enumerable.First) when args.Count is 0 => Int(0),
            nameof(Enumerable.Last) when args.Count is 0 && await CanUseIndexFromEnd(expression, context) => PrefixUnaryExpression(SyntaxKindEx.IndexExpression, Int(1)),
            nameof(Enumerable.ElementAt) when args.Count is 1 => args[0].Expression.WithoutTrivia(),
            _ => null,
        };

    private static LiteralExpressionSyntax Int(int value) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    private static bool IsArrayCreation(ExpressionSyntax expression) =>
        expression is ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax;

    // Index from end (^1) requires C# 8 and is not allowed in expression trees (CS8790).
    private static async Task<bool> CanUseIndexFromEnd(SyntaxNode node, SonarCodeFixContext context) =>
        node.SyntaxTree.Options is CSharpParseOptions options
        && options.LanguageVersion >= LanguageVersionEx.CSharp8
        && await context.Document.GetSemanticModelAsync(context.Cancel).ConfigureAwait(false) is { } model
        && !node.IsInExpressionTree(model);
}
