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
    Map<String, String> fileMap = new HashMap<>() {
      {
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestMethod1", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldFail", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldSkip", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldError", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");
        put("XUnitTestProj2.XUnitTestProject2.UnitTest2.XUnitTestNotRun", "C:\\dev\\Playground\\XUnit2\\UnitTest1.cs");
      }
    };

    var sut = new XUnitTestResultsParser();

    sut.parse(new File("src/test/resources/xunit/valid.xml"), results, fileMap);

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
    assertThat(debugLogs)
      .hasSize(7)
      .contains(
        "XUnit Assembly found, assembly name: C:\\dev\\Playground\\XUnit\\bin\\Debug\\net9.0\\XUnitTestProj.dll, Extracted dllName: XUnitTestProj",
        "Added Test Method: XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestShouldFail to File: C:\\dev\\Playground\\XUnit\\UnitTest1.cs",
        "XUnit Assembly found, assembly name: C:\\dev\\Playground\\XUnit\\bin\\Debug\\net9.0\\XUnitTestProj2.dll, Extracted dllName: XUnitTestProj2",
        "Added Test Method: XUnitTestProj2.XUnitTestProject2.UnitTest2.XUnitTestNotRun to File: C:\\dev\\Playground\\XUnit2\\UnitTest1.cs"
      );
  }

  @Test
  public void valid_no_execution_time() {
    Map<String, UnitTestResults> results = new HashMap<>();
    Map<String, String> fileMap = Map.of("XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestMethod1", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");

    var sut = new XUnitTestResultsParser();

    sut.parse(new File("src/test/resources/xunit/valid_no_execution_time.xml"), results, fileMap);

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
    assertThat(debugLogs)
      .hasSize(2)
      .contains(
        "XUnit Assembly found, assembly name: C:\\dev\\Playground\\XUnit\\bin\\Debug\\net9.0\\XUnitTestProj.dll, Extracted dllName: XUnitTestProj",
        "Added Test Method: XUnitTestProj.XUnitTestProject1.UnitTest1.XUnitTestMethod1 to File: C:\\dev\\Playground\\XUnit\\UnitTest1.cs"
      );

    assertThat(logTester.logs(Level.DEBUG).get(0)).startsWith("XUnit Assembly found, assembly name:");
    assertThat(logTester.logs(Level.DEBUG).get(1)).startsWith("Added Test Method:");
  }

  @Test
  public void valid_data_attribute()
  {
    Map<String, UnitTestResults> results = new HashMap<>();
    Map<String, String> fileMap = Map.of("XUnitTestProj.DataDrivenWithXUnit.Test.CalculatorTestWithClassData.Add_ShouldReturnCorrectSum", "C:\\dev\\Playground\\XUnit\\UnitTest1.cs");

    var sut = new XUnitTestResultsParser();

    sut.parse(new File("src/test/resources/xunit/valid_data_attribute.xml"), results, fileMap);

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
    assertThat(debugLogs)
      .hasSize(3)
      .contains(
        "XUnit Assembly found, assembly name: C:\\dev\\Playground\\XUnit\\bin\\Debug\\net9.0\\XUnitTestProj.dll, Extracted dllName: XUnitTestProj",
        "Added Test Method: XUnitTestProj.DataDrivenWithXUnit.Test.CalculatorTestWithClassData.Add_ShouldReturnCorrectSum to File: C:\\dev\\Playground\\XUnit\\UnitTest1.cs"
      );
  }

  @Test
  public void valid_generic_method_csharp() {
    var sut = new XUnitTestResultsParser();
    Map<String, String> fileMap = Map.of(
      "Calculator.xUnit.Calculator.xUnit.GenericTests.GenericTestMethod", "CalculatorTests.cs",
      "Calculator.xUnit.Calculator.xUnit.Derived.VirtualMethodInBaseClass", "CalculatorTests.cs",
      "Calculator.xUnit.Calculator.xUnit.Derived.TestMethodInBaseClass", "CalculatorTests.cs",
      "Calculator.xUnit.Calculator.xUnit.Tests.TestMethod1", "CalculatorTests.cs",
      "Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.VirtualMethodInBaseClass", "CalculatorTests.cs",
      "Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.GenericDerivedFromGenericClass_PassMethod", "CalculatorTests.cs",
      "Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.TestMethodInBaseClass", "CalculatorTests.cs"
    );
    Map<String, UnitTestResults> results = new HashMap<>();

    sut.parse(new File("src/test/resources/xunit/valid_generic_methods_csharp.xml"), results, fileMap);

    assertThat(results).hasSize(1);
    var calculatorTestsResult = results.get("CalculatorTests.cs");
    assertThat(calculatorTestsResult.tests()).isEqualTo(8);
    assertThat(calculatorTestsResult.failures()).isEqualTo(3);
    assertThat(calculatorTestsResult.skipped()).isZero();
    assertThat(calculatorTestsResult.errors()).isZero();
    assertThat(calculatorTestsResult.executionTime()).isEqualTo(29);

    assertThat(logTester.logs(Level.WARN)).isEmpty();

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the XUnit Test Results file ");

    assertThat(logTester.logs(Level.DEBUG))
      .hasSize(9)
      .contains(
        "XUnit Assembly found, assembly name: Calculator.xUnit\\bin\\Debug\\net9.0\\Calculator.xUnit.dll, Extracted dllName: Calculator.xUnit",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.VirtualMethodInBaseClass to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.GenericDerivedFromGenericClass_PassMethod to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.TestMethodInBaseClass to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.Derived.VirtualMethodInBaseClass to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.Derived.TestMethodInBaseClass to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericTests.GenericTestMethod to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericTests.GenericTestMethod to File: CalculatorTests.cs",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.Tests.TestMethod1 to File: CalculatorTests.cs"
      );
  }

  @Test
  public void valid_generic_method_vbnet() {
    Map<String, String> fileMap = Map.of(
        "Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.VirtualMethodInBaseClass", "CalculatorTests.vb",
        "Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.TestMethodInBaseClass", "CalculatorTests.vb",
        "Calculator.xUnit.Calculator.xUnit.Derived.VirtualMethodInBaseClass", "CalculatorTests.vb",
        "Calculator.xUnit.Calculator.xUnit.Derived.TestMethodInBaseClass", "CalculatorTests.vb",
        "Calculator.xUnit.Calculator.xUnit.GenericTests.GenericTestMethod", "CalculatorTests.vb",
        "Calculator.xUnit.Calculator.xUnit.Tests.TestMethod1", "CalculatorTests.vb",
        "Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.GenericMethod", "CalculatorTests.vb"
      );
    var sut = new XUnitTestResultsParser();
    Map<String, UnitTestResults> results = new HashMap<>();

    sut.parse(new File("src/test/resources/xunit/valid_generic_methods_vbnet.xml"), results, fileMap);

    assertThat(results).hasSize(1);
    var calculatorTestsResult = results.get("CalculatorTests.vb");
    assertThat(calculatorTestsResult.tests()).isEqualTo(8);
    assertThat(calculatorTestsResult.failures()).isEqualTo(3);
    assertThat(calculatorTestsResult.skipped()).isZero();
    assertThat(calculatorTestsResult.errors()).isZero();
    assertThat(calculatorTestsResult.executionTime()).isEqualTo(45);

    assertThat(logTester.logs(Level.WARN)).isEmpty();

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the XUnit Test Results file ");

    assertThat(logTester.logs(Level.DEBUG))
      .hasSize(9)
      .contains(
        "XUnit Assembly found, assembly name: Calculator.xUnit\\bin\\Debug\\net9.0\\Calculator.xUnit.dll, Extracted dllName: Calculator.xUnit",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.VirtualMethodInBaseClass to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.GenericMethod to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericDerivedFromGenericClass.TestMethodInBaseClass to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.Derived.VirtualMethodInBaseClass to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.Derived.TestMethodInBaseClass to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericTests.GenericTestMethod to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.GenericTests.GenericTestMethod to File: CalculatorTests.vb",
        "Added Test Method: Calculator.xUnit.Calculator.xUnit.Tests.TestMethod1 to File: CalculatorTests.vb"
      );
  }

  @Test
  public void test_name_not_mapped() {
    var sut = new XUnitTestResultsParser();
    var file = new File("src/test/resources/xunit/test_name_not_mapped.xml");

    sut.parse(file, new HashMap<>(), Map.of("Some.Other.TestMethod", "C:\\dev\\Playground\\XUnit.Test\\Sample.cs"));

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs).hasSize(2);
    assertThat(debugLogs.get(1)).isEqualTo("Test method XUnitTestProj.XUnitTestProject1.UnitTest1.TestMethodDoesNotExist cannot be mapped to the test source file. The test will not be included.");
  }

  @Test
  public void invalid_root() {
    var sut = new XUnitTestResultsParser();
    var file = new File("src/test/resources/xunit/invalid_root.xml");
    var exception = assertThrows(ParseErrorException.class, () -> sut.parse(file, new HashMap<>(), new HashMap<>()));
    assertThat(exception.getMessage()).startsWith("Missing or incorrect root element. Expected one of [<assembly>, <assemblies>], but got <foo> instead");
  }

  @Test
  public void invalid_test_outcome() {
    var sut = new XUnitTestResultsParser();
    var file = new File("src/test/resources/xunit/invalid_test_outcome.xml");
    Map<String, String> fileMap = Map.of("Playground.Test.TestProject1.UnitTest1.InvalidOutcome", "C:\\dev\\Playground\\Playground.Test\\Sample.cs");

    var exception = assertThrows(IllegalArgumentException.class, () -> sut.parse(file, new HashMap<>(), fileMap));

    assertEquals("Outcome of unit test must match XUnit Test Format", exception.getMessage());
  }
}
