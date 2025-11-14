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

namespace SonarAnalyzer.TestFramework.MetadataReferences;

public static class NugetPackageVersions
{
    public const string Latest = TestConstants.NuGetLatestVersion;

    public static class FluentAssertionsVersions
    {
        public const string Ver1 = "1.6.0";
        public const string Ver4 = "4.19.4";
        public const string Ver5 = "5.9.0";
    }

    public static class MsTest
    {
        public const string Ver1_1 = "1.1.11";
        public const string Ver1_2 = "1.2.0";
        public const string Ver3 = "3.11.0";
    }

    public static class NUnit
    {
        public const string Ver3 = "3.11.0";
        public const string Ver3Latest = "3.14.0";
        public const string Ver25 = "2.5.7.10213";
        public const string Ver27 = "2.7.0";
    }
}
