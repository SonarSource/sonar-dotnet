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

public class VisualStudioTestResultsFileParserOldTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void no_counters() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("The mandatory <Counters> tag is missing in ");
    thrown.expectMessage(new File("src/test/resources/visualstudio_test_results/old/no_counters.trx").getAbsolutePath());
    new VisualStudioTestResultsFileParserOld().accept(new File("src/test/resources/visualstudio_test_results/old/no_counters.trx"), mock(UnitTestResults.class));
  }

  @Test
  public void valid() {
    UnitTestResults results = new UnitTestResults();
    new VisualStudioTestResultsFileParserOld().accept(new File("src/test/resources/visualstudio_test_results/old/valid.trx"), results);

    assertThat(results.failures()).isEqualTo(14);
    assertThat(results.errors()).isEqualTo(3);
    assertThat(results.tests()).isEqualTo(43);
    assertThat(results.skipped()).isEqualTo(12); // 43 - 31
    assertThat(results.executionTime()).isEqualTo(816l);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the Visual Studio Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("The current user dir is '");
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Times - duration: 816.");
    assertThat(logTester.logs(Level.DEBUG).get(2)).startsWith("Parsed Visual Studio Test Counters - total: 43, failed: 2, errors: 3, timeout: 5, aborted: 7, executed: 31.");
  }

  @Test
  public void valid_missing_attributes() {
    UnitTestResults results = new UnitTestResults();
    new VisualStudioTestResultsFileParserOld().accept(new File("src/test/resources/visualstudio_test_results/old/valid_missing_attributes.trx"), results);

    assertThat(results.tests()).isEqualTo(3);
    assertThat(results.skipped()).isZero();
    assertThat(results.failures()).isZero();
    assertThat(results.errors()).isZero();

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Counters - total: 3, failed: 0, errors: 0, timeout: 0, aborted: 0, executed: 3.");
  }

  @Test
  public void invalid_date() {
    UnitTestResults results = new UnitTestResults();
    thrown.expect(ParseErrorException.class);
    new VisualStudioTestResultsFileParserOld().accept(new File("src/test/resources/visualstudio_test_results/old/invalid_date.trx"), results);

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Counters - total: 3, failed: 0, errors: 0, timeout: 0, aborted: 0, executed: 3.");
  }

  @Test
  public void invalid_entity_does_not_fail() {
    UnitTestResults results = new UnitTestResults();
    new VisualStudioTestResultsFileParserOld().accept(new File("src/test/resources/visualstudio_test_results/old/invalid_entities.trx"), results);

    assertThat(results.failures()).isEqualTo(14);
    assertThat(results.errors()).isEqualTo(3);
    assertThat(results.tests()).isEqualTo(43);
    assertThat(results.skipped()).isEqualTo(12);
    assertThat(results.executionTime()).isEqualTo(816l);

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Times - duration: 816.");
    assertThat(logTester.logs(Level.DEBUG).get(2)).startsWith("Parsed Visual Studio Test Counters - total: 43, failed: 2, errors: 3, timeout: 5, aborted: 7, executed: 31.");
  }

  @Test
  public void invalid_character_fail() {
    UnitTestResults results = new UnitTestResults();
    thrown.expect(IllegalStateException.class);

    new VisualStudioTestResultsFileParserOld().accept(new File("src/test/resources/visualstudio_test_results/old/invalid_character.trx"), results);

  }

}
