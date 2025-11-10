/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.Mockito;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.ArgumentMatchers.notNull;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.sonarsource.dotnet.protobuf.SonarAnalyzer.MethodDeclarationsInfo;
import static org.sonarsource.dotnet.protobuf.SonarAnalyzer.MethodDeclarationInfo;

public class UnitTestResultsAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void hasUnitTestResultsProperty() {
    Configuration configuration = mock(Configuration.class);

    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isFalse();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile2", "nunit2", "xunit2");
    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isFalse();
  }

  @Test
  public void aggregate_visual_studio_only() {
    AggregateTestContext context = new AggregateTestContext("visualStudioTestResultsFile", "foo.trx");
    context.run();

    verify(context.visualStudio).parse(eq(new File("foo.trx")), notNull(), any());
    verify(context.nunit, Mockito.never()).parse(any(), any(), any());
    verify(context.xunit, Mockito.never()).parse(any(), any(), any());
  }

  @Test
  public void aggregate_nunit_only() {
    AggregateTestContext context = new AggregateTestContext("nunitTestResultsFile", "foo.xml");
    context.run();

    verify(context.visualStudio, Mockito.never()).parse(any(), any(), any());
    verify(context.nunit).parse(eq(new File("foo.xml")), any(), any());
    verify(context.xunit, Mockito.never()).parse(any(), any(), any());
  }

  @Test
  public void aggregate_xunit_only() {
    AggregateTestContext context = new AggregateTestContext("xunitTestResultsFile", "foo.xml");
    context.run();

    verify(context.visualStudio, Mockito.never()).parse(any(), any(), any());
    verify(context.nunit, Mockito.never()).parse(any(), any(), any());
    verify(context.xunit).parse(eq(new File("foo.xml")), notNull(), any());
  }

  @Test
  public void aggregate_all_formats_configured() {
    AggregateTestContext context = new AggregateTestContext();
    context.add("visualStudioTestResultsFile", "vs.trx");
    context.add("nunitTestResultsFile", "nunit.xml");
    context.add("xunitTestResultsFile", "xunit.xml");
    context.run();

    verify(context.visualStudio).parse(eq(new File("vs.trx")), notNull(), any());
    verify(context.nunit).parse(eq(new File("nunit.xml")), any(), any());
    verify(context.xunit).parse(eq(new File("xunit.xml")), notNull(), any());
  }

  @Test
  public void aggregate_none_formats_configured() {
    AggregateTestContext context = new AggregateTestContext();
    context.run();

    verify(context.visualStudio, Mockito.never()).parse(any(), any(), any());
    verify(context.nunit, Mockito.never()).parse(any(), any(), any());
    verify(context.xunit, Mockito.never()).parse(any(), any(), any());
  }

  @Test
  public void aggregate_multiple_files_configured() {
    AggregateTestContext context = new AggregateTestContext();
    context.settings.setProperty("visualStudioTestResultsFile", ", *.trx, visualStudioSecond.trx");
    when(context.fileProvider.listFiles("*.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("visualStudioFirst.trx"))));
    when(context.fileProvider.listFiles("visualStudioSecond.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("visualStudioSecond.trx"))));
    context.settings.setProperty("nunitTestResultsFile", ", nunitFirst.xml, nunitSecond.xml");
    when(context.fileProvider.listFiles("nunitFirst.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("nunitFirst.xml"))));
    when(context.fileProvider.listFiles("nunitSecond.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("nunitSecond.xml"))));
    context.settings.setProperty("xunitTestResultsFile", ", xunitFirst.xml, xunitSecond.xml");
    when(context.fileProvider.listFiles("xunitFirst.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("xunitFirst.xml"))));
    when(context.fileProvider.listFiles("xunitSecond.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("xunitSecond.xml"))));
    context.run();

    verify(context.fileProvider).listFiles("*.trx");
    verify(context.fileProvider).listFiles("visualStudioSecond.trx");
    verify(context.fileProvider).listFiles("nunitFirst.xml");
    verify(context.fileProvider).listFiles("nunitSecond.xml");
    verify(context.fileProvider).listFiles("xunitFirst.xml");
    verify(context.fileProvider).listFiles("xunitSecond.xml");

    verify(context.visualStudio).parse(eq(new File("visualStudioFirst.trx")), notNull(), any());
    verify(context.visualStudio).parse(eq(new File("visualStudioSecond.trx")), notNull(), any());
    verify(context.nunit).parse(eq(new File("nunitFirst.xml")), notNull(), any());
    verify(context.nunit).parse(eq(new File("nunitSecond.xml")), notNull(), any());
    verify(context.xunit).parse(eq(new File("xunitFirst.xml")), notNull(), any());
    verify(context.xunit).parse(eq(new File("xunitSecond.xml")), notNull(), any());
  }

  @Test
  public void aggregate_logs_warning_on_exception() {
    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");
    MapSettings settings = new MapSettings();
    settings.setProperty("visualStudioTestResultsFile", "foo.trx");

    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.trx"))));

    new UnitTestResultsAggregator(unitTestConf, settings.asConfig())
      .aggregate(wildcardPatternFileProvider, new ArrayList<>(Collections.emptyList()));

    assertThat(logTester.logs(Level.WARN)).containsOnly("Could not import unit test report 'foo.trx': java.io.FileNotFoundException: foo.trx (The system cannot find the file specified)");
  }

  @Test
  public void aggregate_withMethodDeclarations() {
    AggregateTestContext context = new AggregateTestContext("visualStudioTestResultsFile", "src/test/resources/visualstudio_test_results/valid.trx");

    var methodDeclarations = new ArrayList<MethodDeclarationsInfo>();
    methodDeclarations.add(MethodDeclarationsInfo.newBuilder()
      .setFilePath("C:\\dev\\Playground\\Playground.Test\\Sample.cs")
      .setAssemblyName("Playground.Test.TestProject1")
      .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
        .setTypeName("UnitTest1")
        .setMethodName("TestMethod1")
        .build())
      .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
        .setTypeName("UnitTest2")
        .setMethodName("TestMethod5")
        .build())
      .build());
    methodDeclarations.add(MethodDeclarationsInfo.newBuilder()
      .setFilePath("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs")
      .setAssemblyName("Playground.Test.TestProject1")
      .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
        .setTypeName("UnitTest1")
        .setMethodName("TestShouldFail")
        .build())
      .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
        .setTypeName("UnitTest1")
        .setMethodName("TestShouldSkip")
        .build())
      .addMethodDeclarations(MethodDeclarationInfo.newBuilder()
        .setTypeName("UnitTest1")
        .setMethodName("TestShouldError")
        .build())
      .build());
    var sut = new UnitTestResultsAggregator(context.config, context.settings.asConfig());
    var resultsMap = sut.aggregate(context.fileProvider, methodDeclarations);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(resultsMap).hasSize(2);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").tests()).isEqualTo(13);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").failures()).isZero();
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").skipped()).isEqualTo(7);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").errors()).isEqualTo(1);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\Sample.cs").executionTime()).isEqualTo(47);

    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").tests()).isEqualTo(3);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").failures()).isEqualTo(2);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").skipped()).isEqualTo(1);
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").errors()).isZero();
    assertThat(resultsMap.get("C:\\dev\\Playground\\Playground.Test\\UnitTest1.cs").executionTime()).isEqualTo(22);

    assertThat(logTester.logs(Level.INFO)).hasSize(1);
    assertThat(logTester.logs(Level.INFO).get(0)).startsWith("Parsing the Visual Studio Test Results file ");
  }

  private static class AggregateTestContext {
    public final MapSettings settings = new MapSettings();
    public final WildcardPatternFileProvider fileProvider = mock(WildcardPatternFileProvider.class);
    public final VisualStudioTestResultParser visualStudio = mock(VisualStudioTestResultParser.class);
    public final XUnitTestResultsParser xunit = mock(XUnitTestResultsParser.class);
    public final NUnitTestResultsParser nunit = mock(NUnitTestResultsParser.class);
    public final UnitTestConfiguration config = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");

    public AggregateTestContext() { }

    public AggregateTestContext(String propertyName, String fileName) {
      add(propertyName, fileName);
    }

    public void add(String propertyName, String fileName) {
      settings.setProperty(propertyName, fileName);
      when(fileProvider.listFiles(fileName)).thenReturn(new HashSet<>(Collections.singletonList(new File(fileName))));
    }

    public void run() {
      new UnitTestResultsAggregator(config, settings.asConfig(), visualStudio, nunit, xunit).aggregate(fileProvider, Collections.emptyList());
    }
  }
}
