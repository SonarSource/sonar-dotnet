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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class VisualIndentHelperTest
    {
        [TestMethod]
        public void TestVisualIndent_NonTabsOnly()
        {
            VisualIndentComparer.IsSecondIndentLonger("", "").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("", "@").Should().Be(true);
            VisualIndentComparer.IsSecondIndentLonger("123", "AB").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("123", "ABC").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("123", "ABCD").Should().Be(true);
        }

        [TestMethod]
        public void TestVisualIndent_TabsOnly()
        {
            VisualIndentComparer.IsSecondIndentLonger("\t\t\t", "\t\t").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("\t\t\t", "\t").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("\t", "\t\t").Should().Be(true);
            VisualIndentComparer.IsSecondIndentLonger("\t", "\t").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("", "\t").Should().Be(true);
        }

        [TestMethod]
        public void TestVisualIndent_Mix_ResultIsCertain()
        {
            // More tabs and same chars -> certain of outcome
            VisualIndentComparer.IsSecondIndentLonger("\t\t123", "\tABCD").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("\tABCD", "\t\t123").Should().Be(true);

            // More tabs and more chars -> certain of outcome
            VisualIndentComparer.IsSecondIndentLonger("\t\t123", "\t12").Should().Be(false);
            VisualIndentComparer.IsSecondIndentLonger("\t12", "\t\t123").Should().Be(true);
        }

        [TestMethod]
        public void TestVisualIndent_Mix_ResultIsUncertain()
        {
            // More tabs but fewer characters -> depends on tab spacing
            // -> error on the side of caution and return false
            VisualIndentComparer.IsSecondIndentLonger("\t\t", "          ").Should().Be(true);
            VisualIndentComparer.IsSecondIndentLonger("          ", "\t\t").Should().Be(true);

            VisualIndentComparer.IsSecondIndentLonger("\t\t\t", "\t\t12").Should().Be(true);
            VisualIndentComparer.IsSecondIndentLonger("\t\t12", "\t\t\t").Should().Be(true);

            // Example ITs: sources\Nancy\src\Nancy\Json\JsonDeserializer.cs #389,390, same pattern in #620,621
            // "			} else if (... )"
            // "				buffer.Append (ch);"
            VisualIndentComparer.IsSecondIndentLonger("\t\t\t} ", "\t\t\t\t").Should().Be(true);
            VisualIndentComparer.IsSecondIndentLonger("\t\t\t\t", "\t\t\t} ").Should().Be(true);

            // Example ITs: "sources\Nancy\src\Nancy\TinyIoc\TinyIoC.cs, #1448,1450
            // "                if (!registrationType.IsAssignableFrom(type))"
            // "#endif"  <-- not part of syntax tree
            // "					throw new ArgumentException(String.Format("types: The type {0} is not assignable from {1}", registrationType.FullName, type.FullName));"
            VisualIndentComparer.IsSecondIndentLonger("                ", "\t\t\t\t\t").Should().Be(true);
            VisualIndentComparer.IsSecondIndentLonger("\t\t\t\t", "                ").Should().Be(true);
        }
    }
}
