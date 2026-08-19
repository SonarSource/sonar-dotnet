/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ShimLayer.Common;

internal static class AccessorFactory
{
    private static readonly MethodInfo UnboundEnumerableSelectMethod = typeof(Enumerable).GetMethods().Single(IsEnumerableSelect);

    public static TFunc CreateMethod<TFunc>(Type runtimeSenderType, string methodName) where TFunc : Delegate
    {
        var types = new AccessorTypes(typeof(TFunc));
        return CreateAccessor<TFunc>(types, runtimeSenderType, methodName, runtimeSenderType?.GetMethods().FirstOrDefault(IsMethodMatch));

        bool IsMethodMatch(MethodInfo method) =>
            method.Name == methodName
            && method.ReturnType.Equals(types.ResultType)
            && method.GetParameters() is var parameters
            && parameters.Length == types.AllTypes.Length - 2     // Except the first TSender and last TResult
            && parameters.Select((x, i) => x.ParameterType.Equals(types.AllTypes[i + 1])).All(x => x);
    }

    public static TFunc CreateProperty<TFunc>(Type runtimeSenderType, string propertyName) where TFunc : Delegate
    {
        return CreateAccessor<TFunc>(new(typeof(TFunc)), runtimeSenderType, propertyName, FindProperty()?.GetMethod);

        PropertyInfo FindProperty()
        {
            if (runtimeSenderType?.GetTypeInfo().GetDeclaredProperty(propertyName) is { } declaredProperty)
            {
                return declaredProperty;
            }
            else if (runtimeSenderType?.IsInterface ?? false)
            {
                return runtimeSenderType.GetInterfaces().Select(x => x.GetTypeInfo().GetDeclaredProperty(propertyName)).FirstOrDefault(x => x is not null);
            }
            else
            {
                return null;
            }
        }
    }

    private static TFunc CreateAccessor<TFunc>(AccessorTypes types, Type runtimeSenderType, string memberName, MethodInfo method) where TFunc : Delegate
    {
        if (!typeof(TFunc).Name.StartsWith("Func`"))
        {
            throw new NotSupportedException("This method only supports Func<..., TResult>");    // We assume the last one is TResult, and fallback returns "default".
        }
        var lambdaParameters = types.AllTypes.Take(types.AllTypes.Length - 1).Select((x, i) => Expression.Parameter(x, i == 0 ? "sender" : "p" + i)).ToArray();
        var senderLambdaParameter = lambdaParameters.First();
        var lambdaReturnValue = runtimeSenderType is null || method is null
            ? CreateFallback()                                      // Fallback: return default;
            : WrapConvert(CreateWrappedCall(), types.ResultType);   // Actual shim for given method call
        var body = senderLambdaParameter.Type.IsValueType
            ? lambdaReturnValue
            : CreateCoalesceThrow();
        var lambda = Expression.Lambda<TFunc>(body, "ShimLayer_RuntimeLambdaExpressionFor_" + memberName, lambdaParameters);
        return lambda.Compile();

        Expression CreateCoalesceThrow()
        {
            // Generate expression: _ = sender ?? throw new NullReferenceException("Object reference ... ");     // The discard is implicit
            var message = $"Object reference not set to an instance of an object. This ShimLayer accessor for {memberName} was called with 'null' sender.";
            var coalesceThrow = Expression.Coalesce(senderLambdaParameter, Expression.Throw(Expression.New(typeof(NullReferenceException).GetConstructor([typeof(string)]), Expression.Constant(message)), types.SenderType));
            return Expression.Block(types.ResultType, coalesceThrow, lambdaReturnValue);
        }

        Expression CreateFallback()
        {
            if (types.ResultType.IsGenericType && types.ResultType.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
            {
                return Expression.Field(null, types.ResultType, nameof(ImmutableArray<>.Empty));
            }
            else if (types.ResultType.IsGenericType && types.ResultType.GetGenericTypeDefinition() == typeof(SeparatedSyntaxListWrapper<>))
            {
                return Expression.New(types.ResultType.GetConstructors().Single(), Expression.NewArrayInit(types.ResultType.GenericTypeArguments[0]), Expression.NewArrayInit(typeof(SyntaxToken)));
            }
            else
            {
                return Expression.Default(types.ResultType);
            }
        }

        Expression CreateWrappedCall()
        {
            var sender = WrapConvert(senderLambdaParameter, runtimeSenderType);
            var methodParameters = method.GetParameters();
            var result = Expression.Call(sender, method, lambdaParameters.Skip(1).Select((x, i) => WrapConvert(x, methodParameters[i].ParameterType)));
            if (types.ResultType.IsGenericType && types.ResultType.GetGenericTypeDefinition() == typeof(ImmutableArray<>) && method.ReturnType.GenericTypeArguments.Single() is var runtimeTypeArgument && typeof(IOperation).IsAssignableFrom(runtimeTypeArgument))
            {
                var castUp = typeof(ImmutableArray<IOperation>).GetMethod(nameof(ImmutableArray<>.CastUp)).MakeGenericMethod(runtimeTypeArgument);
                return Expression.Call(castUp, result);
            }
            else if (types.ResultType.IsGenericType && types.ResultType.GetGenericTypeDefinition() == typeof(SeparatedSyntaxListWrapper<>))
            {
                // Generate: new SeparatedSyntaxListWrapper<XxxWrapper>(x.Property.Select(x => XxxWrapper.From(x)), x.Property.GetSeparators())
                var itemRuntimeType = method.ReturnType.GetGenericArguments().Single();
                var itemWrapperType = types.ResultType.GetGenericArguments().Single();
                var selectorParameter = Expression.Parameter(itemRuntimeType, "x");
                var selectorLambda = Expression.Lambda(Expression.Call(itemWrapperType.GetMethod("From"), selectorParameter), selectorParameter);
                var items = Expression.Call(UnboundEnumerableSelectMethod.MakeGenericMethod(itemRuntimeType, itemWrapperType), Expression.Convert(result, typeof(IEnumerable<>).MakeGenericType(itemRuntimeType)), selectorLambda);
                var separators = Expression.Call(result, result.Type.GetMethod("GetSeparators"));
                return Expression.New(types.ResultType.GetConstructors().Single(), items, separators);
            }
            else if (types.ResultType.FullName == typeof(CaptureId).FullName)    // ToDo: This should be removed once we shim structs
            {
                return Expression.New(typeof(CaptureId).GetConstructors().Single(), Expression.Convert(result, typeof(object)));
            }
            else
            {
                return result;
            }
        }
    }

    private static Expression WrapConvert(Expression expression, Type type) =>
        type.IsAssignableFrom(expression.Type) ? expression : Expression.Convert(expression, type);

    private static bool IsEnumerableSelect(MethodInfo method) =>
        method.Name == nameof(Enumerable.Select)
        && method.GetParameters() is { Length: 2 } parameters
        && parameters[1].ParameterType.GenericTypeArguments.Length == 2;    // Func<TSource, TResult> instead of Func<TSource, int, TResult>

    private readonly struct AccessorTypes
    {
        public readonly Type[] AllTypes;
        public readonly Type SenderType;
        public readonly Type ResultType;

        public AccessorTypes(Type func)
        {
            AllTypes = func.GenericTypeArguments;   // First one is TSender (compile time), followed by lambda parameters. Last lambda parameter is TResult.
            SenderType = AllTypes.First();
            ResultType = AllTypes.Last();
        }
    }
}
