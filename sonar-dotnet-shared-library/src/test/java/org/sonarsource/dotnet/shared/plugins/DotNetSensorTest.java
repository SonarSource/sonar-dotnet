/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2021 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.rule.internal.NewActiveRule;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.internal.google.common.collect.ImmutableList;
import org.sonar.api.internal.google.common.collect.ImmutableMap;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.mockito.Mockito.when;

public class DotNetSensorTest {

  private static final String REPO_KEY = "REPO_KEY";
  private static final String LANG_KEY = "LANG_KEY";
  private static final String LANG_NAME = "LANG_NAME";

  @Rule
  public LogTester logTester = new LogTester();
  @Rule
  public ExpectedException thrown = ExpectedException.none();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private List<Path> reportPaths;
  private RoslynDataImporter roslynDataImporter = mock(RoslynDataImporter.class);
  private ProtobufDataImporter protobufDataImporter = mock(ProtobufDataImporter.class);
  private ReportPathCollector reportPathCollector = mock(ReportPathCollector.class);

  private SensorContextTester tester;
  private DotNetSensor sensor;
  private Path workDir;

  @Before
  public void prepare() throws Exception {
    workDir = temp.newFolder().toPath();
    reportPaths = Arrays.asList(new Path[] {workDir.getRoot()});
    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir);
    when(reportPathCollector.protobufDirs()).thenReturn(reportPaths);
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn(LANG_KEY);
    when(pluginMetadata.repositoryKey()).thenReturn(REPO_KEY);
    when(pluginMetadata.shortLanguageName()).thenReturn(LANG_NAME);
    sensor = new DotNetSensor(pluginMetadata, reportPathCollector, protobufDataImporter, roslynDataImporter);
  }

  @Test
  public void checkDescriptor() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.languages()).containsOnly(LANG_KEY);
    assertThat(sensorDescriptor.name()).isEqualTo(LANG_NAME);
  }

  @Test
  public void isProjectSensor() {
    assertThat(ProjectSensor.class.isAssignableFrom(sensor.getClass())).isTrue();
  }

  @Test
  public void whenNoProtobufFiles_shouldNotFail() {
    addMainFileToFs();
    when(reportPathCollector.protobufDirs()).thenReturn(Collections.emptyList());
    when(reportPathCollector.roslynReports()).thenReturn(Collections.singletonList(new RoslynReport(null, workDir.getRoot())));
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "S1186")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "[parameters_key]")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of("roslyn.foo", "custom-roslyn")).build())
      .build());

    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.WARN)).containsExactly(
      "No protobuf reports found. The " + LANG_NAME + " files will not have highlighting and metrics.",
      "Your project contains " + LANG_NAME + " files which cannot be analyzed with the scanner you are using." +
        " To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html");
    verify(reportPathCollector).protobufDirs();
    verifyZeroInteractions(protobufDataImporter);
    ImmutableMap<String, List<RuleKey>> expectedMap = ImmutableMap.of(
      "sonaranalyzer-" + LANG_KEY, ImmutableList.of(RuleKey.of(REPO_KEY, "S1186"), RuleKey.of(REPO_KEY, "[parameters_key]")),
      "foo", ImmutableList.of(RuleKey.of("roslyn.foo", "custom-roslyn")));
    verify(roslynDataImporter).importRoslynReports(eq(Collections.singletonList(new RoslynReport(null, workDir.getRoot()))), eq(tester), eq(expectedMap),
      any(RealPathProvider.class));
  }

  @Test
  public void whenNoRoslynReport_shouldNotFail() {
    addMainFileToFs();

    sensor.execute(tester);

    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    verifyZeroInteractions(roslynDataImporter);
    assertThat(logTester.logs(LoggerLevel.WARN)).containsExactly(
      "No Roslyn issue reports were found. The " + LANG_NAME + " files have not been analyzed.",
      "Your project contains " + LANG_NAME + " files which cannot be analyzed with the scanner you are using." +
        " To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html");
  }

  @Test
  public void whenReportsArePresent_thereAreNoWarnings() {
    addMainFileToFs();
    addRoslynReports();

    sensor.execute(tester);

    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
  }

  @Test
  public void whenThereAreBothMainAndTestFiles_doNotLog() {
    addMainFileToFs();
    addTestFileToFs();
    // add roslyn reports to avoid warnings in the logs for this test
    addRoslynReports();

    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
  }

  @Test
  public void whenThereAreOnlyTestFiles_logWarning() {
    addTestFileToFs();

    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN))
      .containsExactly("This solution contains only TEST files. Because there are no MAIN files, this sensor will be skipped. Your SonarQube/SonarCloud project will not have results for LANG_KEY files.");
  }

  @Test
  public void whenThereAreNoFiles_logDebug() {
    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG))
      .containsExactly("No files to analyze. Skip Sensor.");
  }

  private void addMainFileToFs() {
    DefaultInputFile mainFile = new TestInputFileBuilder("mod", "file.language")
      .setLanguage(LANG_KEY)
      .setType(Type.MAIN)
      .build();
    addFileToFs(mainFile);
  }

  private void addTestFileToFs() {
    DefaultInputFile testFile = new TestInputFileBuilder("mod", "SomeTest.language")
      .setLanguage(LANG_KEY)
      .setType(Type.TEST)
      .build();
    addFileToFs(testFile);
  }

  private void addFileToFs(InputFile inputFile) {
    tester.fileSystem().add(inputFile);
  }

  private void addRoslynReports() {
    when(reportPathCollector.roslynReports()).thenReturn(Collections.singletonList(new RoslynReport(null, workDir.getRoot())));
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "S1186")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "[parameters_key]")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of("roslyn.foo", "custom-roslyn")).build())
      .build());
  }
}
