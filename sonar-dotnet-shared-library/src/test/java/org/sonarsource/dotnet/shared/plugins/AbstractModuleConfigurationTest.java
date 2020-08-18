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

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.config.Configuration;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class AbstractModuleConfigurationTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();

  private Path workDir;

  @Before
  public void setUp() {
    workDir = temp.getRoot().toPath();
  }

  @Test
  public void onlyNewRoslynReportPresent() throws IOException {
    Path path = temp.newFile("roslyn-report.json").toPath();
    Path path2 = temp.newFile("roslyn-report2.json").toPath();

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.getStringArray("sonar.cs.roslyn.reportFilePaths")).thenReturn(new String[]{path.toString(), path2.toString()});

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(config.roslynReportPaths()).containsOnly(workDir.resolve("roslyn-report.json"), workDir.resolve("roslyn-report2.json"));
  }

  @Test
  public void onlyOldRoslynReportPresent() throws IOException {
    Path path = temp.newFile("roslyn-report.json").toPath();

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.get("sonar.cs.roslyn.reportFilePath")).thenReturn(Optional.of(path.toString()));

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(config.roslynReportPaths()).containsOnly(workDir.resolve("roslyn-report.json"));
  }

  @Test
  public void giveWarningsWhenGettingProtobufPathAndNoPropertyAvailable() {
    Configuration configuration = createEmptyMockConfiguration();

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("Property missing: 'sonar.cs.analyzer.projectOutPaths'. No protobuf files will be loaded for this project.");
  }

  @Test
  public void noWarningsWhenGettingProtobufPathAndNoPropertyAvailable_TestProject() {
    // Test projects have sonar.tests property set, main projects don't
    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.hasKey("sonar.tests")).thenReturn(true);

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
  }

  @Test
  public void giveWarningsWhenGettingProtobufPathAndNoFolderAvailable() throws IOException {
    Path path1 = createProtobufOut("report");

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[] {path1.toString(), "non-existing"});

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).containsOnly(path1.resolve("output-cs"));
    assertThat(logTester.logs(LoggerLevel.WARN)).hasOnlyOneElementSatisfying(s -> s.startsWith("Analyzer working directory does not exist"));
  }

  @Test
  public void giveWarningsWhenGettingOldProtobufPathAndNoFolderAvailable() {
    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.get("sonar.cs.analyzer.projectOutPath")).thenReturn(Optional.of("non-existing"));

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN)).hasOnlyOneElementSatisfying(s -> s.startsWith("Analyzer working directory does not exist"));
  }

  @Test
  public void whenProtobufReportsArePresent_informHowManyProtoFilesAreFound() throws IOException {
    Configuration configuration = createEmptyMockConfiguration();
    mockProtobufOutPaths(configuration);

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isNotEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG))
      .containsExactly(
        "Analyzer working directory '" + workDir.toString() + "\\report1\\output-cs' contains 1 .pb file(s)",
        "Analyzer working directory '" + workDir.toString() + "\\report2\\output-cs' contains 1 .pb file(s)");
    assertThat(logTester.logs(LoggerLevel.WARN)).isEmpty();
  }

  @Test
  public void whenProtobufReportsArePresent_protobufReportPathsContainsCorrectElements() throws IOException {
    Configuration configuration = createEmptyMockConfiguration();
    mockProtobufOutPaths(configuration);

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isNotEmpty();
    assertThat(config.roslynReportPaths()).isEmpty();
    assertThat(config.protobufReportPaths()).containsOnly(
      workDir.resolve("report1").resolve("output-cs"),
      workDir.resolve("report2").resolve("output-cs"));
  }

  @Test
  public void giveWarningsWhenGettingProtobufPathAndFolderIsEmpty() throws IOException {
    Path path = workDir.resolve("report");
    Path outputCs = path.resolve("output-cs");
    Files.createDirectories(outputCs);

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.get("sonar.cs.analyzer.projectOutPath")).thenReturn(Optional.of(path.toString()));

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN).get(0)).matches(s -> s.endsWith("contains no .pb file(s). Analyzer results won't be loaded from this directory."));
  }

  @Test
  public void onlyOldProtobufReportsPresent() throws IOException {
    Path path = createProtobufOut("report");

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.get("sonar.cs.analyzer.projectOutPath")).thenReturn(Optional.of(path.toString()));

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isNotEmpty();
    assertThat(config.roslynReportPaths()).isEmpty();
    assertThat(config.protobufReportPaths()).containsOnly(workDir.resolve("report").resolve("output-cs"));
  }

  @Test
  public void ignoreExternalIssuesIsFalseByDefault() {
    Configuration configuration = mock(Configuration.class);

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.ignoreThirdPartyIssues()).isFalse();
  }

  @Test
  public void optOutExternalIssues() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getBoolean("sonar.cs.roslyn.ignoreIssues")).thenReturn(Optional.of(true));

    AbstractModuleConfiguration config = createAbstractModuleConfiguration(configuration);
    assertThat(config.ignoreThirdPartyIssues()).isTrue();
  }

  private Path createProtobufOut(String name) throws IOException {
    Path path = workDir.resolve(name);
    Path outputCs = path.resolve("output-cs");
    Files.createDirectories(outputCs);
    Files.createFile(outputCs.resolve("dummy.pb"));
    return path;
  }

  private void mockProtobufOutPaths(Configuration configuration) throws IOException{
    Path path1 = createProtobufOut("report1");
    Path path2 = createProtobufOut("report2");
    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[] {path1.toString(), path2.toString()});
  }

  private Configuration createEmptyMockConfiguration(){
    Configuration configuration = mock(Configuration.class);

    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[0]);
    when(configuration.getStringArray("sonar.cs.roslyn.reportFilePaths")).thenReturn(new String[0]);

    return configuration;
  }

  private AbstractModuleConfiguration createAbstractModuleConfiguration(Configuration configuration){
    return new AbstractModuleConfiguration(configuration, "cs"){
    };
  }
}
