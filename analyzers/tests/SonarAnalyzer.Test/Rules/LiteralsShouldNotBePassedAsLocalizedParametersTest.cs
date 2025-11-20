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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class LiteralsShouldNotBePassedAsLocalizedParametersTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<LiteralsShouldNotBePassedAsLocalizedParameters>().AddReferences(MetadataReferenceFacade.SystemComponentModelPrimitives);

        [TestMethod]
        public void LiteralsShouldNotBePassedAsLocalizedParameters() =>
            builder.AddPaths("LiteralsShouldNotBePassedAsLocalizedParameters.cs").Verify();

        [TestMethod]
        public void LiteralsShouldNotBePassedAsLocalizedParameters_CSharp9() =>
            builder.AddPaths("LiteralsShouldNotBePassedAsLocalizedParameters.CSharp9.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void LiteralsShouldNotBePassedAsLocalizedParameters_CSharp10() =>
            builder.AddPaths("LiteralsShouldNotBePassedAsLocalizedParameters.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void LiteralsShouldNotBePassedAsLocalizedParameters_CSharp11() =>
            builder.AddPaths("LiteralsShouldNotBePassedAsLocalizedParameters.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();
    }
}
