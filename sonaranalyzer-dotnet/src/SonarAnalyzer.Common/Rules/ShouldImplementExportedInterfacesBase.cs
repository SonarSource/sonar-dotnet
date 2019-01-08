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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class ShouldImplementExportedInterfacesBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4159";
        protected const string MessageFormat = "{0} '{1}' on '{2}' or remove this export attribute.";
        protected const string ActionForInterface = "Implement";
        protected const string ActionForClass = "Derive from";

        internal static readonly ImmutableArray<KnownType> ExportAttributes =
            ImmutableArray.Create(
                KnownType.System_ComponentModel_Composition_ExportAttribute,
                KnownType.System_ComponentModel_Composition_InheritedExportAttribute
            );
    }

    public abstract class ShouldImplementExportedInterfacesBase<TArgumentSyntax, TExpressionSyntax, TClassSyntax>
        : ShouldImplementExportedInterfacesBase
        where TArgumentSyntax : SyntaxNode
        where TExpressionSyntax : SyntaxNode
        where TClassSyntax : SyntaxNode
    {
        protected static bool IsOfExportType(ITypeSymbol type, INamedTypeSymbol exportedType) =>
            type.GetSelfAndBaseTypes()
                .Union(type.AllInterfaces)
                .Any(currentType => currentType.Equals(exportedType));

        protected INamedTypeSymbol GetExportedTypeSymbol(SeparatedSyntaxList<TArgumentSyntax>? attributeArguments,
            SemanticModel semanticModel)
        {
            if (!attributeArguments.HasValue)
            {
                return null;
            }

            var arguments = attributeArguments.Value;
            if (arguments.Count == 0 ||
                arguments.Count > 2)
            {
                return null;
            }

            var argumentSyntax = GetArgumentFromNamedArgument(arguments) ??
                GetArgumentFromSingleArgumentAttribute(arguments) ??
                GetArgumentFromDoubleArgumentAttribute(arguments, semanticModel);
            var typeOfOrGetTypeExpression = GetExpression(argumentSyntax);

            var exportedTypeSyntax = GetTypeOfOrGetTypeExpression(typeOfOrGetTypeExpression);
            if (exportedTypeSyntax == null)
            {
                return null;
            }

            return semanticModel.GetSymbolInfo(exportedTypeSyntax).Symbol as INamedTypeSymbol;
        }

        private TArgumentSyntax GetArgumentFromNamedArgument(IEnumerable<TArgumentSyntax> arguments) =>
            // it's ok to use case insensitive even for C# because if that casing is incorrect the code won't compile
            arguments.FirstOrDefault(x => "contractType".Equals(GetIdentifier(x), StringComparison.OrdinalIgnoreCase));

        private TArgumentSyntax GetArgumentFromSingleArgumentAttribute(SeparatedSyntaxList<TArgumentSyntax> arguments)
        {
            if (arguments.Count != 1)
            {
                return null;
            }

            // Only one argument, should be typeof expression
            return arguments[0];
        }

        private TArgumentSyntax GetArgumentFromDoubleArgumentAttribute(SeparatedSyntaxList<TArgumentSyntax> arguments,
            SemanticModel semanticModel)
        {
            if (arguments.Count != 2)
            {
                return null;
            }

            var firstArgument = GetExpression(arguments[0]);
            if (firstArgument != null &&
                semanticModel.GetConstantValue(firstArgument).Value is string)
            {
                // Two arguments, second should be typeof expression
                return arguments[1];
            }

            return null;
        }

        protected ITypeSymbol GetAttributeTargetSymbol(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            // Parent is AttributeListSyntax, we handle only class attributes
            if (!(syntaxNode.Parent?.Parent is TClassSyntax attributeTarget))
            {
                return null;
            }

            return semanticModel.GetDeclaredSymbol(attributeTarget) as ITypeSymbol;
        }

        protected abstract string GetIdentifier(TArgumentSyntax argumentSyntax);
        protected abstract TExpressionSyntax GetExpression(TArgumentSyntax argumentSyntax);
        // Retrieve the expression inside of the typeof()/GetType() (e.g. typeof(Foo) => Foo)
        protected abstract SyntaxNode GetTypeOfOrGetTypeExpression(TExpressionSyntax expressionSyntax);
    }
}
