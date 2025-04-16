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
        var location = tag.CreateLocation("Test", tag.Name, tag);
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
        var location = passwordTag.CreateLocation("Test", passwordTag.Name, passwordTag);
        location.Should().Be(Location.Create("Test", new TextSpan(3, 13), new LinePositionSpan(new LinePosition(3, 5), new LinePosition(3, 18))));
    }

    [TestMethod]
    public void Element_WithDefaultNamespace()
    {
        var doc = XDocument.Parse("""
            <?xml version="1.0" encoding="utf-8"?>
            <Security
                xmlns="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
                <Password>xxx</Password>
            </Security>
            """, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var passwordTag = doc.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "Password");
        var location = passwordTag.CreateLocation("Test", passwordTag.Name, passwordTag);
        location.Should().Be(Location.Create("Test", new TextSpan(3, 8), new LinePositionSpan(new LinePosition(3, 5), new LinePosition(3, 13))));
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
        var location = passwordAttribute.CreateLocation("Test", passwordAttribute.Name, passwordAttribute.Parent);
        location.Should().Be(Location.Create("Test", new TextSpan(3, 13), new LinePositionSpan(new LinePosition(3, 4), new LinePosition(3, 17))), because: """
        passwordAttribute.Name is "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}Password"
        """);
    }
}
