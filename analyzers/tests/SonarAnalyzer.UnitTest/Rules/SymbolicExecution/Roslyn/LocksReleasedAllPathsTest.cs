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
        private readonly VerifierBuilder verifierBuilder = new VerifierBuilder<SymbolicExecutionRunner>()
            .AddReferences(MetadataReferenceFacade.SystemThreading)
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .WithOnlyDiagnostics(LocksReleasedAllPaths.S2222)
            .WithBasePath(@"SymbolicExecution\Roslyn");

        [TestMethod]
        public void LocksReleasedAllPaths_CS() =>
            verifierBuilder.AddPaths(
                "LocksReleasedAllPaths.Monitor.Conditions.cs",
                "LocksReleasedAllPaths.Monitor.GoTo.cs",
                "LocksReleasedAllPaths.Monitor.Linear.cs",
                "LocksReleasedAllPaths.Monitor.Loops.cs",
                "LocksReleasedAllPaths.Monitor.TryCatch.cs",
                "LocksReleasedAllPaths.Monitor.TryEnter.cs",
                "LocksReleasedAllPaths.Mutex.cs",
                "LocksReleasedAllPaths.ReaderWriterLock.cs",
                "LocksReleasedAllPaths.ReaderWriterLockSlim.cs",
                "LocksReleasedAllPaths.SpinLock.cs")
                .Verify();

        [TestMethod]
        public void LocksReleasedAllPaths_CSharp8() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Monitor.Conditions.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NETFRAMEWORK

        [TestMethod]
        public void LocksReleasedAllPaths_CS_NetFx() =>
            verifierBuilder.AddPaths("LocksReleasedAllPaths.Mutex.NetFx.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#endif
    }
}
