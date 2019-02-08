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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ParametersCorrectOrder : ParametersCorrectOrderBase<ArgumentSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

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

            var methodParameterLookup = new VisualBasicMethodParameterLookup(argumentList, analysisContext.SemanticModel);

            ReportIncorrectlyOrderedParameters(analysisContext, methodParameterLookup, argumentList.Arguments, getLocation);
        }

        protected override TypeInfo GetArgumentTypeSymbolInfo(ArgumentSyntax argument, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(argument.GetExpression());

        protected override Location GetMethodDeclarationIdentifierLocation(SyntaxNode syntaxNode) =>
            (syntaxNode as MethodBlockBaseSyntax)?.FindIdentifierLocation();

        protected override SyntaxToken? GetArgumentIdentifier(ArgumentSyntax argument) =>
            (argument.GetExpression() as IdentifierNameSyntax)?.Identifier;

        protected override SyntaxToken? GetNameColonArgumentIdentifier(ArgumentSyntax argument) =>
            (argument as SimpleArgumentSyntax)?.NameColonEquals?.Name.Identifier;
    }
}

