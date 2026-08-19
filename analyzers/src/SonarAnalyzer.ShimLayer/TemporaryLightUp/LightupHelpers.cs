// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace SonarAnalyzer.ShimLayer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Roslyn.Utilities;

    internal static class LightupHelpers
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>> SupportedObjectWrappers
            = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<SyntaxKind, bool>> SupportedSyntaxWrappers
            = new ConcurrentDictionary<Type, ConcurrentDictionary<SyntaxKind, bool>>();

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<OperationKind, bool>> SupportedOperationWrappers
            = new ConcurrentDictionary<Type, ConcurrentDictionary<OperationKind, bool>>();

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

        internal static Func<TSyntax, TProperty, TSyntax> CreateSyntaxWithPropertyAccessor<TSyntax, TProperty>(Type type, string propertyName)
        {
            TSyntax FallbackAccessor(TSyntax syntax, TProperty newValue)
            {
                if (syntax == null)
                {
                    // Unlike an extension method which would throw ArgumentNullException here, the light-up
                    // behavior needs to match behavior of the underlying property.
                    throw new NullReferenceException();
                }

                if (Equals(newValue, default(TProperty)))
                {
                    return syntax;
                }

                throw new NotSupportedException();
            }

            if (type == null)
            {
                return FallbackAccessor;
            }

            if (!typeof(TSyntax).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            var property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
            if (property == null)
            {
                return FallbackAccessor;
            }

            if (!typeof(TProperty).GetTypeInfo().IsAssignableFrom(property.PropertyType.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            var methodInfo = type.GetTypeInfo().GetDeclaredMethods("With" + propertyName)
                .SingleOrDefault(m => !m.IsStatic && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Equals(property.PropertyType));
            if (methodInfo is null)
            {
                return FallbackAccessor;
            }

            var syntaxParameter = Expression.Parameter(typeof(TSyntax), "syntax");
            var valueParameter = Expression.Parameter(typeof(TProperty), methodInfo.GetParameters()[0].Name);
            Expression instance =
                type.GetTypeInfo().IsAssignableFrom(typeof(TSyntax).GetTypeInfo())
                ? (Expression)syntaxParameter
                : Expression.Convert(syntaxParameter, type);
            Expression value =
                property.PropertyType.GetTypeInfo().IsAssignableFrom(typeof(TProperty).GetTypeInfo())
                ? (Expression)valueParameter
                : Expression.Convert(valueParameter, property.PropertyType);

            Expression<Func<TSyntax, TProperty, TSyntax>> expression =
                Expression.Lambda<Func<TSyntax, TProperty, TSyntax>>(
                    Expression.Call(instance, methodInfo, value),
                    syntaxParameter,
                    valueParameter);
            return expression.Compile();
        }
    }
}
