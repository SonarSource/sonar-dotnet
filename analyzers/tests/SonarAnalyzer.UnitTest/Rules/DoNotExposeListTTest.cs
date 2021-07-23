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
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DoNotExposeListTTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotExposeListT() =>
            Verifier.VerifyConcurrentAnalyzer(@"TestCases\DoNotExposeListT.cs",
                                              new DoNotExposeListT(),
                                              MetadataReferences.MetadataReferenceFacade.SystemXml);

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotExposeListT_CSharp8() =>
            Verifier.VerifyAnalyzer(@"TestCases\DoNotExposeListT.CSharp8.cs", new DoNotExposeListT(), ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotExposeListT_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\DoNotExposeListT.CSharp9.cs", new DoNotExposeListT());
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotExposeListT_InvalidCode()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class InvalidCode
{
    public List<int> () => null;

    public List<T> { get; set; }

    public List<InvalidType> Method() => null;

    public InvalidType Method2() => null;
}", new DoNotExposeListT(), CompilationErrorBehavior.Ignore);
        }
    }
}
