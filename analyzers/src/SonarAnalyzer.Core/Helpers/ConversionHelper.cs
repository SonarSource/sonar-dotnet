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

namespace SonarAnalyzer.Helpers;

public static class ConversionHelper
{
    public static int? TryConvertToInt(object o) =>
        TryConvertWith(o, Convert.ToInt32, out var ret)
            ? (int?)ret
            : null;

    public static bool TryConvertWith<T>(object o, Func<object, T> converter, out T value)
        where T : struct
    {
        try
        {
            value = converter(o);
            return true;
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
        {
            value = default;
            return false;
        }
    }
}
