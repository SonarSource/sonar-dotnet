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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MarkWindowsFormsMainWithStaThreadTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MarkWindowsFormsMainWithStaThread>().WithConcurrentAnalysis(false);
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MarkWindowsFormsMainWithStaThread>().WithConcurrentAnalysis(false);

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_CS() =>
            builderCS
                .AddPaths("MarkWindowsFormsMainWithStaThread.cs")
                .WithOutputKind(OutputKind.WindowsApplication)
                .AddReferences(MetadataReferenceFacade.SystemWindowsForms)
                .Verify();

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_VB() =>
            builderVB
                .AddPaths("MarkWindowsFormsMainWithStaThread.vb")
                .WithOutputKind(OutputKind.WindowsApplication)
                .AddReferences(MetadataReferenceFacade.SystemWindowsForms)
                .Verify();

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_ClassLibrary_CS() =>
            builderCS
                .AddPaths("MarkWindowsFormsMainWithStaThread.cs")
                .WithErrorBehavior(CompilationErrorBehavior.Ignore)
                .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                .AddReferences(MetadataReferenceFacade.SystemWindowsForms)
                .VerifyNoIssues();

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_ClassLibrary_VB() =>
            builderVB
                .AddPaths("MarkWindowsFormsMainWithStaThread.vb")
                .WithOutputKind(OutputKind.DynamicallyLinkedLibrary)
                .AddReferences(MetadataReferenceFacade.SystemWindowsForms)
                .VerifyNoIssuesIgnoreErrors();

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_CS_NoWindowsForms() =>
            builderCS
                .AddPaths("MarkWindowsFormsMainWithStaThread_NoWindowsForms.cs")
                .WithOutputKind(OutputKind.WindowsApplication)
                .Verify();

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_VB_NoWindowsForms() =>
            builderVB
                .AddPaths("MarkWindowsFormsMainWithStaThread_NoWindowsForms.vb")
                .WithErrorBehavior(CompilationErrorBehavior.Ignore)
                .WithOutputKind(OutputKind.WindowsApplication)
                .Verify();
    }
}
