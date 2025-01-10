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

namespace SonarAnalyzer.CFG.Common;

public static class RoslynVersion
{
    public const int VS2017MajorVersion = 2;
    public const int MinimalSupportedMajorVersion = 3;

    public static bool IsRoslynCfgSupported(int minimalVersion = MinimalSupportedMajorVersion) =>
        !IsVersionLessThan(minimalVersion);

    public static bool IsVersionLessThan(int minimalVersion = MinimalSupportedMajorVersion) =>
        CurrentVersion().Major < minimalVersion;

    public static bool IsVersionLessThan(Version version) =>
        CurrentVersion() < version;

    private static Version CurrentVersion() =>
        typeof(SemanticModel).Assembly.GetName().Version;
}
