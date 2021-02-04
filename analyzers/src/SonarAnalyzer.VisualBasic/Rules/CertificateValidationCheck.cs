/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<
        MethodBlockSyntax,
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
        public CertificateValidationCheck() : base(RspecStrings.ResourceManager) { }

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
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckAssignmentSyntax(c), SyntaxKind.SimpleAssignmentStatement);

            // Handling of constructor parameter syntax (SslStream)
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckConstructorParameterSyntax(c), SyntaxKind.ObjectCreationExpression);
        }

        protected override Location ExpressionLocation(SyntaxNode expression) =>
            // For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
            ((expression is LambdaExpressionSyntax lambda) ? (SyntaxNode)lambda.SubOrFunctionHeader.ParameterList : expression).GetLocation();

        protected override void SplitAssignment(AssignmentStatementSyntax assignment, out IdentifierNameSyntax leftIdentifier, out ExpressionSyntax right)
        {
            leftIdentifier = assignment.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            right = assignment.Right;
        }

        protected override IEqualityComparer<ExpressionSyntax> CreateNodeEqualityComparer() =>
            new VisualBasicSyntaxNodeEqualityComparer<ExpressionSyntax>();

        protected override SyntaxNode FindRootClassOrModule(SyntaxNode node)
        {
            if (node.FirstAncestorOrSelf<ModuleBlockSyntax>() is { } module)
            {
                return module; // Modules can't be nested. If there's one, it's the Root
            }
            ClassBlockSyntax candidate;
            var current = node.FirstAncestorOrSelf<ClassBlockSyntax>();
            while (current != null && (candidate = current.Parent?.FirstAncestorOrSelf<ClassBlockSyntax>()) != null)  // Search for parent of nested class
            {
                current = candidate;
            }
            return current;
        }

        protected override ExpressionSyntax[] FindReturnAndThrowExpressions(InspectionContext c, SyntaxNode block)
        {
            // Return value set by assignment to function variable/value
            var assignments = block.DescendantNodes()
                .OfType<AssignmentStatementSyntax>()
                .Where(x => c.Context.SemanticModel.GetSymbolInfo(x.Left).Symbol is ILocalSymbol local && local.IsFunctionValue);
            // And normal Return statements and throws
            return block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression)
                // Throw statements #2825. x.Expression can be NULL for standalone Throw and we need that one as well.
                .Concat(block.DescendantNodes().OfType<ThrowStatementSyntax>().Select(x => x.Expression))
                .Concat(assignments.Select(x => x.Right))
                .ToArray();
        }

        protected override bool IsTrueLiteral(ExpressionSyntax expression) =>
            expression?.RemoveParentheses().Kind() == SyntaxKind.TrueLiteralExpression;

        protected override string IdentifierText(SyntaxNode node) =>
            node switch
            {
                IdentifierNameSyntax ident => ident.Identifier.ValueText,
                ParameterSyntax param => param.Identifier.Identifier.ValueText,
                ModifiedIdentifierSyntax variable => variable.Identifier.ValueText,
                _ => null,
            };

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

        private protected override KnownType GenericDelegateType() => KnownType.System_Func_T1_T2_T3_T4_TResult_VB;
    }
}
