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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public interface IMethodParameterLookup
    {
        bool TryGetSyntax(IParameterSymbol parameter, out ImmutableArray<SyntaxNode> expressions);
        bool TryGetSyntax(string parameterName, out ImmutableArray<SyntaxNode> expressions);
        bool TryGetNonParamsSyntax(IParameterSymbol parameter, out SyntaxNode expression);
    }

    // This should come from the Roslyn API (https://github.com/dotnet/roslyn/issues/9)
    internal abstract class MethodParameterLookupBase<TArgumentSyntax> : IMethodParameterLookup
        where TArgumentSyntax : SyntaxNode
    {
        private readonly SeparatedSyntaxList<TArgumentSyntax>? argumentList;

        protected abstract SyntaxToken? GetNameColonArgumentIdentifier(TArgumentSyntax argument);
        protected abstract SyntaxNode Expression(TArgumentSyntax argument);

        public IMethodSymbol MethodSymbol { get; }

        protected MethodParameterLookupBase(SeparatedSyntaxList<TArgumentSyntax>? argumentList, IMethodSymbol methodSymbol)
        {
            this.argumentList = argumentList;
            MethodSymbol = methodSymbol;
        }

        public bool TryGetSymbol(TArgumentSyntax argument, out IParameterSymbol parameter)
        {
            parameter = null;

            if (!argumentList.HasValue
                || !argumentList.Value.Contains(argument)
                || MethodSymbol == null
                || MethodSymbol.IsVararg)
            {
                return false;
            }

            if (GetNameColonArgumentIdentifier(argument) is { } nameColonArgumentIdentifier)
            {
                parameter = MethodSymbol.Parameters.FirstOrDefault(symbol => symbol.Name == nameColonArgumentIdentifier.ValueText);
                return parameter != null;
            }

            var index = argumentList.Value.IndexOf(argument);
            if (index >= MethodSymbol.Parameters.Length)
            {
                var lastParameter = MethodSymbol.Parameters.Last();
                parameter = lastParameter.IsParams ? lastParameter : null;
                return parameter != null;
            }
            parameter = MethodSymbol.Parameters[index];
            return true;
        }

        /// <summary>
        /// Method returns array of argument syntaxes that represents all syntaxes passed to the parameter.
        ///
        /// There could be multiple syntaxes for ParamArray/params.
        /// There could be zero or one result for optional parameters.
        /// There will be single result for normal parameters.
        /// </summary>
        public bool TryGetSyntax(IParameterSymbol parameter, out ImmutableArray<SyntaxNode> expressions) =>
            TryGetSyntax(parameter.Name, out expressions);

        /// <summary>
        /// Method returns array of argument syntaxes that represents all syntaxes passed to the parameter.
        ///
        /// There could be multiple syntaxes for ParamArray/params.
        /// There could be zero or one result for optional parameters.
        /// There will be single result for normal parameters.
        public bool TryGetSyntax(string parameterName, out ImmutableArray<SyntaxNode> expressions)
        {
            expressions = GetAllArgumentParameterMappings().Where(x => x.Symbol.Name == parameterName).Select(x => Expression(x.Node)).ToImmutableArray();
            return !expressions.IsEmpty;
        }

        /// <summary>
        /// Method returns zero or one argument syntax that represents syntax passed to the parameter.
        ///
        /// Caller must ensure that given parameter is not ParamArray/params.
        /// </summary>
        public bool TryGetNonParamsSyntax(IParameterSymbol parameter, out SyntaxNode expression)
        {
            if (parameter.IsParams)
            {
                throw new System.InvalidOperationException("Cannot call TryGetNonParamsSyntax on ParamArray/params parameters.");
            }
            if (TryGetSyntax(parameter, out var all))
            {
                expression = all.Single();
                return true;
            }
            expression = null;
            return false;
        }

        internal IEnumerable<NodeAndSymbol<TArgumentSyntax, IParameterSymbol>> GetAllArgumentParameterMappings()
        {
            if (argumentList.HasValue)
            {
                foreach (var argument in argumentList)
                {
                    if (TryGetSymbol(argument, out var parameter))
                    {
                        yield return new NodeAndSymbol<TArgumentSyntax, IParameterSymbol>(argument, parameter);
                    }
                }
            }
        }
    }
}
