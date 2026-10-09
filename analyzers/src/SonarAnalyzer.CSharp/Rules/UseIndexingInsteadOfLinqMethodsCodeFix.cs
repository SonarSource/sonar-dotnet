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
    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        if (context.FindNode(root).FirstAncestorOrSelf<InvocationExpressionSyntax>() is { } expression
            && expression.GetName() is { } name
            && Index(name, expression.ArgumentList.Arguments) is { } index)
        {
            context.RegisterCodeFix(
                Title,
                _ => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(expression, Change(expression, Args(index)).WithTriviaFrom(expression)))),
                context.Diagnostics);
        }
        return Task.CompletedTask;
    }

    private static BracketedArgumentListSyntax Args(ExpressionSyntax index) =>
        BracketedArgumentList(SingletonSeparatedList(Argument(index)));

    private static ExpressionSyntax Change(InvocationExpressionSyntax expression, BracketedArgumentListSyntax arguments) =>
        expression.Expression is MemberAccessExpressionSyntax member
            ? ElementAccessExpression(member.Expression, arguments)
            : ElementBindingExpression(arguments);

    private static ExpressionSyntax Index(string method, SeparatedSyntaxList<ArgumentSyntax> args) =>
        method switch
        {
            nameof(Enumerable.First) when args.Count is 0 => Int(0),
            nameof(Enumerable.Last) when args.Count is 0 => PrefixUnaryExpression(SyntaxKindEx.IndexExpression, Int(1)),
            nameof(Enumerable.ElementAt) when args.Count is 1 => args[0].Expression.WithoutTrivia(),
            _ => null,
        };

    private static LiteralExpressionSyntax Int(int value) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
}
