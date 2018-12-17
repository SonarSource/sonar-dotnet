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
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.time.Instant;
import java.time.LocalDateTime;
import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.time.temporal.TemporalField;
import java.util.Date;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.sonar.api.utils.Duration;
import org.sonar.api.utils.log.Logger;
import org.sonar.api.utils.log.Loggers;
import org.sonar.plugins.dotnet.tests.UnitTestResult.Status;

import static java.time.temporal.ChronoField.MILLI_OF_SECOND;

public class VisualStudioTestResultsFileParser implements UnitTestResultsParser {

  private static final Logger LOG = Loggers.get(VisualStudioTestResultsFileParser.class);

  @Override
  public void accept(File file, UnitTestResults unitTestResults) {
    LOG.info("Parsing the Visual Studio Test Results file " + file.getAbsolutePath());
    new Parser(file, unitTestResults).parse();
  }

  private static class Parser {

    private final File file;
    private final UnitTestResults unitTestResults;
    private DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSSXXX");
    private Pattern millisecondsPattern = Pattern.compile("(\\.([0-9]{0,3}))[0-9]*+");

    private boolean foundCounters;
    private UnitTestResult currentTestResult;

    public Parser(File file, UnitTestResults unitTestResults) {
      this.file = file;
      this.unitTestResults = unitTestResults;
    }

    public void parse() {
      try (XmlParserHelper xmlParserHelper = new XmlParserHelper(file)) {
        checkRootTag(xmlParserHelper);
        dispatchTags(xmlParserHelper);

        if (!foundCounters) {
          throw new IllegalArgumentException("The mandatory <Counters> tag is missing in " + file.getAbsolutePath());
        }
      } catch (IOException e) {
        throw new IllegalStateException("Unable to close report", e);
      }
    }

    private void dispatchTags(XmlParserHelper xmlParserHelper) {
      String tagName;
      while ((tagName = xmlParserHelper.nextStartTag()) != null) {
        if ("Counters".equals(tagName)) {
          this.currentTestResult = null;
          handleCountersTag(xmlParserHelper);
        } else if ("Times".equals(tagName)) {
          this.currentTestResult = null;
          handleTimesTag(xmlParserHelper);
        } else if ("UnitTestResult".equals(tagName)) {
          this.currentTestResult = null;
          handleUnitTestResultTag(xmlParserHelper);
        } else if ("UnitTest".equals(tagName)) {
          this.currentTestResult = null;
          handleUnitTestTag(xmlParserHelper);
        } else if ("TestMethod".equals(tagName)) {
          handleTestMethodTag(xmlParserHelper);
        }
      }
    }

    private void handleUnitTestResultTag(XmlParserHelper xmlParserHelper) {
      String testId = xmlParserHelper.getAttribute("testId");

      String durationText = xmlParserHelper.getAttribute("duration");
      LocalTime duration = LocalTime.parse(durationText, DateTimeFormatter.ofPattern("HH:mm:ss.SSSSSSS"));

      String outcome = xmlParserHelper.getAttribute("outcome"); // Passed, Failed, NotExecuted
      UnitTestResult.Status status;
      switch (outcome) {
        case "PASSED":
          status = Status.PASSED;
          break;

        case "NotExecuted":
          status = Status.SKIPPED;
          break;

        case "Failed":
          status = Status.ERROR;
          break;

        default:
          status = Status.PASSED;
          break;
      }

      this.unitTestResults.getTestResults().add(new UnitTestResult(duration.getLong(MILLI_OF_SECOND), status, testId));
    }

    private void handleUnitTestTag(XmlParserHelper xmlParserHelper) {
      String id = xmlParserHelper.getAttribute("id");

      this.currentTestResult = this.unitTestResults.getTestResults()
        .stream()
        .filter(test -> test.getFullyQualifiedName().equals(id))
        .findFirst()
        .orElse(null);
    }

    private void handleTestMethodTag(XmlParserHelper xmlParserHelper) {
      String className = xmlParserHelper.getAttribute("className");
      String testName = xmlParserHelper.getAttribute("name");

      this.currentTestResult.setFullyQualifiedName(className + "." + testName);
    }

    private void handleCountersTag(XmlParserHelper xmlParserHelper) {
      foundCounters = true;

      int total = xmlParserHelper.getIntAttributeOrZero("total");
      int failed = xmlParserHelper.getIntAttributeOrZero("failed");
      int errors = xmlParserHelper.getIntAttributeOrZero("error");
      int timeout = xmlParserHelper.getIntAttributeOrZero("timeout");
      int aborted = xmlParserHelper.getIntAttributeOrZero("aborted");
      int executed = xmlParserHelper.getIntAttributeOrZero("executed");

      // IgnoreAttribute and Assert.Inconclusive do not appear in the trx (xml attributes are always 0).
      // There is no official documentation but it seems like the only way to get skipped tests is to do the following
      // maths.
      int skipped = total - executed;
      int failures = timeout + failed + aborted;

      unitTestResults.add(total, skipped, failures, errors, null);
    }

    private void handleTimesTag(XmlParserHelper xmlParserHelper) {
      Date start = getRequiredDateAttribute(xmlParserHelper, "start");
      Date finish = getRequiredDateAttribute(xmlParserHelper, "finish");
      long duration = finish.getTime() - start.getTime();

      unitTestResults.add(0, 0, 0, 0, duration);
    }

    private Date getRequiredDateAttribute(XmlParserHelper xmlParserHelper, String name) {
      String value = xmlParserHelper.getRequiredAttribute(name);
      try {
        value = keepOnlyMilliseconds(value);
        return dateFormat.parse(value);
      } catch (ParseException e) {
        throw xmlParserHelper.parseError("Expected an valid date and time instead of \"" + value + "\" for the attribute \"" + name + "\". " + e.getMessage());
      }
    }

    private String keepOnlyMilliseconds(String value) {
      StringBuffer sb = new StringBuffer();

      Matcher matcher = millisecondsPattern.matcher(value);
      StringBuilder trailingZeros = new StringBuilder();
      while (matcher.find()) {
        String milliseconds = matcher.group(2);
        trailingZeros.setLength(0);
        for (int i = 0; i < 3 - milliseconds.length(); i++) {
          trailingZeros.append("0");
        }
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
