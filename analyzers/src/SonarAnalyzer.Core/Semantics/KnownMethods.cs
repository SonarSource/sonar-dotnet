/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Semantics;

public static class KnownMethods
{
    private const int NumberOfParamsForBinaryOperator = 2;

    /// <summary>
    /// List of partial names that are assumed to indicate an assertion method.
    /// </summary>
    public static readonly ImmutableArray<string> AssertionMethodParts = ImmutableArray.Create(
            "ASSERT",
            "CHECK",
            "EXPECT",
            "MUST",
            "SHOULD",
            "VERIFY",
            "VALIDATE");

    public static bool IsMainMethod(this IMethodSymbol methodSymbol)
    {
        // Based on Microsoft definition: https://msdn.microsoft.com/en-us/library/1y814bzs.aspx
        // Adding support for new async main: https://blogs.msdn.microsoft.com/mazhou/2017/05/30/c-7-series-part-2-async-main
        return methodSymbol is not null
            && methodSymbol.IsStatic
            && methodSymbol.Name.Equals("Main", StringComparison.OrdinalIgnoreCase) // VB.NET is case insensitive
            && HasMainParameters()
            && HasMainReturnType();

        bool HasMainParameters() =>
            methodSymbol.Parameters.Length == 0
            || (methodSymbol.Parameters.Length == 1 && methodSymbol.Parameters[0].Type.Is(KnownType.System_String_Array));

        bool HasMainReturnType() =>
            methodSymbol.ReturnsVoid
            || methodSymbol.ReturnType.IsAny(KnownType.System_Int32, KnownType.System_Threading_Tasks_Task)
            || (methodSymbol.ReturnType.OriginalDefinition.Is(KnownType.System_Threading_Tasks_Task_T)
                && ((methodSymbol.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault().Is(KnownType.System_Int32) ?? false));
    }

    public static bool IsObjectEquals(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
            && methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Name == nameof(Equals)
            && (methodSymbol.IsOverride || methodSymbol.IsInType(KnownType.System_Object))
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.Parameters[0].Type.Is(KnownType.System_Object)
            && methodSymbol.ReturnType.Is(KnownType.System_Boolean);

    public static bool IsStaticObjectEquals(this IMethodSymbol methodSymbol)
    {
        return methodSymbol is not null
            && !methodSymbol.IsOverride
            && methodSymbol.IsStatic
            && methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Name == nameof(Equals)
            && methodSymbol.IsInType(KnownType.System_Object)
            && HasCorrectParameters()
            && methodSymbol.ReturnType.Is(KnownType.System_Boolean);

        bool HasCorrectParameters() =>
            methodSymbol.Parameters.Length == 2
            && methodSymbol.Parameters[0].Type.Is(KnownType.System_Object)
            && methodSymbol.Parameters[1].Type.Is(KnownType.System_Object);
    }

    public static bool IsObjectGetHashCode(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.MethodKind == MethodKind.Ordinary
        && methodSymbol.Name == nameof(GetHashCode)
        && (methodSymbol.IsOverride || methodSymbol.IsInType(KnownType.System_Object))
        && methodSymbol.Parameters.Length == 0
        && methodSymbol.ReturnType.Is(KnownType.System_Int32);

    public static bool IsObjectToString(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.MethodKind == MethodKind.Ordinary
        && methodSymbol.Name == nameof(ToString)
        && (methodSymbol.IsOverride || methodSymbol.IsInType(KnownType.System_Object))
        && methodSymbol.Parameters.Length == 0
        && methodSymbol.ReturnType.Is(KnownType.System_String);

    // The Dispose method is either coming from System.IDisposable for classes and records or declared manually for ref struct types:
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/using#pattern-based-using
    public static bool IsIDisposableDispose(this IMethodSymbol methodSymbol) =>
        methodSymbol is
        {
            IsStatic: false,
            Name: "Dispose" or "System.IDisposable.Dispose",
            Arity: 0,
            ReturnsVoid: true,
            Parameters.Length: 0
        }
        && ((ContainingInterface(methodSymbol) is { } containingInterface && containingInterface.Is(KnownType.System_IDisposable))  // class/record implementing System.IDisposable
            || (methodSymbol.ContainingType is { IsValueType: true } && methodSymbol.ContainingType.IsRefLikeType()));              // or a ref struct type

    public static bool IsIAsyncDisposableDisposeAsync(this IMethodSymbol methodSymbol) =>
        methodSymbol is
        {
            IsStatic: false,
            Name: "DisposeAsync" or "System.IAsyncDisposable.DisposeAsync",
            Arity: 0,
            Parameters.Length: 0
        }
        && methodSymbol.ReturnType.Is(KnownType.System_Threading_Tasks_ValueTask)
        && ContainingInterface(methodSymbol) is { } containingInterface
        && containingInterface.Is(KnownType.System_IAsyncDisposable);

    public static bool IsIEquatableEquals(this IMethodSymbol methodSymbol)
    {
        const string explicitName = "System.IEquatable.Equals";
        return methodSymbol is not null
            && (methodSymbol.Name == nameof(Equals) || methodSymbol.Name == explicitName)
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.ReturnType.Is(KnownType.System_Boolean);
    }

    public static bool IsGetObjectData(this IMethodSymbol methodSymbol)
    {
        const string explicitName = "System.Runtime.Serialization.ISerializable.GetObjectData";
        return methodSymbol is not null
            && (methodSymbol.Name == "GetObjectData" || methodSymbol.Name == explicitName)
            && methodSymbol.Parameters.Length == 2
            && methodSymbol.Parameters[0].Type.Is(KnownType.System_Runtime_Serialization_SerializationInfo)
            && methodSymbol.Parameters[1].Type.Is(KnownType.System_Runtime_Serialization_StreamingContext)
            && methodSymbol.ReturnsVoid;
    }

    public static bool IsSerializationConstructor(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.MethodKind == MethodKind.Constructor
        && methodSymbol.Parameters.Length == 2
        && methodSymbol.Parameters[0].Type.Is(KnownType.System_Runtime_Serialization_SerializationInfo)
        && methodSymbol.Parameters[1].Type.Is(KnownType.System_Runtime_Serialization_StreamingContext);

    public static bool IsArrayClone(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.MethodKind == MethodKind.Ordinary
        && methodSymbol.Name == nameof(Array.Clone)
        && methodSymbol.Parameters.Length == 0
        && methodSymbol.ContainingType.Is(KnownType.System_Array);

    public static bool IsRecordPrintMembers(this IMethodSymbol methodSymbol) =>
        methodSymbol is
        {
            MethodKind: MethodKind.Ordinary,
            Name: "PrintMembers",
            ReturnType.SpecialType: SpecialType.System_Boolean,
            Parameters.Length: 1,
        }
        && methodSymbol.Parameters[0].Type.Is(KnownType.System_Text_StringBuilder)
        && methodSymbol.ContainingType.IsRecord();

    public static bool IsGcSuppressFinalize(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.Name == nameof(GC.SuppressFinalize)
        && methodSymbol.Parameters.Length == 1
        && methodSymbol.ContainingType.Is(KnownType.System_GC);

    public static bool IsDebugAssert(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.Name == nameof(Debug.Assert)
        && methodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Debug);

    public static bool IsDiagnosticDebugMethod(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null && methodSymbol.ContainingType.Is(KnownType.System_Diagnostics_Debug);

    public static bool IsOperatorBinaryPlus(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Addition", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsOperatorBinaryMinus(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Subtraction", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsOperatorBinaryMultiply(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Multiply", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsOperatorBinaryDivide(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Division", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsOperatorBinaryModulus(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Modulus", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsOperatorEquals(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Equality", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsOperatorNotEquals(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator, Name: "op_Inequality", Parameters.Length: NumberOfParamsForBinaryOperator };

    public static bool IsConsoleWriteLine(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.Name == nameof(Console.WriteLine)
        && methodSymbol.IsInType(KnownType.System_Console);

    public static bool IsConsoleWrite(this IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.Name == nameof(Console.Write)
        && methodSymbol.IsInType(KnownType.System_Console);

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
        methodSymbol is not null
        && methodSymbol.Name == "AddRange"
        && methodSymbol.MethodKind == MethodKind.Ordinary
        && methodSymbol.Parameters.Length == 1
        && methodSymbol.ContainingType.ConstructedFrom.Is(KnownType.System_Collections_Generic_List_T);

    public static bool IsEventHandler(this IMethodSymbol methodSymbol) =>
        methodSymbol is { Parameters.Length: 2 }
        && (// Inheritance from EventArgs is not enough for UWP or Xamarin as it uses other kind of event args (e.g. ILeavingBackgroundEventArgs)
            methodSymbol.Parameters[1].Type.Name.EndsWith("EventArgs", StringComparison.Ordinal)
            || methodSymbol.Parameters[1].Type.DerivesFrom(KnownType.System_EventArgs))
        && (methodSymbol.ReturnsVoid
            // The ResolveEventHandler violates the https://learn.microsoft.com/en-us/dotnet/csharp/event-pattern#event-delegate-signatures
            // The ResolveEventHandler dates back to .Net1.1, is present in most runtimes and needs to be supported as an exception to the rule
            // https://github.com/SonarSource/sonar-dotnet/issues/8371
            // https://learn.microsoft.com/dotnet/api/system.resolveeventhandler
            || methodSymbol.ReturnType.Is(KnownType.System_Reflection_Assembly));

    private static bool IsEnumerableMethod(this IMethodSymbol methodSymbol, string methodName, params int[] parametersCount) =>
        methodSymbol is not null
        && methodSymbol.Name == methodName
        && Array.Exists(parametersCount, methodSymbol.HasExactlyNParameters)
        && methodSymbol.ContainingType.Is(KnownType.System_Linq_Enumerable);

    private static bool HasExactlyNParameters(this IMethodSymbol methodSymbol, int parametersCount) =>
        (methodSymbol.MethodKind == MethodKind.Ordinary && methodSymbol.Parameters.Length == parametersCount)
        || (methodSymbol.MethodKind == MethodKind.ReducedExtension && methodSymbol.Parameters.Length == parametersCount - 1);

    private static INamedTypeSymbol ContainingInterface(IMethodSymbol symbol)
    {
        if (symbol.InterfaceMembers().FirstOrDefault() is { } interfaceMember)
        {
            return interfaceMember.ContainingType;
        }
        else if (symbol.ContainingType.IsInterface())
        {
            return symbol.ContainingType;
        }
        else
        {
            return null;
        }
    }
}
