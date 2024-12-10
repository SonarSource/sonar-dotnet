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

public class NUnitTestResultsParserTest {

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() {
    logTester.setLevel(Level.DEBUG);
  }

  @Test
  public void valid_nunit3() {
    Map<String, UnitTestResults> results = new HashMap<>();

    Map<String, String> fileMap = new HashMap<>() {
      {
        put("NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestMethod1", "C:\\dev\\Playground\\NUnit\\UnitTest1.cs");
        put("NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestShouldFail", "C:\\dev\\Playground\\NUnit\\UnitTest1.cs");
        put("NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestShouldSkip", "C:\\dev\\Playground\\NUnit\\UnitTest1.cs");
        put("NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestShouldError", "C:\\dev\\Playground\\NUnit\\UnitTest1.cs");

        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.FailingTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest1", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest2", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest3", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest4", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest5", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.NotRunnableTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.TestWithException", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.BadFixture", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.FixtureWithTestCases.MethodWithParameters", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.InconclusiveTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.TestWithManyProperties", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.ParameterizedFixture", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Singletons.OneTestCase.TestCase", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.TestAssembly.MockTestFixture.MyTest", "D:\\Dev\\NUnit\\OtherTests.cs");
      }
    };
    var sut = new NUnitTestResultsParser();

    sut.parse(new File("src/test/resources/NUnit/valid_nunit3.xml"), results, fileMap);

