/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class InsecureHashAlgorithm : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2070";
        private const string MessageFormat = "Use a stronger encryption algorithm than {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        private static readonly IDictionary<KnownType, string> InsecureHashAlgorithmTypeNames = new Dictionary<KnownType, string>
        {
            { KnownType.System_Security_Cryptography_SHA1, "SHA1"},
            { KnownType.System_Security_Cryptography_MD5, "MD5"}
        }.ToImmutableDictionary();

        private static readonly ISet<string> MethodNamesToReachHashAlgorithm = ImmutableHashSet.Create(
            "System.Security.Cryptography.CryptoConfig.CreateFromName",
            "System.Security.Cryptography.HashAlgorithm.Create");

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckObjectCreation(c),
                SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckInvocation(c),
                SyntaxKind.InvocationExpression);
        }

        private static void CheckInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol;
            if (methodSymbol?.ContainingType == null)
            {
                return;
            }

            var methodName = $"{methodSymbol.ContainingType}.{methodSymbol.Name}";
            string algorithmName;
            if (MethodNamesToReachHashAlgorithm.Contains(methodName) &&
                TryGetAlgorithmName(invocation.ArgumentList, out algorithmName))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, invocation.GetLocation(), algorithmName));
            }
        }

        private static void CheckObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

            var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation);

            if (typeInfo.ConvertedType == null || typeInfo.ConvertedType is IErrorTypeSymbol)
            {
                return;
            }

            ITypeSymbol insecureArgorithmType;
            if (!TryGetInsecureAlgorithmBase(typeInfo.ConvertedType, out insecureArgorithmType))
            {
                return;
            }

            var insecureHashAlgorithmType = InsecureHashAlgorithmTypeNames.FirstOrDefault(t => insecureArgorithmType.Is(t.Key));
            if (!insecureHashAlgorithmType.Equals(default(KeyValuePair<KnownType, string>)))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, objectCreation.Type.GetLocation(), insecureHashAlgorithmType.Value));
            }
        }

        private static bool TryGetAlgorithmName(ArgumentListSyntax argumentList, out string algorithmName)
        {
            if (argumentList == null ||
                argumentList.Arguments.Count == 0 ||
                !argumentList.Arguments.First().Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                algorithmName = null;
                return false;
            }

            var algorithmNameCandidate = ((LiteralExpressionSyntax)argumentList.Arguments.First().Expression).Token.ValueText;
            algorithmName = InsecureHashAlgorithmTypeNames.Values
                .FirstOrDefault(alg =>
                    algorithmNameCandidate.StartsWith(alg, System.StringComparison.Ordinal));

            return algorithmName != null;
        }

        private static bool TryGetInsecureAlgorithmBase(ITypeSymbol type, out ITypeSymbol insecureAlgorithmBase)
        {
            insecureAlgorithmBase = null;
            var currentType = type;

            while (currentType != null &&
                !currentType.Is(KnownType.System_Security_Cryptography_HashAlgorithm))
            {
                insecureAlgorithmBase = currentType;
                currentType = currentType.BaseType;
            }

            return insecureAlgorithmBase != null;
        }
    }
}
