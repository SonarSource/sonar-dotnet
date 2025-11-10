/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<
        SyntaxKind,
        ArgumentSyntax,
        ExpressionSyntax,
        IdentifierNameSyntax,
        AssignmentExpressionSyntax,
        InvocationExpressionSyntax,
        ParameterSyntax,
        VariableDeclaratorSyntax,
        ParenthesizedLambdaExpressionSyntax,
        MemberAccessExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;
        protected override HashSet<SyntaxKind> MethodDeclarationKinds { get; } = [ SyntaxKind.MethodDeclaration, SyntaxKindEx.LocalFunctionStatement ];

        internal override MethodParameterLookupBase<ArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method) =>
            argumentListNode switch
            {
                InvocationExpressionSyntax invocation => new CSharpMethodParameterLookup(invocation.ArgumentList, method),
                _ when ObjectCreationFactory.TryCreate(argumentListNode) is { ArgumentList: { } argumentList } => new CSharpMethodParameterLookup(argumentList, method),
                _ => throw new ArgumentException("Unexpected type.", nameof(argumentListNode))  // This should be throw only by bad usage of this method, not by input dependency
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            // Handling of += syntax
            context.RegisterNodeAction(CheckAssignmentSyntax, SyntaxKind.AddAssignmentExpression);

            // Handling of = syntax
            context.RegisterNodeAction(CheckAssignmentSyntax, SyntaxKind.SimpleAssignmentExpression);

            // Handling of constructor parameter syntax (SslStream)
            context.RegisterNodeAction(CheckConstructorParameterSyntax, SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);
        }

        protected override SyntaxNode FindRootTypeDeclaration(SyntaxNode node) =>
            base.FindRootTypeDeclaration(node) ?? node.FirstAncestorOrSelf<GlobalStatementSyntax>()?.Parent;

        protected override Location ExpressionLocation(SyntaxNode expression) =>
            // For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
            ((expression is ParenthesizedLambdaExpressionSyntax lambda) ? lambda.ParameterList : expression).GetLocation();

        protected override void SplitAssignment(AssignmentExpressionSyntax assignment, out IdentifierNameSyntax leftIdentifier, out ExpressionSyntax right)
        {
            leftIdentifier = assignment.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            right = assignment.Right;
        }

        protected override IEqualityComparer<ExpressionSyntax> CreateNodeEqualityComparer() =>
            new CSharpSyntaxNodeEqualityComparer<ExpressionSyntax>();

        protected override ExpressionSyntax[] FindReturnAndThrowExpressions(InspectionContext c, SyntaxNode block) =>
            block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression)
                // Throw statements #2825. x.Expression can be NULL for standalone Throw and we need that one as well.
                .Concat(block.DescendantNodes().OfType<ThrowStatementSyntax>().Select(x => x.Expression))
                .ToArray();

        protected override bool IsTrueLiteral(ExpressionSyntax expression) =>
            expression?.RemoveParentheses().Kind() == SyntaxKind.TrueLiteralExpression;

        protected override ExpressionSyntax VariableInitializer(VariableDeclaratorSyntax variable) =>
            variable.Initializer?.Value;

        protected override ImmutableArray<Location> LambdaLocations(InspectionContext c, ParenthesizedLambdaExpressionSyntax lambda)
        {
            if (lambda.Body is BlockSyntax block)
            {
                return BlockLocations(c, block);
            }
            if (lambda.Body is ExpressionSyntax expr && IsTrueLiteral(expr))   // LiteralExpressionSyntax or ParenthesizedExpressionSyntax like (((true)))
            {
                return new[] { lambda.Body.GetLocation() }.ToImmutableArray();      // Code was found guilty for lambda (...) => true
            }
            return ImmutableArray<Location>.Empty;
        }

        protected override SyntaxNode LocalVariableScope(VariableDeclaratorSyntax variable) =>
            variable.FirstAncestorOrSelf<BlockSyntax>();

        protected override SyntaxNode ExtractArgumentExpressionNode(SyntaxNode expression) =>
            expression.RemoveParentheses();

        protected override SyntaxNode SyntaxFromReference(SyntaxReference reference) =>
            reference.GetSyntax();   // VB.NET has more complicated logic
    }
}
