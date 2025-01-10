/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource SA
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
import java.util.List;
import java.util.function.Consumer;
import java.util.Map;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.scanner.ScannerSide;

@ScannerSide
public class NUnitTestResultsParser implements UnitTestResultParser {
  private static final Logger LOG = LoggerFactory.getLogger(NUnitTestResultsParser.class);

  @Override
  public void parse(File file, Map<String, UnitTestResults> unitTestResults, Map<String, String> methodFileMap) {
    LOG.info("Parsing the NUnit Test Results file '{}'.", file.getAbsolutePath());
    var parser = new Parser(file, unitTestResults, methodFileMap, List.of("test-run", "test-results"));

    Map<String, Consumer<XmlParserHelper>> tagHandlers = Map.of(
      "test-suite", parser::handleTestSuiteTag,
      "test-case", parser::handleTestCaseTag
    );

    parser.parse(tagHandlers);
  }

  private static class Parser extends XmlTestReportParser {

    private String dllName;

    Parser(File file,  Map<String, UnitTestResults> unitTestResults, Map<String, String> methodFileMap, List<String> rootTags) {
      super(file, unitTestResults, methodFileMap, rootTags);
    }

    private void handleTestSuiteTag(XmlParserHelper xmlParserHelper) {
      var type = xmlParserHelper.getRequiredAttribute("type");
      if(type.equals("Assembly")) {
        var assemblyName = extractName(xmlParserHelper);
        dllName = extractDllNameFromFilePath(assemblyName);
        LOG.debug("NUnit Assembly found, assembly: {}, Extracted dllName: {}", assemblyName, dllName);
      }
    }

    private void handleTestCaseTag(XmlParserHelper xmlParserHelper) {
      var result = xmlParserHelper.getRequiredAttribute("result");
      var label = xmlParserHelper.getAttribute("label");
      var executionTime = extractExecutionTime(xmlParserHelper);
      var testResults = new NUnitTestResults(result, label, executionTime);
      var name = extractName(xmlParserHelper);
      var fullName = getFullName(name, dllName);

      addTestResultToFile(fullName, testResults);
    }

    private static Long extractExecutionTime(XmlParserHelper xmlParserHelper) {
      // NUnit 3
      var time = xmlParserHelper.getDoubleAttribute("duration");
      if (time == null) {
        // NUnit 2
        time = xmlParserHelper.getDoubleAttribute("time");
      }
      return time == null ? null : (long) (time * 1000);
    }

    private static String extractName(XmlParserHelper xmlParserHelper) {
      // NUnit 3
      var name = xmlParserHelper.getAttribute("fullname");
      if (name == null) {
        // NUnit 2
        name = xmlParserHelper.getRequiredAttribute("name");
      }
      return name;
    }
  }
}
