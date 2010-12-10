/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on May 5, 2009
 */
package org.sonar.plugin.dotnet.srcmon;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import javax.xml.stream.XMLStreamException;

import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.in.SMEvent;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.StaxParser;
import org.sonar.api.utils.StaxParser.XmlStreamHandler;
import org.sonar.api.utils.XmlParserException;
import org.sonar.plugin.dotnet.srcmon.model.FileMetrics;
import org.sonar.plugin.dotnet.srcmon.model.MethodMetric;

/**
 * Parses a source monitor result file using a STAX parser
 * 
 */
public class SourceMonitorResultStaxParser implements SourceMonitorResultParser {

  private final static Logger log = LoggerFactory.getLogger(SourceMonitorResultStaxParser.class);
  
  private enum FileMetricEnum {
    /** Line count */
    M0,
    /** Statement count */
    M1,
    /** Percentage of comment lines */
    M2,
    /** Percent Documentation Lines */
    M3,
    /** Number of classes, interfaces, structs */
    M4,
    /** Methods per Class */
    M5,
    /** Calls per Method */
    M6,
    /** Statements per Method */
    M7, 
    /** Average Complexity */
    M14,
    M8,M9,M10,M11,M12,M13
  }

  private enum MethodMetricEnum {
    /**
     * complexity
     */
    COMPLEXITY,

    /**
     * Statements count
     */
    STATEMENTS,

    /**
     * Maximum Depth
     */
    MAXIMUM_DEPTH,

    /**
     * Number of method calls
     */
    CALLS,
  }

  

  private static final int INITIAL_CAPACITY = 100;
  private File baseDir;

  private List<FileMetrics> result;

  /**
   * Parses the report. Not Threadsafe.
   * 
   * @param reportFile
   */
  public List<FileMetrics> parse(File directory, File reportFile) {
    this.baseDir = directory;
    try {
      XmlStreamHandler parserHandler = new XmlStreamHandler() {
        @Override
        public void stream(SMHierarchicCursor rootCursor)
            throws XMLStreamException {
          SMInputCursor project = rootCursor.advance()
              .descendantElementCursor("project").advance()
              .childElementCursor();
          String parentDirectory = "";

          while (null != project.getNext()) {
            String curLocation = project.getLocalName();
            if ("project_directory".equals(curLocation)) {
              parentDirectory = project.collectDescendantText();
            } else if ("checkpoints".equals(curLocation)) {
              File projectDirectory = new File(parentDirectory);
              SMInputCursor filesCursor = project.descendantElementCursor(
                  "files").advance();
              int fileNb;
              try {
                fileNb = Integer.parseInt(filesCursor
                    .getAttrValue("file_count"));
              } catch (Exception e) {
                fileNb = INITIAL_CAPACITY;
              }
              result = new ArrayList<FileMetrics>(fileNb);

              parseMetrics(projectDirectory, filesCursor.childElementCursor());
            }
          }
        }
      };
      StaxParser parser = new StaxParser(parserHandler, false);
      parser.parse(reportFile);
    } catch (Exception e) {
      throw new XmlParserException("Can not parse source monitor reports", e);
    }

    return result;
  }

  protected void parseMetrics(File projectDirectory, SMInputCursor fileCursor)
      throws XMLStreamException {

    SMEvent fileEvent;
    while ((fileEvent = fileCursor.getNext()) != null) {
      if (fileEvent.compareTo(SMEvent.START_ELEMENT) == 0) {
        FileMetrics fileMetrics = createMetrics(projectDirectory, fileCursor);
        if (log.isDebugEnabled()) {
          log.debug("adding metrics for file " + fileMetrics);
        }
        result.add(fileMetrics);
      }
    }
  }

