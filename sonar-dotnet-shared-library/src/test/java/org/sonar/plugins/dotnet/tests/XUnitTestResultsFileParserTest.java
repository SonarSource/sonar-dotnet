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

import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import java.io.File;

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
    assertThat(results.tests()).isEqualTo(0);
    assertThat(results.skipped()).isEqualTo(0);
    assertThat(results.failures()).isEqualTo(0);
    assertThat(results.errors()).isEqualTo(0);
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
  }

  @Test
  public void valid_xunit_1_9_2() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/valid_xunit-1.9.2.xml"), results);

    assertThat(results.failures()).isEqualTo(1);
    assertThat(results.errors()).isEqualTo(0);
    assertThat(results.skipped()).isEqualTo(2);
    assertThat(results.tests()).isEqualTo(6);
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
  }

  @Test
  public void empty() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/empty.xml"), results);

    assertThat(logTester.logs(LoggerLevel.WARN)).contains("One of the assemblies contains no test result, please make sure this is expected.");
    assertThat(results.tests()).isEqualTo(0);
    assertThat(results.skipped()).isEqualTo(0);
    assertThat(results.failures()).isEqualTo(0);
    assertThat(results.errors()).isEqualTo(0);
    assertThat(results.executionTime()).isNull();
  }

  @Test
  public void valid_no_total() {
    UnitTestResults results = new UnitTestResults();
    new XUnitTestResultsFileParser().accept(new File("src/test/resources/xunit/valid_no_total.xml"), results);

    assertThat(logTester.logs(LoggerLevel.WARN)).contains("One of the assemblies contains no test result, please make sure this is expected.");
    assertThat(results.tests()).isEqualTo(0);
    assertThat(results.skipped()).isEqualTo(0);
    assertThat(results.failures()).isEqualTo(0);
    assertThat(results.errors()).isEqualTo(0);
    assertThat(results.executionTime()).isNull();
  }
}
