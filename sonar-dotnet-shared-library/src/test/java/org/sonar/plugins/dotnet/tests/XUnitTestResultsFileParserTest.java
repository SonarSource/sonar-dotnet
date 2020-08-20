/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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
import java.util.List;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class XUnitTestResultsFileParserTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void no_counters() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/no_counters.xml"), results);

    assertThat(logTester.logs(LoggerLevel.WARN)).contains("One of the assemblies contains no test result, please make sure this is expected.");
    assertThat(results.tests()).isZero();
    assertThat(results.skipped()).isZero();
    assertThat(results.failures()).isZero();
    assertThat(results.errors()).isZero();
    assertThat(results.executionTime()).isNull();
  }

  @Test
  public void wrong_passed_number() {
    thrown.expect(ParseErrorException.class);
    thrown.expectMessage("Expected an integer instead of \"invalid\" for the attribute \"total\" in ");
    thrown.expectMessage(new File("src/test/resources/xunit/invalid_total.xml").getAbsolutePath());
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/invalid_total.xml"), mock(UnitTestResults.class));
  }

  @Test
  public void invalid_root() {
    thrown.expect(ParseErrorException.class);
    thrown.expectMessage("Expected either an <assemblies> or an <assembly> root tag, but got <foo> instead.");
    thrown.expectMessage(new File("src/test/resources/xunit/invalid_root.xml").getAbsolutePath());
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/invalid_root.xml"), mock(UnitTestResults.class));
  }

  @Test
  public void valid() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/valid.xml"), results);

    assertThat(results.failures()).isEqualTo(3);
    assertThat(results.errors()).isEqualTo(5);
    assertThat(results.tests()).isEqualTo(17);
    assertThat(results.skipped()).isEqualTo(4);
    assertThat(results.executionTime()).isEqualTo(227 + 228);

    List<String> infoLogs = logTester.logs(LoggerLevel.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the XUnit Test Results file");

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1)).isEqualTo("Parsed XUnit test results - total: 5, failed: 2, skipped: 0, errors: 0, executionTime: 227.");
    assertThat(debugLogs.get(2)).isEqualTo("Parsed XUnit test results - total: 12, failed: 1, skipped: 4, errors: 5, executionTime: 228.");
  }

  @Test
  public void valid_xunit_1_9_2() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/valid_xunit-1.9.2.xml"), results);

    assertThat(results.failures()).isEqualTo(1);
    assertThat(results.errors()).isZero();
    assertThat(results.skipped()).isEqualTo(2);
    assertThat(results.tests()).isEqualTo(6);

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(debugLogs.get(1)).isEqualTo("Parsed XUnit test results - total: 6, failed: 1, skipped: 2, errors: 0, executionTime: 297.");
  }

  @Test
  public void should_not_fail_without_execution_time() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/no_execution_time.xml"), results);

    assertThat(results.failures()).isEqualTo(3);
    assertThat(results.errors()).isEqualTo(5);
    assertThat(results.tests()).isEqualTo(17);
    assertThat(results.skipped()).isEqualTo(4);
    assertThat(results.executionTime()).isNull();

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(debugLogs.get(1)).isEqualTo("Parsed XUnit test results - total: 5, failed: 2, skipped: 0, errors: 0, executionTime: null.");
    assertThat(debugLogs.get(2)).isEqualTo("Parsed XUnit test results - total: 12, failed: 1, skipped: 4, errors: 5, executionTime: null.");
  }

  @Test
  public void empty() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/empty.xml"), results);

    assertThat(logTester.logs(LoggerLevel.WARN)).contains("One of the assemblies contains no test result, please make sure this is expected.");
    assertThat(results.tests()).isZero();
    assertThat(results.skipped()).isZero();
    assertThat(results.failures()).isZero();
    assertThat(results.errors()).isZero();
    assertThat(results.executionTime()).isNull();

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(1);
  }

  @Test
  public void valid_no_total() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/valid_no_total.xml"), results);

    assertThat(logTester.logs(LoggerLevel.WARN)).contains("One of the assemblies contains no test result, please make sure this is expected.");
    assertThat(results.tests()).isZero();
    assertThat(results.skipped()).isZero();
    assertThat(results.failures()).isZero();
    assertThat(results.errors()).isZero();
    assertThat(results.executionTime()).isNull();

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(1);
  }
}
