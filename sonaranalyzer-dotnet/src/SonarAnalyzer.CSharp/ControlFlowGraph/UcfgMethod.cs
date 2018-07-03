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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public struct UcfgMethod
    {
        /// <summary>
        /// The method ID that the Security Engine uses for assignments. It accepts one argument
        /// and returns one value. For example, `a = x` generates `a = __id(x)`.
        /// </summary>
        public static readonly UcfgMethod Assignment = Create("__id");

        /// <summary>
        /// The method ID that the Security Engine uses for concatenations. It accepts two
        /// arguments and returns one value. For example, `x + y` generates `%0 = __concat(x, y)`
        /// </summary>
        public static readonly UcfgMethod Concatenation = Create("__concat");

        /// <summary>
        /// The method ID used by the UCFG Builder to represent a method that has no symbol.
        /// The instructions with this method ID are removed from UCFG.
        /// </summary>
        public static readonly UcfgMethod Unknown = Create("__unknown");

        /// <summary>
        /// The method ID that the security engine uses for attributes/annotations. It accepts
        /// one argument and returns one value. The argument should be the result of the invocation
        /// of the attribute constructor. The returned value should be assigned to a variable
        /// with the same name as the parameter this attribute is applied on.
        /// </summary>
        /// <example>
        /// public void Foo([Bar]string s) { ... }
        ///
        /// Instructions:
        /// %0 = Bar()
        /// s = __annotation(%0)
        /// ...
        /// </example>
        public static readonly UcfgMethod Annotation = Create("__annotation");

        /// <summary>
        /// The method ID that the security engine uses for known tainted entrypoints. All method
        /// arguments should be passed as parameters of this instruction.
        /// </summary>
        /// <example>
        /// public void Foo(string s, string p) { ... }
        ///
        /// Instructions:
        /// %0 = __entrypoint(s, p)
        /// </example>
        public static readonly UcfgMethod EntryPoint = Create("__entrypoint");

        private readonly string id;

        private UcfgMethod(string id)
        {
            this.id = id;
        }

        public override string ToString() =>
            id;

        // TODO: temporary, to reduce merge conflicts in tests
        public static implicit operator string(UcfgMethod method) =>
            method.ToString();

        public static UcfgMethod Create(IMethodSymbol methodSymbol)
        {
            switch (methodSymbol?.MethodKind)
            {
                case MethodKind.ExplicitInterfaceImplementation:
                    return Create(methodSymbol.ConstructedFrom.ToDisplayString());

                case MethodKind.ReducedExtension:
                    return Create(methodSymbol.ReducedFrom.ToDisplayString());

                default:
                    return Create(methodSymbol?.OriginalDefinition?.ToDisplayString());
            }
        }

        public static UcfgMethod Create(INamedTypeSymbol typeSymbol)
        {
            return Create(typeSymbol.ConstructedFrom.ToDisplayString());
        }

        private static UcfgMethod Create(string id) =>
            id == null ? Unknown : new UcfgMethod(id);
    }
}
