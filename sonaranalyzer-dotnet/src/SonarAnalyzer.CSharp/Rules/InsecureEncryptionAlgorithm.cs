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
    public class InsecureEncryptionAlgorithm : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2278";
        private const string MessageFormat = "Use the recommended AES (Advanced Encryption Standard) instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        private const string BaseEncryptionAlgorithmCreate = "System.Security.Cryptography.SymmetricAlgorithm.Create";

        private static readonly ISet<string> AlgorithmNames = ImmutableHashSet.Create(
            "DES",
            "3DES",
            "TripleDES");

        private static readonly ISet<string> MethodNamesToReachEncryptionAlgorithm = ImmutableHashSet.Create(
            "System.Security.Cryptography.DES.Create",
            "System.Security.Cryptography.TripleDES.Create");

        private static readonly ISet<KnownType> BaseClassNamesForEncryptionAlgorithm = ImmutableHashSet.Create(
            KnownType.System_Security_Cryptography_DES,
            KnownType.System_Security_Cryptography_TripleDES);

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
            if (MethodNamesToReachEncryptionAlgorithm.Contains(methodName) ||
                IsBaseEncryptionCreateCalled(methodName, invocation.ArgumentList))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, invocation.GetLocation()));
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

            var insecureArgorithmType = GetInsecureAlgorithmBase(typeInfo.ConvertedType);

            if (insecureArgorithmType != null &&
                insecureArgorithmType.IsAny(BaseClassNamesForEncryptionAlgorithm))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, objectCreation.Type.GetLocation()));
            }
        }

        private static bool IsBaseEncryptionCreateCalled(string methodName, ArgumentListSyntax argumentList)
        {
            if (methodName != BaseEncryptionAlgorithmCreate)
            {
                return false;
            }

            if (argumentList.Arguments.Count == 0 ||
                !argumentList.Arguments.First().Expression.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            var algorithmNameCandidate = ((LiteralExpressionSyntax)argumentList.Arguments.First().Expression).Token.ValueText;
            var algorithmName = AlgorithmNames
                .FirstOrDefault(alg =>
                    algorithmNameCandidate.StartsWith(alg, System.StringComparison.Ordinal));

            return algorithmName != null;
        }

        private static ITypeSymbol GetInsecureAlgorithmBase(ITypeSymbol type)
        {
            var currentType = type;

            while (currentType != null &&
                !currentType.IsAny(BaseClassNamesForEncryptionAlgorithm))
            {
                currentType = currentType.BaseType;
            }

            return currentType;
        }
    }
}
