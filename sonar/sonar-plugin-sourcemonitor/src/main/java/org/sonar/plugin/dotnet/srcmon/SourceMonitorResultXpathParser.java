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
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.srcmon.model.FileMetrics;
import org.sonar.plugin.dotnet.srcmon.model.MethodMetric;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.InputSource;

/**
 * Parses a source monitor result file
 * 
 * @author Jose CHILLAN May 5, 2009
 * @deprecated use instead the stax version {@link SourceMonitorResultXpathParser}
 */
public class SourceMonitorResultXpathParser implements SourceMonitorResultParser {

  private final static Logger log = LoggerFactory
      .getLogger(SourceMonitorResultXpathParser.class);

  private XPath xpath;
  private File baseDir;
  private Map<String, XPathExpression> expressions;

  public SourceMonitorResultXpathParser() {
    XPathFactory factory = XPathFactory.newInstance();
    xpath = factory.newXPath();
    expressions = new HashMap<String, XPathExpression>();
  }

  /* (non-Javadoc)
   * @see org.sonar.plugin.dotnet.srcmon.SourceMonitorResultParser#parse(java.io.File, java.io.File)
   */
  @Override
  public List<FileMetrics> parse(File reportFile) {
    List<FileMetrics> result = new ArrayList<FileMetrics>();

    try {
      URL reportURL = reportFile.toURI().toURL();
      InputSource source = new InputSource(reportURL.openStream());
      XPathExpression expression = xpath.compile("//file");
      NodeList nodes = (NodeList) expression.evaluate(source,
          XPathConstants.NODESET);
      int length = nodes.getLength();

      for (int idxNode = 0; idxNode < length; idxNode++) {
        Element fileNode = (Element) nodes.item(idxNode);
        FileMetrics fileMetrics = createMetrics(fileNode);
        if (log.isDebugEnabled()) {
          log.debug("adding metrics for file " + fileMetrics);
        }
        result.add(fileMetrics);
      }
    } catch (XPathExpressionException e) {
      log.error("Unexpected error while parsing xml source monitor report", e);
    } catch (MalformedURLException e) {
      log.error("Unexpected error while parsing xml source monitor report", e);
    } catch (IOException e) {
      log.error("Unexpected error while parsing xml source monitor report", e);
    }
    return result;
  }

  /**
   * Creates the metrics for a file
   * 
   * @param fileNode
   * @return
   */
  private FileMetrics createMetrics(Element fileNode) {
    String rawFileName = fileNode.getAttribute("file_name");
    String directoryPath = "";
    Element projectElement = (Element) fileNode.getParentNode().getParentNode()
        .getParentNode().getParentNode();
    NodeList projectDirElements = projectElement
        .getElementsByTagName("project_directory");
    if (projectDirElements.getLength() > 0) {
      Element projectDirElement = (Element) projectDirElements.item(0);
      directoryPath = projectDirElement.getTextContent();
    }

    String className = StringUtils.removeEnd(rawFileName, ".cs").replace('\\',
        '.');
    String namespace = StringUtils.substringBeforeLast(className, ".");

    int countLines = getIntMetric(fileNode, "metrics/metric[@id='M0']");
    int countStatements = getIntMetric(fileNode, "metrics/metric[@id='M1']");
    double percentCommentLines = getDoubleMetric(fileNode,
        "metrics/metric[@id='M2']");
    double percentDocumentationLines = getDoubleMetric(fileNode,
        "metrics/metric[@id='M3']");
    int countClasses = getIntMetric(fileNode, "metrics/metric[@id='M4']");
    int countMethods = getIntMetric(fileNode, "metrics/metric[@id='M5']");
    int countCalls = getIntMetric(fileNode, "metrics/metric[@id='M6']");
    int countMethodStatements = getIntMetric(fileNode,
        "metrics/metric[@id='M7']");
    double averageComplexity = getDoubleMetric(fileNode,
        "metrics/metric[@id='M14']");
    int commentLines = (int) (countLines * percentCommentLines / 100.0);
    int documentationLines = (int) (countLines * percentDocumentationLines / 100.0);

    // The file result is populated
    FileMetrics result = new FileMetrics();
    File path = toFullPath(rawFileName);
    int countBlankLines = BlankLineCounter.countBlankLines(path);
    result.setNamespace(namespace);
    result.setSourcePath(path);
    result.setCountBlankLines(countBlankLines);
    result.setClassName(className);
    result.setCountLines(countLines);
    result.setCountStatements(countStatements);
    result.setCommentLines(commentLines);
    result.setDocumentationLines(documentationLines);
    result.setCountClasses(countClasses);
    result.setCountMethods(countMethods);
    result.setCountCalls(countCalls);
    result.setCountMethodStatements(countMethodStatements);
    result.setAverageComplexity(averageComplexity);

    // Builds the methods
    List<MethodMetric> extractMethods = extractMethods(fileNode, path);
    for (MethodMetric methodMetric : extractMethods) {
      result.addMethod(methodMetric);
    }
    return result;
  }

