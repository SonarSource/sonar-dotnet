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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ParametersCorrectOrder : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2234";
        private const string MessageFormat = "Parameters to '{0}' have the same names but not the same order as the method arguments.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

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

        private static void AnalyzeArguments(SyntaxNodeAnalysisContext analysisContext, ArgumentListSyntax argumentList,
            Func<Location> getLocation)
        {
            if (argumentList == null)
            {
                return;
            }

            var methodParameterLookup = new CSharpMethodParameterLookup(argumentList, analysisContext.SemanticModel);
            var argumentParameterMappings = methodParameterLookup.GetAllArgumentParameterMappings()
                .ToDictionary(pair => pair.Argument, pair => pair.Parameter);

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
                    .Select(s => GetIdentifierLocation(s.GetSyntax()))
                    .WhereNotNull();

                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, getLocation(),
                    additionalLocations: secondaryLocations,
                    messageArgs: methodSymbol.Name));
            }

            bool IdentifierWithSameNameAndTypeExists(IParameterSymbol parameter) =>
                identifierArguments.Any(ia =>
                    ia.IdentifierName == parameter.Name &&
                    GetTypeSymbol(ia.ArgumentSyntax.Expression).DerivesOrImplements(parameter.Type));

            ITypeSymbol GetTypeSymbol(SyntaxNode syntaxNode) =>
                analysisContext.SemanticModel.GetTypeInfo(syntaxNode).ConvertedType;
        }

        private static List<IdentifierArgument> GetIdentifierArguments(ArgumentListSyntax argumentList)
        {
            return argumentList.Arguments
                .Select((argument, index) =>
                {
                    var identifier = argument.Expression as IdentifierNameSyntax;
                    var identifierName = identifier?.Identifier.Text;

                    IdentifierArgument identifierArgument;
                    if (argument.NameColon == null)
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
                            DeclaredName = argument.NameColon.Name.Identifier.Text,
                            ArgumentSyntax = argument
                        };
                    }
                    return identifierArgument;
                })
                .ToList();
        }

        private static Location GetIdentifierLocation(SyntaxNode syntax)
        {
            if (syntax is MethodDeclarationSyntax methodDeclaration)
            {
                return methodDeclaration.Identifier.GetLocation();
            }

            if (syntax is ConstructorDeclarationSyntax constructorDeclaration)
            {
                return constructorDeclaration.Identifier.GetLocation();
            }

            return null;
        }

        internal class IdentifierArgument
        {
            public string IdentifierName { get; set; }
            public ArgumentSyntax ArgumentSyntax { get; set; }
        }
        internal class PositionalIdentifierArgument : IdentifierArgument
        {
            public int Position { get; set; }
        }
        internal class NamedIdentifierArgument : IdentifierArgument
        {
            public string DeclaredName { get; set; }
        }
    }
}
