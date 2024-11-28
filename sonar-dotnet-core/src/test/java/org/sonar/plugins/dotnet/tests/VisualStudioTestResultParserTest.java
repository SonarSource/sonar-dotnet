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
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;

public class VisualStudioTestResultParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void valid() {
    Map<String, UnitTestResults> results = new HashMap<>();

    var sut = new VisualStudioTestResultParser(new HashMap<>() {
      {
        put("Playground.Test.TestProject1.UnitTest1.TestMethod1", "C:\\dev\\Playground\\Playground.Test\\Sample.cs");
        put("Playground.Test.TestProject1.UnitTest1.TestShouldFail", "C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs");
        put("Playground.Test.TestProject1.UnitTest1.TestShouldSkip", "C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs");
        put("Playground.Test.TestProject1.UnitTest1.TestShouldError", "C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs");
        put("Playground.Test.TestProject1.UnitTest2.TestMethod5", "C:\\dev\\Playground\\Playground.Test\\Sample.cs");
      }
    });

    sut.accept(new File("src/test/resources/visualstudio_test_results/valid.trx"), results);

    assertThat(results).hasSize(2);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").tests()).isEqualTo(13);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").failures()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").skipped()).isEqualTo(7);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").errors()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").executionTime()).isEqualTo(47);

    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").tests()).isEqualTo(3);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").failures()).isEqualTo(2);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").skipped()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").executionTime()).isEqualTo(22);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the Visual Studio Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(32);
    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("Parsed Visual Studio Unit Test - testId:");
    assertThat(logTester.logs(Level.DEBUG).get(15)).startsWith("Parsed Visual Studio Unit Test - testId:");
    assertThat(logTester.logs(Level.DEBUG).get(16)).startsWith("Associated Visual Studio Unit Test to File - file:");
    assertThat(logTester.logs(Level.DEBUG).get(31)).startsWith("Associated Visual Studio Unit Test to File - file:");
  }

  @Test
  public void test_name_not_mapped() {
    Map<String, UnitTestResults> results = new HashMap<>();
    var sut = new VisualStudioTestResultParser(new HashMap<>() {
      {
        put("Some.Other.TestMethod", "C:\\dev\\Playground\\Playground.Test\\Sample.cs");
      }
    });
    var file = new File("src/test/resources/visualstudio_test_results/test_name_not_mapped.trx");

    var exception = Assert.assertThrows(IllegalStateException.class, () -> sut.accept(file, results));

    Assert.assertEquals("Test method Playground.Test.TestProject1.UnitTest1.TestShouldFail with testId d7744238-9adf-b364-3d70-ae38261a8cd8 cannot be mapped to the test source file",
      exception.getMessage());
  }

  @Test
  public void invalid_date() {
    Map<String, UnitTestResults> results = new HashMap<>();
    var sut = new VisualStudioTestResultParser(new HashMap<>());
    var file = new File("src/test/resources/visualstudio_test_results/invalid_dates.trx");

    var exception = Assert.assertThrows(ParseErrorException.class, () -> sut.accept(file, results));
    assertThat(exception.getMessage()).startsWith("Expected a valid date and time instead of \"2016-xx-14T17:04:31.100+01:00\" for the attribute \"startTime\". Unparseable date: \"2016-xx-14T17:04:31.100+01:00\" in ");
  }

  @Test
  public void invalid_character_fail() {
    Map<String, UnitTestResults> results = new HashMap<>();
    var sut = new VisualStudioTestResultParser(new HashMap<>());
    var file = new File("src/test/resources/visualstudio_test_results/invalid_character.trx");

    var exception = Assert.assertThrows(IllegalStateException.class, () -> sut.accept(file, results));

    assertThat(exception.getMessage()).startsWith("Error while parsing the XML file:");
  }

  @Test
  public void test_result_no_test_method() {
    Map<String, UnitTestResults> results = new HashMap<>();
    var sut = new VisualStudioTestResultParser(new HashMap<>());
    var file = new File("src/test/resources/visualstudio_test_results/test_result_no_test_method.trx");

    var exception =  Assert.assertThrows(ParseErrorException.class, () -> sut.accept(file, results));
    Assert.assertEquals("No TestMethod attribute found on UnitTest tag",
      exception.getMessage());
  }

  @Test
  public void invalid_test_outcome() {
    Map<String, UnitTestResults> results = new HashMap<>();
    var sut = new VisualStudioTestResultParser(Map.of("Playground.Test.TestProject1.UnitTest1.InvalidOutcome", "C:\\dev\\Playground\\Playground.Test\\Sample.cs"));
    var file = new File("src/test/resources/visualstudio_test_results/invalid_test_outcome.trx");

    var exception = Assert.assertThrows(IllegalArgumentException.class, () -> sut.accept(file,results));
    Assert.assertEquals("Outcome of unit test must match VSTest Format",
      exception.getMessage());
  }
}
