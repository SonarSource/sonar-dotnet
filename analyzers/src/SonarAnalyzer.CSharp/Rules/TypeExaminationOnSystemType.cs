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
using System.Collections.Immutable;
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
    public sealed class TypeExaminationOnSystemType : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3443";
        private const string MessageFormat = "{0}";
        private const string MessageGetType = "Remove this use of 'GetType' on a 'System.Type'.";
        private const string MessageIsInstanceOfType = "Pass an argument that is not a 'System.Type' or consider using 'IsAssignableFrom'.";
        private const string MessageIsInstanceOfTypeWithGetType = "Consider removing the 'GetType' call, it's suspicious in an 'IsInstanceOfType' call.";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
               {
                   var invocation = (InvocationExpressionSyntax)c.Node;

                   if (invocation.Expression.ToStringContainsEitherOr(nameof(Type.IsInstanceOfType), nameof(Type.GetType))
                       && c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol)
                   {
                       CheckGetTypeCallOnType(invocation, methodSymbol, c);
                       CheckIsInstanceOfTypeCallWithTypeArgument(invocation, methodSymbol, c);
                   }
               },
               SyntaxKind.InvocationExpression);

        private static void CheckIsInstanceOfTypeCallWithTypeArgument(InvocationExpressionSyntax invocation, ISymbol methodSymbol, SyntaxNodeAnalysisContext context)
        {
            if (methodSymbol.Name != nameof(Type.IsInstanceOfType) || !methodSymbol.ContainingType.Is(KnownType.System_Type))
            {
                return;
            }

            var argument = invocation.ArgumentList.Arguments.First().Expression;

            var typeInfo = context.SemanticModel.GetTypeInfo(argument).Type;
            if (!typeInfo.Is(KnownType.System_Type))
            {
                return;
            }

            var invocationInArgument = argument as InvocationExpressionSyntax;
            var message = IsGetTypeCall(invocationInArgument, context.SemanticModel)
                ? MessageIsInstanceOfTypeWithGetType
                : MessageIsInstanceOfType;

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, argument.GetLocation(), message));
        }

        private static void CheckGetTypeCallOnType(InvocationExpressionSyntax invocation, ISymbol invokedMethod, SyntaxNodeAnalysisContext context)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberCall)
                || IsException(memberCall, context.SemanticModel)
                || !IsGetTypeCall(invokedMethod))
            {
                return;
            }

            var expressionType = context.SemanticModel.GetTypeInfo(memberCall.Expression).Type;
            if (!expressionType.Is(KnownType.System_Type))
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, memberCall.OperatorToken.CreateLocation(invocation), MessageGetType));
        }

        private static bool IsException(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
            memberAccess.Expression is TypeOfExpressionSyntax typeOf
            && typeOf.Type.IsKnownType(KnownType.System_Type, semanticModel);

        private static bool IsGetTypeCall(ISymbol invokedMethod) =>
            invokedMethod.Name == nameof(Type.GetType)
            && !invokedMethod.IsStatic
            && invokedMethod.ContainingType != null
            && IsObjectOrType(invokedMethod.ContainingType);

        private static bool IsObjectOrType(ITypeSymbol namedType) =>
            namedType.SpecialType == SpecialType.System_Object
            || namedType.Is(KnownType.System_Type);

        internal static bool IsGetTypeCall(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (invocation == null)
            {
                return false;
            }

            return semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol && IsGetTypeCall(methodSymbol);
        }
    }
}
