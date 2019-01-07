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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    internal static class CSharpDebugOnlyCodeHelper
    {
        // Looking for an exact case-sensitive match
        public static bool IsDebugString(string text) =>
            "DEBUG".Equals(text, System.StringComparison.Ordinal);

        #region DEBUG directive blocks

        public static bool IsInDebugBlock(this SyntaxNode node) =>
            CSharpIfDirectiveHelper.GetActiveConditionalCompilationSections(node)
            .Any(IsDebugString);

        #endregion

        #region DEBUG conditional method attributes

        public static bool IsCallerInConditionalDebug(SyntaxNode node, SemanticModel semanticModel)
        {
            var methodSymbol = FindContainingMethod(node, semanticModel);
            return IsConditionalDebugMethod(methodSymbol);
        }

        public static bool IsConditionalDebugMethod(this IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
            {
                return false;
            }

            // Conditional attribute can be applied to a class, but it does nothing unless
            // the class is an attribute class. So we only need to worry about whether the
            // conditional attribute is on the method.
            return methodSymbol.GetAttributes(KnownType.System_Diagnostics_ConditionalAttribute)
                .Any(attribute => attribute.ConstructorArguments.Any(
                    constructorArg => constructorArg.Type.Is(KnownType.System_String)
                          && IsDebugString((string)constructorArg.Value)));

        }

        private static IMethodSymbol FindContainingMethod(SyntaxNode node, SemanticModel semanticModel)
        {
            var methodDecl = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDecl != null)
            {
                return semanticModel.GetDeclaredSymbol(methodDecl);
            }
            return null;
        }

        #endregion

    }
}
