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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class NullPointerDereferenceTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_ValidatedNotNull()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;

public sealed class ValidatedNotNullAttribute : Attribute { }

public static class Guard
{
    public static void NotNull<T>([ValidatedNotNullAttribute] this T value, string name) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(name);
    }
}

public static class Utils
{
    public static string ToUpper(string value)
    {
        Guard.NotNull(value, nameof(value));
        if (value != null)
        {
            return value.ToUpper(); // Compliant
        }
        return value.ToUpper(); // Compliant
    }
}
", new NullPointerDereference());
        }
        //FIXME: Temporary silence for CFG defork
        [Ignore("Temporary disabled for CFG defork")]
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereference.cs",
                new NullPointerDereference());
        }

        //FIXME: Temporary silence for CFG defork
        [Ignore("Temporary disabled for CFG defork")]
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_CSharp6()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereferenceCSharp6.cs",
                new NullPointerDereference(),
                ParseOptionsHelper.FromCSharp6);
        }

        //FIXME: Temporary silence for CFG defork
        [Ignore("Temporary disabled for CFG defork")]
        [TestMethod]
        [TestCategory("Rule")]
        public void NullPointerDereference_CSharp7()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NullPointerDereferenceCSharp7.cs",
                new NullPointerDereference(),
                ParseOptionsHelper.FromCSharp7);
        }
    }
}
