/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    internal class CSharpConstructorParameterLookup
    {
        private readonly SeparatedSyntaxList<ArgumentSyntax>? argumentList;
        public IMethodSymbol ConstructorSymbol { get; }

        protected CSharpConstructorParameterLookup(SeparatedSyntaxList<ArgumentSyntax>? argumentList, IMethodSymbol constructorSymbol)
        {
            this.argumentList = argumentList;
            ConstructorSymbol = constructorSymbol;
        }

        public CSharpConstructorParameterLookup(ObjectCreationExpressionSyntax creation, SemanticModel semanticModel)
            : this(creation.ArgumentList, semanticModel) { }

        public CSharpConstructorParameterLookup(ArgumentListSyntax argumentList, SemanticModel semanticModel)
            : this(argumentList?.Arguments, argumentList == null ? null : semanticModel.GetSymbolInfo(argumentList.Parent).Symbol as IMethodSymbol) { }

        public CSharpConstructorParameterLookup(ArgumentListSyntax argumentList, IMethodSymbol constructorSymbol)
            : this(argumentList?.Arguments, constructorSymbol) { }

        public bool TryGetSymbol(ArgumentSyntax argument, out IParameterSymbol parameter)
        {
            parameter = null;

            if (!argumentList.HasValue
                || !argumentList.Value.Contains(argument)
                || ConstructorSymbol == null
                || ConstructorSymbol.IsVararg)
            {
                return false;
            }

            if (argument.NameColon?.Name.Identifier is { } nameColonArgumentIdentifier)
            {
                parameter = ConstructorSymbol.Parameters.FirstOrDefault(symbol => symbol.Name == nameColonArgumentIdentifier.ValueText);
                return parameter != null;
            }

            var index = argumentList.Value.IndexOf(argument);
            if (index >= ConstructorSymbol.Parameters.Length)
            {
                var lastParameter = ConstructorSymbol.Parameters.Last();
                parameter = lastParameter.IsParams ? lastParameter : null;
                return parameter != null;
            }
            parameter = ConstructorSymbol.Parameters[index];
            return true;
        }
    }
}
