/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantCastTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void RedundantCast()
        {
            Verifier.VerifyAnalyzer(
                @"TestCases\RedundantCast.cs",
                new RedundantCast());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void RedundantCast_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\RedundantCast.cs",
                @"TestCases\RedundantCast.Fixed.cs",
                new RedundantCast(),
                new RedundantCastCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void RedundantCast_DefaultLiteral()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public static class MyClass
{
    public static void RunAction(Action action)
    {
        bool myBool = (bool)default; // FN - the cast is unneeded
        RunFunc(() => { action(); return default; }, (bool)default); // should not raise because of the generic the cast is mandatory
        RunFunc<bool>(() => { action(); return default; }, (bool)default); // FN - the cast is unneeded
    }

     public static T RunFunc<T>(Func<T> func, T returnValue = default) => returnValue;
}",
                new RedundantCast(),
                new[] { new CSharpParseOptions(LanguageVersion.CSharp7_1) });
        }
    }
}
