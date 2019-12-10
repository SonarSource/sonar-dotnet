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

import java.io.File;
import java.util.List;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class VisualStudioTestResultsFileParserTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void no_counters() {
    thrown.expect(IllegalArgumentException.class);
    thrown.expectMessage("The mandatory <Counters> tag is missing in ");
    thrown.expectMessage(new File("src/test/resources/visualstudio_test_results/no_counters.trx").getAbsolutePath());
    new VisualStudioTestResultsFileParser().accept(new File("src/test/resources/visualstudio_test_results/no_counters.trx"), mock(UnitTestResults.class));
  }

  @Test
  public void valid() {
    UnitTestResults results = new UnitTestResults();
    new VisualStudioTestResultsFileParser().accept(new File("src/test/resources/visualstudio_test_results/valid.trx"), results);

    assertThat(results.failures()).isEqualTo(14);
    assertThat(results.errors()).isEqualTo(3);
    assertThat(results.tests()).isEqualTo(43);
    assertThat(results.skipped()).isEqualTo(12); // 43 - 31
    assertThat(results.executionTime()).isEqualTo(816l);

    List<String> infoLogs = logTester.logs(LoggerLevel.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the Visual Studio Test Results file ");

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).startsWith("The current user dir is '");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Times - duration:816.");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(2)).startsWith("Parsed Visual Studio Test Counters - total:43, failed:2, errors:3, timeout:5, aborted:7, executed:31.");
  }

  @Test
  public void valid_missing_attributes() {
    UnitTestResults results = new UnitTestResults();
    new VisualStudioTestResultsFileParser().accept(new File("src/test/resources/visualstudio_test_results/valid_missing_attributes.trx"), results);

    assertThat(results.tests()).isEqualTo(3);
    assertThat(results.skipped()).isEqualTo(0);
    assertThat(results.failures()).isEqualTo(0);
    assertThat(results.errors()).isEqualTo(0);

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Counters - total:3, failed:0, errors:0, timeout:0, aborted:0, executed:3.");
  }

  @Test
  public void invalid_date() {
    UnitTestResults results = new UnitTestResults();
    thrown.expect(ParseErrorException.class);
    new VisualStudioTestResultsFileParser().accept(new File("src/test/resources/visualstudio_test_results/invalid_date.trx"), results);

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Counters - total:3, failed:0, errors:0, timeout:0, aborted:0, executed:3.");
  }

  @Test
  public void invalid_entity_does_not_fail() {
    UnitTestResults results = new UnitTestResults();
    new VisualStudioTestResultsFileParser().accept(new File("src/test/resources/visualstudio_test_results/invalid_entities.trx"), results);

    assertThat(results.failures()).isEqualTo(14);
    assertThat(results.errors()).isEqualTo(3);
    assertThat(results.tests()).isEqualTo(43);
    assertThat(results.skipped()).isEqualTo(12);
    assertThat(results.executionTime()).isEqualTo(816l);

    List<String> debugLogs = logTester.logs(LoggerLevel.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(1)).startsWith("Parsed Visual Studio Test Times - duration:816.");
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(2)).startsWith("Parsed Visual Studio Test Counters - total:43, failed:2, errors:3, timeout:5, aborted:7, executed:31.");
  }

  @Test
  public void invalid_character_fail() {
    UnitTestResults results = new UnitTestResults();
    thrown.expect(IllegalStateException.class);

    new VisualStudioTestResultsFileParser().accept(new File("src/test/resources/visualstudio_test_results/invalid_character.trx"), results);

  }

}
