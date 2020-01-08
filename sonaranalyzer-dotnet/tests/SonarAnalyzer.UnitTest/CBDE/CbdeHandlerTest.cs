extern alias csharp;

using System.IO;
using System.Text.RegularExpressions;
/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2019 SonarSource SA
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

using csharp::SonarAnalyzer.CBDE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CbdeHandlerTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void CbdeHandler_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CbdeHandler.cs",
                CS.CbdeHandlerRule.MakeUnitTestInstance(CbdeHandler.MockType.NoMock, null));
        }

        [TestMethod]
        [TestCategory("CBDE")]
        public void CbdeHandlerWait()
        {
            bool cbdeExecuted = false;
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\CbdeHandlerDummy.cs",
                CS.CbdeHandlerRule.MakeUnitTestInstance(CbdeHandler.MockType.CBDEWaitAndSucceeds,
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("Running CBDE: Success"));
                    var workingSet = Regex.Match(logContent, "peak_working_set: ([0-9]*) MB");
                    Assert.IsTrue(workingSet.Success);
                    var peak = workingSet.Groups[1].Value;
                    Assert.IsTrue(int.TryParse(peak, out int peakValue));
                    Assert.AreNotEqual(peakValue, 0); // We had enough time to at least use some memory
                }
                ));
            Assert.IsTrue(cbdeExecuted);
        }

        [TestMethod]
        [TestCategory("CBDE")]
        public void CbdeHandlerExecutableNotFound()
        {
            bool cbdeExecuted = false;
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\CbdeHandlerDummy.cs",
                CS.CbdeHandlerRule.MakeUnitTestInstance(CbdeHandler.MockType.NonExistingExecutable,
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("Running CBDE: Cannot start process"));
                }
                ));
            Assert.IsTrue(cbdeExecuted);
        }

        [TestMethod]
        [TestCategory("CBDE")]
        public void CbdeHandlerFailed()
        {
            bool cbdeExecuted = false;
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\CbdeHandlerDummy.cs",
                CS.CbdeHandlerRule.MakeUnitTestInstance(CbdeHandler.MockType.CBDEFails,
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("Running CBDE: Failure"));
                }
                ));
            Assert.IsTrue(cbdeExecuted);
        }

        [TestMethod]
        [TestCategory("CBDE")]
        public void CbdeHandlerIncorrectOutput()
        {
            bool cbdeExecuted = false;
            Verifier.VerifyNoIssueReportedInTest(@"TestCases\CbdeHandlerDummy.cs",
                CS.CbdeHandlerRule.MakeUnitTestInstance(CbdeHandler.MockType.CBDESucceedsWithIncorrectResults,
                s =>
                {
                    cbdeExecuted = true;
                    var logContent = File.ReadAllText(s);
                    Assert.IsTrue(logContent.Contains("error parsing result file"));
                }
                ));
            Assert.IsTrue(cbdeExecuted);
        }    }

}
