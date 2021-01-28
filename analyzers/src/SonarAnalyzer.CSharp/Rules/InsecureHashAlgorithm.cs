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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    [Obsolete("This rule is deprecated in favor of S4790")]
    public sealed class InsecureHashAlgorithm : DoNotCallInsecureSecurityAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ObjectCreationExpressionSyntax>
    {
        internal const string DiagnosticId = "S2070";
        private const string MessageFormat = "Use a stronger hashing/asymmetric algorithm.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override SyntaxKind ObjectCreation => SyntaxKind.ObjectCreationExpression;
        protected override SyntaxKind Invocation => SyntaxKind.InvocationExpression;
        protected override SyntaxKind StringLiteral => SyntaxKind.StringLiteralExpression;
        protected override ILanguageFacade LanguageFacade => CSharpFacade.Instance;

        protected override ISet<string> AlgorithmParameterlessFactoryMethods { get; } =
            new HashSet<string>
            {
                "System.Security.Cryptography.HMAC.Create"
            };

        protected override ISet<string> AlgorithmParameterizedFactoryMethods { get; } =
            new HashSet<string>
            {
                "System.Security.Cryptography.CryptoConfig.CreateFromName",
                "System.Security.Cryptography.HashAlgorithm.Create",
                "System.Security.Cryptography.KeyedHashAlgorithm.Create",
                "System.Security.Cryptography.AsymmetricAlgorithm.Create",
                "System.Security.Cryptography.HMAC.Create"
            };

        protected override ISet<string> FactoryParameterNames { get; } =
            new HashSet<string>
            {
                "SHA1",
                "MD5",
                "DSA",
                "HMACMD5",
                "HMACRIPEMD160",
                "RIPEMD160",
                "RIPEMD160Managed"
            };

        private protected override ImmutableArray<KnownType> AlgorithmTypes { get; } =
            ImmutableArray.Create(
                KnownType.System_Security_Cryptography_SHA1,
                KnownType.System_Security_Cryptography_MD5,
                KnownType.System_Security_Cryptography_DSA,
                KnownType.System_Security_Cryptography_RIPEMD160,
                KnownType.System_Security_Cryptography_HMACSHA1,
                KnownType.System_Security_Cryptography_HMACMD5,
                KnownType.System_Security_Cryptography_HMACRIPEMD160
            );

        protected override SyntaxNode InvocationExpression(InvocationExpressionSyntax invocation) =>
           invocation.Expression;

        protected override Location Location(ObjectCreationExpressionSyntax objectCreation) =>
            objectCreation.Type.GetLocation();

        protected override bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocationExpression)
        {
            var argumentList = invocationExpression.ArgumentList;

            if (argumentList == null || methodSymbol.ContainingType == null)
            {
                return false;
            }

            var methodFullName = $"{methodSymbol.ContainingType}.{methodSymbol.Name}";

            if (argumentList.Arguments.Count == 0)
            {
                return AlgorithmParameterlessFactoryMethods.Contains(methodFullName);
            }

            if (argumentList.Arguments.Count > 1 || !argumentList.Arguments.First().Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            if (!AlgorithmParameterizedFactoryMethods.Contains(methodFullName))
            {
                return false;
            }

            var literalExpressionSyntax = (LiteralExpressionSyntax)argumentList.Arguments.First().Expression;
            return FactoryParameterNames.Any(alg => alg.Equals(literalExpressionSyntax.Token.ValueText, StringComparison.Ordinal));
        }
    }
}
