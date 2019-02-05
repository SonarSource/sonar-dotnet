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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        private static void AnalyzeArguments(SyntaxNodeAnalysisContext analysisContext, ArgumentListSyntax argumentList,
            Func<Location> getLocation)
        {
            if (argumentList == null)
            {
                return;
            }

            var methodParameterLookup = new VisualBasicMethodParameterLookup(argumentList, analysisContext.SemanticModel);
            var argumentParameterMappings = methodParameterLookup.GetAllArgumentParameterMappings()
                .ToDictionary(pair => pair.SyntaxNode, pair => pair.Symbol);

            var methodSymbol = methodParameterLookup.MethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            var parameterNames = argumentParameterMappings.Values
                .Select(symbol => symbol.Name)
                .Distinct()
                .ToList();

            var identifierArguments = GetIdentifierArguments(argumentList);
            var identifierNames = identifierArguments
                .Select(p => p.IdentifierName)
                .ToList();

            if (!parameterNames.Intersect(identifierNames).Any())
            {
                return;
            }

            var methodCallHasIssue = false;

            for (var i = 0; !methodCallHasIssue && i < identifierArguments.Count; i++)
            {
                var identifierArgument = identifierArguments[i];
                var identifierName = identifierArgument.IdentifierName;
                var parameter = argumentParameterMappings[identifierArgument.ArgumentSyntax];
                var parameterName = parameter.Name;

                if (string.IsNullOrEmpty(identifierName) ||
                    !parameterNames.Contains(identifierName) ||
                    !IdentifierWithSameNameAndTypeExists(parameter))
                {
                    continue;
                }

                if (identifierArgument is PositionalIdentifierArgument positional &&
                    (parameter.IsParams || identifierName == parameterName))
                {
                    continue;
                }

                if (identifierArgument is NamedIdentifierArgument named &&
                    (!identifierNames.Contains(named.DeclaredName) || named.DeclaredName == named.IdentifierName))
                {
                    continue;
                }

                methodCallHasIssue = true;
            }

            if (methodCallHasIssue)
            {
                var secondaryLocations = methodSymbol.DeclaringSyntaxReferences
                    .Select(s => s.GetSyntax() as MethodBlockBaseSyntax)
                    .Select(s => s.FindIdentifierLocation())
                    .WhereNotNull();

                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, getLocation(),
                    additionalLocations: secondaryLocations,
                    messageArgs: methodSymbol.Name));
            }

            bool IdentifierWithSameNameAndTypeExists(IParameterSymbol parameter) =>
                identifierArguments.Any(ia =>
                    ia.IdentifierName == parameter.Name &&
                    GetTypeSymbol(ia.ArgumentSyntax.GetExpression()).DerivesOrImplements(parameter.Type));

            ITypeSymbol GetTypeSymbol(SyntaxNode syntaxNode) =>
                analysisContext.SemanticModel.GetTypeInfo(syntaxNode).ConvertedType;
        }

        private static List<IdentifierArgument> GetIdentifierArguments(ArgumentListSyntax argumentList)
        {
            return argumentList.Arguments
                .Select((argument, index) =>
                {
                    var identifier = argument.GetExpression() as IdentifierNameSyntax;
                    var identifierName = identifier?.Identifier.Text;

                    var simpleArgument = argument as SimpleArgumentSyntax;

                    IdentifierArgument identifierArgument;
                    if (simpleArgument?.NameColonEquals == null)
                    {
                        identifierArgument = new PositionalIdentifierArgument
                        {
                            IdentifierName = identifierName,
                            Position = index,
                            ArgumentSyntax = argument
                        };
                    }
                    else
                    {
                        identifierArgument = new NamedIdentifierArgument
                        {
                            IdentifierName = identifierName,
                            DeclaredName = simpleArgument.NameColonEquals.Name.Identifier.Text,
                            ArgumentSyntax = argument
                        };
                    }
                    return identifierArgument;
                })
                .ToList();
        }
    }
}

