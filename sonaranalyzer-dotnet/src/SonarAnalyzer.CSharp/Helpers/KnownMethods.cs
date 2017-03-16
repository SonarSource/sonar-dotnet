/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public static class KnownMethods
    {
        public static bool IsMainMethod(this IMethodSymbol methodSymbol)
        {
            // Based on Microsoft definition: https://msdn.microsoft.com/en-us/library/1y814bzs.aspx
            return methodSymbol.IsStatic &&
                (methodSymbol.ReturnsVoid || methodSymbol.ReturnType.Is(KnownType.System_Int32)) &&
                methodSymbol.Name.Equals("Main", StringComparison.Ordinal) &&
                (methodSymbol.Parameters.Length == 0 ||
                    (methodSymbol.Parameters.Length == 1 && methodSymbol.Parameters[0].IsType(KnownType.System_String_Array)));
        }

        public static bool IsObjectEquals(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsOverride &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Name == nameof(object.Equals) &&
                methodSymbol.Parameters.Length == 1 &&
                methodSymbol.Parameters[0].Type.Is(KnownType.System_Object) &&
                methodSymbol.ReturnType.Is(KnownType.System_Boolean);
        }

        public static bool IsObjectGetHashCode(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsOverride &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Name == nameof(object.GetHashCode) &&
                methodSymbol.Parameters.Length == 0 &&
                methodSymbol.ReturnType.Is(KnownType.System_Int32);
        }

        public static bool IsObjectToString(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.IsOverride &&
                methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Name == nameof(object.ToString) &&
                methodSymbol.Parameters.Length == 0 &&
                methodSymbol.ReturnType.Is(KnownType.System_String);
        }

        public static bool IsIDisposableDispose(this IMethodSymbol methodSymbol)
        {
            const string explicitName = "System.IDisposable.Dispose";
            return methodSymbol.ReturnsVoid &&
                methodSymbol.Parameters.Length == 0 &&
                (methodSymbol.Name == nameof(IDisposable.Dispose) ||
                 methodSymbol.Name == explicitName);
        }

        public static bool IsIEquatableEquals(this IMethodSymbol methodSymbol)
        {
            const string explicitName = "System.IEquatable.Equals";
            return  methodSymbol.Parameters.Length == 1 &&
                methodSymbol.ReturnType.Is(KnownType.System_Boolean) &&
                (methodSymbol.Name == nameof(object.Equals) ||
                methodSymbol.Name == explicitName);
        }

        public static bool IsSerializationConstructor(this IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.Constructor &&
                methodSymbol.Parameters.Length == 2 &&
                methodSymbol.Parameters[0].Type.Is(KnownType.System_Runtime_Serialization_SerializationInfo) &&
                methodSymbol.Parameters[1].Type.Is(KnownType.System_Runtime_Serialization_StreamingContext);
        }
    }
}