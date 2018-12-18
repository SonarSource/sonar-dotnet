/*
 * SonarQube .NET Tests Library
 * Copyright (C) 2014-2018 SonarSource SA
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
import java.io.IOException;
import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonar.plugins.dotnet.tests.UnitTestResult.Status;

public class VisualStudioTestResultsFileParser implements UnitTestResultsParser {

  private static final Logger LOG = Loggers.get(VisualStudioTestResultsFileParser.class);

  @Override
  public List<UnitTestResult> apply(File file) {
    LOG.info("Parsing the Visual Studio Test Results file " + file.getAbsolutePath());
    return new Parser().parse(file);
  }

  private static class Parser {

    private UnitTestResult currentTestResult;

    public List<UnitTestResult> parse(File file) {

      List<UnitTestResult> testResults = new ArrayList<>();

      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {
        xmlParserHelper.checkRootTag("TestRun");

        String tagName;
        while ((tagName = xmlParserHelper.nextStartTag()) != null) {
          if ("UnitTestResult".equals(tagName)) {
            this.currentTestResult = null;
            testResults.add(handleUnitTestResultTag(xmlParserHelper));
          } else if ("UnitTest".equals(tagName)) {
            this.currentTestResult = null;
            handleUnitTestTag(xmlParserHelper, testResults);
          } else if ("TestMethod".equals(tagName)) {
            handleTestMethodTag(xmlParserHelper);
          }
        }
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }

      return testResults;
    }

    private UnitTestResult handleUnitTestResultTag(XmlParserHelper xmlParserHelper) {
      String testId = xmlParserHelper.getAttribute("testId");

      String durationText = xmlParserHelper.getAttribute("duration");
      LocalTime duration = LocalTime.parse(durationText, DateTimeFormatter.ofPattern("HH:mm:ss.SSSSSSS"));

      Status status = getStatus(xmlParserHelper);

      return new UnitTestResult(duration.toNanoOfDay(), status, testId);
    }

    private Status getStatus(XmlParserHelper xmlParserHelper) {
      String outcome = xmlParserHelper.getAttribute("outcome"); // Passed, Failed, NotExecuted

      switch (outcome) {
        case "PASSED":
          return Status.PASSED;

        case "NotExecuted":
          return Status.SKIPPED;

        case "Failed":
          return Status.FAILED;

        default:
          return Status.PASSED;
      }
    }

    private void handleUnitTestTag(XmlParserHelper xmlParserHelper, List<UnitTestResult> testResults) {
      String id = xmlParserHelper.getAttribute("id");

      this.currentTestResult = testResults.stream()
        .filter(test -> test.getFullyQualifiedName().equals(id))
        .findFirst()
        .orElse(null);
    }

    private void handleTestMethodTag(XmlParserHelper xmlParserHelper) {
      String className = xmlParserHelper.getAttribute("className");
      String testName = xmlParserHelper.getAttribute("name");

      this.currentTestResult.setFullyQualifiedName(className + "." + testName);
    }
  }

}
