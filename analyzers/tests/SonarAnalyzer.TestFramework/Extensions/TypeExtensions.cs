/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.TestFramework.Extensions;

public static class TypeExtensions
{
    public static AnalyzerLanguage AnalyzerTargetLanguage(this Type analyzerType)
    {
        var languages = analyzerType.GetCustomAttributes<DiagnosticAnalyzerAttribute>().SingleOrDefault()?.Languages
                        ?? throw new NotSupportedException($"Can not find any language for the given type {analyzerType.Name}!");
        return languages.Length == 1
            ? AnalyzerLanguage.FromName(languages.Single())
            : throw new NotSupportedException($"Analyzer can not have multiple languages: {analyzerType.Name}");
    }
}