  /**
   * Creates the metrics for a file
   * 
   * @param fileNode
   * @return
   */
  private FileMetrics createMetrics(File projectDirectory,
      SMInputCursor fileCursor) throws XMLStreamException {
    String rawFileName = fileCursor.getAttrValue("file_name");
    String className = StringUtils.removeEnd(rawFileName, ".cs").replace('\\',
        '.');
    String namespace = StringUtils.substringBeforeLast(className, ".");

    FileMetrics fileMetric = new FileMetrics();
    fileMetric.setProjectDirectory(projectDirectory);
    fileMetric.setClassName(className);
    fileMetric.setNamespace(namespace);
    
    File path = toFullPath(rawFileName);
    fileMetric.setSourcePath(path);
    
    int countBlankLines = BlankLineCounter.countBlankLines(path);
    fileMetric.setCountBlankLines(countBlankLines);

    SMInputCursor childCursor = fileCursor.childElementCursor();
    SMEvent childEvent;
    while ((childEvent = childCursor.getNext()) != null) {
      if (childEvent.compareTo(SMEvent.START_ELEMENT) == 0) {
        String curLocation = childCursor.getLocalName();
        if ("metrics".equals(curLocation)) {
          SMInputCursor metricCursor = childCursor.childElementCursor("metric");
          extractMetrics(fileMetric, metricCursor);
        } else if ("method_metrics".equals(curLocation)) {
          SMInputCursor methodCursor = childCursor.childElementCursor("method");
          extractMethodMetrics(fileMetric, methodCursor, path);
        }
      }
    }

    return fileMetric;
  }

  private void extractMethodMetrics(FileMetrics fileMetric,
      SMInputCursor methodCursor, File file) throws XMLStreamException {
    SMEvent methodEvent;
    while ((methodEvent = methodCursor.getNext()) != null) {
      if (methodEvent.compareTo(SMEvent.START_ELEMENT) == 0) {
        fileMetric.addMethod(generateMethod(methodCursor, file));
      }
    }

  }

  private void extractMetrics(FileMetrics fileMetric, SMInputCursor metricCursor)
      throws XMLStreamException {
    SMEvent metricEvent;
    while ((metricEvent = metricCursor.getNext()) != null) {
      if (metricEvent.compareTo(SMEvent.START_ELEMENT) == 0) {
        String id = metricCursor.getAttrValue("id");
        FileMetricEnum metricId;
        try {
          metricId = FileMetricEnum.valueOf(id);
        } catch (IllegalArgumentException iae) {
          log.error("parsing error : " + id, iae);
          continue;
        }
        switch (metricId) {
        case M0:
          fileMetric.setCountLines(getIntMetric(metricCursor));
          break;
        case M1:
          fileMetric.setCountStatements(getIntMetric(metricCursor));
          break;
        case M2:
          fileMetric.setPercentCommentLines(getDoubleMetric(metricCursor));
          break;
        case M3:
          fileMetric
              .setPercentDocumentationLines(getDoubleMetric(metricCursor));
          break;
        case M4:
          fileMetric.setCountClasses(getIntMetric(metricCursor));
          break;
        case M5:
          fileMetric.setCountMethods(getIntMetric(metricCursor));
          break;
        case M6:
          fileMetric.setCountCalls(getIntMetric(metricCursor));
          break;
        case M7:
          fileMetric.setCountMethodStatements(getIntMetric(metricCursor));
          break;
        case M14:
          fileMetric.setAverageComplexity(getDoubleMetric(metricCursor));
          break;
        default:
          break;
        }
      }
    }

    int documentationLines = (int) (fileMetric.getCountLines()
        * fileMetric.getPercentDocumentationLines() / 100.0);
    fileMetric.setDocumentationLines(documentationLines);

    int commentLines = (int) (fileMetric.getCountLines()
        * fileMetric.getPercentCommentLines() / 100.0);
    fileMetric.setCommentLines(commentLines);

  }

