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

namespace SonarAnalyzer.ShimLayer.Common;

internal static class AccessorFactory
{
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
        // Generate expression: _ = sender ?? throw new NullReferenceException("Object reference ... ");     // The discard is implicit
        var message = $"Object reference not set to an instance of an object. This ShimLayer accessor for {memberName} was called with 'null' sender.";
        var coalesceThrow = Expression.Coalesce(senderLambdaParameter, Expression.Throw(Expression.New(typeof(NullReferenceException).GetConstructor([typeof(string)]), Expression.Constant(message)), types.SenderType));
        var lambdaReturnValue = runtimeSenderType is null || method is null
            ? Expression.Default(types.ResultType)                  // Fallback: return default;
            : WrapConvert(CreateWrappedCall(), types.ResultType);   // Actual shim for given method call
        var lambda = Expression.Lambda<TFunc>(Expression.Block(types.ResultType, coalesceThrow, lambdaReturnValue), "ShimLayer_RuntimeLambdaExpressionFor_" + memberName, lambdaParameters);
        return lambda.Compile();

        Expression CreateWrappedCall()
        {
            var sender = WrapConvert(senderLambdaParameter, runtimeSenderType);
            var result = Expression.Call(sender, method);
            if (types.ResultType.FullName == typeof(CaptureId).FullName)    // ToDo: This should be removed once we shim structs
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
