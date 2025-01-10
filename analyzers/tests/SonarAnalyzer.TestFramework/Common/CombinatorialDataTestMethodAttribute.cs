/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Reflection;

namespace SonarAnalyzer.TestFramework.Common;

// Based on https://stackoverflow.com/a/75531690
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class DataValuesAttribute : Attribute
{
    public object[] Values { get; }

    public DataValuesAttribute(params object[] values)
    {
        Values = values;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class CombinatorialDataTestMethodAttribute : TestMethodAttribute, ITestDataSource
{
    public IEnumerable<object[]> GetData(MethodInfo methodInfo)
    {
        var valuesPerParameter = methodInfo.GetParameters().Select(x => x.GetCustomAttribute<DataValuesAttribute>()?.Values
            ?? throw new InvalidOperationException("Combinatorial test requires all parameters to have the [DataValues] attribute set")).ToArray();
        if (valuesPerParameter.Length == 0)
        {
            throw new InvalidOperationException("Combinatorial test must specify parameters with [DataValues] attributes");
        }
        var arrayLengths = valuesPerParameter.Select(x => x.Length == 0
            ? throw new InvalidOperationException($"[DataValues] attribute must have values set for all parameters")
            : x.Length).ToArray();
        var totalCombinations = arrayLengths.Aggregate(1, (x, y) => x * y);
        var parameterIndices = new int[arrayLengths.Length];

        for (var i = 0; i < totalCombinations; i++)
        {
            var temp = i;
            for (var j = arrayLengths.Length - 1; j >= 0; j--)
            {
                parameterIndices[j] = temp % arrayLengths[j];
                temp /= arrayLengths[j];
            }
            yield return CreateArguments(valuesPerParameter, parameterIndices);
        }
    }

    public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
        $"{methodInfo.Name} ({(data is not null ? string.Join(",", data) : null)})";

    private static object[] CreateArguments(object[][] valuesPerParameter, int[] parameterIndices)
    {
        var arg = new object[parameterIndices.Length];
        for (var i = 0; i < parameterIndices.Length; i++)
        {
            arg[i] = valuesPerParameter[i][parameterIndices[i]];
        }

        return arg;
    }
}
