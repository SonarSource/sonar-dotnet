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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class LockReleasedAllPathsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Monitor_Linear_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Monitor.Linear.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Monitor_Conditions_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Monitor.Conditions.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Monitor_Conditions_CSharp8() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Monitor.Conditions.CSharp8.cs", new LockReleasedAllPaths(), ParseOptionsHelper.FromCSharp8);

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Monitor_TryCatch_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Monitor.TryCatch.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Monitor_Loops_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Monitor.Loops.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Monitor_TryEnter_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Monitor.TryEnter.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_SpinLock_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.SpinLock.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_ReaderWriterLock_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.ReaderWriterLock.cs", new LockReleasedAllPaths(), MetadataReferenceFacade.SystemThreading);

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_ReaderWriterLockSlim_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.ReaderWriterLockSlim.cs", new LockReleasedAllPaths());

        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Mutex_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Mutex.cs", new LockReleasedAllPaths(), ParseOptionsHelper.FromCSharp8);

#if NETFRAMEWORK
        [TestMethod]
        [TestCategory("Rule")]
        public void LockReleasedAllPaths_Mutex_NetFramework_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LockReleasedAllPaths.Mutex.NetFx.cs", new LockReleasedAllPaths(), ParseOptionsHelper.FromCSharp8);
#endif
    }
}