    assertThat(results).hasSize(2);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").tests()).isEqualTo(4);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").failures()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").skipped()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").errors()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").executionTime()).isEqualTo(37);

    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").tests()).isEqualTo(18);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").failures()).isEqualTo(1);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").skipped()).isEqualTo(4);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").errors()).isEqualTo(1);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").executionTime()).isEqualTo(33);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the NUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs)
      .hasSize(24)
      .contains(
        "NUnit Assembly found, assembly: C:\\dev\\Playground\\NUnit\\bin\\Debug\\net9.0\\NUnitTestProj.dll, Extracted dllName: NUnitTestProj",
        "Added Test Method: NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestMethod1 to File: C:\\dev\\Playground\\NUnit\\UnitTest1.cs",
        "NUnit Assembly found, assembly: D:\\Dev\\NUnit\\nunit-3.0\\work\\bin\\vs2008\\Debug\\mock-assembly.dll, Extracted dllName: mock-assembly"
      );
  }

  @Test
  public void valid_nunit2() {
    Map<String, UnitTestResults> results = new HashMap<>();
    var sut = new NUnitTestResultsParser();

    Map<String, String> fileMap = new HashMap<>() {
      {
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.FailingTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.InconclusiveTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest1", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest2", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest3", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest4", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest5", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.NotRunnableTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.TestWithException", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.FixtureWithTestCases.MethodWithParameters", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.TestWithManyProperties", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.ParameterizedFixture", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.FixtureWithTestCases.GenericMethod", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.GenericFixture", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.IgnoredFixture.Test1", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.IgnoredFixture.Test2", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.IgnoredFixture.Test3", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.BadFixture.SomeTest", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.Singletons.OneTestCase.TestCase", "D:\\Dev\\NUnit\\OtherTests.cs");
        put("mock-assembly.NUnit.Tests.TestAssembly.MockTestFixture.MyTest", "D:\\Dev\\NUnit\\OtherTests.cs");
      }
    };

    sut.parse(new File("src/test/resources/NUnit/valid_nunit2.xml"), results, fileMap);

    assertThat(results).hasSize(1);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").tests()).isEqualTo(28);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").failures()).isEqualTo(1);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").skipped()).isEqualTo(8);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").errors()).isEqualTo(1);
    assertThat(results.get("D:\\Dev\\NUnit\\OtherTests.cs").executionTime()).isEqualTo(27);

    var warnings = logTester.logs(Level.WARN);
    assertThat(warnings).isEmpty();

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the NUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs)
      .hasSize(29)
      .contains(
        "Added Test Method: mock-assembly.NUnit.Tests.Assemblies.MockTestFixture.MockTest2 to File: D:\\Dev\\NUnit\\OtherTests.cs",
        "NUnit Assembly found, assembly: \\home\\charlie\\Dev\\NUnit\\nunit-2.5\\work\\src\\bin\\Debug\\tests\\mock-assembly.dll, Extracted dllName: mock-assembly"
      );
 }

  @Test
  public void valid_no_execution_time() {
    Map<String, UnitTestResults> results = new HashMap<>();

    var sut = new NUnitTestResultsParser();

    sut.parse(new File("src/test/resources/NUnit/valid_no_execution_time.xml"), results, Map.of("NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestMethod1", "C:\\dev\\Playground\\NUnit\\UnitTest1.cs"));

    assertThat(results).hasSize(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").tests()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").failures()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").skipped()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").executionTime()).isNull();

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the NUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs)
      .hasSize(2)
      .contains(
        "Added Test Method: NUnitTestProj.NUnitTestProject.UnitTest1.NUnitTestMethod1 to File: C:\\dev\\Playground\\NUnit\\UnitTest1.cs",
        "NUnit Assembly found, assembly: C:\\dev\\Playground\\NUnit\\bin\\Debug\\net9.0\\NUnitTestProj.dll, Extracted dllName: NUnitTestProj"
      );
  }
  
  @Test
  public void valid_comma_in_double()
  {
    Map<String, UnitTestResults> results = new HashMap<>();

    var sut = new NUnitTestResultsParser();

    sut.parse(new File("src/test/resources/NUnit/valid_comma_in_double.xml"), results, Map.of("NUnitTestProj.NUnitTestProject.UnitTest1.CommaInDouble", "C:\\dev\\Playground\\NUnit\\UnitTest1.cs"));

    assertThat(results).hasSize(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").tests()).isEqualTo(1);
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").failures()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").skipped()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").errors()).isZero();
    assertThat(results.get("C:\\dev\\Playground\\NUnit\\UnitTest1.cs").executionTime()).isEqualTo(1041);

    List<String> infoLogs = logTester.logs(Level.INFO);
    assertThat(infoLogs).hasSize(1);
    assertThat(infoLogs.get(0)).startsWith("Parsing the NUnit Test Results file ");

    List<String> debugLogs = logTester.logs(Level.DEBUG);
    assertThat(debugLogs)
      .hasSize(2)
      .contains(
        "Added Test Method: NUnitTestProj.NUnitTestProject.UnitTest1.CommaInDouble to File: C:\\dev\\Playground\\NUnit\\UnitTest1.cs",
        "NUnit Assembly found, assembly: C:\\dev\\Playground\\NUnit\\bin\\Debug\\net9.0\\NUnitTestProj.dll, Extracted dllName: NUnitTestProj"
      );
  }

  @Test
  public void test_name_not_mapped() {
    var sut = new NUnitTestResultsParser();
    var file = new File("src/test/resources/NUnit/test_name_not_mapped.xml");

    sut.parse(file, new HashMap<>(), Map.of("Some.Other.TestMethod", "C:\\dev\\Playground\\NUnit.Test\\Sample.cs"));

    List<String> warnLogs = logTester.logs(Level.WARN);
    assertThat(warnLogs).hasSize(1);
    assertThat(warnLogs.get(0)).isEqualTo("Test method NUnitTestProj.NUnitTestProject.UnitTest1.TestMethodDoesNotExist cannot be mapped to the test source file. The test will not be included.");
  }

  @Test
  public void invalid_root() {
    var sut = new NUnitTestResultsParser();
    var file = new File("src/test/resources/NUnit/invalid_root.xml");

    var exception = assertThrows(ParseErrorException.class, () -> sut.parse(file, new HashMap<>(), new HashMap<>()));
    assertThat(exception.getMessage()).startsWith("Missing or incorrect root element. Expected one of [<test-run>, <test-results>], but got <foo> instead");
  }

  @Test
  public void invalid_test_outcome() {
    var sut = new NUnitTestResultsParser();
    var file = new File("src/test/resources/NUnit/invalid_test_outcome.xml");

    var exception = assertThrows(IllegalArgumentException.class, () -> sut.parse(file,new HashMap<>(), Map.of("NUnitTestProj.NUnitTestProject.UnitTest1.InvalidOutcome", "C:\\dev\\Playground\\Playground.Test\\Sample.cs")));
    assertEquals("Outcome of unit test must match NUnit Test Format", exception.getMessage());
  }
}
