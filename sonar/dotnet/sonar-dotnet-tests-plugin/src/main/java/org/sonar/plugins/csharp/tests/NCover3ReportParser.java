/*
 * Sonar .NET Plugin :: Tests
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.tests;

import com.google.common.base.Charsets;
import com.google.common.base.Throwables;
import com.google.common.collect.Maps;
import com.google.common.io.Closeables;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import javax.annotation.Nullable;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Map;

public class NCover3ReportParser implements CoverageParser {

  private static final Logger LOG = LoggerFactory.getLogger(NCover3ReportParser.class);

  private final File file;

  public NCover3ReportParser(File file) {
    this.file = file;
  }

  @Override
  public Coverage parse() {
    return new Parser(file).parse();
  }

  private static class Parser {

    private final File file;
    private XMLStreamReader stream;
    private final Map<String, String> documents = Maps.newHashMap();
    private final Coverage coverage = new Coverage();

    public Parser(File file) {
      this.file = file;
    }

    public Coverage parse() {
      LOG.info("Parsing the NCover3 report " + file.getAbsolutePath());

      InputStreamReader reader = null;
      XMLInputFactory xmlFactory = XMLInputFactory.newInstance();

      try {
        reader = new InputStreamReader(new FileInputStream(file), Charsets.UTF_8);
        stream = xmlFactory.createXMLStreamReader(reader);

        checkRootTag();

        while (stream.hasNext()) {
          if (stream.next() == XMLStreamConstants.START_ELEMENT) {
            String tagName = stream.getLocalName();

            if ("doc".equals(tagName)) {
              handleDocTag();
            } else if ("seqpnt".equals(tagName)) {
              handleSegmentPointTag();
            }
          }
        }
      } catch (IOException e) {
        throw Throwables.propagate(e);
      } catch (XMLStreamException e) {
        throw Throwables.propagate(e);
      } finally {
        closeXmlStream();
        Closeables.closeQuietly(reader);
      }

      return coverage;
    }

    private void closeXmlStream() {
      if (stream != null) {
        try {
          stream.close();
        } catch (XMLStreamException e) {
          /* do nothing */
        }
      }
    }

    private void handleDocTag() throws XMLStreamException, ParseErrorException {
      String id = getRequiredAttribute("id");
      String url = getRequiredAttribute("url");

      if (!isExcludedId(id)) {
        documents.put(id, url);
      }
    }

    private static boolean isExcludedId(String id) {
      return "0".equals(id);
    }

    private void handleSegmentPointTag() throws XMLStreamException, ParseErrorException {
      String doc = getRequiredAttribute("doc");
      int line = getRequiredIntAttribute("l");
      int vc = getRequiredIntAttribute("vc");

      if (documents.containsKey(doc) && !isExcludedLine(line)) {
        coverage.addHits(documents.get(doc), line, vc);
      }
    }

    private static boolean isExcludedLine(Integer line) {
      return 0 == line;
    }

    private int getRequiredIntAttribute(String name) throws ParseErrorException {
      String value = getRequiredAttribute(name);

      try {
        return Integer.parseInt(value);
      } catch (NumberFormatException e) {
        throw parseError("Expected an integer instead of \"" + value + "\" for the attribute \"" + name + "\"");
      }
    }

    private void checkRootTag() throws XMLStreamException, ParseErrorException {
      int event = stream.nextTag();

      if (event != XMLStreamConstants.START_ELEMENT || !"coverage".equals(stream.getLocalName())) {
        throw parseError("Missing root element <coverage>");
      }

      checkRequiredAttribute("exportversion", 3);
    }

    private void checkRequiredAttribute(String name, int expectedValue) throws ParseErrorException {
      int actualValue = getRequiredIntAttribute(name);
      if (expectedValue != actualValue) {
        throw parseError("Expected \"" + expectedValue + "\" instead of \"" + actualValue + "\" for the \"" + name + "\" attribute");
      }
    }

    private String getRequiredAttribute(String name) {
      String value = getAttribute(name);
      if (value == null) {
        throw parseError("Missing attribute \"" + name + "\" in element <" + stream.getLocalName() + ">");
      }

      return value;
    }

    @Nullable
    private String getAttribute(String name) {
      for (int i = 0; i < stream.getAttributeCount(); i++) {
        if (name.equals(stream.getAttributeLocalName(i))) {
          return stream.getAttributeValue(i);
        }
      }

      return null;
    }

    private ParseErrorException parseError(String message) {
      return new ParseErrorException(message + " in " + file.getAbsolutePath() + " at line " + stream.getLocation().getLineNumber());
    }

  }

  private static class ParseErrorException extends RuntimeException {

    private static final long serialVersionUID = 1L;

    public ParseErrorException(String message) {
      super(message);
    }

  }

}
