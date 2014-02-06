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

import com.google.common.collect.Maps;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import javax.annotation.Nullable;
import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;

import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.Map;

public class NCover3ReportParser implements CoverageProvider {

  private static final Logger LOG = LoggerFactory.getLogger(NCover3ReportParser.class);

  private final File file;
  private final Map<String, String> documents = Maps.newHashMap();
  private final Coverage coverage = new Coverage();

  public NCover3ReportParser(File file) {
    this.file = file;
  }

  @Override
  public Coverage coverage() {
    LOG.info("Parsing the report " + file.getAbsolutePath());

    FileReader reader = null;
    XMLStreamReader stream = null;
    XMLInputFactory xmlFactory = XMLInputFactory.newInstance();

    try {
      reader = new FileReader(file);
      stream = xmlFactory.createXMLStreamReader(reader);

      if (!checkRootTag(stream)) {
        LOG.error("Invalid document start in the report " + file.getAbsolutePath());
      } else {
        while (stream.hasNext()) {
          if (stream.next() == XMLStreamConstants.START_ELEMENT) {
            String tagName = stream.getLocalName();

            if ("doc".equals(tagName)) {
              handleDocTag(stream);
            } else if ("seqpnt".equals(tagName)) {
              handleSegmentPointTag(stream);
            }
          }
        }
      }
    } catch (MissingAttributeException e) {
      logMissingAttribute(e);
    } catch (IOException e) {
      logException(e);
    } catch (XMLStreamException e) {
      logException(e);
    } finally {
      if (stream != null) {
        closeQuietly(stream);
      }
      if (reader != null) {
        closeQuietly(reader);
      }
    }

    return coverage;
  }

  private void logException(Exception e) {
    LOG.error("Unable to parse the report " + file.getAbsolutePath(), e);
  }

  private static void closeQuietly(XMLStreamReader reader) {
    try {
      reader.close();
    } catch (XMLStreamException e) {
      /* do nothing */
    }
  }

  private static void closeQuietly(FileReader reader) {
    try {
      reader.close();
    } catch (IOException e) {
      /* do nothing */
    }
  }

  private void handleDocTag(XMLStreamReader stream) throws XMLStreamException, MissingAttributeException {
    String id = getRequiredAttribute(stream, "id");
    String url = getRequiredAttribute(stream, "url");

    if (!isExcludedId(id)) {
      documents.put(id, url);
    }
  }

  private static boolean isExcludedId(String id) {
    return "0".equals(id);
  }

  private void handleSegmentPointTag(XMLStreamReader stream) throws XMLStreamException, MissingAttributeException {
    String doc = getRequiredAttribute(stream, "doc");
    Integer line = parseInteger(getRequiredAttribute(stream, "l"));
    Integer vc = parseInteger(getRequiredAttribute(stream, "vc"));

    if (documents.containsKey(doc) && line != null && !isExcludedLine(line) && vc != null) {
      coverage.addHits(documents.get(doc), line, vc);
    }
  }

  private static boolean isExcludedLine(Integer line) {
    return 0 == line;
  }

  @Nullable
  private Integer parseInteger(@Nullable String s) {
    Integer result;

    if (s == null) {
      result = null;
    } else {
      try {
        result = Integer.parseInt(s);
      } catch (NumberFormatException e) {
        result = null;
      }
    }

    return result;
  }

  private static boolean checkRootTag(XMLStreamReader stream) throws XMLStreamException {
    int event = stream.nextTag();

    return event == XMLStreamConstants.START_ELEMENT &&
      "coverage".equals(stream.getLocalName()) &&
      hasAttribute(stream, "exportversion", "3");
  }

  private static boolean hasAttribute(XMLStreamReader stream, String name, String value) {
    return value.equals(getAttribute(stream, name));
  }

  private String getRequiredAttribute(XMLStreamReader stream, String name) throws MissingAttributeException {
    String value = getAttribute(stream, name);
    if (value == null) {
      throw new MissingAttributeException(name, stream.getLocalName(), stream.getLocation().getLineNumber());
    }

    return value;
  }

  @Nullable
  private static String getAttribute(XMLStreamReader stream, String name) {
    for (int i = 0; i < stream.getAttributeCount(); i++) {
      if (name.equals(stream.getAttributeLocalName(i))) {
        return stream.getAttributeValue(i);
      }
    }

    return null;
  }

  private void logMissingAttribute(MissingAttributeException e) {
    LOG.error("Missing attribute \"" + e.missingAttributeName +
      "\" in " + file.getAbsolutePath() + " at line " + e.currentLine +
      " in element <" + e.currentElementName + ">.");
  }

  private static class MissingAttributeException extends Exception {

    private static final long serialVersionUID = 1L;

    private final String missingAttributeName;
    private final String currentElementName;
    private final int currentLine;

    public MissingAttributeException(String missingAttributeName, String currentElementName, int currentLine) {
      this.missingAttributeName = missingAttributeName;
      this.currentElementName = currentElementName;
      this.currentLine = currentLine;
    }

  }

}
