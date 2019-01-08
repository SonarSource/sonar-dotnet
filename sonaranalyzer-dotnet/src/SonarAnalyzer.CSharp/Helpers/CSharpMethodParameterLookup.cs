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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    // todo: this should come from the Roslyn API (https://github.com/dotnet/roslyn/issues/9)
    internal class CSharpMethodParameterLookup
    {
        private readonly ArgumentListSyntax argumentList;
        public IMethodSymbol MethodSymbol { get; }

        public CSharpMethodParameterLookup(InvocationExpressionSyntax invocation, SemanticModel semanticModel) :
            this(invocation.ArgumentList, semanticModel)
        {
        }

        public CSharpMethodParameterLookup(ArgumentListSyntax argumentList, SemanticModel semanticModel)
        {
            this.argumentList = argumentList;
            MethodSymbol = semanticModel.GetSymbolInfo(argumentList.Parent).Symbol as IMethodSymbol;
        }

        public static bool TryGetParameterSymbol(ArgumentSyntax argument, ArgumentListSyntax argumentList,
            IMethodSymbol method, out IParameterSymbol parameter)
        {
            parameter = null;
            if (!argumentList.Arguments.Contains(argument) ||
                method == null ||
                method.IsVararg)
            {
                return false;
            }

            if (argument.NameColon != null)
            {
                parameter = method.Parameters
                    .FirstOrDefault(symbol => symbol.Name == argument.NameColon.Name.Identifier.ValueText);
                return parameter != null;
            }

            var argumentIndex = argumentList.Arguments.IndexOf(argument);
            var parameterIndex = argumentIndex;

            if (parameterIndex >= method.Parameters.Length)
            {
                var lastParameter = method.Parameters.Last();
                parameter = lastParameter.IsParams ? lastParameter : null;
                return parameter != null;
            }
            parameter = method.Parameters[parameterIndex];
            return true;
        }

        public bool TryGetParameterSymbol(ArgumentSyntax argument, out IParameterSymbol parameter)
        {
            return TryGetParameterSymbol(argument, this.argumentList, MethodSymbol, out parameter);
        }

        internal IEnumerable<ArgumentParameterMapping> GetAllArgumentParameterMappings()
        {
            foreach (var argument in this.argumentList.Arguments)
            {
                if (TryGetParameterSymbol(argument, out var parameter))
                {
                    yield return new ArgumentParameterMapping(argument, parameter);
                }
            }
        }

        public class ArgumentParameterMapping
        {
            public ArgumentSyntax Argument { get; set; }
            public IParameterSymbol Parameter { get; set; }

            public ArgumentParameterMapping(ArgumentSyntax argument, IParameterSymbol parameter)
            {
                Argument = argument;
                Parameter = parameter;
            }
        }
    }
}
