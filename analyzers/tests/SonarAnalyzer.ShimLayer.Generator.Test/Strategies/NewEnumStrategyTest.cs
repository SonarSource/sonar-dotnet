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

using SonarAnalyzer.TestFramework.Extensions;

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class NewEnumStrategyTest
{
    [TestMethod]
    public void Generate()
    {
        using var typeLoader = new TypeLoader();
        var type = typeLoader.LoadLatest().Single(x => x.Type.Name == nameof(IncrementalGeneratorOutputKind));
        var sut = new NewEnumStrategy(type.Type, type.Members.OfType<FieldInfo>().Where(x => x.Name != "value__").ToArray());
        sut.Generate([]).Should().BeIgnoringLineEndings("""
            namespace SonarAnalyzer.ShimLayer;

            [System.FlagsAttribute]
            public enum IncrementalGeneratorOutputKind : System.Int32
            {
                None = 0,
                Source = 1,
                PostInit = 2,
                Implementation = 4,
                Host = 8,
            }

            """);
    }
}
