/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2020 SonarSource SA
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

  public static final String REPO_KEY = "repoKey";
  public static final String LANG_KEY = "langKey";

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
    when(pluginMetadata.shortLanguageName()).thenReturn("LangName");
    sensor = new DotNetSensor(pluginMetadata, reportPathCollector, protobufDataImporter, roslynDataImporter);
  }

  private void addFileToFs() {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", "file.vb").setLanguage(LANG_KEY).build();
    tester.fileSystem().add(inputFile);
  }

  @Test
  public void checkDescriptor() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.languages()).containsOnly(LANG_KEY);
    assertThat(sensorDescriptor.name()).isEqualTo("LangName");
  }

  @Test
  public void isProjectSensor() {
    assertThat(ProjectSensor.class.isAssignableFrom(sensor.getClass())).isTrue();
  }

  @Test
  public void noProtobufFilesShouldNotFail() {
    addFileToFs();
    when(reportPathCollector.protobufDirs()).thenReturn(Collections.emptyList());
    when(reportPathCollector.roslynDirs()).thenReturn(Collections.singletonList(new RoslynReport(null, workDir.getRoot())));
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "S1186")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "[parameters_key]")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of("roslyn.foo", "custom-roslyn")).build())
      .build());

    sensor.execute(tester);

    verify(reportPathCollector).protobufDirs();
    verifyZeroInteractions(protobufDataImporter);
    ImmutableMap<String, List<RuleKey>> expectedMap = ImmutableMap.of(
      "sonaranalyzer-langKey", ImmutableList.of(RuleKey.of(REPO_KEY, "S1186"), RuleKey.of(REPO_KEY, "[parameters_key]")),
      "foo", ImmutableList.of(RuleKey.of("roslyn.foo", "custom-roslyn")));
    verify(roslynDataImporter).importRoslynReports(eq(Collections.singletonList(new RoslynReport(null, workDir.getRoot()))), eq(tester), eq(expectedMap),
      any(RealPathProvider.class));
  }

  @Test
  public void noRoslynReportShouldNotFail() {
    addFileToFs();
    sensor.execute(tester);

    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    verifyZeroInteractions(roslynDataImporter);
  }

  @Test
  public void noAnalysisIfNoFilesDetected() {
    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).isEqualTo("No files to analyze. Skip Sensor.");
  }
}
