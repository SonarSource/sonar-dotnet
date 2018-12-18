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

import java.util.ArrayList;
import java.util.List;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;

import java.io.File;
import java.io.IOException;
import org.sonar.plugins.dotnet.tests.UnitTestResult.Status;

public class XUnitTestResultsFileParser implements UnitTestResultsParser {

  private static final Logger LOG = Loggers.get(XUnitTestResultsFileParser.class);

  @Override
  public List<UnitTestResult> apply(File file) {
    LOG.info("Parsing the XUnit Test Results file " + file.getAbsolutePath());
    return Parser.parse(file);
  }

  private static class Parser {

    public static List<UnitTestResult> parse(File file) {

      List<UnitTestResult> testResults = new ArrayList<>();

      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {

        String tagName = xmlParserHelper.nextStartTag();
        if (!"assemblies".equals(tagName) && !"assembly".equals(tagName)) {
          throw xmlParserHelper.parseError("Expected either an <assemblies> or an <assembly> root tag, but got <" + tagName + "> instead.");
        }

        while ((tagName = xmlParserHelper.nextStartTag()) != null) {
          if ("test".equals(tagName)) {
            testResults.add(handleTestTag(xmlParserHelper));
          }
        }
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }

      return testResults;
    }

    private static UnitTestResult handleTestTag(XmlParserHelper xmlParserHelper) {
      Double time = xmlParserHelper.getDoubleAttribute("time");
      Long executionTime = time != null ? (long) (time * 1000) : null;

      Status status = getStatus(xmlParserHelper);

      String testName = xmlParserHelper.getAttribute("method");
      String typeName = xmlParserHelper.getAttribute("type");

      return new UnitTestResult(executionTime, status, typeName + "." + testName);
    }

    private static Status getStatus(XmlParserHelper xmlParserHelper) {
      String result = xmlParserHelper.getAttribute("result");

      switch (result) {
        case "Pass":
          return Status.PASSED;

        case "Fail":
          return Status.FAILED;

        case "Skip":
          return Status.SKIPPED;

        default:
          return Status.PASSED;
      }
    }

  }

}
