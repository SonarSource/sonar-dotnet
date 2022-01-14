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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RedundantCastTest
    {
        [TestMethod]
        public void RedundantCast() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\RedundantCast.cs",
                new RedundantCast());

        [TestMethod]
        public void RedundantCast_CSharp8() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\RedundantCast.CSharp8.cs",
                new RedundantCast(),
                ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        public void RedundantCast_CSharp9() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\RedundantCast.CSharp9.cs",
                new RedundantCast(),
                ParseOptionsHelper.FromCSharp9);
#endif

        [TestMethod]
        public void RedundantCast_CodeFix() =>
            OldVerifier.VerifyCodeFix(
                @"TestCases\RedundantCast.cs",
                @"TestCases\RedundantCast.Fixed.cs",
                new RedundantCast(),
                new RedundantCastCodeFixProvider());

        [TestMethod]
        public void RedundantCast_DefaultLiteral() =>
            OldVerifier.VerifyCSharpAnalyzer(@"
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
                new RedundantCast(), ImmutableArray.Create<ParseOptions>(new CSharpParseOptions(LanguageVersion.CSharp7_1)));  //ToDo: Use WithLanguageVersion instead
    }
}
