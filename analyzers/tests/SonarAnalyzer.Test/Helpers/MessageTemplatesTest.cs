/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class MessageTemplatesTest
{
    [DataTestMethod]
    [DataRow("")]
    [DataRow("{")]
    [DataRow("{{}}")]
    [DataRow("{{")]
    [DataRow("}}")]
    [DataRow("}}{{{{")]
    [DataRow("hello world")]
    [DataRow("hello {{world}}")]
    [DataRow("{{hello {{world}}}}")]
    public void Parse_NoPlaceholder(string template)
    {
        var result = MessageTemplates.Parse(template);
        ShouldBeSuccess(result);
    }

    [DataTestMethod]
    // named
    [DataRow("{world}", "world", 1, 5)]
    [DataRow("hello {world}", "world", 7, 5)]
    [DataRow("hello {{{world42}}}", "world42", 9, 7)]
    [DataRow("hello {{ {42world}", "42world", 10, 7)]
    [DataRow("hello {{ {w_0_rld}", "w_0_rld", 10, 7)]
    [DataRow("hello {{ {_}", "_", 10, 1)]
    [DataRow("hello {{ {_world}", "_world", 10, 6)]
    // index
    [DataRow("{0}", "0", 1, 1)]
    [DataRow("hello {0}", "0", 7, 1)]
    [DataRow("hello {{{42}}}", "42", 9, 2)]
    [DataRow("hello {{ {199}", "199", 10, 3)]
    // prefix optional operator
    [DataRow("{@world}", "world", 2, 5)]
    [DataRow("{$199}", "199", 2, 3)]
    [DataRow("{@0}", "0", 2, 1)]
    [DataRow("{$world_42}", "world_42", 2, 8)]
    public void Parse_Placeholder(string template, string placeholder, int start, int end)
    {
        var result = MessageTemplates.Parse(template);
        ShouldBeSuccess(result, 1);
        ShouldBe(result.Placeholders[0], placeholder, start, end);
    }

    [DataTestMethod]
    // alignment
    [DataRow("hello {world,1}")]
    [DataRow("hello {world,42}")]
    [DataRow("hello {world,-1}")]
    [DataRow("hello {world,-199}")]
    // format
    [DataRow("hello {world:format}")]
    [DataRow("hello {world:42}")]
    [DataRow("hello {world:{}")]
    [DataRow("hello {world:!@#$%^&*()_}")]
    [DataRow("hello {world:42}")]
    [DataRow("hello {world:#for_mat42}")]
    // mixed
    [DataRow("hello {world,1:format}")]
    [DataRow("hello {world,42:!@#$%}")]
    [DataRow("hello {world,-1:dd-MM-yyy}")]
    [DataRow("hello {world,-42:3,14159}")]
    public void Parse_Placeholder_Named_Alignment_Format(string template)
    {
        var result = MessageTemplates.Parse(template);
        ShouldBeSuccess(result, 1);
        ShouldBe(result.Placeholders[0], "world", 7, 5);
    }

    [TestMethod]
    public void Parse_Placeholder_Multiple()
    {
        var template = "{{what}} {$an_amazing,20} {@day:dd-MM-yyyy} to be alive! {42,-2:format}";
        var result = MessageTemplates.Parse(template);
        ShouldBeSuccess(result, 3);
        ShouldBe(result.Placeholders[0], "an_amazing", 11, 10);
        ShouldBe(result.Placeholders[1], "day", 28, 3);
        ShouldBe(result.Placeholders[2], "42", 58, 2);
    }

    [DataTestMethod]

    //[DataRow("Login failed for {User")]         // Missing closing bracket
    //[DataRow("Login failed for {{User}")]       //  Opening bracket is escaped
    //[DataRow("Login failed for {User_Name}")]   //  Only alphanumerics and '_' are allowed for placeholders

    [DataRow("}")]                              // Right bracket is not allowed
    [DataRow("}}}")]                            // Third right bracket is not allowed (first two are valid)
    [DataRow("{{}")]                            // Empty placeholder is not allowed
    [DataRow("Login failed for {&User}")]       // Only '@' and '$' are allowed as prefix
    [DataRow("Login failed for {User_%Name}")]  // Only alphanumerics and '_' are allowed for placeholders
    [DataRow("Retry attempt {Cnt,r}")]          // The alignment specifier must be numeric
    [DataRow("Retry attempt {Cnt,}")]           // Empty alignment specifier is not allowed
    [DataRow("Retry attempt {Cnt:}")]           // Empty format specifier is not allowed
    public void Parse_Placeholder_Failure(string template)
    {
        var result = MessageTemplates.Parse(template);
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Placeholders.Should().BeNull();
    }

    private static void ShouldBeSuccess(MessageTemplates.ParseResult actual, int placeholderCount = 0)
    {
        actual.Should().NotBeNull();
        actual.Success.Should().BeTrue();
        actual.Placeholders.Should().NotBeNull().And.HaveCount(placeholderCount);
    }

    private static void ShouldBe(MessageTemplates.Placeholder actual, string name, int start, int length)
    {
        actual.Should().NotBeNull();
        actual.Name.Should().Be(name);
        actual.Start.Should().Be(start);
        actual.Length.Should().Be(length);
    }
}
