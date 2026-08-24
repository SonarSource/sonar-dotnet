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
public sealed class CatchRethrow : CatchRethrowBase<SyntaxKind, CatchClauseSyntax>
{
    private static readonly BlockSyntax ThrowBlock = SyntaxFactory.Block(SyntaxFactory.ThrowStatement());

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool ContainsOnlyThrow(CatchClauseSyntax currentCatch) =>
        CSharpEquivalenceChecker.AreEquivalent(currentCatch.Block, ThrowBlock);

    protected override CatchClauseSyntax[] AllCatches(SyntaxNode node) =>
        ((TryStatementSyntax)node).Catches.ToArray();

    protected override SyntaxNode DeclarationType(CatchClauseSyntax catchClause) =>
        catchClause.Declaration?.Type;

    protected override bool HasFilter(CatchClauseSyntax catchClause) =>
        catchClause.Filter is not null;

    /// <summary>Determines whether a bare rethrow restores a recognized temporary context.</summary>
    /// <param name="catchClause">The catch clause being analyzed.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the catch clause forms a required unwind boundary.</returns>
    protected override bool IsTemporaryContextBoundary(CatchClauseSyntax catchClause, SemanticModel semanticModel) =>
        catchClause.Parent is TryStatementSyntax tryStatement
        && (HasTemporaryContextInvocation(tryStatement, semanticModel)
            || HasWindowsImpersonationUsing(tryStatement, semanticModel)
            || HasCodeAccessPermissionBoundary(tryStatement, semanticModel));

    /// <summary>Determines whether a try block invokes a temporary-context API.</summary>
    /// <param name="tryStatement">The try statement associated with the catch clause.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the try block invokes a recognized API.</returns>
    private static bool HasTemporaryContextInvocation(TryStatementSyntax tryStatement, SemanticModel semanticModel) =>
        tryStatement.Block.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsInLocalTryScope(invocation, tryStatement)
                               && (IsMethod(semanticModel, invocation, KnownType.System_Threading_ExecutionContext, "Run")
                                   || IsMethod(semanticModel, invocation, KnownType.System_Security_Principal_WindowsIdentity, "RunImpersonated")));

    /// <summary>Determines whether a try block uses a legacy Windows impersonation scope.</summary>
    /// <param name="tryStatement">The try statement associated with the catch clause.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the try block uses a recognized impersonation scope.</returns>
    private static bool HasWindowsImpersonationUsing(TryStatementSyntax tryStatement, SemanticModel semanticModel) =>
        tryStatement.Block.DescendantNodes().OfType<UsingStatementSyntax>()
            .Any(usingStatement => IsInLocalTryScope(usingStatement, tryStatement) && IsWindowsImpersonationUsing(usingStatement, semanticModel));

    /// <summary>Determines whether a using statement owns a Windows impersonation context.</summary>
    /// <param name="usingStatement">The using statement to inspect.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the statement owns the recognized context.</returns>
    private static bool IsWindowsImpersonationUsing(UsingStatementSyntax usingStatement, SemanticModel semanticModel) =>
        usingStatement.Expression is { } expression
            ? IsWindowsImpersonationContext(expression, semanticModel)
            : usingStatement.Declaration is { } declaration && IsWindowsImpersonationUsing(declaration, semanticModel);

    /// <summary>Determines whether a using declaration owns a Windows impersonation context.</summary>
    /// <param name="declaration">The declaration to inspect.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the declaration owns the recognized context.</returns>
    private static bool IsWindowsImpersonationUsing(VariableDeclarationSyntax declaration, SemanticModel semanticModel) =>
        declaration.Variables.Any(variable => variable.Initializer?.Value is { } expression && IsWindowsImpersonationContext(expression, semanticModel));

    /// <summary>Determines whether an expression creates a Windows impersonation context.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the expression invokes the recognized API.</returns>
    private static bool IsWindowsImpersonationContext(ExpressionSyntax expression, SemanticModel semanticModel) =>
        expression is InvocationExpressionSyntax invocation
        && IsMethod(semanticModel, invocation, KnownType.System_Security_Principal_WindowsIdentity, "Impersonate")
        && semanticModel.GetTypeInfo(expression).Type.Is(KnownType.System_Security_Principal_WindowsImpersonationContext);

    /// <summary>Determines whether an inner finally restores a code-access assertion.</summary>
    /// <param name="tryStatement">The try statement associated with the catch clause.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the try block has a recognized assertion boundary.</returns>
    private static bool HasCodeAccessPermissionBoundary(TryStatementSyntax tryStatement, SemanticModel semanticModel) =>
        tryStatement.Block.DescendantNodes().OfType<TryStatementSyntax>()
            .Where(innerTry => innerTry.Finally is not null && IsInLocalTryScope(innerTry, tryStatement))
            .Any(innerTry => ContainsRevertAssert(innerTry.Finally.Block, semanticModel)
                             && ContainsAssertBefore(tryStatement, innerTry, semanticModel));

    /// <summary>Determines whether a block invokes CodeAccessPermission.RevertAssert.</summary>
    /// <param name="block">The finally block to inspect.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when the block restores an assertion.</returns>
    private static bool ContainsRevertAssert(BlockSyntax block, SemanticModel semanticModel) =>
        block.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Any(invocation => IsMethod(semanticModel, invocation, KnownType.System_Security_CodeAccessPermission, "RevertAssert"));

    /// <summary>Determines whether an assertion precedes an inner try statement.</summary>
    /// <param name="outerTry">The try statement associated with the catch clause.</param>
    /// <param name="innerTry">The inner try statement that restores the assertion.</param>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <returns><see langword="true" /> when an assertion is established before the inner try.</returns>
    private static bool ContainsAssertBefore(TryStatementSyntax outerTry, TryStatementSyntax innerTry, SemanticModel semanticModel) =>
        outerTry.Block.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Any(invocation => invocation.SpanStart < innerTry.SpanStart
                               && IsInLocalTryScope(invocation, outerTry)
                               && IsMethod(semanticModel, invocation, KnownType.System_Security_CodeAccessPermission, "Assert"));

    /// <summary>Determines whether a node is not protected by an intervening catch clause.</summary>
    /// <param name="node">The node inside the try statement.</param>
    /// <param name="tryStatement">The try statement associated with the catch clause.</param>
    /// <returns><see langword="true" /> when the associated catch is the local unwind boundary.</returns>
    private static bool IsInLocalTryScope(SyntaxNode node, TryStatementSyntax tryStatement) =>
        node.AncestorsAndSelf().TakeWhile(x => x != tryStatement).OfType<TryStatementSyntax>().All(x => x.Catches.Count == 0);

    /// <summary>Determines whether an invocation resolves to a known framework method.</summary>
    /// <param name="semanticModel">The semantic model for the analyzed file.</param>
    /// <param name="invocation">The invocation to inspect.</param>
    /// <param name="containingType">The expected containing type.</param>
    /// <param name="methodName">The expected method name.</param>
    /// <returns><see langword="true" /> when the invocation resolves to the expected method.</returns>
    private static bool IsMethod(SemanticModel semanticModel, InvocationExpressionSyntax invocation, KnownType containingType, string methodName) =>
        semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method
        && method.Name == methodName
        && method.ContainingType.Is(containingType);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(RaiseOnInvalidCatch, SyntaxKind.TryStatement);
}
