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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ParametersCorrectOrderBase<TArgumentSyntax> : SonarDiagnosticAnalyzer
        where TArgumentSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2234";
        protected const string MessageFormat = "Parameters to '{0}' have the same names but not the same order as the method arguments.";

        protected abstract TypeInfo GetArgumentTypeSymbolInfo(TArgumentSyntax argument, SemanticModel semanticModel);
        protected abstract Location GetMethodDeclarationIdentifierLocation(SyntaxNode syntaxNode);
        protected abstract SyntaxToken? GetArgumentIdentifier(TArgumentSyntax argument);
        protected abstract SyntaxToken? GetNameColonArgumentIdentifier(TArgumentSyntax argument);

        internal void ReportIncorrectlyOrderedParameters(SyntaxNodeAnalysisContext analysisContext,
            AbstractMethodParameterLookup<TArgumentSyntax> methodParameterLookup, SeparatedSyntaxList<TArgumentSyntax> argumentList,
            Func<Location> getLocationToReport)
        {
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
            for (var i = 0; i < identifierArguments.Count; i++)
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
                break;
            }

            if (methodCallHasIssue)
            {
                var secondaryLocations = methodSymbol.DeclaringSyntaxReferences
                    .Select(s => GetMethodDeclarationIdentifierLocation(s.GetSyntax()))
                    .WhereNotNull();

                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], getLocationToReport(),
                    additionalLocations: secondaryLocations,
                    messageArgs: methodSymbol.Name));
            }

            bool IdentifierWithSameNameAndTypeExists(IParameterSymbol parameter) =>
                identifierArguments.Any(ia =>
                    ia.IdentifierName == parameter.Name &&
                    GetArgumentTypeSymbolInfo(ia.ArgumentSyntax, analysisContext.SemanticModel).ConvertedType.DerivesOrImplements(parameter.Type));
        }

        private List<IdentifierArgument> GetIdentifierArguments(SeparatedSyntaxList<TArgumentSyntax> argumentList)
        {
            return argumentList.Select((argument, index) =>
                {
                    var identifierName = GetArgumentIdentifier(argument)?.Text;
                    var nameColonIdentifier = GetNameColonArgumentIdentifier(argument);

                    if (nameColonIdentifier == null)
                    {
                        return new PositionalIdentifierArgument
                        {
                            IdentifierName = identifierName,
                            Position = index,
                            ArgumentSyntax = argument
                        };
                    }

                    return (IdentifierArgument)new NamedIdentifierArgument
                    {
                        IdentifierName = identifierName,
                        DeclaredName = nameColonIdentifier.Value.Text,
                        ArgumentSyntax = argument
                    };
                })
                .ToList();
        }

        private class IdentifierArgument
        {
            public string IdentifierName { get; set; }
            public TArgumentSyntax ArgumentSyntax { get; set; }
        }

        private class PositionalIdentifierArgument : IdentifierArgument
        {
            public int Position { get; set; }
        }

        private class NamedIdentifierArgument : IdentifierArgument
        {
            public string DeclaredName { get; set; }
        }
    }
}

