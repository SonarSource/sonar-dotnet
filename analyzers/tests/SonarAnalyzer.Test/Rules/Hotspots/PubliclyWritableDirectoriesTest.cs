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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class PubliclyWritableDirectoriesTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.PubliclyWritableDirectories(AnalyzerConfiguration.AlwaysEnabled));
    private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.PubliclyWritableDirectories(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void PubliclyWritableDirectories_CS() =>
        builderCS.AddPaths(@"Hotspots\PubliclyWritableDirectories.cs").Verify();

#if NET

    [TestMethod]
    public void PubliclyWritableDirectories_CS_Latest() =>
        builderCS.AddPaths(@"Hotspots\PubliclyWritableDirectories.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

    [TestMethod]
    public void PubliclyWritableDirectories_VB() =>
        builderVB.AddPaths(@"Hotspots\PubliclyWritableDirectories.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();
}
