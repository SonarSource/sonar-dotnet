/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public static class KnownMethods
    {
        public static bool IsMainMethod(this IMethodSymbol methodSymbol)
        {
            // Based on Microsoft definition: https://msdn.microsoft.com/en-us/library/1y814bzs.aspx
            // Adding support for new async main: https://blogs.msdn.microsoft.com/mazhou/2017/05/30/c-7-series-part-2-async-main
            return methodSymbol != null
                && methodSymbol.IsStatic
                && methodSymbol.Name.Equals("Main", StringComparison.OrdinalIgnoreCase) // VB.NET is case insensitive
                && HasMainParameters()
                && HasMainReturnType();

            bool HasMainParameters() =>
                methodSymbol.Parameters.Length == 0
                || (methodSymbol.Parameters.Length == 1 && methodSymbol.Parameters[0].Type.IsAny(KnownType.System_String_Array, KnownType.System_String_Array_VB));

            bool HasMainReturnType() =>
                methodSymbol.ReturnsVoid
                || methodSymbol.ReturnType.IsAny(KnownType.System_Int32, KnownType.System_Threading_Tasks_Task)
                || (
                    methodSymbol.ReturnType.OriginalDefinition.Is(KnownType.System_Threading_Tasks_Task_T)
                    && ((methodSymbol.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault().Is(KnownType.System_Int32) ?? false));
        }

        public static bool IsObjectEquals(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
                && methodSymbol.MethodKind == MethodKind.Ordinary
                && methodSymbol.Name == nameof(object.Equals)
                && (methodSymbol.IsOverride || methodSymbol.IsInType(KnownType.System_Object))
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Parameters[0].Type.Is(KnownType.System_Object)
                && methodSymbol.ReturnType.Is(KnownType.System_Boolean);

        public static bool IsStaticObjectEquals(this IMethodSymbol methodSymbol)
        {
            return methodSymbol != null
                && !methodSymbol.IsOverride
                && methodSymbol.IsStatic
                && methodSymbol.MethodKind == MethodKind.Ordinary
                && methodSymbol.Name == nameof(object.Equals)
                && methodSymbol.IsInType(KnownType.System_Object)
                && HasCorrectParameters()
                && methodSymbol.ReturnType.Is(KnownType.System_Boolean);

            bool HasCorrectParameters() =>
                methodSymbol.Parameters.Length == 2
                && methodSymbol.Parameters[0].Type.Is(KnownType.System_Object)
                && methodSymbol.Parameters[1].Type.Is(KnownType.System_Object);
        }

        public static bool IsObjectGetHashCode(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Name == nameof(object.GetHashCode)
            && (methodSymbol.IsOverride || methodSymbol.IsInType(KnownType.System_Object))
            && methodSymbol.Parameters.Length == 0
            && methodSymbol.ReturnType.Is(KnownType.System_Int32);

        public static bool IsObjectToString(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Name == nameof(object.ToString)
            && (methodSymbol.IsOverride || methodSymbol.IsInType(KnownType.System_Object))
            && methodSymbol.Parameters.Length == 0
            && methodSymbol.ReturnType.Is(KnownType.System_String);

        public static bool IsIDisposableDispose(this IMethodSymbol methodSymbol)
        {
            const string explicitName = "System.IDisposable.Dispose";
            return methodSymbol != null
                && (methodSymbol.Name == nameof(IDisposable.Dispose) || methodSymbol.Name == explicitName)
                && methodSymbol.ReturnsVoid
                && methodSymbol.Parameters.Length == 0;
        }

        public static bool IsIAsyncDisposableDisposeAsync(this IMethodSymbol methodSymbol)
        {
            const string explicitNameAsync = "System.IAsyncDisposable.DisposeAsync";
            return methodSymbol != null
                && (methodSymbol.Name == "DisposeAsync" || methodSymbol.Name == explicitNameAsync)
                && methodSymbol.ReturnType.Is(KnownType.System_Threading_Tasks_ValueTask)
                && methodSymbol.Parameters.Length == 0
                && methodSymbol.GetInterfaceMember() is { } interfaceMember
                && interfaceMember.ContainingType.Is(KnownType.System_IAsyncDisposable);
        }

        public static bool IsIEquatableEquals(this IMethodSymbol methodSymbol)
        {
            const string explicitName = "System.IEquatable.Equals";
            return methodSymbol != null
                && (methodSymbol.Name == nameof(object.Equals) || methodSymbol.Name == explicitName)
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.ReturnType.Is(KnownType.System_Boolean);
        }

        public static bool IsGetObjectData(this IMethodSymbol methodSymbol)
        {
            const string explicitName = "System.Runtime.Serialization.ISerializable.GetObjectData";
            return methodSymbol != null
                && (methodSymbol.Name == "GetObjectData" || methodSymbol.Name == explicitName)
                && methodSymbol.Parameters.Length == 2
                && methodSymbol.Parameters[0].Type.Is(KnownType.System_Runtime_Serialization_SerializationInfo)
                && methodSymbol.Parameters[1].Type.Is(KnownType.System_Runtime_Serialization_StreamingContext)
                && methodSymbol.ReturnsVoid;
        }

        public static bool IsSerializationConstructor(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.Constructor
            && methodSymbol.Parameters.Length == 2
            && methodSymbol.Parameters[0].Type.Is(KnownType.System_Runtime_Serialization_SerializationInfo)
            && methodSymbol.Parameters[1].Type.Is(KnownType.System_Runtime_Serialization_StreamingContext);

        public static bool IsArrayClone(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Name == nameof(Array.Clone)
            && methodSymbol.Parameters.Length == 0
            && methodSymbol.ContainingType.Is(KnownType.System_Array);

        public static bool IsGcSuppressFinalize(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.Name == nameof(GC.SuppressFinalize)
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.ContainingType.Is(KnownType.System_GC);

        public static bool IsDebugAssert(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.Name == nameof(Debug.Assert)
            && methodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Debug);

        public static bool IsDiagnosticDebugMethod(this IMethodSymbol methodSymbol) =>
            methodSymbol != null && methodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Debug);

        public static bool IsOperatorBinaryPlus(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.UserDefinedOperator
            && methodSymbol.Name == "op_Addition"
            && methodSymbol.Parameters.Length == 2;

        public static bool IsOperatorBinaryMinus(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.UserDefinedOperator
            && methodSymbol.Name == "op_Subtraction"
            && methodSymbol.Parameters.Length == 2;

        public static bool IsOperatorEquals(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.UserDefinedOperator
            && methodSymbol.Name == "op_Equality"
            && methodSymbol.Parameters.Length == 2;

        public static bool IsOperatorNotEquals(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.MethodKind == MethodKind.UserDefinedOperator
            && methodSymbol.Name == "op_Inequality"
            && methodSymbol.Parameters.Length == 2;

        public static bool IsConsoleWriteLine(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.Name == nameof(Console.WriteLine)
            && methodSymbol.IsInType(KnownType.System_Console);

        public static bool IsConsoleWrite(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.Name == nameof(Console.Write)
            && methodSymbol.IsInType(KnownType.System_Console);

        private static bool IsEnumerableMethod(this IMethodSymbol methodSymbol, string methodName, params int[] parametersCount) =>
            methodSymbol != null
            && methodSymbol.Name == methodName
            && parametersCount.Any(count => methodSymbol.HasExactlyNParameters(count))
            && methodSymbol.ContainingType.Is(KnownType.System_Linq_Enumerable);

        public static bool IsEnumerableConcat(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.Concat), 2);

        public static bool IsEnumerableCount(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.Count), 1, 2);

        public static bool IsEnumerableExcept(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.Except), 2, 3);

        public static bool IsEnumerableIntersect(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.Intersect), 2, 3);

        public static bool IsEnumerableSequenceEqual(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.SequenceEqual), 2, 3);

        public static bool IsEnumerableToList(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.ToList), 1);

        public static bool IsEnumerableToArray(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.ToArray), 1);

        public static bool IsEnumerableUnion(this IMethodSymbol methodSymbol) =>
            methodSymbol.IsEnumerableMethod(nameof(Enumerable.Union), 2, 3);

        public static bool IsListAddRange(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.Name == "AddRange"
            && methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.ContainingType.ConstructedFrom.Is(KnownType.System_Collections_Generic_List_T);

        public static bool IsEventHandler(this IMethodSymbol methodSymbol) =>
            methodSymbol != null
            && methodSymbol.ReturnsVoid
            && methodSymbol.Parameters.Length == 2
            && (methodSymbol.Parameters[0].Name.Equals("sender", StringComparison.OrdinalIgnoreCase) || methodSymbol.Parameters[0].Type.Is(KnownType.System_Object))
            && (
                    // Inheritance from EventArgs is not enough for UWP or Xamarin as it uses other kind of event args (e.g. ILeavingBackgroundEventArgs)
                    methodSymbol.Parameters[1].Type.ToString().EndsWith("EventArgs", StringComparison.Ordinal) ||
                    methodSymbol.Parameters[1].Type.DerivesFrom(KnownType.System_EventArgs)
                );

        private static bool HasExactlyNParameters(this IMethodSymbol methodSymbol, int parametersCount) =>
            (methodSymbol.MethodKind == MethodKind.Ordinary && methodSymbol.Parameters.Length == parametersCount)
            || (methodSymbol.MethodKind == MethodKind.ReducedExtension && methodSymbol.Parameters.Length == parametersCount - 1);
    }
}
