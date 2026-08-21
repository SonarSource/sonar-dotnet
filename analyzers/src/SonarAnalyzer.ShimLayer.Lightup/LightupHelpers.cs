// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

namespace StyleCop.Analyzers.Lightup
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

    public static class LightupHelpers
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>> SupportedObjectWrappers
            = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, bool>>();

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

        internal static Func<TProperty> CreateStaticPropertyAccessor<TProperty>(Type type, string propertyName)
        {
            static TProperty FallbackAccessor()
            {
                return default;
            }

            if (type == null)
            {
                return FallbackAccessor;
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

            Expression<Func<TProperty>> expression =
                Expression.Lambda<Func<TProperty>>(
                    Expression.Call(null, property.GetMethod));
            return expression.Compile();
        }

        public static Func<TSyntax, TProperty> CreateSyntaxPropertyAccessor<TSyntax, TProperty>(Type type, string propertyName)
        {
            TProperty FallbackAccessor(TSyntax syntax)
            {
                if (syntax == null)
                {
                    // Unlike an extension method which would throw ArgumentNullException here, the light-up
                    // behavior needs to match behavior of the underlying property.
                    throw new NullReferenceException();
                }

                return default;
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
            var syntaxParameter = Expression.Parameter(typeof(TSyntax), "syntax"); // Sonar - begin
            Expression instance =
                type.GetTypeInfo().IsAssignableFrom(typeof(TSyntax).GetTypeInfo())
                ? (Expression)syntaxParameter
                : Expression.Convert(syntaxParameter, type);

            Expression body = Expression.Call(instance, property.GetMethod);

            if (!typeof(TProperty).GetTypeInfo().IsAssignableFrom(property.PropertyType.GetTypeInfo()))
            {
                body = Expression.Convert(body, typeof(TProperty));
            }

            return Expression.Lambda<Func<TSyntax, TProperty>>(body, syntaxParameter).Compile(); // Sonar - end
        }

        internal static TryGetValueAccessor<TSender, TFirst, TSecond, TValue> CreateTryGetValueAccessor<TSender, TFirst, TSecond, TValue>(Type type, Type firstType, Type secondType, string methodName) // Sonar
        {
            static bool FallbackAccessor(TSender sender, TFirst first, TSecond second, out TValue value)
            {
                if (sender == null)
                {
                    // Unlike an extension method which would throw ArgumentNullException here, the light-up
                    // behavior needs to match behavior of the underlying property.
                    throw new NullReferenceException();
                }

                value = default;
                return false;
            }

            if (type == null)
            {
                return FallbackAccessor;
            }

            if (!typeof(TSender).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            if (!typeof(TFirst).GetTypeInfo().IsAssignableFrom(firstType.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            if (!typeof(TSecond).GetTypeInfo().IsAssignableFrom(secondType.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            var methods = type.GetTypeInfo().GetDeclaredMethods(methodName);
            MethodInfo method = null;
            foreach (var candidate in methods)
            {
                var parameters = candidate.GetParameters();
                if (parameters.Length != 4)
                {
                    continue;
                }

                if (Equals(firstType, parameters[0].ParameterType)
                    && Equals(secondType, parameters[1].ParameterType)
                    && Equals(typeof(TValue).MakeByRefType(), parameters[2].ParameterType))
                {
                    method = candidate;
                    break;
                }
            }

            if (method == null)
            {
                return FallbackAccessor;
            }

            if (method.ReturnType != typeof(bool))
            {
                throw new InvalidOperationException();
            }

            var senderParameter = Expression.Parameter(typeof(TSender), "sender");
            var firstParameter = Expression.Parameter(typeof(TFirst), "first");
            var secondParameter = Expression.Parameter(typeof(TSecond), "second");
            var valueParameter = Expression.Parameter(typeof(TValue).MakeByRefType(), "value");
            Expression instance =
                type.GetTypeInfo().IsAssignableFrom(typeof(TSender).GetTypeInfo())
                ? (Expression)senderParameter
                : Expression.Convert(senderParameter, type);
            Expression first =
                firstType.GetTypeInfo().IsAssignableFrom(typeof(TFirst).GetTypeInfo())
                ? (Expression)firstParameter
                : Expression.Convert(firstParameter, firstType);
            Expression second =
                secondType.GetTypeInfo().IsAssignableFrom(typeof(TSecond).GetTypeInfo())
                ? (Expression)secondParameter
                : Expression.Convert(secondParameter, secondType);

            Expression<TryGetValueAccessor<TSender, TFirst, TSecond, TValue>> expression =
                Expression.Lambda<TryGetValueAccessor<TSender, TFirst, TSecond, TValue>>(
                    Expression.Call(instance, method, first, second, valueParameter),
                    senderParameter,
                    firstParameter,
                    secondParameter,
                    valueParameter);
            return expression.Compile();
        }

        internal static TryGetValueAccessor<TSender, TFirst, TSecond, TThird, TValue> CreateTryGetValueAccessor<TSender, TFirst, TSecond, TThird, TValue>(Type type, Type firstType, Type secondType, Type thirdType, string methodName) // Sonar
        {
            static bool FallbackAccessor(TSender sender, TFirst first, TSecond second, TThird third, out TValue value)
            {
                if (sender == null)
                {
                    // Unlike an extension method which would throw ArgumentNullException here, the light-up
                    // behavior needs to match behavior of the underlying property.
                    throw new NullReferenceException();
                }

                value = default;
                return false;
            }

            if (type == null)
            {
                return FallbackAccessor;
            }

            if (!typeof(TSender).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            if (!typeof(TFirst).GetTypeInfo().IsAssignableFrom(firstType.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            if (!typeof(TSecond).GetTypeInfo().IsAssignableFrom(secondType.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            if (!typeof(TThird).GetTypeInfo().IsAssignableFrom(thirdType.GetTypeInfo()))
            {
                throw new InvalidOperationException();
            }

            var methods = type.GetTypeInfo().GetDeclaredMethods(methodName);
            MethodInfo method = null;
            foreach (var candidate in methods)
            {
                var parameters = candidate.GetParameters();
                if (parameters.Length != 4)
                {
                    continue;
                }

                if (Equals(firstType, parameters[0].ParameterType)
                    && Equals(secondType, parameters[1].ParameterType)
                    && Equals(thirdType, parameters[2].ParameterType)
                    && Equals(typeof(TValue).MakeByRefType(), parameters[3].ParameterType))
                {
                    method = candidate;
                    break;
                }
            }

            if (method == null)
            {
                return FallbackAccessor;
            }

            if (method.ReturnType != typeof(bool))
            {
                throw new InvalidOperationException();
            }

            var senderParameter = Expression.Parameter(typeof(TSender), "sender");
            var firstParameter = Expression.Parameter(typeof(TFirst), "first");
            var secondParameter = Expression.Parameter(typeof(TSecond), "second");
            var thirdParameter = Expression.Parameter(typeof(TThird), "third");
            var valueParameter = Expression.Parameter(typeof(TValue).MakeByRefType(), "value");
            Expression instance =
                type.GetTypeInfo().IsAssignableFrom(typeof(TSender).GetTypeInfo())
                ? (Expression)senderParameter
                : Expression.Convert(senderParameter, type);
            Expression first =
                firstType.GetTypeInfo().IsAssignableFrom(typeof(TFirst).GetTypeInfo())
                ? (Expression)firstParameter
                : Expression.Convert(firstParameter, firstType);
            Expression second =
                secondType.GetTypeInfo().IsAssignableFrom(typeof(TSecond).GetTypeInfo())
                ? (Expression)secondParameter
                : Expression.Convert(secondParameter, secondType);
            Expression third =
                thirdType.GetTypeInfo().IsAssignableFrom(typeof(TThird).GetTypeInfo())
                ? (Expression)thirdParameter
                : Expression.Convert(thirdParameter, thirdType);

            Expression<TryGetValueAccessor<TSender, TFirst, TSecond, TThird, TValue>> expression =
                Expression.Lambda<TryGetValueAccessor<TSender, TFirst, TSecond, TThird, TValue>>(
                    Expression.Call(instance, method, first, second, third, valueParameter),
                    senderParameter,
                    firstParameter,
                    secondParameter,
                    thirdParameter,
                    valueParameter);
            return expression.Compile();
        }
    }
}
