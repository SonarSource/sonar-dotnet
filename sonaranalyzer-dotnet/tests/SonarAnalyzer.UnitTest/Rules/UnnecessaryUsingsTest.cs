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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using CS = SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;
using Microsoft.CodeAnalysis.CSharp;
using FluentAssertions;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UnnecessaryUsingsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UnnecessaryUsings() =>
            Verifier.VerifyAnalyzer(@"TestCases\UnnecessaryUsings.cs",
                                    new CS.UnnecessaryUsings(),
                                    additionalReferences: GetAdditionalReferences());

#if NETCOREAPP

        [TestMethod]
        [TestCategory("Rule")]
        public void UnnecessaryUsings_TupleDeconstruct_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\UnnecessaryUsings.TupleDeconstruct.NetCore.cs", new CS.UnnecessaryUsings());

#elif NETFRAMEWORK

        [TestMethod]
        [TestCategory("Rule")]
        public void UnnecessaryUsings_TupleDeconstruct_NetFx() =>
            Verifier.VerifyAnalyzer(@"TestCases\UnnecessaryUsings.TupleDeconstruct.NetFx.cs", new CS.UnnecessaryUsings());

#endif

        [TestMethod]
        [TestCategory("CodeFix")]
        public void UnnecessaryUsings_CodeFix() =>
            Verifier.VerifyCodeFix(@"TestCases\UnnecessaryUsings.cs",
                                   @"TestCases\UnnecessaryUsings.Fixed.cs",
                                   @"TestCases\UnnecessaryUsings.Fixed.Batch.cs",
                                   new CS.UnnecessaryUsings(),
                                   new CS.UnnecessaryUsingsCodeFixProvider(),
                                   additionalReferences: GetAdditionalReferences());

        [TestMethod]
        public void EquivalentNameSyntax_Equals_Object()
        {
            var main = new CS.EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            object same = new CS.EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            object different = new CS.EquivalentNameSyntax(SyntaxFactory.IdentifierName("Ipsum"));

            main.Equals(same).Should().BeTrue();
            main.Equals(null).Should().BeFalse();
            main.Equals("different type").Should().BeFalse();
            main.Equals(different).Should().BeFalse();
        }

        [TestMethod]
        public void EquivalentNameSyntax_Equals_EquivalentNameSyntax()
        {
            var main = new CS.EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            var same = new CS.EquivalentNameSyntax(SyntaxFactory.IdentifierName("Lorem"));
            var different = new CS.EquivalentNameSyntax(SyntaxFactory.IdentifierName("Ipsum"));

            main.Equals(same).Should().BeTrue();
            main.Equals(null).Should().BeFalse();
            main.Equals(different).Should().BeFalse();
        }

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.GetMicrosoftWin32Primitives()
                                   .Union(MetadataReferenceFacade.GetSystemSecurityCryptography());
    }
}

