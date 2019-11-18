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
using System.Resources;
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
    public sealed class CertificateValidationCheck : CertificateValidationCheckBase<MethodDeclarationSyntax, ArgumentSyntax, ExpressionSyntax>
    {

        internal override ResourceManager RspecResouceManager => RspecStrings.ResourceManager;

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
            if (argumentListNode is ObjectCreationExpressionSyntax ctor)
            {
                return new CSharpMethodParameterLookup(ctor.ArgumentList, method);
            }
            else if(argumentListNode is InvocationExpressionSyntax invocation)
            {
                return new CSharpMethodParameterLookup(invocation.ArgumentList, method);
            }
            else
            {
                throw new ArgumentException("Unexpected type.", nameof(argumentListNode));
            }
        }

        //private void CheckAssignmentSyntax(SyntaxNodeAnalysisContext c)
        //{
        //    var assignmentNode = (AssignmentExpressionSyntax)c.Node;
        //    var leftIdentifier = assignmentNode.Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
        //    var right = assignmentNode.Right;
        //    if (leftIdentifier != null && right != null
        //        && c.SemanticModel.GetSymbolInfo(leftIdentifier).Symbol is IPropertySymbol left
        //        && IsValidationDelegateType(left.Type))
        //    {
        //        TryReportLocations(new InspectionContext(c), leftIdentifier.GetLocation(), right);
        //    }
        //}

        //private void CheckConstructorParameterSyntax(SyntaxNodeAnalysisContext c)
        //{
        //    if (c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol ctor)
        //    {
        //        CSharpMethodParameterLookup methodParamLookup = null;       //Cache, there might be more of them
        //        foreach (var param in ctor.Parameters.Where(x => IsValidationDelegateType(x.Type)))
        //        {
        //            methodParamLookup = methodParamLookup ?? new CSharpMethodParameterLookup((c.Node as ObjectCreationExpressionSyntax).ArgumentList, ctor);
        //            if (methodParamLookup.TryGetSymbolParameter(param, out var argument))
        //            {
        //                //For Lambda expression extract location of the parentheses only to separate them from secondary location of "true"
        //                var primaryLocation = ((argument.Expression is ParenthesizedLambdaExpressionSyntax Lambda) ? (SyntaxNode)Lambda.ParameterList : argument).GetLocation();
        //                TryReportLocations(new InspectionContext(c), primaryLocation, argument.Expression);
        //            }
        //        }
        //    }
        //}

        private void TryReportLocations(InspectionContext c, Location primaryLocation, ExpressionSyntax expression)
        {
            var locations = ArgumentLocations(c, expression);
            if (!locations.IsEmpty)
            {
                //Report both, assignemnt as well as all implementation occurances
                c.Context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, primaryLocation, additionalLocations: locations));
            }
        }

        private static ImmutableArray<Location> ArgumentLocations(InspectionContext c, ExpressionSyntax expression)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            switch (expression)
            {
                case IdentifierNameSyntax identifier:
                    var identSymbol = c.Context.SemanticModel.GetSymbolInfo(identifier).Symbol;
                    if (identSymbol != null && identSymbol.DeclaringSyntaxReferences.Length == 1)
                    {
                        ret.AddRange(IdentifierLocations(c, identSymbol.DeclaringSyntaxReferences.Single().GetSyntax()));
                    }
                    break;
                case ParenthesizedLambdaExpressionSyntax lambda:
                    if ((lambda.Body as LiteralExpressionSyntax)?.Kind() == SyntaxKind.TrueLiteralExpression)
                    {
                        ret.Add(lambda.Body.GetLocation());   //Code was found guilty for lambda (...) => true
                    }
                    else if (lambda.Body is BlockSyntax block)
                    {
                        ret.AddRange(BlockLocations(block));
                    }
                    break;
                case InvocationExpressionSyntax invocation:
                    var invSymbol = c.Context.SemanticModel.GetSymbolInfo(invocation).Symbol;
                    if (invSymbol != null && invSymbol.DeclaringSyntaxReferences.Length == 1 && invSymbol.DeclaringSyntaxReferences.Single().GetSyntax() is MethodDeclarationSyntax syntax)
                    {
                        c.VisitedMethods.Add(syntax);
                        ret.AddRange(InvocationLocations(c, syntax));
                    }
                    break;
            }
            return ret.ToImmutableArray();
        }

        private static ImmutableArray<Location> IdentifierLocations(InspectionContext c, SyntaxNode syntax)
        {
            switch (syntax)
            {
                case MethodDeclarationSyntax method:        //Direct delegate name
                    return BlockLocations(method.Body);
                case ParameterSyntax parameter:             //Value arrived as a parameter
                    return ParamLocations(c, parameter);
                case VariableDeclaratorSyntax variable:     //Value passed as variable
                    return VariableLocations(c, variable);
            }
            return ImmutableArray<Location>.Empty;
        }

        private static ImmutableArray<Location> ParamLocations(InspectionContext c, ParameterSyntax param)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            var containingMethodDeclaration = param.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (!c.VisitedMethods.Contains(containingMethodDeclaration))
            {
                c.VisitedMethods.Add(containingMethodDeclaration);
                var containingMethod = c.Context.SemanticModel.GetDeclaredSymbol(containingMethodDeclaration);
                var paramSymbol = containingMethod.Parameters.Single(x => x.Name == param.Identifier.ValueText);
                foreach (var invocation in FindInvocationList(c.Context, FindRootClass(param), containingMethod))
                {
                    var methodParamLookup = new CSharpMethodParameterLookup(invocation.ArgumentList, containingMethod);
                    if (methodParamLookup.TryGetSymbolParameter(paramSymbol, out var argument))
                    {
                        ret.AddRange(CallStackSublocations(c, argument.Expression));
                    }
                }
            }
            return ret.ToImmutable();
        }

        private static ImmutableArray<Location> VariableLocations(InspectionContext c, VariableDeclaratorSyntax variable)
        {
            var allAssignedExpressions = new List<ExpressionSyntax>();
            var parentBlock = variable.FirstAncestorOrSelf<BlockSyntax>();
            if (parentBlock != null)
            {
                allAssignedExpressions.AddRange(parentBlock.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                    .Where(x => x.Left is IdentifierNameSyntax Ident && Ident.Identifier.ValueText == variable.Identifier.ValueText)
                    .Select(x => x.Right));
            }
            if (variable.Initializer != null)       //Declarator initializer is counted as (default) assignment as well
            {
                allAssignedExpressions.Add(variable.Initializer.Value);
            }
            return MultiExpressionSublocations(c, allAssignedExpressions);
        }

        private static ImmutableArray<Location> InvocationLocations(InspectionContext c, MethodDeclarationSyntax method)
        {
            var returnExpressionSublocationsList = method.Body.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Select(x => x.Expression)
                .Where(x => !IsVisited(c, x));      //Ignore all return statements with recursive call. Result depends on returns that could return compliant validator.
            return MultiExpressionSublocations(c, returnExpressionSublocationsList);
        }

        private static bool IsVisited(InspectionContext c, ExpressionSyntax expression)
        {
            if (expression is InvocationExpressionSyntax invocation)
            {
                var symbol = c.Context.SemanticModel.GetSymbolInfo(invocation).Symbol;
                return symbol != null && !symbol.DeclaringSyntaxReferences.IsEmpty
                    && symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax() as MethodDeclarationSyntax).Any(x => x != null && c.VisitedMethods.Contains(x));
            }
            return false;
        }

        private static ImmutableArray<Location> MultiExpressionSublocations(InspectionContext c, IEnumerable<ExpressionSyntax> expressions)
        {
            var exprSublocationsList = expressions.Distinct(new Helpers.CSharp.CSharpSyntaxNodeEqualityComparer<ExpressionSyntax>())
                .Select(x => CallStackSublocations(c, x))
                .ToArray();
            if (exprSublocationsList.Any(x => x.IsEmpty))   //If there's at leat one concurrent expression, that returns compliant delegate, than there's some logic and this scope is compliant
            {
                return ImmutableArray<Location>.Empty;
            }
            return exprSublocationsList.SelectMany(x => x).ToImmutableArray();      //Else every return statement is noncompliant
        }

        private static ImmutableArray<Location> CallStackSublocations(InspectionContext c, ExpressionSyntax expression)
        {
            var lst = ArgumentLocations(c, expression);
            if (!lst.IsEmpty)        //There's noncompliant issue in this chain
            {
                var Loc = expression.GetLocation();
                if (!lst.Any(x => x.SourceSpan.IntersectsWith(Loc.SourceSpan)))
                {
                    //Add 2nd, 3rd, 4th etc //Secondary marker. If it is not marked already from direct Delegate name or direct Lambda occurance
                    return lst.Concat(new[] { Loc }).ToImmutableArray();
                }
            }
            return lst;
        }

        private static ImmutableArray<Location> BlockLocations(BlockSyntax block)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            if (block != null)
            {
                //TODO: VB.NET vs. return by assign to function name
                var returnExpressions = block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression).ToArray();
                if (returnExpressions.All(x => x.Kind() == SyntaxKind.TrueLiteralExpression))    //There must be at least one return, that does not return true to be compliant
                {
                    ret.AddRange(returnExpressions.Select(x => x.GetLocation()));
                }
            }
            return ret.ToImmutable();
        }

        private static ClassDeclarationSyntax FindRootClass(SyntaxNode node)
        {
            ClassDeclarationSyntax current, candidate;
            current = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            while (current != null && (candidate = current.Parent?.FirstAncestorOrSelf<ClassDeclarationSyntax>()) != null)  //Search for parent of nested class
            {
                current = candidate;
            }
            return current;
        }

        private static ImmutableArray<InvocationExpressionSyntax> FindInvocationList(SyntaxNodeAnalysisContext c, ClassDeclarationSyntax root, IMethodSymbol method)
        {
            if (root == null || method == null)
            {
                return ImmutableArray<InvocationExpressionSyntax>.Empty;
            }
            var ret = ImmutableArray.CreateBuilder<InvocationExpressionSyntax>();
            foreach (var Invocation in root.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                if (c.SemanticModel.GetSymbolInfo(Invocation).Symbol == method)
                {
                    ret.Add(Invocation);
                }
            }
            return ret.ToImmutable();
        }

        //private struct InspectionContext
        //{
        //    public readonly SyntaxNodeAnalysisContext Context;
        //    public readonly HashSet<MethodDeclarationSyntax> VisitedMethods;

        //    public InspectionContext(SyntaxNodeAnalysisContext context)
        //    {
        //        this.Context = context;
        //        this.VisitedMethods = new HashSet<MethodDeclarationSyntax>();
        //    }
        //}

    }
}