  /**
   * Parses the method metrics
   * 
   * @param fileNode
   * @return
   */
  private List<MethodMetric> extractMethods(Element fileNode, File file) {
    List<MethodMetric> result = new ArrayList<MethodMetric>();
    // We extract the method metrics
    try {
      NodeList methodNodes = (NodeList) xpath.evaluate("method_metrics/method",
          fileNode, XPathConstants.NODESET);
      int countNodes = methodNodes.getLength();
      // We extract all the methods metrics
      for (int idxMethod = 0; idxMethod < countNodes; idxMethod++) {
        Element methodNode = (Element) methodNodes.item(idxMethod);
        MethodMetric methodMetrics = generateMethod(methodNode, file);
        result.add(methodMetrics);
      }
    } catch (XPathExpressionException e) {
      log.error("Unexpected error while parsing xml source monitor report", e);
    }
    return result;
  }

  /**
   * Generates the metrics for a method
   * 
   * @param methodNode
   * @param file
   * @return
   */
  private MethodMetric generateMethod(Element methodNode, File file) {
    String rawName = methodNode.getAttribute("name");
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
    int complexity = getIntElement(methodNode, "complexity");
    int methodLine = getIntAttribute(methodNode, "line");
    int countStatements = getIntElement(methodNode, "statements");
    int maximumDepth = getIntElement(methodNode, "maximum_depth");
    int countCalls = getIntElement(methodNode, "calls");
    MethodMetric methodMetrics = new MethodMetric();
    methodMetrics.setClassName(className);
    methodMetrics.setFile(file);
    methodMetrics.setMethodName(methodName);
    methodMetrics.setComplexity(complexity);
    methodMetrics.setCountCalls(countCalls);
    methodMetrics.setMaximumDepth(maximumDepth);
    methodMetrics.setMethodLine(methodLine);
    methodMetrics.setCountStatements(countStatements);
    methodMetrics.setAccessor(isAccessor);
    return methodMetrics;
  }

  /**
   * Gets an element content as an integer
   */
  private int getIntAttribute(Element element, String attributeName) {
    String value = getElementAttribute(element, attributeName);
    int result = convertToInteger(value);
    return result;
  }

  /**
   * Gets an element content as an integer
   */
  private int getIntElement(Element element, String attributeName) {
    Element subElement = getSubElement(element, attributeName);
    if (subElement == null) {
      return 0;
    }
    String value = subElement.getTextContent();
    int result = convertToInteger(value);
    return result;
  }

  /**
   * @param value
   * @return
   */
  private int convertToInteger(String value) {
    if (value == null) {
      return 0;
    }
    int result = 0;
    try {
      if (value != null) {
        // We need a double here since source monitor has sometime a strange
        // behaviour
        result = (int) Double.parseDouble(value);
      }
    } catch (NumberFormatException nfe) {
      log.error("Error while parsing source monitor measure " + value, nfe);
    }
    return result;
  }

  /**
   * Gets the attribute of an element, having the given name.
   * 
   * @param element
   *          the element that contains the attribute
   * @param attributeName
   * @return
   */
  private String getElementAttribute(Element element, String attributeName) {
    if (element == null) {
      return null;
    }
    String value = element.getAttribute(attributeName);
    return value;
  }

  /**
   * Gets the first sub element having the given name, if it exists.
   * 
   * @param element
   *          the node on which the element is sought
   * @param elementName
   *          the element name
   * @return
   */
  private Element getSubElement(Element element, String elementName) {
    if (element == null) {
      return null;
    }
    NodeList subElements = element.getElementsByTagName(elementName);
    if (subElements.getLength() == 0) {
      // No element found
      return null;
    }
    return (Element) subElements.item(0);
  }

  private File toFullPath(String rawFileName) {
    // fix tests on unix system
    // but should not be necessary
    // on windows build machines
    String rawPortableFileName =  StringUtils.replaceChars(rawFileName, '\\', File.separatorChar);
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
   */
  private int getIntMetric(Node node, String path) {
    String value = getAttributeMetric(node, path);
    int result = 0;
    try {
      if (value != null) {
        // We need a double here since source monitor has sometime a strange
        // behaviour
        result = (int) Double.parseDouble(value);
      }
    } catch (NumberFormatException nfe) {
      // Nothing
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
  private double getDoubleMetric(Node node, String path) {
    String value = getAttributeMetric(node, path);
    double result = 0;
    try {
      if (value != null) {
        result = Double.parseDouble(value);
      }
    } catch (NumberFormatException nfe) {
      // Nothing
    }
    return result;
  }

  private String getAttributeMetric(Node node, String path) {
    try {
      XPathExpression expression = expressions.get(path);
      if (expression == null) {
        expression = xpath.compile(path);
        expressions.put(path, expression);
      }
      String result = expression.evaluate(node);
      return result;
    } catch (XPathExpressionException e) {
      // Nothing
    }
    return null;
  }

}