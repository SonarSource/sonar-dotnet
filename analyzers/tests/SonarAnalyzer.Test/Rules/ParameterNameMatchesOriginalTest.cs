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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class ParameterNameMatchesOriginalTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ParameterNameMatchesOriginal>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ParameterNameMatchesOriginal>();

        [TestMethod]
        public void ParameterNameMatchesOriginal_CS() =>
            builderCS.AddPaths("ParameterNameMatchesOriginal.cs")
                .WithOptions(LanguageOptions.FromCSharp8)
                .AddReferences(MetadataReferenceFacade.NetStandard21)
                .Verify();

#if NET
        [TestMethod]
        public void ParameterNameMatchesOriginal_CS_Latest() =>
            builderCS
                .AddPaths("ParameterNameMatchesOriginal.Latest.cs")
                .AddPaths("ParameterNameMatchesOriginal.Latest.Partial.cs")
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();
#endif

        [TestMethod]
        public void ParameterNameMatchesOriginal_VB() =>
            builderVB.AddPaths("ParameterNameMatchesOriginal.vb").Verify();
    }
}
