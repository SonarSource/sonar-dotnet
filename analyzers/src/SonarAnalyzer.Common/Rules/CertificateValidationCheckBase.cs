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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CertificateValidationCheckBase<
        TSyntaxKind,
        TArgumentSyntax,
        TExpressionSyntax,
        TIdentifierNameSyntax,
        TAssignmentExpressionSyntax,
        TInvocationExpressionSyntax,
        TParameterSyntax,
        TVariableSyntax,
        TLambdaSyntax,
        TMemberAccessSyntax
        > : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TArgumentSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
        where TIdentifierNameSyntax : SyntaxNode
        where TAssignmentExpressionSyntax : SyntaxNode
        where TInvocationExpressionSyntax : SyntaxNode
        where TParameterSyntax : SyntaxNode
        where TVariableSyntax : SyntaxNode
        where TLambdaSyntax : SyntaxNode
        where TMemberAccessSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S4830";
        private const string MessageFormat = "Enable server certificate validation on this SSL/TLS connection";
        private readonly DiagnosticDescriptor rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        internal /* for testing */ abstract MethodParameterLookupBase<TArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method);
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TSyntaxKind[] MethodDeclarationKinds { get; }
        protected abstract Location ExpressionLocation(SyntaxNode expression);
        protected abstract void SplitAssignment(TAssignmentExpressionSyntax assignment, out TIdentifierNameSyntax leftIdentifier, out TExpressionSyntax right);
        protected abstract IEqualityComparer<TExpressionSyntax> CreateNodeEqualityComparer();
        protected abstract TExpressionSyntax[] FindReturnAndThrowExpressions(InspectionContext c, SyntaxNode block);
        protected abstract bool IsTrueLiteral(TExpressionSyntax expression);
        protected abstract TExpressionSyntax VariableInitializer(TVariableSyntax variable);
        protected abstract ImmutableArray<Location> LambdaLocations(InspectionContext c, TLambdaSyntax lambda);
        protected abstract SyntaxNode LocalVariableScope(TVariableSyntax variable);
        protected abstract SyntaxNode ExtractArgumentExpressionNode(SyntaxNode expression);
        protected abstract SyntaxNode SyntaxFromReference(SyntaxReference reference);
        private protected abstract KnownType GenericDelegateType();

        protected CertificateValidationCheckBase()
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);
        }

        protected void CheckAssignmentSyntax(SyntaxNodeAnalysisContext c)
        {
            SplitAssignment((TAssignmentExpressionSyntax)c.Node, out var leftIdentifier, out var right);
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
                MethodParameterLookupBase<TArgumentSyntax> methodParamLookup = null;       // Cache, there might be more of them
                // Validation for TryGetNonParamsSyntax, ParamArray/params and therefore array arguments are not inspected
                foreach (var param in ctor.Parameters.Where(x => !x.IsParams && IsValidationDelegateType(x.Type)))
                {
                    methodParamLookup ??= CreateParameterLookup(c.Node, ctor);
                    if (methodParamLookup.TryGetNonParamsSyntax(param, out var expression))
                    {
                        TryReportLocations(new InspectionContext(c), ExpressionLocation(expression), expression);
                    }
                }
            }
        }

        protected ImmutableArray<Location> ParamLocations(InspectionContext c, TParameterSyntax param)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            var containingMethodDeclaration = param.FirstAncestorOrSelf((SyntaxNode x) => Language.Syntax.IsAnyKind(x, MethodDeclarationKinds));
            if (containingMethodDeclaration != null && !c.VisitedMethods.Contains(containingMethodDeclaration))
            {
                c.VisitedMethods.Add(containingMethodDeclaration);
                var containingMethod = c.Context.SemanticModel.GetDeclaredSymbol(containingMethodDeclaration) as IMethodSymbol;
                var identText = Language.Syntax.NodeIdentifier(param)?.ValueText;
                var paramSymbol = containingMethod?.Parameters.Single(x => x.Name == identText);
                if (paramSymbol is {IsParams: false})  // Validation for TryGetNonParamsSyntax, ParamArray/params and therefore array arguments are not inspected
                {
                    foreach (var invocation in FindInvocationList(c.Context, FindRootTypeDeclaration(param), containingMethod))
                    {
                        var methodParamLookup = CreateParameterLookup(invocation, containingMethod);
                        if (methodParamLookup.TryGetNonParamsSyntax(paramSymbol, out var expression))
                        {
                            ret.AddRange(CallStackSublocations(c, expression));
                        }
                    }
                }
            }
            return ret.ToImmutable();
        }

        protected ImmutableArray<Location> BlockLocations(InspectionContext c, SyntaxNode block)
        {
            var ret = ImmutableArray.CreateBuilder<Location>();
            if (block != null)
            {
                var returnExpressions = FindReturnAndThrowExpressions(c, block);
                // There must be at least one return, that does not return true to be compliant. There can be NULL from standalone Throw statement.
                if (returnExpressions.All(IsTrueLiteral))
                {
                    ret.AddRange(returnExpressions.Select(x => x.GetLocation()));
                }
            }
            return ret.ToImmutable();
        }

        protected virtual SyntaxNode FindRootTypeDeclaration(SyntaxNode node)
        {
            SyntaxNode candidate;
            var current = node.FirstAncestorOrSelf<SyntaxNode>(IsTypeDeclaration);
            while (current != null && (candidate = current.Parent.FirstAncestorOrSelf<SyntaxNode>(IsTypeDeclaration)) != null)  // Search for parent of nested type
            {
                current = candidate;
            }
            return current;
        }

        private bool IsTypeDeclaration(SyntaxNode expression) =>
            Language.Syntax.IsAnyKind(expression, Language.SyntaxKind.TypeDeclaration);

        private void TryReportLocations(InspectionContext c, Location primaryLocation, SyntaxNode expression)
        {
            var locations = ArgumentLocations(c, expression);
            if (!locations.IsEmpty)
            {
                // Report both, assignment as well as all implementation occurrences
                c.Context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, primaryLocation, locations));
            }
        }

        private bool IsValidationDelegateType(ITypeSymbol type)
        {
            if (type.Is(KnownType.System_Net_Security_RemoteCertificateValidationCallback))
            {
                return true;
            }
            if (type is INamedTypeSymbol namedSymbol && type.OriginalDefinition.Is(GenericDelegateType()))
            {
                // HttpClientHandler.ServerCertificateCustomValidationCallback uses Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                // We're actually looking for Func<Any Sender, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                var parameters = namedSymbol.DelegateInvokeMethod.Parameters;
                return parameters.Length == 4   // And it should! T1, T2, T3, T4
                    && parameters[0].Type.IsClassOrStruct() // We don't care about common (Object) nor specific (HttpRequestMessage) type of Sender
                    && parameters[1].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Certificate2)
                    && parameters[2].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Chain)
                    && parameters[3].Type.Is(KnownType.System_Net_Security_SslPolicyErrors)
                    && namedSymbol.DelegateInvokeMethod.ReturnType.Is(KnownType.System_Boolean);
            }
            return false;
        }

        private ImmutableArray<Location> ArgumentLocations(InspectionContext c, SyntaxNode expression)
        {
            switch (ExtractArgumentExpressionNode(expression))
            {
                case TIdentifierNameSyntax identifier:
                    if (c.Context.SemanticModel.GetSymbolInfo(identifier).Symbol is {DeclaringSyntaxReferences: {Length: 1}} identSymbol)
                    {
                        return IdentifierLocations(c, SyntaxFromReference(identSymbol.DeclaringSyntaxReferences.Single()));
                    }
                    break;
                case TLambdaSyntax lambda:
                    return LambdaLocations(c, lambda);
                case TInvocationExpressionSyntax invocation:
                    if (c.Context.SemanticModel.GetSymbolInfo(invocation).Symbol is {DeclaringSyntaxReferences: {Length: 1}} invSymbol
                        && SyntaxFromReference(invSymbol.DeclaringSyntaxReferences.Single()) is { } syntax)
                    {
                        c.VisitedMethods.Add(syntax);
                        return InvocationLocations(c, syntax);
                    }
                    break;
                case TMemberAccessSyntax memberAccess:
                    if (c.Context.SemanticModel.GetSymbolInfo(memberAccess).Symbol is { } maSymbol
                        && maSymbol.IsInType(KnownType.System_Net_Http_HttpClientHandler)
                        && maSymbol.Name == "DangerousAcceptAnyServerCertificateValidator")
                    {
                        return new[] { memberAccess.GetLocation() }.ToImmutableArray();
                    }
                    break;
            }
            return ImmutableArray<Location>.Empty;
        }

        private ImmutableArray<Location> IdentifierLocations(InspectionContext c, SyntaxNode syntax) =>
            syntax switch
            {
                TParameterSyntax parameter => ParamLocations(c, parameter), // Value arrived as a parameter
                TVariableSyntax variable => VariableLocations(c, variable), // Value passed as variable
                _ => Language.Syntax.IsAnyKind(syntax, MethodDeclarationKinds)
                    ? BlockLocations(c, syntax)                             // Direct delegate name
                    : ImmutableArray<Location>.Empty,
            };

        private ImmutableArray<Location> VariableLocations(InspectionContext c, TVariableSyntax variable)
        {
            var allAssignedExpressions = new List<TExpressionSyntax>();
            var parentScope = LocalVariableScope(variable);
            if (parentScope != null)
            {
                var identText = Language.Syntax.NodeIdentifier(variable)?.ValueText;
                allAssignedExpressions.AddRange(parentScope.DescendantNodes().OfType<TAssignmentExpressionSyntax>()
                    .Select(x =>
                    {
                        SplitAssignment(x, out var leftIdentifier, out var right);
                        return new { leftIdentifier, right };
                    })
                    .Where(x => x.leftIdentifier != null && Language.Syntax.NodeIdentifier(x.leftIdentifier) is { } identifier && identifier.ValueText == identText)
                    .Select(x => x.right));
            }
            var initializer = VariableInitializer(variable);
            if (initializer != null)       // Declarator initializer is counted as (default) assignment as well
            {
                allAssignedExpressions.Add(initializer);
            }
            return MultiExpressionSublocations(c, allAssignedExpressions);
        }

        private ImmutableArray<Location> InvocationLocations(InspectionContext c, SyntaxNode method)
        {
            // Ignore all return statements with recursive call. Result depends on returns that could return compliant validator.
            var returnExpressionSublocationsList = FindReturnAndThrowExpressions(c, method).Where(x => !IsVisited(c, x));
            return MultiExpressionSublocations(c, returnExpressionSublocationsList);
        }

        private bool IsVisited(InspectionContext c, SyntaxNode expression)
        {
            if (expression is TInvocationExpressionSyntax invocation)
            {
                var symbol = c.Context.SemanticModel.GetSymbolInfo(invocation).Symbol;
                return symbol.DeclaringSyntaxReferences.Select(SyntaxFromReference).Any(x => c.VisitedMethods.Contains(x));
            }
            return false;
        }

        private ImmutableArray<Location> MultiExpressionSublocations(InspectionContext c, IEnumerable<TExpressionSyntax> expressions)
        {
            var exprSublocationsList = expressions.Distinct(CreateNodeEqualityComparer())
                .Select(x => CallStackSublocations(c, x))
                .ToArray();
            // If there's at least one concurrent expression, that returns compliant delegate, then there's some logic and this scope is compliant
            if (exprSublocationsList.Any(x => x.IsEmpty))
            {
                return ImmutableArray<Location>.Empty;
            }
            return exprSublocationsList.SelectMany(x => x).ToImmutableArray();      // Else every return statement is noncompliant
        }

        private ImmutableArray<Location> CallStackSublocations(InspectionContext c, SyntaxNode expression)
        {
            var lst = ArgumentLocations(c, expression);
            if (!lst.IsEmpty)        // There's noncompliant issue in this chain
            {
                var loc = expression.GetLocation();
                if (!lst.Any(x => x.SourceSpan.IntersectsWith(loc.SourceSpan)))
                {
                    // Add 2nd, 3rd, 4th etc //Secondary marker. If it is not marked already from direct Delegate name or direct Lambda occurrence
                    return lst.Concat(new[] { loc }).ToImmutableArray();
                }
            }
            return lst;
        }

        private static ImmutableArray<TInvocationExpressionSyntax> FindInvocationList(SyntaxNodeAnalysisContext c, SyntaxNode root, IMethodSymbol method)
        {
            if (root == null || method == null)
            {
                return ImmutableArray<TInvocationExpressionSyntax>.Empty;
            }
            var ret = ImmutableArray.CreateBuilder<TInvocationExpressionSyntax>();
            foreach (var invocation in root.DescendantNodesAndSelf().OfType<TInvocationExpressionSyntax>())
            {
                if (c.SemanticModel.GetSymbolInfo(invocation).Symbol is { } symbol
                    && symbol.Equals(method))
                {
                    ret.Add(invocation);
                }
            }
            return ret.ToImmutable();
        }

        protected readonly struct InspectionContext
        {
            public readonly SyntaxNodeAnalysisContext Context;
            public readonly HashSet<SyntaxNode> VisitedMethods;

            public InspectionContext(SyntaxNodeAnalysisContext context)
            {
                Context = context;
                VisitedMethods = new HashSet<SyntaxNode>();
            }
        }
    }
}
