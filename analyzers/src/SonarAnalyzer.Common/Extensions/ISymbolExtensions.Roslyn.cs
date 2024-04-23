// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Shared.Extensions;

// Copied from https://github.com/dotnet/roslyn/blob/ca66296efa86bd8078508fe7b38b91b415364f78/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs
[ExcludeFromCodeCoverage]
public static class ISymbolExtensions
{
    /// <summary>
    /// If the <paramref name="symbol"/> is a method symbol, returns <see langword="true"/> if the method's return type is "awaitable", but not if it's <see langword="dynamic"/>.
    /// If the <paramref name="symbol"/> is a type symbol, returns <see langword="true"/> if that type is "awaitable".
    /// An "awaitable" is any type that exposes a GetAwaiter method which returns a valid "awaiter". This GetAwaiter method may be an instance method or an extension method.
    /// </summary>
    /// <remarks>
    /// Copied from <seealso href="https://github.com/dotnet/roslyn/blob/ca66296efa86bd8078508fe7b38b91b415364f78/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L572-L577"/>.
    /// </remarks>
    public static bool IsAwaitableNonDynamic(this ISymbol? symbol, SemanticModel semanticModel, int position)
    {
        var methodSymbol = symbol as IMethodSymbol;
        ITypeSymbol? typeSymbol = null;

        if (methodSymbol == null)
        {
            typeSymbol = symbol as ITypeSymbol;
            if (typeSymbol == null)
            {
                return false;
            }
        }
        else
        {
            if (methodSymbol.ReturnType == null)
            {
                return false;
            }
        }

        // otherwise: needs valid GetAwaiter
        // SONAR: Performance: LookupSymbols is slow. We use the less precise GetMembers instead:
        // Misses extension methods and method from base classes
        var container = typeSymbol ?? methodSymbol!.ReturnType.OriginalDefinition;
        var potentialGetAwaiters = container.GetMembers(WellKnownMemberNames.GetAwaiter);
        var getAwaiters = potentialGetAwaiters.OfType<IMethodSymbol>().Where(x => !x.Parameters.Any());
        return getAwaiters.Any(VerifyGetAwaiter);
    }

    // Copied from https://github.com/dotnet/roslyn/blob/ca66296efa86bd8078508fe7b38b91b415364f78/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L611
    private static bool VerifyGetAwaiter(IMethodSymbol getAwaiter)
    {
        var returnType = getAwaiter.ReturnType;
        if (returnType == null)
        {
            return false;
        }

        // bool IsCompleted { get }
        if (!returnType.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == WellKnownMemberNames.IsCompleted && p.Type.SpecialType == SpecialType.System_Boolean && p.GetMethod != null))
        {
            return false;
        }

        var methods = returnType.GetMembers().OfType<IMethodSymbol>();

        // NOTE: (vladres) The current version of C# Spec, §7.7.7.3 'Runtime evaluation of await expressions', requires that
        // NOTE: the interface method INotifyCompletion.OnCompleted or ICriticalNotifyCompletion.UnsafeOnCompleted is invoked
        // NOTE: (rather than any OnCompleted method conforming to a certain pattern).
        // NOTE: Should this code be updated to match the spec?

        // void OnCompleted(Action)
        // Actions are delegates, so we'll just check for delegates.
        if (!methods.Any(x => x.Name == WellKnownMemberNames.OnCompleted && x.ReturnsVoid && x.Parameters is { Length: 1 } parameter && parameter[0] is { Type.TypeKind: TypeKind.Delegate }))
            return false;

        // void GetResult() || T GetResult()
        return methods.Any(m => m.Name == WellKnownMemberNames.GetResult && !m.Parameters.Any());
    }
}
