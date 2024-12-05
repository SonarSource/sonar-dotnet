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
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class NUnitTestResultsFileParserOldTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void no_counters() {
    thrown.expect(ParseErrorException.class);
    thrown.expectMessage("Missing attribute \"total\" in element <test-results> in ");
    thrown.expectMessage(new File("src/test/resources/nunit/old/no_counters.xml").getAbsolutePath());
    new NUnitTestResultsFileParserOld().accept(new File("src/test/resources/nunit/old/no_counters.xml"), mock(UnitTestResults.class));
  }

  @Test
  public void wrong_passed_number() {
    thrown.expect(ParseErrorException.class);
    thrown.expectMessage("Expected an integer instead of \"invalid\" for the attribute \"total\" in ");
    thrown.expectMessage(new File("src/test/resources/nunit/old/invalid_total.xml").getAbsolutePath());
    new NUnitTestResultsFileParserOld().accept(new File("src/test/resources/nunit/old/invalid_total.xml"), mock(UnitTestResults.class));
  }

  @Test
  public void valid() {
    UnitTestResults results = new UnitTestResults();
    new NUnitTestResultsFileParserOld().accept(new File("src/test/resources/nunit/old/valid.xml"), results);

    assertThat(results.errors()).isEqualTo(30);
    assertThat(results.failures()).isEqualTo(20);
    assertThat(results.tests()).isEqualTo(200);
    assertThat(results.skipped()).isEqualTo(9); // 4 + 3 + 2
    assertThat(results.executionTime()).isEqualTo(51);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the NUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(debugLogs.get(0)).startsWith("The current user dir is '");
    assertThat(debugLogs.get(1)).startsWith("Parsed NUnit results - total: 200, totalSkipped: 9, failures: 20, errors: 30, execution time: 51.");
  }

  @Test
  public void valid_comma_in_double() {
    UnitTestResults results = new UnitTestResults();
    new NUnitTestResultsFileParserOld().accept(new File("src/test/resources/nunit/old/valid_comma_in_double.xml"), results);

    assertThat(results.executionTime()).isEqualTo(1051);

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(debugLogs.get(1)).startsWith("Parsed NUnit results - total: 200, totalSkipped: 9, failures: 20, errors: 30, execution time: 1051.");
  }

  @Test
  public void valid_no_execution_time() {
    UnitTestResults results = new UnitTestResults();
    new NUnitTestResultsFileParserOld().accept(new File("src/test/resources/nunit/old/valid_no_execution_time.xml"), results);

    assertThat(results.failures()).isEqualTo(20);
    assertThat(results.errors()).isEqualTo(30);
    assertThat(results.tests()).isEqualTo(200);
    assertThat(results.skipped()).isEqualTo(9); // 4 + 3 + 2
    assertThat(results.executionTime()).isNull();

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(debugLogs.get(1)).startsWith("Parsed NUnit results - total: 200, totalSkipped: 9, failures: 20, errors: 30, execution time: null.");
  }

  @Test
  public void nunit3_sample() {
    UnitTestResults results = new UnitTestResults();
    new NUnitTestResultsFileParserOld().accept(new File("src/test/resources/nunit/old/nunit3_sample.xml"), results);

    assertThat(results.failures()).isEqualTo(2);
    assertThat(results.errors()).isEqualTo(1);
    assertThat(results.tests()).isEqualTo(18);
    assertThat(results.skipped()).isEqualTo(4); // 1 + 3
    assertThat(results.executionTime()).isEqualTo(154);

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(debugLogs.get(1)).startsWith("Parsed NUnit test run - total: 18, totalSkipped: 4, failures: 2, errors: 1, execution time: 154.");
  }

}
