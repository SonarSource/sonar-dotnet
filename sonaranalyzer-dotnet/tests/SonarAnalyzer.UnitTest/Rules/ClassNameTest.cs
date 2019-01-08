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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ClassNameTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_CSharp()
        {
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\ClassName.cs",
                    @"TestCases\ClassName.Partial.cs",
                }, new SonarAnalyzer.Rules.CSharp.ClassAndMethodName());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ClassName.vb", new SonarAnalyzer.Rules.VisualBasic.ClassName());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodName()
        {
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\MethodName.cs",
                    @"TestCases\MethodName.Partial.cs",
                },
                new SonarAnalyzer.Rules.CSharp.ClassAndMethodName());
        }

        [TestMethod]
        public void TestSplitToParts()
        {
            new[]
            {
                ("foo", new [] { "foo" }),
                ("Foo", new [] { "Foo" }),
                ("FFF", new [] { "FFF" }),
                ("FfF", new [] { "Ff", "F" }),
                ("Ff9F", new [] { "Ff", "9", "F" }),
                ("你好", new [] { "你", "好" }),
                ("FFf", new [] { "F", "Ff" }),
                ("", new string[0]),
                ("FF9d", new [] { "FF", "9", "d" }),
                ("y2x5__w7", new[] { "y", "2", "x", "5", "_", "_", "w", "7" }),
                ("3%c#account", new[] { "3", "%", "c", "#", "account" }),
            }
            .Select(x =>
            (
                actual: SonarAnalyzer.Rules.CSharp.ClassAndMethodName.SplitToParts(x.Item1).ToArray(),
                expected: x.Item2
            ))
            .ToList()
            .ForEach(x => x.actual.Should().Equal(x.expected));
        }
    }
}
