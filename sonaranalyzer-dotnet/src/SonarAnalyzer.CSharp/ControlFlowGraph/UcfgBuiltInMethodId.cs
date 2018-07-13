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


namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    internal static class UcfgBuiltInMethodId
    {
        /// <summary>
        /// The method ID that the Security Engine uses for assignments. It accepts one argument
        /// and returns one value. For example, `a = x` generates `a = __id(x)`.
        /// </summary>
        public static readonly string Identity = "__id";

        /// <summary>
        /// The method ID that the Security Engine uses for concatenations. It accepts two
        /// arguments and returns one value. For example, `x + y` generates `%0 = __concat(x, y)`
        /// </summary>
        public static readonly string Concatenation = "__concat";

        /// <summary>
        /// The method ID used by the UCFG Builder to represent a method that has no symbol.
        /// The instructions with this method ID are removed from UCFG.
        /// </summary>
        public static readonly string Unknown = "__unknown";

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
        public static readonly string Annotation = "__annotation";

        /// <summary>
        /// The method ID that the security engine uses to differentiate a method call from an attribute call.
        /// It accepts only 2 parameters, the first being the method ID of the attribute and the second being the parameter on
        /// which the attribute applies to.
        /// </summary>
        public static readonly string Annotate = "__annotate";

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
        public static readonly string EntryPoint = "__entrypoint";

        /// <summary>
        /// The method ID that the Security Engine uses for reading array elements. It accepts one
        /// argument - the array instance, and returns one value. For example, `var a = x[i]` generates `a = __arrayGet(x)`
        /// </summary>
        public static readonly string ArrayGet = "__arrayGet";

        /// <summary>
        /// The method ID that the Security Engine uses for writing array elements. It accepts one
        /// argument - the array instance, and returns one value. For example, `x[i] = a` generates `%0 = __arraySet(x, a)`
        /// </summary>
        public static readonly string ArraySet = "__arraySet";
    }
}
