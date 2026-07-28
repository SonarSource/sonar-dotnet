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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EmptyMethod : EmptyMethodBase<SyntaxKind>
{
    internal static readonly HashSet<SyntaxKind> SupportedSyntaxKinds =
    [
        SyntaxKind.MethodDeclaration,
        SyntaxKindEx.LocalFunctionStatement,
        SyntaxKind.SetAccessorDeclaration,
        SyntaxKindEx.InitAccessorDeclaration
    ];

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override HashSet<SyntaxKind> SyntaxKinds => SupportedSyntaxKinds;

    protected override void CheckMethod(SonarSyntaxNodeReportingContext context)
    {
        // No need to check for ExpressionBody as arrowed methods can't be empty
        if (context.Node.GetBody() is { } body
            && body.IsEmpty()
            && !ShouldBeExcluded(context, context.Node, context.Node.GetModifiers()))
        {
            context.ReportIssue(Rule, ReportingToken(context.Node));
        }
    }

    private static bool ShouldBeExcluded(SonarSyntaxNodeReportingContext context, SyntaxNode node, SyntaxTokenList modifiers) =>
        modifiers.Any(SyntaxKind.VirtualKeyword) // This quick check only works for methods, for accessors we need to check the symbol
        || (context.Model.GetDeclaredSymbol(node) is IMethodSymbol symbol
            && (symbol is { IsVirtual: true }
                || symbol is { IsOverride: true, OverriddenMethod.IsAbstract: true }
                || !symbol.ExplicitOrImplicitInterfaceImplementations().IsEmpty
                || IsAwaiterGetResult(symbol)))
        || (modifiers.Any(SyntaxKind.OverrideKeyword) && context.IsTestProject());

    // An awaitable type must have a GetAwaiter() method that returns a type that has an IsCompleted property, a GetResult() method, and implements INotifyCompletion.
    // GetResult() is required for this implementation and should not raise.
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions#12992-awaitable-expressions
    private static bool IsAwaiterGetResult(IMethodSymbol symbol) =>
        symbol is { IsStatic: false, Parameters.IsEmpty: true, Arity: 0 }
        && symbol.Name == WellKnownMemberNames.GetResult
        && symbol.ContainingType.Implements(KnownType.System_Runtime_CompilerServices_INotifyCompletion);

    private static SyntaxToken ReportingToken(SyntaxNode node) =>
        node switch
        {
            MethodDeclarationSyntax method => method.Identifier,
            AccessorDeclarationSyntax accessor => accessor.Keyword,
            _ => ((LocalFunctionStatementSyntaxWrapper)node).Identifier
        };
}
