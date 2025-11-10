/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Common;

public static class Conversions
{
    public static int? ToInt(object value) =>
        ConvertWith(value, Convert.ToInt32);

    public static double? ToDouble(object value)
    {
        if (value is char)  // 'char' needs to roundtrip char -> int -> double, can't go char -> double
        {
            value = Convert.ToInt32(value);
        }
        return ConvertWith(value, Convert.ToDouble) is { } doubleValue && !double.IsInfinity(doubleValue)
            ? doubleValue
            : null;
    }

    public static T? ConvertWith<T>(object value, Func<object, T> converter) where T : struct
    {
        try
        {
            return converter(value);
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
        {
            return null;
        }
    }
}
