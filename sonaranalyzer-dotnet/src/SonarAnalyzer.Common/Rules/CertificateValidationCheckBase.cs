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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;


namespace SonarAnalyzer.Rules
{
    public abstract class CertificateValidationCheckBase<TMethodSyntax, TArgumentSyntax, TExpressionSyntax, TIdentifierNameSyntax, TAssignmentExpressionSyntax, TInvocationExpressionSyntax, TParameterSyntax, TVariableDeclaratorSyntax, TLambdaSyntax> : SonarDiagnosticAnalyzer
        where TMethodSyntax : SyntaxNode
        where TArgumentSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
        where TIdentifierNameSyntax : SyntaxNode
        where TAssignmentExpressionSyntax : SyntaxNode
        where TInvocationExpressionSyntax : SyntaxNode
        where TParameterSyntax : SyntaxNode
        where TVariableDeclaratorSyntax : SyntaxNode
        where TLambdaSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S4830";
        protected const string MessageFormat = "Enable server certificate validation on this SSL/TLS connection";
        private readonly DiagnosticDescriptor rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
       
        internal abstract AbstractMethodParameterLookup<TArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method);
        protected abstract Location ArgumentLocation(TArgumentSyntax argument);
        protected abstract TExpressionSyntax ArgumentExpression(TArgumentSyntax argument);
        protected abstract void SplitAssignment(TAssignmentExpressionSyntax assignment, out TIdentifierNameSyntax leftIdentifier, out TExpressionSyntax right);
        protected abstract IEqualityComparer<TExpressionSyntax> CreateNodeEqualityComparer();
        protected abstract SyntaxNode FindRootClassOrModule(SyntaxNode node);
        protected abstract TExpressionSyntax[] FindReturnExpressions(SyntaxNode block);
        protected abstract bool IsTrueLiteral(TExpressionSyntax expression);
        protected abstract string IdentifierText(SyntaxNode node);
        protected abstract TExpressionSyntax VariableInitializer(TVariableDeclaratorSyntax variable);
        protected abstract ImmutableArray<Location> LambdaLocations(TLambdaSyntax lambda);
        protected abstract ImmutableArray<SyntaxNode> LocalVariableScopeStatements(TVariableDeclaratorSyntax variable);

        protected CertificateValidationCheckBase(System.Resources.ResourceManager rspecResources)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);
        }

        protected void CheckAssignmentSyntax(SyntaxNodeAnalysisContext c)
        {
            SplitAssignment((TAssignmentExpressionSyntax)c.Node, out var leftIdentifier, out var right);
            //var leftIdentifier = assignmentNode.Left.DescendantNodesAndSelf().OfType<TIdentifierNameSyntax>().LastOrDefault();
            //var right = assignmentNode.Right;
            if (leftIdentifier != null && right != null
                && c.SemanticModel.GetSymbolInfo(leftIdentifier).Symbol is IPropertySymbol left
                && IsValidationDelegateType(left.Type))
            {
                TryReportLocations(new InspectionContext(c), leftIdentifier.GetLocation(), right);
            }
        }

        protected void CheckConstructorParameterSyntax(SyntaxNodeAnalysisContext c)
        {
            if (c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol ctor)
            {
                AbstractMethodParameterLookup<TArgumentSyntax> methodParamLookup = null;       //Cache, there might be more of them
                foreach (var param in ctor.Parameters.Where(x => IsValidationDelegateType(x.Type)))
                {
                    methodParamLookup = methodParamLookup ?? CreateParameterLookup(c.Node, ctor);
                    if (methodParamLookup.TryGetSymbolParameter(param, out var argument))
                    {
                        TryReportLocations(new InspectionContext(c), ArgumentLocation(argument), ArgumentExpression(argument));
                    }
                }
            }
        }

        private void TryReportLocations(InspectionContext c, Location primaryLocation, TExpressionSyntax expression)
        {
            var locations = ArgumentLocations(c, expression);
            if (!locations.IsEmpty)
            {
                //Report both, assignemnt as well as all implementation occurances
                c.Context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, primaryLocation, additionalLocations: locations));
            }
        }

        protected static bool IsValidationDelegateType(ITypeSymbol type)
        {
            if (type.Is(KnownType.System_Net_Security_RemoteCertificateValidationCallback))
            {
                return true;
            }
            else if (type is INamedTypeSymbol NamedSymbol && type.Is(KnownType.System_Func_T1_T2_T3_T4_TResult))
            {
                //HttpClientHandler.ServerCertificateCustomValidationCallback uses Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                //We're actually looking for Func<Any Sender, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                var parameters = NamedSymbol.DelegateInvokeMethod.Parameters;
                return parameters.Length == 4   //And it should! T1, T2, T3, T4
                    && parameters[0].Type.IsClassOrStruct() //We don't care about common (Object) nor specific (HttpRequestMessage) type of Sender
                    && parameters[1].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Certificate2)
                    && parameters[2].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Chain)
                    && parameters[3].Type.Is(KnownType.System_Net_Security_SslPolicyErrors)
                    && NamedSymbol.DelegateInvokeMethod.ReturnType.Is(KnownType.System_Boolean);
            }
            return false;
        }

        protected ImmutableArray<Location> ArgumentLocations(InspectionContext c, TExpressionSyntax expression)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            switch (expression)
            {
                case TIdentifierNameSyntax identifier:
                    var identSymbol = c.Context.SemanticModel.GetSymbolInfo(identifier).Symbol;
                    if (identSymbol != null && identSymbol.DeclaringSyntaxReferences.Length == 1)
                    {
                        ret.AddRange(IdentifierLocations(c, identSymbol.DeclaringSyntaxReferences.Single().GetSyntax()));
                    }
                    break;
                case TLambdaSyntax lambda:
                    ret.AddRange(LambdaLocations(lambda));
                    break;
                case TInvocationExpressionSyntax invocation:
                    var invSymbol = c.Context.SemanticModel.GetSymbolInfo(invocation).Symbol;
                    if (invSymbol != null && invSymbol.DeclaringSyntaxReferences.Length == 1 && invSymbol.DeclaringSyntaxReferences.Single().GetSyntax() is TMethodSyntax syntax)
                    {
                        c.VisitedMethods.Add(syntax);
                        ret.AddRange(InvocationLocations(c, syntax));
                    }
                    break;
            }
            return ret.ToImmutableArray();
        }

        protected ImmutableArray<Location> IdentifierLocations(InspectionContext c, SyntaxNode syntax)
        {
            switch (syntax)
            {
                case TMethodSyntax method:                  //Direct delegate name
                    return BlockLocations(method);  //FIXME: Revize, bylo tady .Body
                case TParameterSyntax parameter:            //Value arrived as a parameter
                    return ParamLocations(c, parameter);
                case TVariableDeclaratorSyntax variable:     //Value passed as variable
                    return VariableLocations(c, variable);
            }
            return ImmutableArray<Location>.Empty;
        }

        protected ImmutableArray<Location> ParamLocations(InspectionContext c, TParameterSyntax param)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            var containingMethodDeclaration = param.FirstAncestorOrSelf<TMethodSyntax>();
            if (!c.VisitedMethods.Contains(containingMethodDeclaration))
            {
                c.VisitedMethods.Add(containingMethodDeclaration);
                var containingMethod = c.Context.SemanticModel.GetDeclaredSymbol(containingMethodDeclaration) as IMethodSymbol;
                var paramSymbol = containingMethod.Parameters.Single(x => x.Name == IdentifierText(param));
                foreach (var invocation in FindInvocationList(c.Context, FindRootClassOrModule(param), containingMethod))
                {
                    var methodParamLookup = CreateParameterLookup(invocation, containingMethod);
                    if (methodParamLookup.TryGetSymbolParameter(paramSymbol, out var argument))
                    {
                        ret.AddRange(CallStackSublocations(c, ArgumentExpression(argument)));
                    }
                }
            }
            return ret.ToImmutable();
        }

        protected ImmutableArray<Location> VariableLocations(InspectionContext c, TVariableDeclaratorSyntax variable)
        {
            var allAssignedExpressions = new List<TExpressionSyntax>();
            var parentBlock = variable.Parent;  //FIXME: Revize, bylo tu hledani nadrazeneho bloku
            if (parentBlock != null)
            {
                allAssignedExpressions.AddRange(parentBlock.DescendantNodes().OfType<TAssignmentExpressionSyntax>()
                    .Select(x =>
                    {
                        SplitAssignment(x, out var leftIdentifier, out var right);
                        return new { leftIdentifier, right };
                    })
                    .Where( x => x.leftIdentifier != null && IdentifierText(x.leftIdentifier) == IdentifierText(variable))
                    .Select(x => x.right));
                    //FIXME: Zkontrolovat a Vyhodit .Where(x => x.Left is TIdentifierNameSyntax ident && IdentifierText(ident) == IdentifierText(variable))
            }
            var initializer = VariableInitializer(variable);
            if (initializer != null)       //Declarator initializer is counted as (default) assignment as well
            {
                allAssignedExpressions.Add(initializer);
            }
            return MultiExpressionSublocations(c, allAssignedExpressions);
        }

        private ImmutableArray<Location> InvocationLocations(InspectionContext c, TMethodSyntax method)
        {
            var returnExpressionSublocationsList = FindReturnExpressions(method).Where(x => !IsVisited(c, x));      //Ignore all return statements with recursive call. Result depends on returns that could return compliant validator.
            return MultiExpressionSublocations(c, returnExpressionSublocationsList);
        }

        protected static bool IsVisited(InspectionContext c, TExpressionSyntax expression)
        {
            if (expression is TInvocationExpressionSyntax invocation)
            {
                var symbol = c.Context.SemanticModel.GetSymbolInfo(invocation).Symbol;
                return symbol != null && !symbol.DeclaringSyntaxReferences.IsEmpty
                    && symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax() as TMethodSyntax).Any(x => x != null && c.VisitedMethods.Contains(x));
            }
            return false;
        }

        protected ImmutableArray<Location> MultiExpressionSublocations(InspectionContext c, IEnumerable<TExpressionSyntax> expressions)
        {
            var exprSublocationsList = expressions.Distinct(CreateNodeEqualityComparer())
                .Select(x => CallStackSublocations(c, x))
                .ToArray();
            if (exprSublocationsList.Any(x => x.IsEmpty))   //If there's at leat one concurrent expression, that returns compliant delegate, than there's some logic and this scope is compliant
            {
                return ImmutableArray<Location>.Empty;
            }
            return exprSublocationsList.SelectMany(x => x).ToImmutableArray();      //Else every return statement is noncompliant
        }

        protected ImmutableArray<Location> CallStackSublocations(InspectionContext c, TExpressionSyntax expression)
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

        protected ImmutableArray<Location> BlockLocations(SyntaxNode block)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            if (block != null)
            {
                var returnExpressions = FindReturnExpressions(block);
                if (returnExpressions.All(x => IsTrueLiteral(x)))    //There must be at least one return, that does not return true to be compliant
                {
                    ret.AddRange(returnExpressions.Select(x => x.GetLocation()));
                }
            }
            return ret.ToImmutable();
        }

        protected static ImmutableArray<TInvocationExpressionSyntax> FindInvocationList(SyntaxNodeAnalysisContext c, SyntaxNode root, IMethodSymbol method)
        {
            if (root == null || method == null)
            {
                return ImmutableArray<TInvocationExpressionSyntax>.Empty;
            }
            var ret = ImmutableArray.CreateBuilder<TInvocationExpressionSyntax>();
            foreach (var invocation in root.DescendantNodesAndSelf().OfType<TInvocationExpressionSyntax>())
            {
                if (c.SemanticModel.GetSymbolInfo(invocation).Symbol == method)
                {
                    ret.Add(invocation);
                }
            }
            return ret.ToImmutable();
        }

        protected struct InspectionContext
        {
            public readonly SyntaxNodeAnalysisContext Context;
            public readonly HashSet<TMethodSyntax> VisitedMethods;

            public InspectionContext(SyntaxNodeAnalysisContext context)
            {
                this.Context = context;
                this.VisitedMethods = new HashSet<TMethodSyntax>();
            }
        }

    }
}

