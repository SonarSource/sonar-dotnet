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

            var argumentIdentifiers = argumentList
                .Select(argument => ConvertToArgumentIdentifier(argument))
                .ToList();
            var identifierNames = argumentIdentifiers
                .Select(p => p.IdentifierName)
                .ToList();

            if (parameterNames.Intersect(identifierNames).Any() &&
                HasIncorrectlyOrderedParameters(argumentIdentifiers, argumentParameterMappings, parameterNames, identifierNames,
                    analysisContext.SemanticModel))
            {
                // for VB the symbol does not contain the method syntax reference
                var secondaryLocations = methodSymbol.DeclaringSyntaxReferences
                    .Select(s => GetMethodDeclarationIdentifierLocation(s.GetSyntax()))
                    .WhereNotNull();

                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], getLocationToReport(),
                    additionalLocations: secondaryLocations,
                    messageArgs: methodSymbol.Name));
            }
        }

        private bool HasIncorrectlyOrderedParameters(List<ArgumentIdentifier> argumentIdentifiers,
            Dictionary<TArgumentSyntax, IParameterSymbol> argumentParameterMappings, List<string> parameterNames,
            List<string> identifierNames, SemanticModel semanticModel)
        {
            for (var i = 0; i < argumentIdentifiers.Count; i++)
            {
                var argumentIdentifier = argumentIdentifiers[i];
                var identifierName = argumentIdentifier.IdentifierName;
                var parameter = argumentParameterMappings[argumentIdentifier.ArgumentSyntax];
                var parameterName = parameter.Name;

                if (string.IsNullOrEmpty(identifierName) ||
                    !parameterNames.Contains(identifierName) ||
                    !IdentifierWithSameNameAndTypeExists(parameter))
                {
                    continue;
                }

                if (argumentIdentifier is PositionalArgumentIdentifier positional &&
                    (parameter.IsParams || identifierName == parameterName))
                {
                    continue;
                }

                if (argumentIdentifier is NamedArgumentIdentifier named &&
                    (!identifierNames.Contains(named.DeclaredName) || named.DeclaredName == named.IdentifierName))
                {
                    continue;
                }

                return true;
            }

            return false;

            bool IdentifierWithSameNameAndTypeExists(IParameterSymbol parameter) =>
                argumentIdentifiers.Any(ia =>
                    ia.IdentifierName == parameter.Name &&
                    GetArgumentTypeSymbolInfo(ia.ArgumentSyntax, semanticModel).ConvertedType.DerivesOrImplements(parameter.Type));
        }

        private ArgumentIdentifier ConvertToArgumentIdentifier(TArgumentSyntax argument)
        {
            var identifierName = GetArgumentIdentifier(argument)?.Text;
            var nameColonIdentifier = GetNameColonArgumentIdentifier(argument);

            if (nameColonIdentifier == null)
            {
                return new PositionalArgumentIdentifier(identifierName, argument);
            }

            return new NamedArgumentIdentifier(identifierName, argument, nameColonIdentifier.Value.Text);
        }

        private class ArgumentIdentifier
        {
            protected ArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax)
            {
                IdentifierName = identifierName;
                ArgumentSyntax = argumentSyntax;
            }

            public string IdentifierName { get; }
            public TArgumentSyntax ArgumentSyntax { get; }
        }

        private class PositionalArgumentIdentifier : ArgumentIdentifier
        {
            public PositionalArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax)
                : base(identifierName, argumentSyntax)
            {
            }
        }

        private class NamedArgumentIdentifier : ArgumentIdentifier
        {
            public NamedArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax, string declaredName)
                : base(identifierName, argumentSyntax)
            {
                DeclaredName = declaredName;
            }

            public string DeclaredName { get; }
        }
    }
}

