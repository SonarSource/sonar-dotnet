/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

        [TestMethod]
        public void DangerousGetHandleShouldNotBeCalled_CS_TopLevelStatements() =>
            builderCS.AddPaths("DangerousGetHandleShouldNotBeCalled.TopLevelStatements.cs")
                .AddReferences(MetadataReferenceFacade.MicrosoftWin32Registry)
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void DangerousGetHandleShouldNotBeCalled_CS_Latest() =>
            builderCS.AddPaths("DangerousGetHandleShouldNotBeCalled.Latest.cs")
                .AddReferences(MetadataReferenceFacade.SystemDiagnosticsProcess)
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

        [TestMethod]
        public void DangerousGetHandleShouldNotBeCalled_VB() =>
            builderVB.AddPaths("DangerousGetHandleShouldNotBeCalled.vb")
                .Verify();
    }
}
