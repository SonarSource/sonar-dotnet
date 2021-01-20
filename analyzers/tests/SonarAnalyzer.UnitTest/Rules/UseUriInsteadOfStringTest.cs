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

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UseUriInsteadOfStringTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UseUriInsteadOfString() =>
            Verifier.VerifyAnalyzer(@"TestCases\UseUriInsteadOfString.cs",
                new CS.UseUriInsteadOfString(),
                MetadataReferenceFacade.SystemDrawing);

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void UseUriInsteadOfString_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\UseUriInsteadOfString.CSharp9.cs",
                new CS.UseUriInsteadOfString(),
                MetadataReferenceFacade.SystemDrawing);
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void UseUriInsteadOfString_InvalidCode() =>
            Verifier.VerifyCSharpAnalyzer(@"
public class Foo
{
}

public class Bar : Foo
{
    public override string UriProperty { get; set; }
    public override string UriMethod() => "";

    public void Main()
    {
        Uri.TryCreate(new object(), UriKind.Absolute, out result); // Compliant - invalid code
    }
}", new CS.UseUriInsteadOfString(), CompilationErrorBehavior.Ignore);
    }
}
