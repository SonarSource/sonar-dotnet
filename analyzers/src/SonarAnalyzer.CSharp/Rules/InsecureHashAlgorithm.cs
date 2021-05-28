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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    [Obsolete("This rule is deprecated in favor of S4790")]
    public sealed class InsecureHashAlgorithm : DoNotCallInsecureSecurityAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ArgumentListSyntax, ArgumentSyntax>
    {
        private const string DiagnosticId = "S2070";
        private const string MessageFormat = "Use a stronger hashing/asymmetric algorithm.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
        protected override ILanguageFacade<SyntaxKind> Language { get; } = CSharpFacade.Instance;

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

        protected override Location Location(SyntaxNode objectCreation) =>
            objectCreation is ObjectCreationExpressionSyntax objectCreationExpression
                ? objectCreationExpression.Type.GetLocation()
                : objectCreation.GetLocation();

        protected override ArgumentListSyntax ArgumentList(InvocationExpressionSyntax invocationExpression) =>
            invocationExpression.ArgumentList;

        protected override SeparatedSyntaxList<ArgumentSyntax> Arguments(ArgumentListSyntax argumentList) =>
            argumentList.Arguments;

        protected override bool IsStringLiteralArgument(ArgumentSyntax argument) =>
            argument.Expression.IsKind(SyntaxKind.StringLiteralExpression);

        protected override string StringLiteralValue(ArgumentSyntax argument) =>
            ((LiteralExpressionSyntax)argument.Expression).Token.ValueText;
    }
}
