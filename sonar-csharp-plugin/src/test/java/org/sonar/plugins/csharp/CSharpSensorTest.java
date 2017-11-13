/*
 * SonarC#
 * Copyright (C) 2014-2017 SonarSource SA
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
package org.sonar.plugins.csharp;

import java.io.File;
import java.nio.file.Path;
import java.util.List;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.internal.google.common.collect.ImmutableList;
import org.sonar.api.internal.google.common.collect.ImmutableMap;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.utils.MessageException;
import org.sonar.api.utils.System2;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;
import org.sonarsource.dotnet.shared.plugins.ProtobufDataImporter;
import org.sonarsource.dotnet.shared.plugins.RoslynDataImporter;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.mockito.Mockito.when;

public class CSharpSensorTest {
  @Rule
  public LogTester logTester = new LogTester();
  @Rule
  public ExpectedException thrown = ExpectedException.none();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private System2 system = mock(System2.class);
  private RoslynDataImporter roslynDataImporter = mock(RoslynDataImporter.class);
  private ProtobufDataImporter protobufDataImporter = mock(ProtobufDataImporter.class);
  private CSharpConfiguration csConfigConfiguration = mock(CSharpConfiguration.class);

  private SensorContextTester tester;
  private CSharpSensor sensor;
  private Path workDir;

  @Before
  public void prepare() throws Exception {
    workDir = temp.newFolder().toPath();
    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir);
    when(system.isOsWindows()).thenReturn(true);
    sensor = new CSharpSensor(csConfigConfiguration, system, protobufDataImporter, roslynDataImporter);
  }

  private void addFileToFs() {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", "file.cs").setLanguage(CSharpPlugin.LANGUAGE_KEY).build();
    tester.fileSystem().add(inputFile);
  }

  @Test
  public void noProtobufFilesShouldNotFail() {
    addFileToFs();
    when(csConfigConfiguration.protobufReportPath()).thenReturn(Optional.empty());
    when(csConfigConfiguration.roslynReportPath()).thenReturn(Optional.of(workDir.getRoot()));
    tester.setActiveRules(new ActiveRulesBuilder()
      .create(RuleKey.of(CSharpSonarRulesDefinition.REPOSITORY_KEY, "S1186"))
      .activate()
      .create(RuleKey.of(CSharpSonarRulesDefinition.REPOSITORY_KEY, "[parameters_key]"))
      .activate()
      .create(RuleKey.of("roslyn.foo", "custom-roslyn"))
      .activate()
      .build());

    sensor.execute(tester);

    verify(csConfigConfiguration).protobufReportPath();
    verifyZeroInteractions(protobufDataImporter);
    // {"sonaranalyzer-cs" = [csharpsquid:S1186, csharpsquid:[parameters_key]], "foo" = [roslyn.foo:custom-roslyn]}
    ImmutableMap<String, List<RuleKey>> expectedMap = ImmutableMap.of(
      "sonaranalyzer-cs", ImmutableList.of(RuleKey.of("csharpsquid", "S1186"), RuleKey.of("csharpsquid", "[parameters_key]")),
      "foo", ImmutableList.of(RuleKey.of("roslyn.foo", "custom-roslyn")));
    verify(roslynDataImporter).importRoslynReport(workDir.getRoot(), tester, expectedMap);
  }

  @Test
  public void noRoslynReportShouldNotFail() {
    addFileToFs();
    when(csConfigConfiguration.roslynReportPath()).thenReturn(Optional.empty());
    when(csConfigConfiguration.protobufReportPath()).thenReturn(Optional.of(workDir.getRoot()));
    sensor.execute(tester);

    verify(csConfigConfiguration).protobufReportPath();
    verify(protobufDataImporter).importResults(tester, workDir.getRoot(), CSharpSonarRulesDefinition.REPOSITORY_KEY, true);
    verifyZeroInteractions(roslynDataImporter);
  }

  @Test
  public void noAnalysisIfNoFilesDetected() throws Exception {
    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).hasSize(1);
    assertThat(logTester.logs(LoggerLevel.DEBUG).get(0)).isEqualTo("No files to analyze. Skip Sensor.");
  }

  @Test
  public void failWhenOsIsNotWindows() throws MessageException {
    // Arrange
    addFileToFs();
    when(system.isOsWindows()).thenReturn(false);

    // Assert exception
    thrown.expect(MessageException.class);
    thrown.expectMessage("C# analysis is not supported");

    // Act
    sensor.execute(tester);
  }

  @Test
  public void doNotFail_WhenOsIsNotWindows_And_NoCS_FilesDetected() throws MessageException {
    // Arrange
    when(system.isOsWindows()).thenReturn(false);

    // Act
    sensor.execute(tester);
  }

}
