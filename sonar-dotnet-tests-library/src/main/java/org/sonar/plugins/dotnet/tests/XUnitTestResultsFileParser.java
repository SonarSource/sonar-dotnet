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

import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.File;
import java.io.IOException;
import org.sonar.plugins.dotnet.tests.UnitTestResult.Status;

public class XUnitTestResultsFileParser implements UnitTestResultsParser {

  private static final Logger LOG = Loggers.get(XUnitTestResultsFileParser.class);

  @Override
  public void accept(File file, UnitTestResults unitTestResults) {
    LOG.info("Parsing the XUnit Test Results file " + file.getAbsolutePath());
    new Parser(file, unitTestResults).parse();
  }

  private static class Parser {

    private final File file;
    private final UnitTestResults unitTestResults;

    Parser(File file, UnitTestResults unitTestResults) {
      this.file = file;
      this.unitTestResults = unitTestResults;
    }

    public void parse() {
      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {

        String tagName = xmlParserHelper.nextStartTag();
        if (!"assemblies".equals(tagName) && !"assembly".equals(tagName)) {
          throw xmlParserHelper.parseError("Expected either an <assemblies> or an <assembly> root tag, but got <" + tagName + "> instead.");
        }

        do {
          if ("assembly".equals(tagName)) {
            handleAssemblyTag(xmlParserHelper);
          } else if ("test".equals(tagName)) {
            handleTestTag(xmlParserHelper);
          }
        } while ((tagName = xmlParserHelper.nextStartTag()) != null);
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void handleAssemblyTag(XmlParserHelper xmlParserHelper) {

      String totalString = xmlParserHelper.getAttribute("total");
      if (totalString == null) {
        LOG.warn("One of the assemblies contains no test result, please make sure this is expected.");
        return;
      }

      int total = xmlParserHelper.tagToIntValue("total", totalString);
      int failed = xmlParserHelper.getRequiredIntAttribute("failed");
      int skipped = xmlParserHelper.getRequiredIntAttribute("skipped");
      int errors = xmlParserHelper.getIntAttributeOrZero("errors");

      Double time = xmlParserHelper.getDoubleAttribute("time");
      Long executionTime = time != null ? (long) (time * 1000) : null;

      unitTestResults.add(total, skipped, failed, errors, executionTime);
    }

    private void handleTestTag(XmlParserHelper xmlParserHelper) {
      Double time = xmlParserHelper.getDoubleAttribute("time");
      Long executionTime = time != null ? (long) (time * 1000) : null;

      String result = xmlParserHelper.getAttribute("result");
      Status status;
      switch (result) {
        case "Pass":
          status = Status.PASSED;
          break;

        case "Fail":
          status = Status.FAILED;
          break;

        case "Skip":
          status = Status.SKIPPED;
          break;

        default:
          status = Status.PASSED;
          break;
      }

      String testName = xmlParserHelper.getAttribute("method");
      String typeName = xmlParserHelper.getAttribute("type");

      this.unitTestResults.getTestResults().add(new UnitTestResult(executionTime, status, typeName + "." + testName));
    }

  }

}
