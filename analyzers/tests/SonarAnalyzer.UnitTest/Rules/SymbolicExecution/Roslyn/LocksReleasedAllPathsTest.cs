/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class LocksReleasedAllPathsTest
    {
        private readonly VerifierBuilder verifierBuilder = new VerifierBuilder<SymbolicExecutionRunner>().WithOnlyDiagnostics(LocksReleasedAllPaths.S2222).WithBasePath(@"SymbolicExecution\Roslyn");

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Conditions_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.Conditions.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Conditions_CSharp8() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.Conditions.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_GoTo_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.GoTo.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Linear_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.Linear.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Loops_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.Loops.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_TryCatch_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.TryCatch.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_TryEnter_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.TryEnter.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Mutex_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Mutex.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NETFRAMEWORK

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Mutex_NetFramework_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Mutex.NetFx.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_ReaderWriterLock_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.ReaderWriterLock.cs").AddReferences(MetadataReferenceFacade.SystemThreading).Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_ReaderWriterLockSlim_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.ReaderWriterLockSlim.cs").Verify();

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_SpinLock_CS() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.SpinLock.cs").Verify();
    }
}
