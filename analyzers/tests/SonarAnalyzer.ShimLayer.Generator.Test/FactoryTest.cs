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

using VerifyMSTest;
using VerifyTests;

namespace SonarAnalyzer.ShimLayer.Generator.Test;

[TestClass]
[UsesVerify]
public partial class FactoryTest
{
    [TestMethod]
    public async Task SnapshotAsync() =>
        // If this test fails in CI, execute it locally to update the snapshots and push the changes.
        await Verifier.Verify(Factory.CreateAllFiles().Select(x => new Target("cs", x.Content, x.Name)))
            .UseDirectory("Snapshots")
            .AutoVerify(includeBuildServer: false)
            .UseFileName("Snap");

    [TestMethod]
    public async Task Run() =>
        await VerifyChecks.Run();
}
