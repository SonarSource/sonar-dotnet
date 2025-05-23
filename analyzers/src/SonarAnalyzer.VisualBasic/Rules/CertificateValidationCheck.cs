﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<
        SyntaxKind,
        ArgumentSyntax,
        ExpressionSyntax,
        IdentifierNameSyntax,
        AssignmentStatementSyntax,
        InvocationExpressionSyntax,
        ParameterSyntax,
        ModifiedIdentifierSyntax,
        LambdaExpressionSyntax,
        MemberAccessExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language { get; } = VisualBasicFacade.Instance;
        protected override SyntaxKind[] MethodDeclarationKinds { get; } = { SyntaxKind.FunctionBlock, SyntaxKind.SubBlock };

        internal override MethodParameterLookupBase<ArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method) =>
            argumentListNode switch
            {
                InvocationExpressionSyntax invocation => new VisualBasicMethodParameterLookup(invocation.ArgumentList, method),
                ObjectCreationExpressionSyntax ctor => new VisualBasicMethodParameterLookup(ctor.ArgumentList, method),
                _ => throw new ArgumentException("Unexpected type.", nameof(argumentListNode)) // This should be throw only by bad usage of this method, not by input dependency
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            // C# += equivalent:
            // AddHandler situation does not exist in VB.NET. Delegate is pointer to one function only and can not have multiple handlers. It's not an event.
            // Only assignment and object creation are valid cases for VB.NET

            // Handling of = syntax
            context.RegisterNodeAction(CheckAssignmentSyntax, SyntaxKind.SimpleAssignmentStatement);

            // Handling of constructor parameter syntax (SslStream)
            context.RegisterNodeAction(CheckConstructorParameterSyntax, SyntaxKind.ObjectCreationExpression);
        }

        protected override Location ExpressionLocation(SyntaxNode expression) =>
            // For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
            ((expression is LambdaExpressionSyntax lambda) ? lambda.SubOrFunctionHeader.ParameterList : expression).GetLocation();

        protected override void SplitAssignment(AssignmentStatementSyntax assignment, out IdentifierNameSyntax leftIdentifier, out ExpressionSyntax right)
        {
            leftIdentifier = assignment.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            right = assignment.Right;
        }

        protected override IEqualityComparer<ExpressionSyntax> CreateNodeEqualityComparer() =>
            new VisualBasicSyntaxNodeEqualityComparer<ExpressionSyntax>();

        protected override SyntaxNode FindRootTypeDeclaration(SyntaxNode node)
        {
            if (node.FirstAncestorOrSelf<ModuleBlockSyntax>() is { } module)
            {
                return module; // Modules can't be nested. If there's one, it's the Root
            }
            return base.FindRootTypeDeclaration(node);
        }

        protected override ExpressionSyntax[] FindReturnAndThrowExpressions(InspectionContext c, SyntaxNode block)
        {
            // Return value set by assignment to function variable/value
            var assignments = block.DescendantNodes()
                .OfType<AssignmentStatementSyntax>()
                .Where(x => c.Context.SemanticModel.GetSymbolInfo(x.Left).Symbol is ILocalSymbol {IsFunctionValue: true});
            // And normal Return statements and throws
            return block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression)
                // Throw statements #2825. x.Expression can be NULL for standalone Throw and we need that one as well.
                .Concat(block.DescendantNodes().OfType<ThrowStatementSyntax>().Select(x => x.Expression))
                .Concat(assignments.Select(x => x.Right))
                .ToArray();
        }

        protected override bool IsTrueLiteral(ExpressionSyntax expression) =>
            expression?.RemoveParentheses().Kind() == SyntaxKind.TrueLiteralExpression;

        protected override ExpressionSyntax VariableInitializer(ModifiedIdentifierSyntax variable) =>
            variable.FirstAncestorOrSelf<VariableDeclaratorSyntax>()?.Initializer?.Value;

        protected override ImmutableArray<Location> LambdaLocations(InspectionContext c, LambdaExpressionSyntax lambda) =>
            lambda switch
            {
                SingleLineLambdaExpressionSyntax single => single.Body is ExpressionSyntax expr && IsTrueLiteral(expr)    // LiteralExpressionSyntax or ParenthesizedExpressionSyntax like (((true)))
                                                            ? new[] { single.Body.GetLocation() }.ToImmutableArray()
                                                            : ImmutableArray<Location>.Empty,
                MultiLineLambdaExpressionSyntax multi => BlockLocations(c, multi),
                _ => ImmutableArray<Location>.Empty,
            };

        protected override SyntaxNode LocalVariableScope(ModifiedIdentifierSyntax variable) =>
            variable.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>()?.Parent;

        protected override SyntaxNode ExtractArgumentExpressionNode(SyntaxNode expression)
        {
            expression = expression.RemoveParentheses();
            return expression is UnaryExpressionSyntax unary && unary.Kind() == SyntaxKind.AddressOfExpression
                ? unary.Operand   // Parentheses can not wrap AddressOf operand
                : expression;
        }

        protected override SyntaxNode SyntaxFromReference(SyntaxReference reference)
        {
            var syntax = reference.GetSyntax();
            return syntax is MethodStatementSyntax ? syntax.Parent : syntax;
        }
    }
}
