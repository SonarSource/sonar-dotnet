/*
 * SonarSource :: .NET :: Core
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */
package org.sonarsource.dotnet.shared.plugins;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Arrays;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.slf4j.event.Level;
import org.sonar.api.config.Configuration;
import org.sonar.api.internal.apachecommons.lang3.function.Failable;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class ModuleConfigurationTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();
  @Rule
  public LogTester logTester = new LogTester();

  private Path workDir;

  @Before
  public void setUp() {
    logTester.setLevel(Level.TRACE);
    workDir = temp.getRoot().toPath();
  }

  @Test
  public void traceObjectCreation() {
    Configuration configuration = createEmptyMockConfiguration();
    createModuleConfiguration(configuration);
    assertThat(logTester.logs(Level.TRACE)).containsOnly("Project 'Test Project': AbstractModuleConfiguration has been created.");
  }

  @Test
  public void onlyNewRoslynReportPresent() throws IOException {
    Path path = temp.newFile("roslyn-report.json").toPath();
    Path path2 = temp.newFile("roslyn-report2.json").toPath();

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.getStringArray("sonar.cs.roslyn.reportFilePaths")).thenReturn(new String[]{path.toString(), path2.toString()});

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(config.roslynReportPaths()).containsOnly(workDir.resolve("roslyn-report.json"), workDir.resolve("roslyn-report2.json"));
    assertThat(logTester.logs(Level.DEBUG)).containsExactly(
      "Project 'Test Project': Property missing: 'sonar.cs.analyzer.projectOutPaths'. No protobuf files will be loaded for this project.",
      "Project 'Test Project': The Roslyn JSON report path has '"
        + workDir.toString() + "\\roslyn-report.json,"
        + workDir.toString() + "\\roslyn-report2.json'");
  }

  @Test
  public void giveWarningsWhenGettingProtobufPathAndNoPropertyAvailable() {
    Configuration configuration = mock(Configuration.class);

    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[0]);
    when(configuration.getStringArray("sonar.cs.roslyn.reportFilePaths")).thenReturn(new String[0]);
    // no projectKey is set

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsOnly(
      "Project '<NONE>': Property missing: 'sonar.cs.analyzer.projectOutPaths'. No protobuf files will be loaded for this project.");
  }

  @Test
  public void noWarningsWhenGettingProtobufPathAndNoPropertyAvailable_TestProject() {
    // Test projects have sonar.tests property set, main projects don't
    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.hasKey("sonar.tests")).thenReturn(true);

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void giveWarningsWhenGettingProtobufPathAndNoFolderAvailable() throws IOException {
    Path path1 = createProtobufOut("report");

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[]{path1.toString(), "non-existing"});

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).containsOnly(path1.resolve("output-cs"));
    assertThat(logTester.logs(Level.DEBUG))
      .containsExactly(
        "Project 'Test Project': Analyzer working directory '" + workDir.toString() + "\\report\\output-cs' contains 1 .pb file(s)",
        "Project 'Test Project': Analyzer working directory does not exist: 'non-existing\\output-cs'. Analyzer results won't be loaded from this directory.");
  }

  @Test
  public void giveWarningsWhenGettingOldProtobufPathAndNoFolderAvailable() {
    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.get("sonar.cs.analyzer.projectOutPath")).thenReturn(Optional.of("non-existing"));

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).containsOnly(
      "Project 'Test Project': Property missing: 'sonar.cs.analyzer.projectOutPaths'. No protobuf files will be loaded for this project."
    );
  }

  @Test
  public void whenProtobufReportsArePresent_informHowManyProtoFilesAreFound() throws IOException {
    Configuration configuration = createEmptyMockConfiguration();
    mockProtobufOutPaths(configuration);

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isNotEmpty();
    assertThat(logTester.logs(Level.DEBUG))
      .containsExactly(
        "Project 'Test Project': Analyzer working directory '" + workDir.toString() + "\\report1\\output-cs' contains 1 .pb file(s)",
        "Project 'Test Project': Analyzer working directory '" + workDir.toString() + "\\report2\\output-cs' contains 1 .pb file(s)");
    assertThat(logTester.logs(Level.WARN)).isEmpty();
  }

  @Test
  public void whenProtobufReportsArePresent_protobufReportPathsContainsCorrectElements() throws IOException {
    Configuration configuration = createEmptyMockConfiguration();
    mockProtobufOutPaths(configuration);

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.roslynReportPaths()).isEmpty();
    assertThat(config.protobufReportPaths()).containsOnly(
      workDir.resolve("report1").resolve("output-cs"),
      workDir.resolve("report2").resolve("output-cs"));
    assertThat(logTester.logs(Level.DEBUG))
      .hasSize(3)
      .contains(
        // roslynReportPaths
        "Project 'Test Project': No Roslyn issues reports have been found.",
        // protobufReportPaths
        "Project 'Test Project': Analyzer working directory '" + workDir.toString() + "\\report1\\output-cs' contains 1 .pb file(s)",
        "Project 'Test Project': Analyzer working directory '" + workDir.toString() + "\\report2\\output-cs' contains 1 .pb file(s)"
      );
  }

  @Test
  public void giveWarningsWhenGettingProtobufPathAndFolderIsEmpty() throws IOException {
    Path path = workDir.resolve("report");
    Path outputCs = path.resolve("output-cs");
    Files.createDirectories(outputCs);

    Configuration configuration = createEmptyMockConfiguration();
    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[]{path.toString()});

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.protobufReportPaths()).isEmpty();
    assertThat(logTester.logs(Level.DEBUG).get(0)).matches(s -> s.endsWith("contains no .pb file(s). Analyzer results won't be loaded from this directory."));
  }

  @Test
  public void reads_correct_language() {
    Configuration configuration = mock(Configuration.class);
    when(configuration.getStringArray("sonar.cs.roslyn.reportFilePaths")).thenReturn(new String[]{"C#"});
    when(configuration.getStringArray("sonar.vbnet.roslyn.reportFilePaths")).thenReturn(new String[]{"VB.NET"});
    ModuleConfiguration config = createModuleConfiguration(configuration);

    assertThat(config.roslynReportPaths()).containsExactly(Paths.get("C#"));
  }

  @Test
  public void telemetryJsonPathsAreFound() {
    Configuration configuration = createEmptyMockConfiguration();
    var expectedPaths = new Path[]{workDir.resolve("0").resolve(ModuleConfiguration.TELEMETRY_JSON), workDir.resolve("1").resolve(ModuleConfiguration.TELEMETRY_JSON)};
    Arrays.stream(expectedPaths).forEach(Failable.asConsumer(x -> {
      Files.createDirectory(x.getParent());
      Files.createFile(x);
    }));

    when(configuration.getStringArray("sonar.cs.scanner.telemetry")).thenReturn(Arrays.stream(expectedPaths).map(Path::toString).toArray(String[]::new));

    ModuleConfiguration config = createModuleConfiguration(configuration);
    assertThat(config.telemetryJsonPaths()).containsExactly(expectedPaths);
  }

  private Path createProtobufOut(String name) throws IOException {
    Path path = workDir.resolve(name);
    Path outputCs = path.resolve("output-cs");
    Files.createDirectories(outputCs);
    Files.createFile(outputCs.resolve("dummy.pb"));
    return path;
  }

  private void mockProtobufOutPaths(Configuration configuration) throws IOException {
    Path path1 = createProtobufOut("report1");
    Path path2 = createProtobufOut("report2");
    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[]{path1.toString(), path2.toString()});
  }

  private Configuration createEmptyMockConfiguration() {
    Configuration configuration = mock(Configuration.class);

    when(configuration.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(new String[0]);
    when(configuration.getStringArray("sonar.cs.roslyn.reportFilePaths")).thenReturn(new String[0]);
    when(configuration.get("sonar.projectKey")).thenReturn(Optional.of("Test Project"));

    return configuration;
  }

  private ModuleConfiguration createModuleConfiguration(Configuration configuration) {
    PluginMetadata metadata = mock(PluginMetadata.class);
    when(metadata.languageKey()).thenReturn("cs");
    return new ModuleConfiguration(configuration, metadata);
  }
}