  /**
   * Generates the metrics for a method
   * 
   * @param methodNode
   * @param file
   * @return
   * @throws XMLStreamException
   */
  private MethodMetric generateMethod(SMInputCursor methodCursor, File file)
      throws XMLStreamException {
    String rawName = methodCursor.getAttrValue("name");
    int methodLine = methodCursor.getAttrIntValue(methodCursor.findAttrIndex(
        null, "line"));
    String className;
    String methodName;

    boolean isAccessor = false;
    if (rawName.endsWith(".get()") || rawName.endsWith(".set()")) {
      isAccessor = true;
      String fullPropertyName = StringUtils.substringBeforeLast(rawName, ".");
      String propertyName = StringUtils.substringAfterLast(fullPropertyName,
          ".");
      className = StringUtils.substringBeforeLast(fullPropertyName, ".");
      methodName = StringUtils.removeEnd(
          StringUtils.substringAfterLast(rawName, "."), "()")
          + "_" + propertyName + "()";
    } else {
      methodName = StringUtils.substringAfterLast(rawName, ".");
      className = StringUtils.substringBeforeLast(rawName, ".");
    }
    MethodMetric methodMetrics = new MethodMetric();
    methodMetrics.setClassName(className);
    methodMetrics.setFile(file);
    methodMetrics.setMethodName(methodName);
    methodMetrics.setAccessor(isAccessor);
    methodMetrics.setMethodLine(methodLine);

    SMInputCursor childCursor = methodCursor.childElementCursor();
    SMEvent childEvent;
    while ((childEvent = childCursor.getNext()) != null) {
      if (childEvent.compareTo(SMEvent.START_ELEMENT) == 0) {
        MethodMetricEnum methodMetricEnum;
        String metricName = childCursor.getLocalName().toUpperCase();
        try {
          methodMetricEnum = MethodMetricEnum.valueOf(metricName);
        } catch (IllegalArgumentException iae) {
          log.error("unsupported method metric : " + metricName, iae);
          continue;
        }
        int metricValue = getIntMetric(childCursor);
        switch (methodMetricEnum) {
        case COMPLEXITY:
          methodMetrics.setComplexity(metricValue);
          break;
        case STATEMENTS:
          methodMetrics.setCountStatements(metricValue);
          break;
        case MAXIMUM_DEPTH:
          methodMetrics.setMaximumDepth(metricValue);
          break;
        case CALLS:
          methodMetrics.setCountCalls(metricValue);
          break;
        }
      }
    }

    return methodMetrics;
  }

  private File toFullPath(String rawFileName) {
    // fix tests on unix system
    // but should not be necessary
    // on windows build machines
    String rawPortableFileName = StringUtils.replaceChars(rawFileName, '\\',
        File.separatorChar);
    File file;
    try {
      file = new File(baseDir, rawPortableFileName).getCanonicalFile();
    } catch (IOException e) {
      file = new File(baseDir, rawPortableFileName);
    }
    return file;
  }

  /**
   * Gets a metric as an integer
   * 
   * @param node
   * @param path
   * @return
   * @throws XMLStreamException
   */
  public int getIntMetric(SMInputCursor cursor) throws XMLStreamException {
    String value = cursor.getElemStringValue();
    int result = 0;
    try {
      if (value != null) {
        // We need a double here since source monitor has sometime a strange
        // behaviour (cursor.getElemIntValue() would fail if value=1.0 for
        // example)
        result = (int) Double.parseDouble(value);
      }
    } catch (NumberFormatException nfe) {
      log.error("int parsing error : " + value, nfe);
    }
    return result;
  }

  /**
   * Gets a metric as a double.
   * 
   * @param node
   * @param path
   * @return
   */
  public double getDoubleMetric(SMInputCursor cursor) throws XMLStreamException {
    String value = cursor.getElemStringValue();
    double result = 0;
    try {
      if (value != null) {
        result = Double.parseDouble(value);
      }
    } catch (NumberFormatException nfe) {
      log.error("double parsing error : " + value, nfe);
    }
    return result;
  }

}