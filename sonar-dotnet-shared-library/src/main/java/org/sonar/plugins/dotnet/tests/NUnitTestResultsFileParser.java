/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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

import javax.annotation.CheckForNull;
import java.io.File;
import java.io.IOException;

public class NUnitTestResultsFileParser implements UnitTestResultsParser {

  private static final Logger LOG = Loggers.get(NUnitTestResultsFileParser.class);

  @Override
  public void accept(File file, UnitTestResults unitTestResults) {
    LOG.info("Parsing the NUnit Test Results file " + file.getAbsolutePath());
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
        String rootTag = xmlParserHelper.nextStartTag();
        if ("test-results".equals(rootTag)) {
          handleTestResultsTag(xmlParserHelper);
        } else if ("test-run".equals(rootTag)) {
          handleTestRunTag(xmlParserHelper);
        } else {
          throw xmlParserHelper.parseError("Unrecognized root element <" + rootTag + ">");
        }

      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void handleTestResultsTag(XmlParserHelper xmlParserHelper) {
      int total = xmlParserHelper.getRequiredIntAttribute("total");
      int errors = xmlParserHelper.getRequiredIntAttribute("errors");
      int failures = xmlParserHelper.getRequiredIntAttribute("failures");
      int inconclusive = xmlParserHelper.getRequiredIntAttribute("inconclusive");
      int ignored = xmlParserHelper.getRequiredIntAttribute("ignored");
      int skipped = xmlParserHelper.getRequiredIntAttribute("skipped");

      int totalSkipped = skipped + inconclusive + ignored;

      Double duration = readExecutionTimeFromDirectlyNestedTestSuiteTags(xmlParserHelper);
      Long executionTime = duration != null ? (long) duration.doubleValue() : null;

      unitTestResults.add(total, totalSkipped, failures, errors, executionTime);
    }

    private void handleTestRunTag(XmlParserHelper xmlParserHelper) {
      int total = xmlParserHelper.getRequiredIntAttribute("total");
      int failures = xmlParserHelper.getRequiredIntAttribute("failed");
      int inconclusive = xmlParserHelper.getRequiredIntAttribute("inconclusive");
      int skipped = xmlParserHelper.getRequiredIntAttribute("skipped");

      int totalSkipped = skipped + inconclusive;

      Double duration = xmlParserHelper.getDoubleAttribute("duration");
      Long executionTime = duration != null ? (long) (duration * 1000) : null;

      int errors = readErrorCountFromNestedTestCaseTags(xmlParserHelper);

      unitTestResults.add(total, totalSkipped, failures, errors, executionTime);
    }

    @CheckForNull
    private static Double readExecutionTimeFromDirectlyNestedTestSuiteTags(XmlParserHelper xmlParserHelper) {
      Double executionTime = null;

      String tag;
      int level = 0;
      while ((tag = xmlParserHelper.nextStartOrEndTag()) != null) {
        if ("<test-suite>".equals(tag)) {
          level++;
          Double time = xmlParserHelper.getDoubleAttribute("time");

          if (level == 1 && time != null) {
            if (executionTime == null) {
              executionTime = 0d;
            }
            executionTime += time * 1000;
          }
        } else if ("</test-suite>".equals(tag)) {
          level--;
        }
      }

      return executionTime;
    }

    private static int readErrorCountFromNestedTestCaseTags(XmlParserHelper xmlParserHelper) {
      int errors = 0;

      String tag;
      int level = 0;
      while ((tag = xmlParserHelper.nextStartOrEndTag()) != null) {
        if ("<test-case>".equals(tag)) {
          level++;
          String label = xmlParserHelper.getAttribute("label");

          if (level == 1 && "Error".equals(label)) {
            errors++;
          }
        } else if ("</test-case>".equals(tag)) {
          level--;
        }
      }

      return errors;
    }
  }

}
