/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class AnalyzerOptionsExtensionsTest
{
    [TestMethod]
    public void ParseCustomDictionaryAcronyms_ReadsCasingExceptions()
    {
        var sut = Parse("""
            <Dictionary>
                <Acronyms>
                    <CasingExceptions>
                        <Acronym>IAC</Acronym>
                        <Acronym>NSA</Acronym>
                    </CasingExceptions>
                </Acronyms>
            </Dictionary>
            """);
        sut.Should().BeEquivalentTo("IAC", "NSA");
    }

    [TestMethod]
    public void ParseCustomDictionaryAcronyms_NormalizesToUpperInvariant()
    {
        var sut = Parse("""
            <Dictionary>
                <Acronyms>
                    <CasingExceptions>
                        <Acronym>iAc</Acronym>
                    </CasingExceptions>
                </Acronyms>
            </Dictionary>
            """);
        sut.Should().BeEquivalentTo("IAC");
    }

    [TestMethod]
    public void ParseCustomDictionaryAcronyms_TrimsAndIgnoresEmptyEntries()
    {
        var sut = Parse("""
            <Dictionary>
                <Acronyms>
                    <CasingExceptions>
                        <Acronym>  IAC  </Acronym>
                        <Acronym></Acronym>
                        <Acronym>   </Acronym>
                    </CasingExceptions>
                </Acronyms>
            </Dictionary>
            """);
        sut.Should().BeEquivalentTo("IAC");
    }

    [TestMethod]
    public void ParseCustomDictionaryAcronyms_NoAcronymsElement_ReturnsEmpty()
    {
        var sut = Parse("<Dictionary />");
        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void ParseCustomDictionaryAcronyms_NoCasingExceptionsElement_ReturnsEmpty()
    {
        var sut = Parse("""
            <Dictionary>
                <Acronyms />
            </Dictionary>
            """);
        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void ParseCustomDictionaryAcronyms_UnexpectedRootElement_ReturnsEmpty()
    {
        var sut = Parse("<Unexpected><Acronyms><CasingExceptions><Acronym>IAC</Acronym></CasingExceptions></Acronyms></Unexpected>");
        sut.Should().BeEmpty();
    }

    [TestMethod]
    public void ParseCustomDictionaryAcronyms_MalformedXml_ReturnsEmpty()
    {
        var sut = Parse("not valid xml");
        sut.Should().BeEmpty();
    }

    private static ImmutableHashSet<string> Parse(string xml) =>
        AnalyzerOptionsExtensions.ParseCustomDictionaryAcronyms(SourceText.From(xml));
}
