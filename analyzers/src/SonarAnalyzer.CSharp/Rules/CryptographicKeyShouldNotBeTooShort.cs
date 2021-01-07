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
        private const string UselessAssignmentInfo = " This assignment does not update the underlying key size.";

        private const int MinimalCommonKeyLength = 2048;
        private const int MinimalEllipticCurveKeyLength = 224;
        private readonly Regex namedEllipticCurve = new Regex("^(secp|sect|prime|c2tnb|c2pnb|brainpoolP|B-|K-|P-)(?<KeyLength>\\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

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
                        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.dsacryptoserviceprovider.keysize
                        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider.keysize
                        if (containingType.IsAny(KnownType.System_Security_Cryptography_DSACryptoServiceProvider, KnownType.System_Security_Cryptography_RSACryptoServiceProvider))
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, assignment.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), UselessAssignmentInfo));
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
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, invocation.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), ""));
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

            if (firstParam.FindStringConstant(c.SemanticModel) is { } curveId)
            {
                CheckCurveNameKeyLength(invocation, curveId, c);
            }
        }

        private void CheckSystemSecurityEllipticCurve(ITypeSymbol containingType, SyntaxNode syntaxElement, ArgumentListSyntax argumentList, SyntaxNodeAnalysisContext c)
        {
            var firstParam = argumentList.Get(0);
            if (firstParam == null || containingType == null || !containingType.DerivesFromAny(SystemSecurityCryptographyCurveClasses))
            {
                return;
            }

            if (c.SemanticModel.GetSymbolInfo(firstParam).Symbol is { }  paramSymbol)
            {
                CheckCurveNameKeyLength(syntaxElement, paramSymbol.Name, c);
            }
        }

        private void CheckCurveNameKeyLength(SyntaxNode syntaxElement, string curveName, SyntaxNodeAnalysisContext c)
        {
            var match = namedEllipticCurve.Match(curveName);
            if (match.Success && int.TryParse(match.Groups["KeyLength"].Value, out var keyLength) && keyLength < MinimalEllipticCurveKeyLength)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, syntaxElement.GetLocation(), MinimalEllipticCurveKeyLength, "EC", ""));
            }
        }

        private static void CheckSystemSecurityCryptographyAlgorithms(ITypeSymbol containingType, ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext c)
        {
            // DSACryptoServiceProvider is always noncompliant as it has a max key size of 1024
            // RSACryptoServiceProvider() and RSACryptoServiceProvider(System.Security.Cryptography.CspParameters) constructors are noncompliants as they have a default key size of 1024
            if (containingType.Is(KnownType.System_Security_Cryptography_DSACryptoServiceProvider)
                || (containingType.Is(KnownType.System_Security_Cryptography_RSACryptoServiceProvider) && HasDefaultSize(objectCreation.ArgumentList.Arguments, c)))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, objectCreation.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), ""));
            }
            else
            {
                var firstParam = objectCreation.ArgumentList.Get(0);
                CheckGenericDsaRsaCryptographyAlgorithms(containingType, objectCreation, firstParam, c);
            }
        }

        private static bool HasDefaultSize(SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxNodeAnalysisContext c) =>
            arguments.Count == 0
            || (arguments.Count == 1 && c.SemanticModel.GetTypeInfo(arguments[0].Expression).Type is ITypeSymbol type && type.Is(KnownType.System_Security_Cryptography_CspParameters));

        private static void CheckGenericDsaRsaCryptographyAlgorithms(ITypeSymbol containingType, SyntaxNode syntaxElement, SyntaxNode keyLengthSyntax, SyntaxNodeAnalysisContext c)
        {
            if (containingType.DerivesFromAny(SystemSecurityCryptographyDsaRsa) && keyLengthSyntax != null && IsInvalidCommonKeyLength(keyLengthSyntax, c))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, syntaxElement.GetLocation(), MinimalCommonKeyLength, CipherName(containingType), ""));
            }
        }

        private static void CheckBouncyCastleParametersGenerators(ITypeSymbol containingType, InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext c)
        {
            if (invocation.ArgumentList.Get(0) is { } firstParam
                && containingType != null
                && containingType.IsAny(KnownType.Org_BouncyCastle_Crypto_Generators_DHParametersGenerator, KnownType.Org_BouncyCastle_Crypto_Generators_DsaParametersGenerator)
                && IsInvalidCommonKeyLength(firstParam, c))
            {
                var cipherAlgorithmName = containingType.Is(KnownType.Org_BouncyCastle_Crypto_Generators_DHParametersGenerator) ? "DH" : "DSA";
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, invocation.GetLocation(), MinimalCommonKeyLength, cipherAlgorithmName, ""));
            }
        }

        private static void CheckBouncyCastleKeyGenerationParameters(ITypeSymbol containingType, ObjectCreationExpressionSyntax objectCreation, SyntaxNodeAnalysisContext c)
        {
            if (objectCreation.ArgumentList.Get(2) is { }  keyLengthParam
                && containingType.Is(KnownType.Org_BouncyCastle_Crypto_Parameters_RsaKeyGenerationParameters)
                && IsInvalidCommonKeyLength(keyLengthParam, c))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, objectCreation.GetLocation(), MinimalCommonKeyLength, "RSA", ""));
            }
        }

        private static bool IsInvalidCommonKeyLength(SyntaxNode keyLengthSyntax, SyntaxNodeAnalysisContext c) =>
            keyLengthSyntax.FindConstantValue(c.SemanticModel) is int keyLength && keyLength < MinimalCommonKeyLength;

        private static string GetMethodName(InvocationExpressionSyntax invocationExpression) =>
            invocationExpression.Expression.GetIdentifier()?.Identifier.ValueText;

        private static string GetPropertyName(ExpressionSyntax expression) =>
            expression.GetIdentifier() is { } nameSyntax ? nameSyntax.Identifier.ValueText : null;

        private static string CipherName(ITypeSymbol containingType) =>
            containingType.Is(KnownType.System_Security_Cryptography_DSA) || containingType.DerivesFrom(KnownType.System_Security_Cryptography_DSA) ? "DSA" : "RSA";
    }
}
