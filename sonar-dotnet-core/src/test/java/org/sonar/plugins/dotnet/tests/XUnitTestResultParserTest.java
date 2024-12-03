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
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.slf4j.event.Level;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertThrows;

public class XUnitTestResultParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void valid() {
    Map<String, UnitTestResults> results = new HashMap<>();

    var sut = new XUnitTestResultsFileParser(new HashMap<>() {
      {
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestMethod1", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldFail", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldSkip", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldError", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj2.XUnitTestProject2.UnitTest2.XUnitTestNotRun", "C:\\dev\\Playground\\XUnit2\\UnitTest1.cs");
      }
    });

    sut.accept(new File("src/test/resources/xunit/valid.xml"), results);

    assertThat(results).hasSize(2);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").tests()).isEqualTo(4);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").failures()).isEqualTo(2);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").skipped()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").executionTime()).isEqualTo(5);

    assertThat(results.get("C:\\dev\\Playground\\XUnit2\\UnitTest1.cs").tests()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\XUnit2\\UnitTest1.cs").failures()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit2\\UnitTest1.cs").skipped()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\XUnit2\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit2\\UnitTest1.cs").executionTime()).isEqualTo(6);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the XUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(7);
    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("XUnit Assembly found, assembly name:");
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Added Test Method:");
    assertThat(logTester.logs(Level.DEBUG).get(4)).startsWith("Added Test Method:");
    assertThat(logTester.logs(Level.DEBUG).get(5)).startsWith("XUnit Assembly found, assembly name:");
    assertThat(logTester.logs(Level.DEBUG).get(6)).startsWith("Added Test Method:");
  }

  @Test
  public void valid_no_execution_time() {
    Map<String, UnitTestResults> results = new HashMap<>();

    var sut = new XUnitTestResultsFileParser(Map.of("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestMethod1", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs"));

    sut.accept(new File("src/test/resources/xunit/valid_no_execution_time.xml"), results);

    assertThat(results).hasSize(1);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").tests()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").failures()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").skipped()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").executionTime()).isNull();

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the XUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("XUnit Assembly found, assembly name:");
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Added Test Method:");
  }

  @Test
  public void valid_data_attribute()
  {
    Map<String, UnitTestResults> results = new HashMap<>();

    var sut = new XUnitTestResultsFileParser(Map.of("XUnitTestProj.DataDrivenWithXUnit.Test.CalculatorTestWithClassData.Add_ShouldReturnCorrectSum", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs"));

    sut.accept(new File("src/test/resources/xunit/valid_data_attribute.xml"), results);

    assertThat(results).hasSize(1);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").tests()).isEqualTo(2);
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").failures()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").skipped()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\XUnit\\UnitTest1.cs").executionTime()).isEqualTo(8);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the XUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(3);
    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("XUnit Assembly found, assembly name:");
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Added Test Method:");
    assertThat(logTester.logs(Level.DEBUG).get(2)).startsWith("Added Test Method:");
  }

  @Test
  public void test_name_not_mapped() {
    var sut = new XUnitTestResultsFileParser(Map.of("Some.Other.TestMethod", "C:\\dev\\Playground\\XUnit.Test\\Sample.cs"));

    var file = new File("src/test/resources/xunit/test_name_not_mapped.xml");

    var exception = assertThrows(IllegalStateException.class, () -> sut.accept(file, new HashMap<>()));
    assertEquals("Test method XUnitTestProj.XUnitTestProject1.UnitTest1.TestMethodDoesNotExist cannot be mapped to the test source file",
      exception.getMessage());
  }

  @Test
  public void invalid_root() {
    var sut = new XUnitTestResultsFileParser(new HashMap<>());
    var file = new File("src/test/resources/xunit/invalid_root.xml");
    var exception = assertThrows(ParseErrorException.class, () -> sut.accept(file, new HashMap<>()));
    assertThat(exception.getMessage()).startsWith("Missing or incorrect root element. Expected one of [<assembly>, <assemblies>], but got <foo> instead");
  }

  @Test
  public void invalid_test_outcome() {
    var sut = new XUnitTestResultsFileParser(Map.of("Playground.Test.TestProject1.UnitTest1.InvalidOutcome", "C:\\dev\\Playground\\Playground.Test\\Sample.cs"));
    var file = new File("src/test/resources/xunit/invalid_test_outcome.xml");

    var exception = assertThrows(IllegalArgumentException.class, () -> sut.accept(file,new HashMap<>()));

    assertEquals("Outcome of unit test must match XUnit Test Format", exception.getMessage());
  }
}
