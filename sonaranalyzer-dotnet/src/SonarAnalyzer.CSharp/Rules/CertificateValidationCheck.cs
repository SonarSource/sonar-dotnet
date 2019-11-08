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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
//FIXME: REMOVE
using System.Diagnostics;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CertificateValidationCheck : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4830";
        private const string MessageFormat = "Enable server certificate validation on this SSL/TLS connection";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            //Handling of += delegate syntax
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckAddHandlerSyntax(c), SyntaxKind.AddAssignmentExpression);

            //Handling of constructor parameter syntax (SslStream)
            context.RegisterSyntaxNodeActionInNonGenerated(c => CheckConstructorParameterSyntax(c), SyntaxKind.ObjectCreationExpression);
        }

        private static void CheckAddHandlerSyntax(SyntaxNodeAnalysisContext c)
        {
            var LeftIdentifier = ((AssignmentExpressionSyntax)c.Node).Left.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault();
            var Right = ((AssignmentExpressionSyntax)c.Node).Right;
            if (LeftIdentifier != null && Right != null)
            {
                var Left = c.SemanticModel.GetSymbolInfo(LeftIdentifier).Symbol as IPropertySymbol;
                if (Left != null && IsValidationDelegateType(Left.Type))
                {
                    TryReportLocations(c, LeftIdentifier.GetLocation(), Right);
                }
            }
        }

        private static void CheckConstructorParameterSyntax(SyntaxNodeAnalysisContext c)
        {
            CSharpMethodParameterLookup methodParameterLookup = null;       //Cache, there might be more of them
            if (c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol Ctor)
            {
                foreach (var Param in Ctor.Parameters.Where(x => IsValidationDelegateType(x.Type)))
                {
                    ArgumentSyntax Argument;
                    methodParameterLookup = methodParameterLookup ?? new CSharpMethodParameterLookup((c.Node as ObjectCreationExpressionSyntax).ArgumentList, Ctor);
                    if (methodParameterLookup.TryGetSymbolParameter(Param, out Argument))
                    { //For Lambda expression extract location of the parenthesizis only to separate them from secondary location of "true"
                        var PrimaryLocation = ((Argument.Expression is ParenthesizedLambdaExpressionSyntax Lambda) ? (SyntaxNode)Lambda.ParameterList : Argument).GetLocation();
                        TryReportLocations(c, PrimaryLocation, Argument.Expression);
                    }
                }
            }
        }

        private static void TryReportLocations(SyntaxNodeAnalysisContext c, Location primaryLocation, ExpressionSyntax expression)
        {
            var Locations = NoncompliantArgumentLocation(c, expression);
            if (Locations.Length != 0)
            {   //Report both, assignemnt as well as all implementation occurances
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, primaryLocation, additionalLocations: Locations));
            }
        }

        private static bool IsValidationDelegateType(ITypeSymbol type)
        {
            if (type.Is(KnownType.System_Net_Security_RemoteCertificateValidationCallback))
            {
                return true;
            }
            else if (type is INamedTypeSymbol NamedSymbol && type.Is(KnownType.System_Func_T1_T2_T3_T4_TResult))
            {
                //HttpClientHandler.ServerCertificateCustomValidationCallback uses Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                //We're actually looking for Func<Any Sender, X509Certificate2, X509Chain, SslPolicyErrors, bool>
                var Params = NamedSymbol.DelegateInvokeMethod.Parameters;
                return Params.Length == 4   //And it should! T1, T2, T3, T4
                    && Params[0].Type.IsClassOrStruct() //We don't care about common (Object) nor specific (HttpRequestMessage) type of Sender
                    && Params[1].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Certificate2)
                    && Params[2].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Chain)
                    && Params[3].Type.Is(KnownType.System_Net_Security_SslPolicyErrors)
                    && NamedSymbol.DelegateInvokeMethod.ReturnType.Is(KnownType.System_Boolean);
            }
            return false;
        }

        private static ImmutableArray<Location> NoncompliantArgumentLocation(SyntaxNodeAnalysisContext c, ExpressionSyntax expression)
        {
            var Ret = ImmutableArray.CreateBuilder<Location>();
            switch (expression)
            {
                case IdentifierNameSyntax Identifier:   //Direct delegate
                    var MS = c.SemanticModel.GetSymbolInfo(Identifier).Symbol;
                    foreach (var Syntax in MS.DeclaringSyntaxReferences.Select(x => x.GetSyntax()))
                    {
                        Debugger.Break();
                        switch (Syntax)
                        {
                            case MethodDeclarationSyntax Method: //Direct delegate name
                                Ret.AddRange(NoncompliantLocations(Method.Body));
                                break;
                            case ParameterSyntax Param:         //Value arrived as parameter
                                //FIXME: Doresit, dohledat volajici

                                break;
                            case VariableDeclaratorSyntax Variable:
                                //FIXME: Doresit 

                                break;
                        }
                    }
                    break;
                case ParenthesizedLambdaExpressionSyntax Lambda:
                    if ((Lambda.Body as LiteralExpressionSyntax)?.Kind() == SyntaxKind.TrueLiteralExpression)
                    {
                        Ret.Add(Lambda.Body.GetLocation());   //Code was found guilty for lambda (...) => true
                    }
                    else if (Lambda.Body is BlockSyntax Block)
                    {
                        Ret.AddRange(NoncompliantLocations(Block));
                    }
                    break;
                case InvocationExpressionSyntax Invocation:
                    //False Negative, current version is not recursively inspecting invocations to get validation delegate
                    //ServerCertificateValidationCallback += FindValidator(false)
                    break;
            }
            return Ret.ToImmutableArray();
        }

        private static ImmutableArray<Location> NoncompliantLocations(BlockSyntax block)
        {
            var Ret = ImmutableArray.CreateBuilder<Location>();
            if (block != null)
            {
                //FIXME: VB.NET vs. return by assign to function name
                var ReturnExpressions = block.DescendantNodes().OfType<ReturnStatementSyntax>().Select(x => x.Expression).ToArray();
                if (ReturnExpressions.All(x => x.Kind() == SyntaxKind.TrueLiteralExpression))    //There must be at least one return, that does not return true to be compliant
                {
                    Ret.AddRange(ReturnExpressions.Select(x => x.GetLocation()));
                }
            }
            return Ret.ToImmutable();
        }

    }
}

