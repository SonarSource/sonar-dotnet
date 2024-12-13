/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DeadStoresTest
    {
        private readonly VerifierBuilder sonarCfg = new VerifierBuilder()
            .AddAnalyzer(() => new DeadStores(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
            .WithOptions(LanguageOptions.FromCSharp8)
            .AddReferences(MetadataReferenceFacade.NetStandard21);

        private readonly VerifierBuilder roslynCfg = new VerifierBuilder<DeadStores>()
            .AddReferences(MetadataReferenceFacade.NetStandard21);

        [TestMethod]
        public void DeadStores_SonarCfg() =>
            sonarCfg.AddPaths("DeadStores.SonarCfg.cs").Verify();

        [TestMethod]
        public void DeadStores_RoslynCfg() =>
            roslynCfg.AddPaths("DeadStores.RoslynCfg.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void DeadStores_CS_Latest() =>
            roslynCfg.AddPaths("DeadStores.Latest.cs")
                .WithTopLevelStatements()
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

    }
}
