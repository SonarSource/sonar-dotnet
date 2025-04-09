/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 */

using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class IXmlLineInfoExtensionsTest
{
    [TestMethod]
    public void Element_Simple()
    {
        var doc = XDocument.Parse("<a/>", LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var tag = doc.Elements().First();
        var location = tag.CreateLocation("Test", tag.Name);
        location.Should().Be(Location.Create("Test", new TextSpan(0, 1), new LinePositionSpan(new LinePosition(0, 1), new LinePosition(0, 2))));
    }

    [TestMethod]
    // https://sonarsource.atlassian.net/browse/USER-292
    // How to fix: https://sonarsource.atlassian.net/browse/USER-292?focusedCommentId=763449
    public void Element_WithNamespace()
    {
        var doc = XDocument.Parse("""
            <?xml version="1.0" encoding="utf-8"?>
            <wsse:Security
                xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
                <wsse:Password>xxx</wsse:Password>
            </wsse:Security>
            """, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var passwordTag = doc.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "Password");
        var location = passwordTag.CreateLocation("Test", passwordTag.Name);
        location.Should().Be(Location.Create("Test", new TextSpan(3, 91), new LinePositionSpan(new LinePosition(3, 5), new LinePosition(3, 96))), because: """
            passwordTag.Name is "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}Password"
            """);
    }

    [TestMethod]
    public void Attribute_WithNamespace()
    {
        var doc = XDocument.Parse("""
            <?xml version="1.0" encoding="utf-8"?>
            <wsse:Security
                xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"
                wsse:Password="xxx">
            </wsse:Security>
            """, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var securityTag = doc.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "Security");
        var passwordAttribute = securityTag.Attributes().First(x => x.Name.LocalName == "Password");
        var location = passwordAttribute.CreateLocation("Test", passwordAttribute.Name);
        location.Should().Be(Location.Create("Test", new TextSpan(3, 91), new LinePositionSpan(new LinePosition(3, 4), new LinePosition(3, 95))), because: """
            passwordAttribute.Name is "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}Password"
            """);
    }
}
