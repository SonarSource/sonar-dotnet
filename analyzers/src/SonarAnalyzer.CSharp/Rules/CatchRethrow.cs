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

    protected override bool IsRequiredUnwindBoundary(CatchClauseSyntax catchClause, SemanticModel model)
    {
        if (!IsCatchAll(catchClause, model))
        {
            return false;
        }
        var walker = new TemporaryContextWalker(model);
        return walker.SafeVisit(((TryStatementSyntax)catchClause.Parent).Block) && walker.HasBoundary;
    }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(RaiseOnInvalidCatch, SyntaxKind.TryStatement);

    private static bool IsCatchAll(CatchClauseSyntax catchClause, SemanticModel model) =>
        catchClause.Filter is null
        && (catchClause.Declaration is null || model.GetTypeInfo(catchClause.Declaration.Type).Type.Is(KnownType.System_Exception));

    private sealed class TemporaryContextWalker : SafeCSharpSyntaxWalker
    {
        private static readonly ISet<SyntaxKind> NestedFunctionKinds = new HashSet<SyntaxKind>
        {
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.AnonymousMethodExpression,
        };

        private readonly SemanticModel model;

        public bool HasBoundary { get; private set; }

        public TemporaryContextWalker(SemanticModel model) =>
            this.model = model;

        public override void Visit(SyntaxNode node)
        {
            if (!HasBoundary && !node.IsAnyKind(NestedFunctionKinds))
            {
                base.Visit(node);
            }
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            if (!HasCatchAll(node))
            {
                if (IsCodeAccessPermissionBoundary(node))
                {
                    HasBoundary = true;
                    return;
                }
                Visit(node.Block);
            }

            // A catch-all protects the try block, but not its own catch and finally blocks.
            foreach (var catchClause in node.Catches)
            {
                Visit(catchClause);
            }
            Visit(node.Finally);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsTemporaryContextInvocation(node) || IsLegacyImpersonationScope(node))
            {
                HasBoundary = true;
                return;
            }
            base.VisitInvocationExpression(node);
        }

        private bool IsTemporaryContextInvocation(InvocationExpressionSyntax invocation) =>
            invocation.IsMethodInvocation(KnownType.System_Threading_ExecutionContext, "Run", model)
            || invocation.IsMethodInvocation(KnownType.System_Security_Principal_WindowsIdentity, "RunImpersonated", model);

        private bool IsLegacyImpersonationScope(InvocationExpressionSyntax invocation) =>
            invocation.IsMethodInvocation(KnownType.System_Security_Principal_WindowsIdentity, "Impersonate", model)
            && model.GetTypeInfo(invocation).Type.Is(KnownType.System_Security_Principal_WindowsImpersonationContext)
            && IsUsingScope(invocation);

        private bool IsCodeAccessPermissionBoundary(TryStatementSyntax tryStatement) =>
            tryStatement.Finally is not null
            && ContainsCodeAccessPermissionCall(tryStatement.Block, "Assert")
            && ContainsCodeAccessPermissionCall(tryStatement.Finally.Block, "RevertAssert");

        private bool ContainsCodeAccessPermissionCall(SyntaxNode scope, string name) =>
            scope.DescendantNodes(x => !x.IsAnyKind(NestedFunctionKinds))
                .OfType<InvocationExpressionSyntax>()
                .Any(x => x.IsMethodInvocation(KnownType.System_Security_CodeAccessPermission, name, model));

        private bool HasCatchAll(TryStatementSyntax tryStatement) =>
            tryStatement.Catches.Any(x => IsCatchAll(x, model));

        private static bool IsUsingScope(InvocationExpressionSyntax invocation) =>
            invocation.Ancestors().OfType<UsingStatementSyntax>().Any(x =>
                IsUsingResource(x.Expression, invocation)
                || x.Declaration?.Variables.Any(y => IsUsingResource(y.Initializer?.Value, invocation)) == true)
            || invocation.Ancestors().OfType<LocalDeclarationStatementSyntax>().Any(x =>
                x.UsingKeyword.IsKind(SyntaxKind.UsingKeyword)
                && x.Declaration.Variables.Any(y => IsUsingResource(y.Initializer?.Value, invocation)));

        private static bool IsUsingResource(ExpressionSyntax resource, InvocationExpressionSyntax invocation) =>
            resource?.RemoveParentheses() switch
            {
                CastExpressionSyntax cast => IsUsingResource(cast.Expression, invocation),
                var candidate => candidate == invocation,
            };
    }
}
