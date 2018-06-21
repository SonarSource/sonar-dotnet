/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Helpers
{
    internal static class DebugOnlyCodeHelper
    {
        // Looking for an exact case-sensitive match
        public static bool IsDebugString(string text) =>
            "DEBUG".Equals(text, System.StringComparison.Ordinal);

        #region DEBUG directive blocks

        public static bool IsInDebugBlock(SyntaxNode node) =>
            IfDirectiveHelper.GetActiveConditionalCompilationSections(node)
            .Any(IsDebugString);

        #endregion

        #region DEBUG conditional method attributes

        public static bool IsCallerInConditionalDebug(InvocationExpressionSyntax invocation,
            SemanticModel semanticModel)
        {
            var methodSymbol = FindContainingMethod(invocation, semanticModel);
            return IsConditionalDebugMethod(methodSymbol);
        }

        public static bool IsCalledMethodInConditionalDebugOnly(InvocationExpressionSyntax invocation,
            SemanticModel semanticModel)
        {
            var calledMethodSymbol = FindCalledMethod(invocation.Expression as IdentifierNameSyntax,
                semanticModel);
            return IsConditionalDebugMethod(calledMethodSymbol);
        }

        public static bool IsConditionalDebugMethod(IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
            {
                return false;
            }

            // Conditional attribute can be applied to a method or a class
            return HasDebugConditionalAttribute(methodSymbol) ||
                GetAllContainingTypes(methodSymbol).Any(t => HasDebugConditionalAttribute(t));
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

        private static IMethodSymbol FindCalledMethod(IdentifierNameSyntax identifierName, SemanticModel semanticModel)
        {
            if (identifierName == null)
            {
                return null;
            }
            return semanticModel.GetSymbolInfo(identifierName).Symbol as IMethodSymbol;
        }

        private static IEnumerable<ISymbol> GetAllContainingTypes(ISymbol symbol)
        {
            var current = symbol.ContainingType;
            while (current != null)
            {
                yield return current;
                current = current.ContainingType;
            }
        }

        private static bool HasDebugConditionalAttribute(ISymbol symbol)
        {
            if (symbol == null)
            {
                return false;
            }

            return symbol.GetAttributes()
                .Where(attribute => attribute.AttributeClass.Is(KnownType.System_Diagnostics_ConditionalAttribute))
                .Any(attribute => attribute.ConstructorArguments.Any(
                    constructorArg => constructorArg.Type.Is(KnownType.System_String)
                                      && IsDebugString((string)constructorArg.Value)));
        }

        #endregion

    }
}
