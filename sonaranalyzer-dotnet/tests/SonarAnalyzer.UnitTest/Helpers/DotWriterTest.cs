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

using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.Helpers.UnitTest
{
    [TestClass]
    public class DotWriterTest
    {
        [TestMethod]
        public void WriteGraphStart_Should_Write_Name()
        {
            var stringBuilder = new StringBuilder();
            var writer = new DotWriter(new StringWriter(stringBuilder));

            writer.WriteGraphStart("test");

            stringBuilder.ToString().Should().BeIgnoringLineEndings("digraph \"test\" {\r\n");
        }

        [TestMethod]
        public void WriteGraphEnd_Test()
        {
            var stringBuilder = new StringBuilder();
            var writer = new DotWriter(new StringWriter(stringBuilder));

            writer.WriteGraphEnd();

            stringBuilder.ToString().Should().BeIgnoringLineEndings("}\r\n");
        }

        [TestMethod]
        public void WriteNode_With_Items()
        {
            var stringBuilder = new StringBuilder();
            var writer = new DotWriter(new StringWriter(stringBuilder));

            writer.WriteNode("1", "header", "a", "b", "c");

            stringBuilder.ToString().Should().BeIgnoringLineEndings("1 [shape=record label=\"{header|a|b|c}\"]\r\n");
        }

        [TestMethod]
        public void WriteNode_With_Encoding()
        {
            var stringBuilder = new StringBuilder();
            var writer = new DotWriter(new StringWriter(stringBuilder));

            writer.WriteNode("1", "header", "\r", "\n", "{", "}", "<", ">", "|", "\"");

            stringBuilder.ToString().Should().BeIgnoringLineEndings(@"1 [shape=record label=""{header||\n|\{|\}|\<|\>|\||\""}""]" + "\r\n");
        }

        [TestMethod]
        public void WriteNode_No_Items()
        {
            var stringBuilder = new StringBuilder();
            var writer = new DotWriter(new StringWriter(stringBuilder));

            writer.WriteNode("1", "header");

            stringBuilder.ToString().Should().BeIgnoringLineEndings("1 [shape=record label=\"{header}\"]\r\n");
        }
    }
}
