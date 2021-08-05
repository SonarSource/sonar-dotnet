/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MarkWindowsFormsMainWithStaThreadTest
    {
// ToDo: Add support for windows forms on .Net Core
// https://github.com/SonarSource/sonar-dotnet/issues/3426
#if NETFRAMEWORK
        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\MarkWindowsFormsMainWithStaThread.cs",
                                    new CS.MarkWindowsFormsMainWithStaThread(),
                                    FrameworkMetadataReference.SystemWindowsForms);

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\MarkWindowsFormsMainWithStaThread.vb",
                                    new VB.MarkWindowsFormsMainWithStaThread(),
                                    FrameworkMetadataReference.SystemWindowsForms);
#endif

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_CS_NoWindowsForms() =>
            Verifier.VerifyAnalyzer(@"TestCases\MarkWindowsFormsMainWithStaThread_NoWindowsForms.cs",
                                    new CS.MarkWindowsFormsMainWithStaThread());

        [TestMethod]
        public void MarkWindowsFormsMainWithStaThread_VB_NoWindowsForms() =>
            Verifier.VerifyAnalyzer(@"TestCases\MarkWindowsFormsMainWithStaThread_NoWindowsForms.vb",
                                    new VB.MarkWindowsFormsMainWithStaThread(),
                                    CompilationErrorBehavior.Ignore);
    }
}
