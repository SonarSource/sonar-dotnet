/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.RegularExpressions.Test;

[TestClass]
public class MessageTemplateParserTest
{
    [TestMethod]
    [DataRow("")]
    [DataRow("}")]
    [DataRow("{{}}")]
    [DataRow("{{")]
    [DataRow("}}")]
    [DataRow("}}{{{{")]
    [DataRow("hello world")]
    [DataRow("hello {{world}}")]
    [DataRow("{{hello {{world}}}}")]
    public void Parse_NoPlaceholder(string template)
    {
        var result = MessageTemplatesParser.Parse(template, MessageTemplatesParser.TemplateRegex);
        ShouldBeSuccess(result);
    }

    [TestMethod]
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
    [DataRow(""" "hello" + "{world}" + "!" """, "world", 13, 5)]
    public void Parse_Placeholder(string template, string placeholder, int start, int length)
    {
        var result = MessageTemplatesParser.Parse(template, MessageTemplatesParser.TemplateRegex);
        ShouldBeSuccess(result, 1);
        ShouldBe(result.Placeholders[0], placeholder, start, length);
    }

    [TestMethod]
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
    [DataRow("hello {world:format,42}")] // semantically looks like a typo, format and alignment are reversed, but it's syntactically valid.
    public void Parse_Placeholder_Named_Alignment_Format(string template)
    {
        var result = MessageTemplatesParser.Parse(template, MessageTemplatesParser.TemplateRegex);
        ShouldBeSuccess(result, 1);
        ShouldBe(result.Placeholders[0], "world", 7, 5);
    }

    [TestMethod]
    public void Parse_Placeholder_Multiple()
    {
        var template = """
            In code's {$silent} realm,
            Logic weaves through lines {@0}f text,
            Errors {@teach,42} us well.

            Syntax {sym_phony,42:dd-MM},
            Logic {@_orchestrates42} the mind,
            Programmer's {ballet:_}.
            """;
        var result = MessageTemplatesParser.Parse(template, MessageTemplatesParser.TemplateRegex);
        ShouldBeSuccess(result, 6);
        ShouldBe(result.Placeholders[0], "silent", 12, 6);
        ShouldBe(result.Placeholders[1], "0", 56, 1);
        ShouldBe(result.Placeholders[2], "teach", 75, 5);

        ShouldBe(result.Placeholders[3], "sym_phony", 103, 9);
        ShouldBe(result.Placeholders[4], "_orchestrates42", 132, 15);
        ShouldBe(result.Placeholders[5], "ballet", 173, 6);
    }

    [TestMethod]
    [DataRow("{")]                                  // Left bracket is not allowed
    [DataRow("{{{")]                                // Third left bracket is not allowed (first two are valid)
    [DataRow("{}")]                                 // Empty placeholder is not allowed
    [DataRow("{{{}}}")]                             // Empty placeholder is not allowed
    [DataRow("Login failed for {User")]             // Missing closing bracket
    [DataRow("Login failed for {&User}")]           // Only '@' and '$' are allowed as prefix
    [DataRow("Login failed for {User_%Name}")]      // Only alphanumerics and '_' are allowed for placeholders
    [DataRow("Retry attempt {Cnt,r}")]              // The alignment specifier must be numeric
    [DataRow("Retry attempt {Cnt,}")]               // Empty alignment specifier is not allowed
    [DataRow("Retry attempt {Cnt:}")]               // Empty format specifier is not allowed
    [DataRow(""" "hello {" + "world" + "}" """)]    // '+' and '"' is not allowed in placeholders
    public void Parse_Placeholder_Failure(string template)
    {
        var result = MessageTemplatesParser.Parse(template, MessageTemplatesParser.TemplateRegex);
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Placeholders.Should().BeNull();
    }

    private static void ShouldBeSuccess(MessageTemplatesParser.ParseResult actual, int placeholderCount = 0)
    {
        actual.Should().NotBeNull();
        actual.Success.Should().BeTrue();
        actual.Placeholders.Should().NotBeNull().And.HaveCount(placeholderCount);
    }

    private static void ShouldBe(MessageTemplatesParser.Placeholder actual, string name, int start, int length)
    {
        actual.Should().NotBeNull();
        actual.Name.Should().Be(name);
        actual.Start.Should().Be(start);
        actual.Length.Should().Be(length);
    }
}
