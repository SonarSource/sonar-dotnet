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
    public class LocksShouldBeReleasedTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_Monitor_Linear_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.Monitor.Linear.cs", new LocksShouldBeReleased());

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_Monitor_Conditions_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.Monitor.Conditions.cs", new LocksShouldBeReleased());

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_Monitor_Conditions_CSharp8() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.Monitor.Conditions.CSharp8.cs", new LocksShouldBeReleased(), ParseOptionsHelper.FromCSharp8);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_Monitor_TryCatch_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.Monitor.TryCatch.cs", new LocksShouldBeReleased());

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_Monitor_Loops_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.Monitor.Loops.cs", new LocksShouldBeReleased());

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_Monitor_TryEnter_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.Monitor.TryEnter.cs", new LocksShouldBeReleased());

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_SpinLock_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.SpinLock.cs", new LocksShouldBeReleased());

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_ReaderWriterLock_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.ReaderWriterLock.cs", new LocksShouldBeReleased(), MetadataReferenceFacade.SystemThreading);

        [TestMethod]
        [TestCategory("Rule")]
        public void LocksShouldBeReleased_ReaderWriterLockSlim_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\SymbolicExecution\Sonar\LocksShouldBeReleased.ReaderWriterLockSlim.cs", new LocksShouldBeReleased());
    }
}
