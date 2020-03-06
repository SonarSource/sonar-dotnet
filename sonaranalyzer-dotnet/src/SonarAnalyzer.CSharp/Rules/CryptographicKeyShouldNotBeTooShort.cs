/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Collections.Immutable;
using System.Text.RegularExpressions;
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
    public sealed class CryptographicKeyShouldNotBeTooShort : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4426";
        private const string MessageFormat = "Use a key length of at least {0} bits for {1} cipher algorithm.{2}";
        private const string AdditionalInfo = " This assignment does not actually update the underlying key size.";

        private const int MinimalCommonKeyLength = 2048;
        private const int MinimalEllipticCurveKeyLength = 224;
        private readonly Regex NamedEllipticCurve = new Regex("^(secp|sect|prime|c2tnb|c2pnb|brainpoolP|B-|K-|P-)(?<KeyLength>\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly ImmutableArray<KnownType> BouncyCastleCurveClasses =
            ImmutableArray.Create(
                KnownType.Org_BouncyCastle_Asn1_Nist_NistNamedCurves,
                KnownType.Org_BouncyCastle_Asn1_Sec_SecNamedCurves,
                KnownType.Org_BouncyCastle_Asn1_TeleTrust_TeleTrusTNamedCurves,
                KnownType.Org_BouncyCastle_Asn1_X9_ECNamedCurveTable,
                KnownType.Org_BouncyCastle_Asn1_X9_X962NamedCurves);

        private static readonly ImmutableArray<KnownType> SystemSecurityCryptographyDsaRsa =
            ImmutableArray.Create(
                KnownType.System_Security_Cryptography_DSA,
                KnownType.System_Security_Cryptography_RSA);

        private static readonly ImmutableArray<KnownType> SystemSecurityCryptographyCurveClasses =
            ImmutableArray.Create(
                KnownType.System_Security_Cryptography_ECDiffieHellman,
                KnownType.System_Security_Cryptography_ECDsa);

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var methodName = GetMethodName(invocation);
                    var containingType = new Lazy<ITypeSymbol>(() => c.SemanticModel.GetSymbolInfo(invocation).Symbol?.ContainingType);

                    switch (methodName)
                    {
                        case "Create":
                            CheckAlgorithmCreation(containingType.Value, invocation, c);
                            break;
                        case "GenerateKey":
                            CheckSystemSecurityEllipticCurve(containingType.Value, invocation, invocation.ArgumentList, c);
                            break;
                        case "GetByName":
                            CheckBouncyCastleEllipticCurve(containingType.Value, invocation, c);
                            break;
                        case "Init":
                            CheckBouncyCastleParametersGenerators(containingType.Value, invocation, c);
                            break;
                        default:
                            // Current method is not related to any cryptographic method of interest
                            break;
                    }
                },
                SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                    if (!(c.SemanticModel.GetTypeInfo(objectCreation).Type is ITypeSymbol containingType))
                    {
                        return;
                    }

                    CheckSystemSecurityEllipticCurve(containingType, objectCreation, objectCreation.ArgumentList, c);
                    CheckSystemSecurityCryptographyAlgorithms(containingType, objectCreation, c);
                    CheckBouncyCastleKeyGenerationParameters(containingType, objectCreation, c);
                },
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;
                    var propertyName = GetPropertyName(assignment.Left);
                    if (propertyName == "KeySize"
                        && assignment.Left is MemberAccessExpressionSyntax memberAccess
                        && memberAccess.Expression != null
                        && c.SemanticModel.GetTypeInfo(memberAccess.Expression).Type is ITypeSymbol containingType)
                    {
                        // Using the KeySize setter on DSACryptoServiceProvider/RSACryptoServiceProvider does not actually change the underlying key size
                        if (containingType.IsAny(KnownType.System_Security_Cryptography_DSACryptoServiceProvider, KnownType.System_Security_Cryptography_RSACryptoServiceProvider))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, assignment.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), AdditionalInfo));
                        }
                        else
                        {
                            CheckGenericDsaRsaCryptographyAlgorithms(containingType, assignment, assignment.Right, c);
                        }
                    }
                },
                SyntaxKind.SimpleAssignmentExpression);
        }

        private void CheckAlgorithmCreation(ITypeSymbol containingType, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext c)
        {
            var firstParam = invocation.ArgumentList.Get(0);
            if (firstParam == null || containingType == null)
            {
                return;
            }

            if (containingType.IsAny(SystemSecurityCryptographyDsaRsa) && IsInvalidCommonKeyLength(firstParam, c))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), ""));
            }
            else
            {
                CheckSystemSecurityEllipticCurve(containingType, invocation, invocation.ArgumentList, c);
            }
        }

        private void CheckBouncyCastleEllipticCurve(ITypeSymbol containingType, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext c)
        {
            var firstParam = invocation.ArgumentList.Get(0);
            if (firstParam == null || containingType == null || !containingType.IsAny(BouncyCastleCurveClasses))
            {
                return;
            }

            var param = c.SemanticModel.GetConstantValue(firstParam);
            if (param.HasValue && param.Value is string curveId)
            {
                CheckCurveNameKeyLength(invocation, curveId, c);
            }
        }

        private void CheckSystemSecurityEllipticCurve(ITypeSymbol containingType, ExpressionSyntax syntaxElement, ArgumentListSyntax argumentList, SyntaxNodeAnalysisContext c)
        {
            var firstParam = argumentList.Get(0);
            if (firstParam == null || containingType == null || !containingType.DerivesFromAny(SystemSecurityCryptographyCurveClasses))
            {
                return;
            }

            var paramSymbol = c.SemanticModel.GetSymbolInfo(firstParam).Symbol;
            if (paramSymbol != null)
            {
                CheckCurveNameKeyLength(syntaxElement, paramSymbol.Name, c);
            }
        }

        private void CheckCurveNameKeyLength(SyntaxNode syntaxElement, string curveName, SyntaxNodeAnalysisContext c)
        {
            var match = NamedEllipticCurve.Match(curveName);
            if (match.Success && int.TryParse(match.Groups["KeyLength"].Value, out var keyLength) && keyLength < MinimalEllipticCurveKeyLength)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, syntaxElement.GetLocation(), MinimalEllipticCurveKeyLength, "EC", ""));
            }
        }

        private void CheckSystemSecurityCryptographyAlgorithms(ITypeSymbol containingType, ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext c)
        {
            var firstParam = objectCreation.ArgumentList.Get(0);

            // DSACryptoServiceProvider is always noncompliant as it has a max key size of 1024
            // RSACryptoServiceProvider default constructor of  is noncompliant as it has default key size of 1024
            if (containingType.Is(KnownType.System_Security_Cryptography_DSACryptoServiceProvider)
                || (containingType.Is(KnownType.System_Security_Cryptography_RSACryptoServiceProvider) && firstParam == null))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, objectCreation.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), ""));
            }
            else
            {
                CheckGenericDsaRsaCryptographyAlgorithms(containingType, objectCreation, firstParam, c);
            }
        }

        private static void CheckGenericDsaRsaCryptographyAlgorithms(ITypeSymbol containingType, SyntaxNode syntaxElement, ExpressionSyntax keyLengthSyntax, SyntaxNodeAnalysisContext c)
        {
            if (containingType.DerivesFromAny(SystemSecurityCryptographyDsaRsa) && keyLengthSyntax != null && IsInvalidCommonKeyLength(keyLengthSyntax, c))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, syntaxElement.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), ""));
            }
        }

        private static void CheckBouncyCastleParametersGenerators(ITypeSymbol containingType, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext c)
        {
            var firstParam = invocation.ArgumentList.Get(0);
            if (firstParam == null
                || containingType == null
                || !containingType.IsAny(KnownType.Org_BouncyCastle_Crypto_Generators_DHParametersGenerator, KnownType.Org_BouncyCastle_Crypto_Generators_DsaParametersGenerator))
            {
                return;
            }

            if (IsInvalidCommonKeyLength(firstParam, c))
            {
                var cipherAlgorithmName = containingType.Is(KnownType.Org_BouncyCastle_Crypto_Generators_DHParametersGenerator) ? "DH" : "DSA";
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(), MinimalCommonKeyLength, cipherAlgorithmName, ""));
            }
        }

        private static void CheckBouncyCastleKeyGenerationParameters(ITypeSymbol containingType, ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext c)
        {
            var keyLengthParam = objectCreation.ArgumentList.Get(2);
            if (keyLengthParam == null || !containingType.Is(KnownType.Org_BouncyCastle_Crypto_Parameters_RsaKeyGenerationParameters))
            {
                return;
            }

            var optionalKeyLength = c.SemanticModel.GetConstantValue(keyLengthParam);
            if (optionalKeyLength.HasValue && optionalKeyLength.Value is int keyLength && keyLength < MinimalCommonKeyLength)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, objectCreation.GetLocation(), MinimalCommonKeyLength, "RSA", ""));
            }
        }

        private static bool IsInvalidCommonKeyLength(SyntaxNode keyLengthSyntax, SyntaxNodeAnalysisContext c)
        {
            var optionalKeyLength = c.SemanticModel.GetConstantValue(keyLengthSyntax);
            return optionalKeyLength.HasValue && optionalKeyLength.Value is int keyLength && keyLength < MinimalCommonKeyLength;
        }

        private static string GetMethodName(InvocationExpressionSyntax invocationExpression) =>
            invocationExpression.Expression.GetIdentifier()?.Identifier.ValueText;

        private static string GetPropertyName(ExpressionSyntax expression)
        {
            var nameSyntax = expression.GetIdentifier();
            if (nameSyntax != null)
            {
                return nameSyntax.Identifier.ValueText;
            }
            return null;
        }

        private static string CipherName(ITypeSymbol containingType)
        {
            if (containingType.Is(KnownType.System_Security_Cryptography_DSA)
                || containingType.DerivesFrom(KnownType.System_Security_Cryptography_DSA))
            {
                return "DSA";
            }
            return "RSA";
        }
    }
}
