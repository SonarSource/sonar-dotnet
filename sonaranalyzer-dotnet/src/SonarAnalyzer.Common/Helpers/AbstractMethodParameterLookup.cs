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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    // TODO: this should come from the Roslyn API (https://github.com/dotnet/roslyn/issues/9)
    internal abstract class AbstractMethodParameterLookup<TArgumentSyntax>
        where TArgumentSyntax : SyntaxNode
    {
        private readonly SeparatedSyntaxList<TArgumentSyntax> argumentList;

        public IMethodSymbol MethodSymbol { get; }

        protected AbstractMethodParameterLookup(SeparatedSyntaxList<TArgumentSyntax> argumentList, IMethodSymbol methodSymbol)
        {
            this.argumentList = argumentList;
            MethodSymbol = methodSymbol;
        }

        public bool TryGetParameterSymbol(TArgumentSyntax argument, out IParameterSymbol parameter)
        {
            parameter = null;

            if (!argumentList.Contains(argument) ||
                MethodSymbol == null ||
                MethodSymbol.IsVararg)
            {
                return false;
            }

            var nameColonArgumentIdenfitier = GetNameColonArgumentIdentifier(argument);
            if (nameColonArgumentIdenfitier != null)
            {
                parameter = MethodSymbol.Parameters
                    .FirstOrDefault(symbol => symbol.Name == nameColonArgumentIdenfitier.Value.ValueText);
                return parameter != null;
            }

            var argumentIndex = argumentList.IndexOf(argument);
            var parameterIndex = argumentIndex;

            if (parameterIndex >= MethodSymbol.Parameters.Length)
            {
                var lastParameter = MethodSymbol.Parameters.Last();
                parameter = lastParameter.IsParams ? lastParameter : null;
                return parameter != null;
            }
            parameter = MethodSymbol.Parameters[parameterIndex];
            return true;
        }

        protected abstract SyntaxToken? GetNameColonArgumentIdentifier(TArgumentSyntax argument);

        internal IEnumerable<SyntaxNodeSymbolSemanticModelTuple<TArgumentSyntax, IParameterSymbol>> GetAllArgumentParameterMappings()
        {
            foreach (var argument in argumentList)
            {
                if (TryGetParameterSymbol(argument, out var parameter))
                {
                    yield return new SyntaxNodeSymbolSemanticModelTuple<TArgumentSyntax, IParameterSymbol> { SyntaxNode = argument, Symbol = parameter };
                }
            }
        }
    }
}
