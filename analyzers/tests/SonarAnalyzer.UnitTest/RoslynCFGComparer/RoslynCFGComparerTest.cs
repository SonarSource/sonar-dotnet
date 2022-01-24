extern alias csharp;

using System.Linq;
/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.UnitTest.TestFramework;
using CS=SonarAnalyzer.Rules.CSharp;
using VB=SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RoslynCfgComparerTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("RoslynCFGComparer").WithConcurrentAnalysis(false);

        [TestMethod]
        [DataRow("AnonymousFunctions.cs")]
        [DataRow("LocalFunctions.cs")]
        [DataRow("Branching.cs")]
        [DataRow("Loop.cs")]
        [DataRow("Nested.cs")]
        [DataRow("PatternMatching.cs")]
        [DataRow("Simple.cs")]
        [DataRow("TryCatch.cs")]
        public void RoslynCfgComparer_RenderCfgs_CS(string filename) =>
            builder.AddAnalyzer(() => new CS.RoslynCfgComparer()).AddPaths(filename).WithOptions(ParseOptionsHelper.CSharpLatest).Verify();

        [TestMethod]
        [DataRow("Branching.vb")]
        [DataRow("TryCatch.vb")]
        public void RoslynCfgComparer_RenderCfgs_VB(string filename) =>
            builder.AddAnalyzer(() => new VB.RoslynCfgComparer()).AddPaths(filename).WithOptions(ParseOptionsHelper.VisualBasicLatest).Verify();
    }
}
