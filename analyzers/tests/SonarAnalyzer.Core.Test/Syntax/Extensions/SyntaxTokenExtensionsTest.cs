/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Syntax.Extensions.Test;

[TestClass]
public class SyntaxTokenExtensionsTest
{
    [TestMethod]
    public void ToSecondaryLocation_NullMessage()
    {
        var code = "public class $$C$$ {}";
        var token = TestCompiler.TokenBetweenMarkersCS(code).Token;
        var secondaryLocation = token.ToSecondaryLocation(null);
        secondaryLocation.Should().NotBeNull();
        secondaryLocation.Location.Should().Be(token.GetLocation());
        secondaryLocation.Message.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow([])]
    public void ToSecondaryLocation_MessageArgs(string[] messageArgs)
    {
        var code = "public class $$C$$ {}";
        var token = TestCompiler.TokenBetweenMarkersCS(code).Token;
        var secondaryLocation = token.ToSecondaryLocation("Message", messageArgs);
        secondaryLocation.Should().NotBeNull();
        secondaryLocation.Location.Should().Be(token.GetLocation());
        secondaryLocation.Message.Should().Be("Message");
    }

    [TestMethod]
    [DataRow("Message {0}", "42")]
    [DataRow("{1} Message {0} ", "42", "21")]
    public void ToSecondaryLocation_MessageFormat(string format, params string[] messageArgs)
    {
        var code = "public class $$C$$ {}";
        var token = TestCompiler.TokenBetweenMarkersCS(code).Token;
        var secondaryLocation = token.ToSecondaryLocation(format, messageArgs);
        secondaryLocation.Should().NotBeNull();
        secondaryLocation.Location.Should().Be(token.GetLocation());
        secondaryLocation.Message.Should().Be(string.Format(format, messageArgs));
    }
}
