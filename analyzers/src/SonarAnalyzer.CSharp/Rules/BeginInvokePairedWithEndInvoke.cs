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
public sealed class BeginInvokePairedWithEndInvoke : BeginInvokePairedWithEndInvokeBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override SyntaxKind InvocationExpressionKind => SyntaxKind.InvocationExpression;
    protected override string CallbackParameterName => "callback";

    protected override bool VisitInvocation(EndInvokeContext context)
    {
        var walker = new InvocationWalker(context);
        return context.Root is CompilationUnitSyntax compilationUnit
            ? compilationUnit.Members.OfType<GlobalStatementSyntax>().All(walker.SafeVisit)
            : walker.SafeVisit(context.Root);
    }

    protected override SyntaxNode EnclosingScope(SyntaxNode node)
    {
        var scope = node.EnclosingScope();
        return scope is QueryClauseSyntax or SelectOrGroupClauseSyntax or QueryContinuationSyntax
            ? EnclosingScope(scope.Parent)
            : scope;
    }

    protected override bool IsInvalidCallback(SyntaxNode callbackArg, SemanticModel model)
    {
        if (FindCallback(callbackArg, model) is { } callback)
        {
            var callbackModel = callback.EnsureCorrectSemanticModelOrDefault(model);
            return callback is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.ToString().Equals(EndInvoke)
                ? callbackModel.GetSymbolInfo(callback).Symbol is not IMethodSymbol
                : Language.Syntax.IsNullLiteral(callback) || !IsParentDeclarationWithEndInvoke(callback, callbackModel);
        }

        return false;
    }

    /// <summary>
    /// This method is looking for the callback code which can be:
    /// - in a identifier initializer (like a lambda)
    /// - passed directly as a lambda argument
    /// - passed as a new delegate instantiation (and the code can be inside the method declaration)
    /// - a mix of the above.
    /// </summary>
    private static SyntaxNode FindCallback(SyntaxNode callbackArg, SemanticModel model)
    {
        var callback = callbackArg.RemoveParentheses();
        if (callback is IdentifierNameSyntax identifier)
        {
            callback = LookupIdentifierInitializer(identifier, model);
        }

        if (callback is ObjectCreationExpressionSyntax objectCreation)
        {
            callback = objectCreation.ArgumentList.Arguments.Count == 1 ? objectCreation.ArgumentList.Arguments.Single().Expression : null;
            if (callback is not null && callback.EnsureCorrectSemanticModelOrDefault(model) is { } safeModel && safeModel.GetSymbolInfo(callback).Symbol is IMethodSymbol methodSymbol)
            {
                callback = methodSymbol.ImplementationSyntax;
            }
        }

        return callback;
    }

    private static SyntaxNode LookupIdentifierInitializer(IdentifierNameSyntax identifier, SemanticModel semantic) =>
        semantic.GetSymbolInfo(identifier).Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax variableDeclarator
        && variableDeclarator.Initializer is EqualsValueClauseSyntax { Value.WithoutEnclosingParentheses: { } value }
            ? value
            : null;

    private sealed class InvocationWalker : SafeCSharpSyntaxWalker
    {
        private readonly EndInvokeContext context;

        public InvocationWalker(EndInvokeContext context) =>
            this.context = context;

        public override void Visit(SyntaxNode node)
        {
            if (context.Visit(node))
            {
                base.Visit(node);
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (context.VisitInvocationExpression(node))
            {
                base.VisitInvocationExpression(node);
            }
        }
    }
}
