/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<MethodDeclarationSyntax, ArgumentSyntax, ExpressionSyntax, IdentifierNameSyntax, AssignmentExpressionSyntax, InvocationExpressionSyntax, ParameterSyntax, VariableDeclaratorSyntax, ParenthesizedLambdaExpressionSyntax>
    {

        public CertificateValidationCheck() : base(RspecStrings.ResourceManager) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            //Handling of += syntax
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckAssignmentSyntax(c), SyntaxKind.AddAssignmentExpression);

            //Handling of = syntax
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckAssignmentSyntax(c), SyntaxKind.SimpleAssignmentExpression);

            //Handling of constructor parameter syntax (SslStream)
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckConstructorParameterSyntax(c), SyntaxKind.ObjectCreationExpression);
        }

        internal override AbstractMethodParameterLookup<ArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method)
        {
            switch (argumentListNode)
            {
                case InvocationExpressionSyntax invocation:
                    return new CSharpMethodParameterLookup(invocation.ArgumentList, method);
                case ObjectCreationExpressionSyntax ctor:
                    return new CSharpMethodParameterLookup(ctor.ArgumentList, method);
                default:
                    //This should be throw only by bad usage of this method, not by input dependency
                    throw new ArgumentException("Unexpected type.", nameof(argumentListNode));
            }
        }

        internal override KnownType GenericDelegateType()
        {
            return KnownType.System_Func_T1_T2_T3_T4_TResult;
        }

        protected override Location ArgumentLocation(ArgumentSyntax argument)
        {
            //For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
            return ((argument.Expression is ParenthesizedLambdaExpressionSyntax Lambda) ? (SyntaxNode)Lambda.ParameterList : argument).GetLocation();
        }

        protected override ExpressionSyntax ArgumentExpression(ArgumentSyntax argument)
        {
            return argument.Expression;
        }

        protected override void SplitAssignment(AssignmentExpressionSyntax assignment, out IdentifierNameSyntax leftIdentifier, out ExpressionSyntax right)
        {
            leftIdentifier = assignment.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            right = assignment.Right;
        }

        protected override IEqualityComparer<ExpressionSyntax> CreateNodeEqualityComparer()
        {
            return new Helpers.CSharp.CSharpSyntaxNodeEqualityComparer<ExpressionSyntax>();
        }

        protected override SyntaxNode FindRootClassOrModule(SyntaxNode node)
        {
            ClassDeclarationSyntax current, candidate;
            current = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            while (current != null && (candidate = current.Parent?.FirstAncestorOrSelf<ClassDeclarationSyntax>()) != null)  //Search for parent of nested class
            {
                current = candidate;
            }
            return current;
        }

        protected override ExpressionSyntax[] FindReturnExpressions(InspectionContext c, SyntaxNode block)
        {
            return block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression).ToArray();
        }

        protected override bool IsTrueLiteral(ExpressionSyntax expression)
        {
            return expression.Kind() == SyntaxKind.TrueLiteralExpression;
        }

        protected override string IdentifierText(SyntaxNode node)
        {
            switch (node)
            {
                case IdentifierNameSyntax ident:
                    return ident.Identifier.ValueText;
                case ParameterSyntax param:
                    return param.Identifier.ValueText;
                case VariableDeclaratorSyntax variable:
                    return variable.Identifier.ValueText;
                default:
                    return null;
            }
        }
        
        protected override ExpressionSyntax VariableInitializer(VariableDeclaratorSyntax variable)
        {
            return variable.Initializer?.Value;
        }

        protected override ImmutableArray<Location> LambdaLocations(InspectionContext c, ParenthesizedLambdaExpressionSyntax lambda)
        {
            if ((lambda.Body as LiteralExpressionSyntax)?.Kind() == SyntaxKind.TrueLiteralExpression)
            {
                return new[] { lambda.Body.GetLocation() }.ToImmutableArray();   //Code was found guilty for lambda (...) => true
            }
            else if (lambda.Body is BlockSyntax block)
            {
                return BlockLocations(c, block);
            }
            else
            {
                return ImmutableArray<Location>.Empty;
            }
        }

        protected override SyntaxNode LocalVariableScope(VariableDeclaratorSyntax variable)
        {
            return variable.FirstAncestorOrSelf<BlockSyntax>();
        }

        protected override ExpressionSyntax TryExtractAddressOfOperand(ExpressionSyntax expression)
        {
            return expression;              //VB.NET only
        }

        protected override SyntaxNode SyntaxFromReference(SyntaxReference reference)
        {
            return reference.GetSyntax();   //VB.NET has more complicated logic
        }
        
    }
}
