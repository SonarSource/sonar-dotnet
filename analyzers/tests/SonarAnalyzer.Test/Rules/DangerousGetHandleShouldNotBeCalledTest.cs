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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DangerousGetHandleShouldNotBeCalledTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.DangerousGetHandleShouldNotBeCalled>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.DangerousGetHandleShouldNotBeCalled>();

        [TestMethod]
        public void DangerousGetHandleShouldNotBeCalled_CS() =>
            builderCS.AddPaths("DangerousGetHandleShouldNotBeCalled.cs")
                .AddReferences(MetadataReferenceFacade.MicrosoftWin32Registry)
                .Verify();

#if NET

        [TestMethod]
        public void DangerousGetHandleShouldNotBeCalled_CS_CSharp9() =>
            builderCS.AddPaths("DangerousGetHandleShouldNotBeCalled.CSharp9.cs")
                .AddReferences(MetadataReferenceFacade.MicrosoftWin32Registry)
                .WithOptions(LanguageOptions.FromCSharp9)
                .WithTopLevelStatements()
                .Verify();

#endif

        [TestMethod]
        public void DangerousGetHandleShouldNotBeCalled_VB() =>
            builderVB.AddPaths("DangerousGetHandleShouldNotBeCalled.vb")
                .Verify();
    }
}
