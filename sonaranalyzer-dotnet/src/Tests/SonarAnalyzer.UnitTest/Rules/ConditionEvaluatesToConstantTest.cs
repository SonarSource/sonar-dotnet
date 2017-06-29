/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ConditionEvaluatesToConstantTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            bool? x = null;
            object o = new object();
            if ((object)x == o)
            {
            }
            else
            {
                if (x == null) // this can be true here if x is null, but is recognized as always false in the SE
                {
                }
            }

            Verifier.VerifyCSharpAnalyzer(@"
class Foo
{
    public void Bar(bool? x)
    {
        object o = new object();
        if ((object)x == o)
        {
        }
        else
        {
            if (x == null) // SE says this is always false
            {
            }
        }
    }
}
", new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ConditionEvaluatesToConstant.cs", new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }
    }
}
