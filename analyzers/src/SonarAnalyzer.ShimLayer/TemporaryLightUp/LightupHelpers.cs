// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslyn.Utilities;

namespace SonarAnalyzer.ShimLayer
{

    internal static class LightupHelpers
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>> SupportedObjectWrappers = new();
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<SyntaxKind, bool>> SupportedSyntaxWrappers = new();
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<OperationKind, bool>> SupportedOperationWrappers = new();

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/8106", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)] // Sonar
        internal static bool CanWrapObject(object obj, Type underlyingType)
        {
            if (obj == null)
            {
                // The wrappers support a null instance
                return true;
            }

            if (underlyingType == null)
            {
                // The current runtime doesn't define the target type of the conversion, so no instance of it can exist
                return false;
            }

            ConcurrentDictionary<Type, bool> wrappedObject = SupportedObjectWrappers.GetOrAdd(underlyingType, static _ => new ConcurrentDictionary<Type, bool>());

            // Avoid creating a delegate and capture class
            if (!wrappedObject.TryGetValue(obj.GetType(), out var canCast))
            {
                canCast = underlyingType.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo());
                wrappedObject.TryAdd(obj.GetType(), canCast);
            }

            return canCast;
        }

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/8106", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)] // Sonar
        internal static bool CanWrapNode(SyntaxNode node, Type underlyingType)
        {
            if (node == null)
            {
                // The wrappers support a null instance
                return true;
            }

            if (underlyingType == null)
            {
                // The current runtime doesn't define the target type of the conversion, so no instance of it can exist
                return false;
            }

            ConcurrentDictionary<SyntaxKind, bool> wrappedSyntax = SupportedSyntaxWrappers.GetOrAdd(underlyingType, static _ => new ConcurrentDictionary<SyntaxKind, bool>());

            // Avoid creating a delegate and capture class
            if (!wrappedSyntax.TryGetValue(node.Kind(), out var canCast))
            {
                canCast = underlyingType.GetTypeInfo().IsAssignableFrom(node.GetType().GetTypeInfo());
                wrappedSyntax.TryAdd(node.Kind(), canCast);
            }

            return canCast;
        }

        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/8106", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)] // Sonar
        internal static bool CanWrapOperation(IOperation operation, Type underlyingType)
        {
            if (operation == null)
            {
                // The wrappers support a null instance
                return true;
            }

            if (underlyingType == null)
            {
                // The current runtime doesn't define the target type of the conversion, so no instance of it can exist
                return false;
            }

            ConcurrentDictionary<OperationKind, bool> wrappedSyntax = SupportedOperationWrappers.GetOrAdd(underlyingType, static _ => new ConcurrentDictionary<OperationKind, bool>());

            // Avoid creating a delegate and capture class
            // Sonar: https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.operationkind Loop && CaseClause are further differentiated by LoopKind & CaseKind, but are not castable between different Kinds which can result in InvalidCast Exceptions
            if (!wrappedSyntax.TryGetValue(operation.Kind, out var canCast) || operation.Kind is OperationKindEx.Loop or OperationKindEx.CaseClause) // Sonar
            {
                canCast = underlyingType.GetTypeInfo().IsAssignableFrom(operation.GetType().GetTypeInfo());
                wrappedSyntax.TryAdd(operation.Kind, canCast);
            }

            return canCast;
        }
    }
}
