﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ParametersCorrectOrder : ParametersCorrectOrderBase<ArgumentSyntax>
    {
        private static readonly DiagnosticDescriptor rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodCall = (InvocationExpressionSyntax)c.Node;

                    var memberAccess = methodCall.Expression as MemberAccessExpressionSyntax;
                    Location getLocation() =>
                        memberAccess == null
                        ? methodCall.Expression.GetLocation()
                        : memberAccess.Name.GetLocation();

                    AnalyzeArguments(c, methodCall.ArgumentList, getLocation);
                }, SyntaxKind.InvocationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreationCall = (ObjectCreationExpressionSyntax)c.Node;

                    var qualifiedAccess = objectCreationCall.Type as QualifiedNameSyntax;
                    Location getLocation() =>
                        qualifiedAccess == null
                        ? objectCreationCall.Type.GetLocation()
                        : qualifiedAccess.Right.GetLocation();

                    AnalyzeArguments(c, objectCreationCall.ArgumentList, getLocation);
                }, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeArguments(SyntaxNodeAnalysisContext analysisContext, ArgumentListSyntax argumentList,
            Func<Location> getLocation)
        {
            if (argumentList == null)
            {
                return;
            }

            var methodParameterLookup = new CSharpMethodParameterLookup(argumentList, analysisContext.SemanticModel);

            ReportIncorrectlyOrderedParameters(analysisContext, methodParameterLookup, argumentList.Arguments, getLocation);
        }

        protected override TypeInfo GetArgumentTypeSymbolInfo(ArgumentSyntax argument, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(argument.Expression);

        protected override Location GetMethodDeclarationIdentifierLocation(SyntaxNode syntaxNode) =>
            (syntaxNode as BaseMethodDeclarationSyntax)?.FindIdentifierLocation();

        protected override SyntaxToken? GetArgumentIdentifier(ArgumentSyntax argument, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext) => GetExpressionSyntaxIdentifier(argument?.Expression, syntaxNodeAnalysisContext);

        protected override SyntaxToken? GetNameColonArgumentIdentifier(ArgumentSyntax argument) =>
            argument.NameColon?.Name.Identifier;

        private static SyntaxToken? GetExpressionSyntaxIdentifier(ExpressionSyntax expression, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext) =>
            expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier,
                MemberAccessExpressionSyntax memberAccess => GetValueAccessIdentifier(memberAccess, syntaxNodeAnalysisContext),
                CastExpressionSyntax cast => GetExpressionSyntaxIdentifier(cast.Expression, syntaxNodeAnalysisContext),
                ParenthesizedExpressionSyntax parentheses => GetExpressionSyntaxIdentifier(parentheses.Expression, syntaxNodeAnalysisContext),
                _ => null
            };

        private static SyntaxToken? GetValueAccessIdentifier(MemberAccessExpressionSyntax expression, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext) =>
            expression.Name.ToString() == "Value" && IsNullableValueAccess(expression, syntaxNodeAnalysisContext)
                ? GetExpressionSyntaxIdentifier(expression.Expression, syntaxNodeAnalysisContext)
                : expression.Name.Identifier;

        private static bool IsNullableValueAccess(MemberAccessExpressionSyntax expression, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var typeInfo = syntaxNodeAnalysisContext.SemanticModel.GetTypeInfo(expression.Expression);
            var type = typeInfo.ConvertedType;
            var nullableT = syntaxNodeAnalysisContext.Compilation.GetTypeByMetadataName(typeof(Nullable<>).FullName);
            return type.OriginalDefinition.DerivesOrImplements(nullableT);
        }
    }
}
