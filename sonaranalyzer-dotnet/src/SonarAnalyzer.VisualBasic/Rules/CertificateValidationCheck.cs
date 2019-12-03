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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<MethodBlockSyntax, ArgumentSyntax, ExpressionSyntax, IdentifierNameSyntax, AssignmentStatementSyntax, InvocationExpressionSyntax, ParameterSyntax, ModifiedIdentifierSyntax, LambdaExpressionSyntax>
    {
        public CertificateValidationCheck() : base(RspecStrings.ResourceManager) { }

        protected override void Initialize(SonarAnalysisContext context)
        {
            //C# += equivalent:
            //AddHandler situation does not exist in VB.NET. Delegate is pointer to one function only and can not have multiple handlers. It's not an event.
            //Only assignment and object creation are valid cases for VB.NET

            //Handling of = syntax
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckAssignmentSyntax(c), SyntaxKind.SimpleAssignmentStatement);

            //Handling of constructor parameter syntax (SslStream)
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckConstructorParameterSyntax(c), SyntaxKind.ObjectCreationExpression);
        }

        internal override AbstractMethodParameterLookup<ArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method)
        {
            switch (argumentListNode)
            {
                case InvocationExpressionSyntax invocation:
                    return new VisualBasicMethodParameterLookup(invocation.ArgumentList, method);
                case ObjectCreationExpressionSyntax ctor:
                    return new VisualBasicMethodParameterLookup(ctor.ArgumentList, method);
                default:
                    //This should be throw only by bad usage of this method, not by input dependency
                    throw new ArgumentException("Unexpected type.", nameof(argumentListNode));
            }
        }

        internal override KnownType GenericDelegateType()
        {
            return KnownType.System_Func_T1_T2_T3_T4_TResult_VB;
        }

        protected override Location ArgumentLocation(ArgumentSyntax argument)
        {
            //For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
            return ((argument.GetExpression() is LambdaExpressionSyntax lambda) ? (SyntaxNode)lambda.SubOrFunctionHeader.ParameterList : argument).GetLocation();
        }

        protected override ExpressionSyntax ArgumentExpression(ArgumentSyntax argument)
        {
            return argument.GetExpression();
        }

        protected override void SplitAssignment(AssignmentStatementSyntax assignment, out IdentifierNameSyntax leftIdentifier, out ExpressionSyntax right)
        {
            leftIdentifier = assignment.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            right = assignment.Right;
        }

        protected override IEqualityComparer<ExpressionSyntax> CreateNodeEqualityComparer()
        {
            return new Helpers.VisualBasic.VisualBasicSyntaxNodeEqualityComparer<ExpressionSyntax>();
        }

        protected override SyntaxNode FindRootClassOrModule(SyntaxNode node)
        {
            SyntaxNode current, candidate;
            current = node.FirstAncestorOrSelf<ModuleBlockSyntax>();
            if (current != null)
            {
                return current; //Modules can't be nested. If there's one, it's the Root
            }
            current = node.FirstAncestorOrSelf<ClassBlockSyntax>();
            while (current != null && (candidate = current.Parent?.FirstAncestorOrSelf<ClassBlockSyntax>()) != null)  //Search for parent of nested class
            {
                current = candidate;
            }
            return current;
        }

        protected override ExpressionSyntax[] FindReturnAndThrowExpressions(InspectionContext c, SyntaxNode block)
        {
            //Return value set by assignment to function variable/value
            var assignments = block.DescendantNodes().OfType<AssignmentStatementSyntax>().Where(x => c.Context.SemanticModel.GetSymbolInfo(x.Left).Symbol is ILocalSymbol local && local.IsFunctionValue);
            //And normal Return statements and throws
            return block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression)
                .Concat(block.DescendantNodes().OfType<ThrowStatementSyntax>().Select(x => x.Expression))   //Throw statements #2825. x.Expression can be NULL for standalone Throw and we need that one as well.
                .Concat(assignments.Select(x => x.Right))
                .ToArray();
        }

        protected override bool IsTrueLiteral(ExpressionSyntax expression)
        {
            return expression?.RemoveParentheses().Kind() == SyntaxKind.TrueLiteralExpression;
        }

        protected override string IdentifierText(SyntaxNode node)
        {
            switch (node)
            {
                case IdentifierNameSyntax ident:
                    return ident.Identifier.ValueText;
                case ParameterSyntax param:
                    return param.Identifier.Identifier.ValueText;
                case ModifiedIdentifierSyntax variable:
                    return variable.Identifier.ValueText;
                default:
                    return null;
            }
        }

        protected override ExpressionSyntax VariableInitializer(ModifiedIdentifierSyntax variable)
        {
            return variable.FirstAncestorOrSelf<VariableDeclaratorSyntax>()?.Initializer?.Value;
        }
        
        protected override ImmutableArray<Location> LambdaLocations(InspectionContext c, LambdaExpressionSyntax lambda)
        {
            switch (lambda)
            {
                case SingleLineLambdaExpressionSyntax single:
                    if (single.Body is ExpressionSyntax expr && IsTrueLiteral(expr))    //LiteralExpressionSyntax or ParenthesizedExpressionSyntax like (((true)))
                    {
                        return new[] { single.Body.GetLocation() }.ToImmutableArray();
                    }
                    break;
                case MultiLineLambdaExpressionSyntax multi:
                    return BlockLocations(c, multi);
            }
            return ImmutableArray<Location>.Empty;
        }

        protected override SyntaxNode LocalVariableScope(ModifiedIdentifierSyntax variable)
        {
            return variable.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>()?.Parent;
        }
        
        protected override SyntaxNode ExtractArgumentExpressionNode(ExpressionSyntax expression)
        {
            expression = expression.RemoveParentheses();
            if (expression is UnaryExpressionSyntax unary && unary.Kind() == SyntaxKind.AddressOfExpression)
            {
                return unary.Operand;   //Parentheses can not wrap AddressOf operand
            }
            return expression;
        }
                             
        protected override SyntaxNode SyntaxFromReference(SyntaxReference reference)
        {
            var syntax = reference.GetSyntax();
            if (syntax is MethodStatementSyntax)
            {
                return syntax.Parent;
            }
            return syntax;
        }
        
    }
}

