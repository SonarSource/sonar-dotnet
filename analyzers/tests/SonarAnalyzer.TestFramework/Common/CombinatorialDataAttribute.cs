/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
public sealed class CombinatorialDataAttribute : TestMethodAttribute, ITestDataSource
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

    private static object[] CreateArguments(object[][] valuesPerParameter, int[] parameterIndices)
    {
        var arg = new object[parameterIndices.Length];
        for (var i = 0; i < parameterIndices.Length; i++)
        {
            arg[i] = valuesPerParameter[i][parameterIndices[i]];
        }

        return arg;
    }

    public string GetDisplayName(MethodInfo methodInfo, object[] data) =>
        $"{methodInfo.Name} ({(data is not null ? string.Join(",", data) : null)})";
}
