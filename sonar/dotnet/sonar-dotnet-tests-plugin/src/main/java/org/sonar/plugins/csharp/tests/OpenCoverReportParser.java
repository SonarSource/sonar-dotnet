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
import java.io.FileReader;
import java.io.IOException;
import java.util.Map;

public class OpenCoverReportParser implements CoverageProvider {

  private static final Logger LOG = LoggerFactory.getLogger(OpenCoverReportParser.class);

  private final File file;
  private final Map<String, String> files = Maps.newHashMap();
  private final Coverage coverage = new Coverage();

  private String fileRef;

  public OpenCoverReportParser(File file) {
    this.file = file;
  }

  @Override
  public Coverage coverage() {
    LOG.info("Parsing the OpenCover report " + file.getAbsolutePath());

    FileReader reader = null;
    XMLStreamReader stream = null;
    XMLInputFactory xmlFactory = XMLInputFactory.newInstance();
    fileRef = null;

    try {
      reader = new FileReader(file);
      stream = xmlFactory.createXMLStreamReader(reader);

      checkRootTag(stream);

      while (stream.hasNext()) {
        if (stream.next() == XMLStreamConstants.START_ELEMENT) {
          String tagName = stream.getLocalName();

          if ("File".equals(tagName)) {
            handleFileTag(stream);
          } else if ("FileRef".equals(tagName)) {
            handleFileRef(stream);
          } else if ("SequencePoint".equals(tagName)) {
            handleSegmentPointTag(stream);
          }
        }
      }
    } catch (ParseErrorException e) {
      logParseError(e);
      throw Throwables.propagate(e);
    } catch (IOException e) {
      logException(e);
      throw Throwables.propagate(e);
    } catch (XMLStreamException e) {
      logException(e);
      throw Throwables.propagate(e);
    } finally {
      closeQuietly(stream);
      Closeables.closeQuietly(reader);
    }

    return coverage;
  }

  private void logException(Exception e) {
    LOG.error("Unable to parse the report " + file.getAbsolutePath(), e);
  }

  private static void closeQuietly(@Nullable XMLStreamReader reader) {
    if (reader != null) {
      try {
        reader.close();
      } catch (XMLStreamException e) {
        /* do nothing */
      }
    }
  }

  private void handleFileRef(XMLStreamReader stream) throws XMLStreamException, ParseErrorException {
    this.fileRef = getRequiredAttribute(stream, "uid");
  }

  private void handleFileTag(XMLStreamReader stream) throws XMLStreamException, ParseErrorException {
    String uid = getRequiredAttribute(stream, "uid");
    String fullPath = getRequiredAttribute(stream, "fullPath");

    files.put(uid, fullPath);
  }

  private void handleSegmentPointTag(XMLStreamReader stream) throws XMLStreamException, ParseErrorException {
    int line = getRequiredIntAttribute(stream, "sl");
    int vc = getRequiredIntAttribute(stream, "vc");

    if (files.containsKey(fileRef)) {
      coverage.addHits(files.get(fileRef), line, vc);
    }
  }

  private int getRequiredIntAttribute(XMLStreamReader stream, String name) throws ParseErrorException {
    String value = getRequiredAttribute(stream, name);

    try {
      return Integer.parseInt(value);
    } catch (NumberFormatException e) {
      throw new ParseErrorException("Expected an integer instead of \"" + value + "\" for the attribute \"" + name + "\"", stream.getLocation().getLineNumber());
    }
  }

  private static void checkRootTag(XMLStreamReader stream) throws XMLStreamException, ParseErrorException {
    int event = stream.nextTag();

    if (event != XMLStreamConstants.START_ELEMENT || !"CoverageSession".equals(stream.getLocalName())) {
      throw new ParseErrorException("Missing root element <CoverageSession>", stream.getLocation().getLineNumber());
    }
  }

  private static String getRequiredAttribute(XMLStreamReader stream, String name) throws ParseErrorException {
    String value = getAttribute(stream, name);
    if (value == null) {
      throw new ParseErrorException("Missing attribute \"" + name + "\" in element <" + stream.getLocalName() + ">", stream.getLocation().getLineNumber());
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

  private void logParseError(ParseErrorException e) {
    LOG.error(e.getMessage() + " in " + file.getAbsolutePath() + " at line " + e.currentLine);
  }

  private static class ParseErrorException extends Exception {

    private static final long serialVersionUID = 1L;

    private final int currentLine;

    public ParseErrorException(String message, int currentLine) {
      super(message);
      this.currentLine = currentLine;
    }

  }

}
