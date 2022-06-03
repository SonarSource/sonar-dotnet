/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Extensions
{
    internal static class SyntaxNodeAnalysisContextExtensions
    {
        /// <summary>
        /// Roslyn invokes the analzyer twice for positional records. The first invocation is for the class declaration and the second for the ctor represented by the positional parameter list.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> for the invocation on the class declaration and <see langword="false"/> for the ctor invocation.
        /// </returns>
        /// <example>
        /// record R(int i);
        /// </example>
        /// <seealso href="https://github.com/dotnet/roslyn/issues/50989"/>
        internal static bool IsRedundantPositionalRecordContext(this SyntaxNodeAnalysisContext context) =>
            context.ContainingSymbol.Kind == SymbolKind.Method;

        public static bool IsAzureFunction(this SyntaxNodeAnalysisContext context) =>
            context.AzureFunctionMethod() is not null;

        public static IMethodSymbol AzureFunctionMethod(this SyntaxNodeAnalysisContext context) =>
            context.ContainingSymbol is IMethodSymbol method && method.HasAttribute(KnownType.Microsoft_Azure_WebJobs_FunctionNameAttribute)
                ? method
                : null;
    }
}
