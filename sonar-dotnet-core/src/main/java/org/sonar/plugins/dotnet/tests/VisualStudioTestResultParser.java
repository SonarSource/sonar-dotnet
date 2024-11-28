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
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class VisualStudioTestResultParser implements UnitTestResultParser {

  private static final Logger LOG = LoggerFactory.getLogger(VisualStudioTestResultParser.class);
  private final Map<String, String> methodFileMap;

  VisualStudioTestResultParser(Map<String, String> methodFileMap) {
    this.methodFileMap = methodFileMap;
  }

  @Override
  public void accept(File file, Map<String, UnitTestResults> unitTestResults) {
    LOG.info("Parsing the Visual Studio Test Results file '{}'.", file.getAbsolutePath());
    new Parser(file, unitTestResults, this.methodFileMap).parse();
  }

  private static class Parser {
    private final File file;
    private final Map<String, UnitTestResults> testIdTestResultMap;
    private final Map<String, UnitTestResults> unitTestResults;
    // Date Format: // https://github.com/microsoft/vstest/blob/7d34b30433259fb914aaaf276fde663a47b6ef2f/src/Microsoft.TestPlatform.Extensions.TrxLogger/XML/XmlPersistence.cs#L557-L572
    private final DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSXXX");
    private final Pattern millisecondsPattern = Pattern.compile("(\\.(\\d{0,3}))\\d*+");
    private final Map<String, String> methodFileMap;

    Parser(File file,  Map<String, UnitTestResults> unitTestResults, Map<String, String> methodFileMap) {
      this.file = file;
      this.unitTestResults = unitTestResults;
      this.testIdTestResultMap = new HashMap<>();
      this.methodFileMap = methodFileMap;
    }

    public void parse() {
      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {
        checkRootTag(xmlParserHelper);
        dispatchTags(xmlParserHelper);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("UnitTestResult".equals(tagName)) {
          handleUnitTestResultTag(xmlParserHelper);
        } else if ("UnitTest".equals(tagName)) {
          handleUnitTestTag(xmlParserHelper);
        }
      }
    }

    private void handleUnitTestResultTag(XmlParserHelper xmlParserHelper) {
      String testId = xmlParserHelper.getRequiredAttribute("testId");
      String outcome = xmlParserHelper.getRequiredAttribute("outcome");
      Date start = getRequiredDateAttribute(xmlParserHelper, "startTime");
      Date finish = getRequiredDateAttribute(xmlParserHelper, "endTime");
      long duration = finish.getTime() - start.getTime();

      var testResult = new VisualStudioTestResults(outcome, duration);
      testIdTestResultMap.put(testId, testResult);
      LOG.debug("Parsed Visual Studio Unit Test - testId: {} outcome: {}, startTime: {}, endTime: {}",
        testId, outcome, start, finish);
    }

    private void handleUnitTestTag(XmlParserHelper xmlParserHelper) {
      String testId = xmlParserHelper.getRequiredAttribute("id");

      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("TestMethod".equals(tagName)) {
          break;
        }
      }
      if (tagName == null){
        throw new ParseErrorException("No TestMethod attribute found on UnitTest tag");
      }

      String methodName = xmlParserHelper.getRequiredAttribute("name");
      String className = xmlParserHelper.getRequiredAttribute("className");
      String codeBase = xmlParserHelper.getRequiredAttribute("codeBase");

      String dllName = codeBase.substring(codeBase.lastIndexOf(File.separator) + 1, codeBase.lastIndexOf('.'));

      String fullyQualifiedName = dllName + "." + className + "." + methodName;

      associateTestIdTestResultWithFile(testId, fullyQualifiedName);
    }

    public void associateTestIdTestResultWithFile(String testId, String methodFullName) {
      if(!methodFileMap.containsKey(methodFullName)) {
        throw new IllegalStateException(String.format("Test method %s with testId %s cannot be mapped to the test source file", methodFullName, testId));
      }

      String fileName = methodFileMap.get(methodFullName);

      if (unitTestResults.containsKey(fileName)) {
        var fileTestResult = unitTestResults.get(fileName);
        var testIdTestResult = testIdTestResultMap.get(testId);
        fileTestResult.add(testIdTestResult);
      } else {
        unitTestResults.put(fileName, testIdTestResultMap.get(testId));
      }

      LOG.debug("Associated Visual Studio Unit Test to File - file: {}, TestId: {} MethodFullName: {}",
        fileName, testId, methodFullName);
    }

    private Date getRequiredDateAttribute(XmlParserHelper xmlParserHelper, String name) {
      String value = xmlParserHelper.getRequiredAttribute(name);
      try {
        value = keepOnlyMilliseconds(value);
        return dateFormat.parse(value);
      } catch (ParseException e) {
        throw xmlParserHelper.parseError("Expected a valid date and time instead of \"" + value + "\" for the attribute \"" + name + "\". " + e.getMessage());
      }
    }

    private String keepOnlyMilliseconds(String value) {
      StringBuilder sb = new StringBuilder();

      Matcher matcher = millisecondsPattern.matcher(value);
      StringBuilder trailingZeros = new StringBuilder();
      while (matcher.find()) {
        String milliseconds = matcher.group(2);
        trailingZeros.setLength(0);
        trailingZeros.append("0".repeat(Math.max(0, 3 - milliseconds.length())));
        matcher.appendReplacement(sb, "$1" + trailingZeros);
      }
      matcher.appendTail(sb);

      return sb.toString();
    }

    private static void checkRootTag(XmlParserHelper xmlParserHelper) {
      xmlParserHelper.checkRootTag("TestRun");
    }
  }
}
