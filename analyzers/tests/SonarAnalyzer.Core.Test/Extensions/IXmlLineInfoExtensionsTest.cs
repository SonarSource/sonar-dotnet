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
        location.Should().Be(Location.Create("Test", new TextSpan(3, 13), new LinePositionSpan(new LinePosition(3, 4), new LinePosition(3, 17))));
    }

    [TestMethod]
    public void Element_WithNestedNamespaces()
    {
        var doc = XDocument.Parse("""
            <?xml version="1.0" encoding="utf-8"?>
            <Root xmlns:c2="http://example.org/root">
                <Child xmlns:c0005="http://example.org/child">
                    <c0005:T0005>
                        <c2:T2>
                        </c2:T2>
                    </c0005:T0005>
                </Child>
            </Root>
            """, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var t2 = doc.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "T2");
        var t0005 = doc.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "T0005");
        var t2Location = t2.CreateLocation("Test", t2.Name, t2);
        var t0005Location = t0005.CreateLocation("Test", t0005.Name, t0005);
        // "c0005".Length + "t0005".Length + ":".Length == 11
        t0005Location.Should().Be(Location.Create("Test", new TextSpan(3, 11), new LinePositionSpan(new LinePosition(3, 9), new LinePosition(3, 20))));
        // "c2".Length + "t2".Length + ":".Length == 5
        t2Location.Should().Be(Location.Create("Test", new TextSpan(4, 5), new LinePositionSpan(new LinePosition(4, 13), new LinePosition(4, 18))));
    }

    [TestMethod]
    public void Element_WithRedefinition()
    {
        var doc = XDocument.Parse("""
            <?xml version="1.0" encoding="utf-8"?>
            <Root xmlns:defined="http://example.org/root">
                <Child xmlns:defined="http://example.org/child">
                    <defined:Nested>
                    </defined:Nested>
                </Child>
            </Root>
            """, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
        var nestedTag = doc.DescendantNodes().OfType<XElement>().First(x => x.Name.LocalName == "Nested");
        var nestedLocation = nestedTag.CreateLocation("Test", nestedTag.Name, nestedTag);
        nestedLocation.Should().Be(Location.Create("Test", new TextSpan(3, 14), new LinePositionSpan(new LinePosition(3, 9), new LinePosition(3, 23))));
    }
}
