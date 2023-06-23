namespace SonarAnalyzer.Trackers
{
    internal class CSharpArgumentTracker : ArgumentTracker<SyntaxKind>
    {
        protected override SyntaxKind[] TrackedSyntaxKinds => new[]
            {
                SyntaxKind.AttributeArgument,
                SyntaxKind.Argument,
            };

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode) =>
            argumentNode switch
            {
                AttributeArgumentSyntax { Parent: AttributeArgumentListSyntax { Arguments: { } list } } => list,
                ArgumentSyntax { Parent: ArgumentListSyntax { Arguments: { } list } } => list,
                ArgumentSyntax { Parent: BracketedArgumentListSyntax { Arguments: { } list } } => list,
                _ => null,
            };

        protected override int? Position(SyntaxNode argumentNode) =>
            argumentNode is ArgumentSyntax { NameColon: not null } or AttributeArgumentSyntax { NameColon: not null } or AttributeArgumentSyntax { NameEquals: not null }
                ? null
                : ArgumentList(argumentNode).IndexOf(x => x == argumentNode);

        protected override RefKind ArgumentRefKind(SyntaxNode argumentNode)
        {
            return argumentNode switch
            {
                AttributeArgumentSyntax => RefKind.None,
                ArgumentSyntax { RefOrOutKeyword: { } refOrOut } => refOrOut.Kind() switch { SyntaxKind.OutKeyword => RefKind.Out, SyntaxKind.RefKeyword => RefKind.Ref, _ => RefKind.None },
                _ => RefKind.None,
            };
        }

        protected override bool InvocationFitsMemberKind(SyntaxNode argumentNode, InvokedMemberKind memberKind)
        {
            var invocationExpression = InvokedExpression(argumentNode);
            return memberKind switch
            {
                InvokedMemberKind.Method => invocationExpression is InvocationExpressionSyntax,
                InvokedMemberKind.Constructor => invocationExpression is ObjectCreationExpressionSyntax || ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(invocationExpression),
                InvokedMemberKind.Indexer => invocationExpression is ElementAccessExpressionSyntax or ElementBindingExpressionSyntax,
                InvokedMemberKind.Attribute => invocationExpression is AttributeSyntax,
                _ => false,
            };
        }

        protected override bool InvokedMemberFits(SemanticModel model, SyntaxNode argumentNode, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint)
        {
            var expression = InvokedExpression(argumentNode);
            return memberKind switch
            {
                InvokedMemberKind.Method => invokedMemberNameConstraint(expression.GetName()),
                InvokedMemberKind.Constructor => expression switch
                {
                    ObjectCreationExpressionSyntax { Type: { } typeName } => invokedMemberNameConstraint(typeName.GetName()),
                    { } ex when ImplicitObjectCreationExpressionSyntaxWrapper.IsInstance(ex) => invokedMemberNameConstraint(model.GetSymbolInfo(ex).Symbol.Name),
                    _ => false,
                },
                InvokedMemberKind.Indexer => true,
                InvokedMemberKind.Attribute => expression is AttributeSyntax { Name: { } typeName } && invokedMemberNameConstraint(typeName.GetName()),
                _ => false,
            };
        }

        protected override SyntaxNode InvokedExpression(SyntaxNode argumentNode) =>
            argumentNode?.Parent?.Parent;

        protected override SyntaxBaseContext CreateContext(SonarSyntaxNodeReportingContext context) =>
            new(context);
    }
}
