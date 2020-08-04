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

        [TestMethod]
        [TestCategory("Rule")]
        public void UnnecessaryUsings_TupleDeconstruct()
        {
            const string code = @"
namespace Repro_3408_Consumer
{
    using System; // Noncompliant - FP, See: https://github.com/SonarSource/sonar-dotnet/issues/3408

    public class Repro3408
    {
        public void Consumer()
        {
            var (_, y) = Repro_3408_Provider.ServiceReturningTuples.GetPair();
        }
    }
}

namespace Repro_3408_Provider
{
    using System;

    public static class ServiceReturningTuples
    {
        public static Tuple<string, int> GetPair() => Tuple.Create(""a"", 1);
        }
    }
";
            Verifier.VerifyCSharpAnalyzer(code, new CS.UnnecessaryUsings());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void UnnecessaryUsings_CodeFix() =>
            Verifier.VerifyCodeFix(@"TestCases\UnnecessaryUsings.cs",
                                   @"TestCases\UnnecessaryUsings.Fixed.cs",
                                   @"TestCases\UnnecessaryUsings.Fixed.Batch.cs",
                                   new CS.UnnecessaryUsings(),
                                   new CS.UnnecessaryUsingsCodeFixProvider(),
                                   additionalReferences: GetAdditionalReferences());

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.GetMicrosoftWin32Primitives()
                                   .Union(MetadataReferenceFacade.GetSystemSecurityCryptography());
    }
}

