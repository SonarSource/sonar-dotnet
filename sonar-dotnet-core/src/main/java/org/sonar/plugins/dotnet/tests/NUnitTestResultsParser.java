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
      String type = xmlParserHelper.getRequiredAttribute("type");
      if(type.equals("Assembly")) {
        String assemblyName = xmlParserHelper.getAttribute("fullname");
        if (assemblyName == null) {
          // NUnit2 does not have filepath in 'fullname' attribute but in 'name'
          assemblyName = xmlParserHelper.getRequiredAttribute("name");
        }
        dllName = extractDllNameFromFilePath(assemblyName);
        LOG.debug("NUnit Assembly found, assembly: {}, Extracted dllName: {}", assemblyName, dllName);
      }
    }

    private void handleTestCaseTag(XmlParserHelper xmlParserHelper) {
      String result = xmlParserHelper.getRequiredAttribute("result");
      String label = xmlParserHelper.getAttribute("label");

      // NUnit 3 time format
      Double time = xmlParserHelper.getDoubleAttribute("duration");
      var executionTime = time == null ? null : (long) (time * 1000);

      if(time == null) {
        // Nunit 2 time format
        time = xmlParserHelper.getDoubleAttribute("time");
        executionTime = time == null ? null : (long) (time * 1000);
      }

      var testResults = new NUnitTestResults(result, label, executionTime);

      String name = xmlParserHelper.getAttribute("fullname");
      if (name == null) {
        // NUnit2 does not have the class & method name in 'fullname' attribute but in 'name'
        name = xmlParserHelper.getRequiredAttribute("name");
      }

      if(name.contains("(")) {
        name = name.substring(0, name.indexOf('('));
      }
      String fullName = String.join(".", dllName, name);

      addTestResultToFile(fullName, testResults);
    }
  }
}
