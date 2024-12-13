/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.io.IOException;
import java.util.List;
import java.util.Map;
import java.util.function.Consumer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public abstract class XmlTestReportParser {

  private static final Logger LOG = LoggerFactory.getLogger(XmlTestReportParser.class);
  protected final File file;
  protected final Map<String, String> methodFileMap;
  protected final Map<String, UnitTestResults> unitTestResults;
  protected final List<String> rootTags;

  XmlTestReportParser(File file,  Map<String, UnitTestResults> unitTestResults, Map<String, String> methodFileMap, List<String> rootTags) {
    this.file = file;
    this.unitTestResults = unitTestResults;
    this.methodFileMap = methodFileMap;
    this.rootTags = rootTags;
  }

  public void parse(Map<String, Consumer<XmlParserHelper>> tagHandlers) {
    try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {

      String tag = xmlParserHelper.checkRootTags(rootTags);
      do {
        if (tagHandlers.containsKey(tag)) {
          tagHandlers.get(tag).accept(xmlParserHelper);
        }
      } while ((tag = xmlParserHelper.nextStartTag()) != null);
    } catch (IOException e) {
      throw new IllegalStateException("Unable to close report", e);
    }
  }

  protected void addTestResultToFile(String methodFullName, UnitTestResults testResults) {
    if(!methodFileMap.containsKey(methodFullName)) {
      LOG.warn("Test method {} cannot be mapped to the test source file. The test will not be included.", methodFullName);
      return;
    }

    String fileName = methodFileMap.get(methodFullName);
    if (unitTestResults.containsKey(fileName)) {
      var fileTestResult = unitTestResults.get(fileName);
      fileTestResult.add(testResults);
    } else {
      unitTestResults.put(fileName, testResults);
    }
    LOG.debug("Added Test Method: {} to File: {}", methodFullName, fileName);
  }

  protected String extractDllNameFromFilePath(String filePath) {
    return filePath.substring(filePath.lastIndexOf(File.separator) + 1, filePath.lastIndexOf('.'));
  }

  protected String getFullName(String methodName, String dllName) {
    var separators = List.of("<", "(");
    for (var separator : separators) {
      if (methodName.contains(separator)) {
        methodName = methodName.substring(0, methodName.indexOf(separator));
      }
    }

    // Remove the generic arguments part from the method name.
    // GenericClass`2.Method -> GenericClass.Method
    if (methodName.contains("`")) {
      var regexPattern = "`\\d+";
      methodName = methodName.replaceAll(regexPattern, "");
    }
    methodName = methodName.replace('+', '.');
    return String.join(".", dllName, methodName);
  }
}
