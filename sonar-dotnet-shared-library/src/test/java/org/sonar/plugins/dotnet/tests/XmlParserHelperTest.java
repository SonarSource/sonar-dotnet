/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.util.function.Function;
import javax.xml.stream.Location;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class XmlParserHelperTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void invalid_prolog() {
    thrown.expectMessage("Error while parsing the XML file: ");
    thrown.expectMessage("invalid_prolog.txt");

    new XmlParserHelper(new File("src/test/resources/xml_parser_helper/invalid_prolog.txt")).nextStartTag();
  }

  @Test
  public void nextStartOrEndTag() {
    XmlParserHelper xml = new XmlParserHelper(new File("src/test/resources/xml_parser_helper/valid.xml"));
    assertThat(xml.nextStartOrEndTag()).isEqualTo("<foo>");
    assertThat(xml.nextStartOrEndTag()).isEqualTo("<bar>");
    assertThat(xml.nextStartOrEndTag()).isEqualTo("</bar>");
    assertThat(xml.nextStartOrEndTag()).isEqualTo("</foo>");
    assertThat(xml.nextStartOrEndTag()).isNull();
  }

  @Test
  public void getDoubleAttribute() {
    XmlParserHelper xml = new XmlParserHelper(new File("src/test/resources/xml_parser_helper/valid.xml"));
    xml.nextStartTag();
    assertThat(xml.getDoubleAttribute("myDouble")).isEqualTo(0.123);
    assertThat(xml.getDoubleAttribute("myCommaDouble")).isEqualTo(1.234);
    assertThat(xml.getDoubleAttribute("nonExisting")).isNull();

    thrown.expectMessage("valid.xml");
    thrown.expectMessage("Expected an double instead of \"hello\" for the attribute \"myString\"");
    xml.getDoubleAttribute("myString");
  }

  @Test
  public void no_next_valid_tag() throws XMLStreamException {
    XMLStreamReader mockedReader = mock(XMLStreamReader.class);
    when(mockedReader.hasNext()).thenAnswer(createAnswerWithCount(i -> i < 3));
    when(mockedReader.getLocation()).thenAnswer(createAnswerWithCount(XmlParserHelperTest::createLocation));
    when(mockedReader.next()).thenThrow(XMLStreamException.class);

    XmlParserHelper parser = createParserWithMockReader(mockedReader);

    assertThat(parser.nextStartTag()).isNull();
  }

  @Test
  public void null_location() throws XMLStreamException {
    XMLStreamReader mockedReader = mock(XMLStreamReader.class);
    when(mockedReader.hasNext()).thenReturn(true);
    when(mockedReader.next()).thenThrow(XMLStreamException.class);

    XmlParserHelper parser = createParserWithMockReader(mockedReader);

    thrown.expect(IllegalStateException.class);
    parser.nextStartTag();
  }

  private static XmlParserHelper createParserWithMockReader(XMLStreamReader mockedReader) {
    return new XmlParserHelper(new File("src/test/resources/xml_parser_helper/valid.xml")) {
      @Override
      XMLStreamReader createXmlStreamReader() {
        return mockedReader;
      }
    };
  }

  private static Location createLocation(int i) {
    return new Location() {
      @Override
      public int getLineNumber() {
        return i;
      }

      @Override
      public int getColumnNumber() {
        return 0;
      }

      @Override
      public int getCharacterOffset() {
        return 0;
      }

      @Override
      public String getPublicId() {
        return null;
      }

      @Override
      public String getSystemId() {
        return null;
      }
    };
  }

  private static Answer createAnswerWithCount(Function<Integer, Object> applyFunction) {
    return new Answer() {
      private int count = 0;

      @Override
      public Object answer(InvocationOnMock invocationOnMock) {
        count++;
        return applyFunction.apply(count);
      }
    };
  }

}
