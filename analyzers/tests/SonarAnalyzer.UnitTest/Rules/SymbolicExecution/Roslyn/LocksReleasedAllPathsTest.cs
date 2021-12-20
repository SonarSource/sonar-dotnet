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

using Microsoft.CodeAnalysis;
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
        private static readonly DiagnosticDescriptor[] OnlyDiagnostics = new[] { LocksReleasedAllPaths.S2222 };

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Conditions_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.Conditions.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Conditions_CSharp8() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.Conditions.CSharp8.cs", new SymbolicExecutionRunner(), ParseOptionsHelper.FromCSharp8, onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_GoTo_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.GoTo.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Linear_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.Linear.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_Loops_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.Loops.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_TryCatch_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.TryCatch.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Monitor_TryEnter_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Monitor.TryEnter.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Mutex_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Mutex.cs", new SymbolicExecutionRunner(), ParseOptionsHelper.FromCSharp8, onlyDiagnostics: OnlyDiagnostics);

#if NETFRAMEWORK
        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_Mutex_NetFramework_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.Mutex.NetFx.cs", new SymbolicExecutionRunner(), ParseOptionsHelper.FromCSharp8, onlyDiagnostics: OnlyDiagnostics);
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_ReaderWriterLock_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.ReaderWriterLock.cs", new SymbolicExecutionRunner(), MetadataReferenceFacade.SystemThreading, onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_ReaderWriterLockSlim_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.ReaderWriterLockSlim.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksReleasedAllPaths_SpinLock_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Roslyn\LocksReleasedAllPaths.SpinLock.cs", new SymbolicExecutionRunner(), onlyDiagnostics: OnlyDiagnostics);
    }
}
