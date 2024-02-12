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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DeadStoresTest
    {
        private readonly VerifierBuilder sonarCfg = new VerifierBuilder()
            .AddAnalyzer(() => new DeadStores(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .AddReferences(MetadataReferenceFacade.NetStandard21);

        private readonly VerifierBuilder roslynCfg = new VerifierBuilder<DeadStores>()
            .AddReferences(MetadataReferenceFacade.NetStandard21);

        [TestMethod]
        public void DeadStores_SonarCfg() =>
            sonarCfg.AddPaths("DeadStores.SonarCfg.cs").Verify();

        [TestMethod]
        public void DeadStores_RoslynCfg() =>
            roslynCfg.AddPaths("DeadStores.RoslynCfg.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void DeadStores_CSharp9() =>
            roslynCfg.AddPaths("DeadStores.CSharp9.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void DeadStores_CSharp10() =>
            roslynCfg.AddPaths("DeadStores.CSharp10.cs")
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void DeadStores_CSharp11() =>
            roslynCfg.AddPaths("DeadStores.CSharp11.cs")
                .WithTopLevelStatements()
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

    }
}
