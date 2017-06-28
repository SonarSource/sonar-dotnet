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
        [TestCategory("Rule")]
        public void ConditionEvaluatesToConstant()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ConditionEvaluatesToConstant.cs", new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case1()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case1()
    {
        bool? b = true;
        if (b == true) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
        {

        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case2()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case2()
    {
        bool? b = true;
        if (b == false) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
        { // Secondary
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case3()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case3(bool? b)
    {
        if (b == null)
        {
            if (null == b) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            {
                b.ToString();
            }
        }
        else
        {
            if (b != null) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            {
                b.ToString();
            }
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case4()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case4(bool? b)
    {
        if (b == true)
        {
            if (true == b) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            {
                b.ToString();
            }
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case5()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case5(bool? b)
    {
        if (b == true)
        {
        }
        else if (b == false)
        {
        }
        else // Compliant
        {
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case6()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case6(bool? b)
    {
        if (b == null)
        {
        }
        else if (b == true)
        {
        }
        else
        {
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case7()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case7(bool? b)
    {
        if (b == null)
        {
            if (b ?? false) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary

            }
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case8()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case8(bool? b)
    {
        if (b != null)
        {
            if (b.HasValue) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            {
            }
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case9()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case9(bool? b)
    {
        if (b == true)
        {
            var x = b.Value;
            if (x == true) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            {
            }
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void Case10()
        {
            Verifier.VerifyCSharpAnalyzer(@"class Foo
{
    void Case10(int? i)
    {
        if (i == null)
        {
            if (i.HasValue) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }
        }
    }
}",
                new ConditionEvaluatesToConstant(),
                new CSharpParseOptions(LanguageVersion.CSharp6));
        }
    }
}
