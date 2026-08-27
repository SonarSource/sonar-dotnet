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

namespace SonarAnalyzer.ShimLayer.Common;

internal static class AccessorFactory
{
    private static readonly MethodInfo UnboundEnumerableSelectMethod = typeof(Enumerable).GetMethods().Single(IsEnumerableSelect);
    private static readonly MethodInfo UnboundEnumerableToArrayMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));
    private static readonly MethodInfo UnboundImmutableArrayCreateRangeMethod = typeof(ImmutableArray).GetMethods().Single(IsImmutableArrayCreateRange);

    public static TFunc CreateMethod<TFunc>(Type runtimeSenderType, string methodName) where TFunc : Delegate
    {
        var types = new AccessorTypes(typeof(TFunc), false);
        return CreateAccessor<TFunc>(types, runtimeSenderType, methodName, runtimeSenderType?.GetMethods().FirstOrDefault(IsMethodMatch));

        bool IsMethodMatch(MethodInfo method) =>
            method.Name == methodName
            && IsReturnTypeMatch(method, types)
            && method.GetParameters() is var parameters
            && parameters.Length == types.ParameterTypes.Length
            && parameters.Select((x, i) => IsParameterMatch(types.ParameterTypes[i], x.ParameterType)).All(x => x);

        static bool IsParameterMatch(Type compiletime, Type runtime) =>
            compiletime.Equals(runtime)
            || (compiletime.IsArray && runtime.IsArray && IsParameterMatch(compiletime.GetElementType(), runtime.GetElementType()))
            || IsEnumMatch(compiletime, runtime)
            || compiletime.Name == $"{runtime.Name}Wrapper";

        static bool IsReturnTypeMatch(MethodInfo method, AccessorTypes types) =>
            types.ResultType.IsAssignableFrom(method.ReturnType)
            || IsEnumMatch(types.ResultType, method.ReturnType);

        static bool IsEnumMatch(Type compiletime, Type runtime) =>
            compiletime.IsEnum && runtime.IsEnum && compiletime.Name == runtime.Name;
    }

    public static TFunc CreateProperty<TFunc>(Type runtimeSenderType, string propertyName) where TFunc : Delegate =>
        CreateProperty<TFunc>(runtimeSenderType, propertyName, new(typeof(TFunc), false));

    public static TFunc CreateStaticProperty<TFunc>(Type runtimeSenderType, string propertyName) where TFunc : Delegate =>
        CreateProperty<TFunc>(runtimeSenderType, propertyName, new(typeof(TFunc), true));

    private static TFunc CreateProperty<TFunc>(Type runtimeSenderType, string propertyName, AccessorTypes types) where TFunc : Delegate
    {
        return CreateAccessor<TFunc>(types, runtimeSenderType, propertyName, FindProperty()?.GetMethod);

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
        var lambdaParameters = types.ParameterTypes.Prepend(types.SenderType).Where(x => x is not null).Select((x, i) => Expression.Parameter(x, i == 0 ? "sender" : "p" + i)).ToArray();
        var senderLambdaParameter = types.SenderType is null ? null : lambdaParameters.First();
        var lambdaReturnValue = runtimeSenderType is null || method is null
            ? CreateFallback()                                      // Fallback: return default;
            : WrapConvert(CreateWrappedCall(), types.ResultType);   // Actual shim for given method call
        var body = senderLambdaParameter is null || senderLambdaParameter.Type.IsValueType
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
            var sender = senderLambdaParameter is null ? null : WrapConvert(senderLambdaParameter, runtimeSenderType);
            var methodParameters = method.GetParameters();
            var result = Expression.Call(sender, method, lambdaParameters.Skip(1).Select((x, i) => ConvertArgument(x, methodParameters[i].ParameterType)));
            if (types.ResultType.IsGenericType
                && types.ResultType.GetGenericTypeDefinition() == typeof(ImmutableArray<>)
                && types.ResultType.GenericTypeArguments.Single() is var wrapperTypeArgument
                && (wrapperTypeArgument.IsEnum || typeof(IOperationWrapper).IsAssignableFrom(wrapperTypeArgument)))
            {
                return CreateImmutableArrayConversion(result, method.ReturnType, wrapperTypeArgument);
            }
            else if (types.ResultType.IsGenericType && types.ResultType.GetGenericTypeDefinition() == typeof(SeparatedSyntaxListWrapper<>))
            {
                return CreateSeparatedSyntaxListConversion(result, method.ReturnType, types.ResultType);
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

    private static Expression ConvertArgument(Expression expression, Type type)
    {
        if (type.IsArray    // XxxWrapper[] => Xxx[]
            && !type.IsAssignableFrom(expression.Type)
            && expression.Type.GetElementType() is var elementWrapperType
            && elementWrapperType.GetProperty("WrappedInstance") is { } elementWrappedInstance)
        {
            // Generate: result.Select(x => (Xxx)x.WrappedInstance).ToArray()
            var itemRuntimeType = type.GetElementType();
            var selectorParameter = Expression.Parameter(elementWrapperType, "x");
            var selectorLambda = Expression.Lambda(WrapConvert(Expression.Property(selectorParameter, elementWrappedInstance), itemRuntimeType), selectorParameter);
            var items = Expression.Call(UnboundEnumerableSelectMethod.MakeGenericMethod(elementWrapperType, itemRuntimeType), expression, selectorLambda);
            return Expression.Call(UnboundEnumerableToArrayMethod.MakeGenericMethod(itemRuntimeType), items);
        }
        else if (expression.Type.GetProperty("WrappedInstance") is { } wrappedInstance)
        {
            return WrapConvert(Expression.Property(expression, wrappedInstance), type);
        }
        else
        {
            return WrapConvert(expression, type);
        }
    }

    private static Expression WrapConvert(Expression expression, Type type)
    {
        var underlayingType = type.IsByRef ? type.GetElementType() : type;
        return underlayingType.IsAssignableFrom(expression.Type) ? expression : Expression.Convert(expression, underlayingType);
    }

    private static Expression CreateImmutableArrayConversion(Expression result, Type runtimeReturnType, Type wrapperTypeArgument)
    {
        // Generate: ImmutableArray.CreateRange(result, x => XxxWrapper.From(x))
        // For enum: ImmutableArray.CreateRange(result, x => (XxxWrapper)(object)(x))
        var itemRuntimeType = runtimeReturnType.GetGenericArguments().Single();
        var selectorParameter = Expression.Parameter(itemRuntimeType, "x");
        var selectorConversion = wrapperTypeArgument.IsEnum
            ? (Expression)Expression.Convert(Expression.Convert(selectorParameter, typeof(object)), wrapperTypeArgument)
            : Expression.Call(wrapperTypeArgument.GetMethod("From"), selectorParameter);
        var selectorLambda = Expression.Lambda(selectorConversion, selectorParameter);
        return Expression.Call(UnboundImmutableArrayCreateRangeMethod.MakeGenericMethod(itemRuntimeType, wrapperTypeArgument), result, selectorLambda);
    }

    private static Expression CreateSeparatedSyntaxListConversion(Expression result, Type runtimeReturnType, Type compiletimeResultType)
    {
        // Generate: new SeparatedSyntaxListWrapper<XxxWrapper>(x.Property.Select(x => XxxWrapper.From(x)), x.Property.GetSeparators())
        var itemRuntimeType = runtimeReturnType.GetGenericArguments().Single();
        var itemWrapperType = compiletimeResultType.GetGenericArguments().Single();
        var selectorParameter = Expression.Parameter(itemRuntimeType, "x");
        var selectorLambda = Expression.Lambda(Expression.Call(itemWrapperType.GetMethod("From"), selectorParameter), selectorParameter);
        var items = Expression.Call(UnboundEnumerableSelectMethod.MakeGenericMethod(itemRuntimeType, itemWrapperType), Expression.Convert(result, typeof(IEnumerable<>).MakeGenericType(itemRuntimeType)), selectorLambda);
        var separators = Expression.Call(result, result.Type.GetMethod("GetSeparators"));
        return Expression.New(compiletimeResultType.GetConstructors().Single(), items, separators);
    }

    private static bool IsEnumerableSelect(MethodInfo method) =>
        method.Name == nameof(Enumerable.Select)
        && method.GetParameters() is { Length: 2 } parameters
        && parameters[1].ParameterType.GenericTypeArguments.Length == 2;    // Func<TSource, TResult> instead of Func<TSource, int, TResult>

    private static bool IsImmutableArrayCreateRange(MethodInfo method) =>
        method.Name == nameof(ImmutableArray.CreateRange)
        && method.GetParameters().Length == 2
        && method.GetGenericArguments().Length == 2;

    private readonly struct AccessorTypes
    {
        public readonly Type SenderType;
        public readonly Type[] ParameterTypes;
        public readonly Type ResultType;

        public AccessorTypes(Type methodDelegate, bool isStatic)
        {
            var invoke = methodDelegate.GetMethod("Invoke");
            var parameters = invoke.GetParameters();    // As declared in our delegates, has additional TSender compared to the runtime method
            SenderType = isStatic ? null : parameters.First().ParameterType;
            ParameterTypes = parameters.Skip(isStatic ? 0 : 1).Select(x => x.ParameterType).ToArray(); // Without TSender and TResult
            ResultType = invoke.ReturnType;     // Can be also typeof(void)
        }
    }
}
