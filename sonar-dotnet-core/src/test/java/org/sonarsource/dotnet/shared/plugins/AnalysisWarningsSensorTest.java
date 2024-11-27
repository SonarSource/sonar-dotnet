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
package org.sonarsource.dotnet.shared.plugins;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;
import org.sonarsource.dotnet.shared.plugins.sensors.AnalysisWarningsSensor;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.anyString;
import static org.mockito.Mockito.doThrow;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class AnalysisWarningsSensorTest {

  private static File basePath;
  private static File sonarFolder;
  private static String absoluteBasePath;
  private static AnalysisWarnings analysisWarningsMock;
  private static Configuration configurationMock;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() throws IOException {
    logTester.setLevel(Level.DEBUG);
    basePath = temp.newFolder();
    absoluteBasePath = basePath.toPath().toAbsolutePath().toString();
    sonarFolder = new File(basePath, ".sonar");
    configurationMock = mock(Configuration.class);
    analysisWarningsMock = mock(AnalysisWarnings.class);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.describe(sensorDescriptor);

    assertThat(sensorDescriptor.name()).isEqualTo("Analysis Warnings import");
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void execute_noWorkingDir_doesNotCallAdd() throws IOException {
    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(temp.newFolder()));

    verify(analysisWarningsMock, never()).addUnique(anyString());
  }

  @Test
  public void execute_workingDirWithoutSonarSuffix_doesNotCallAdd() throws IOException {
    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of("wrong"));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(temp.newFolder()));

    verify(analysisWarningsMock, never()).addUnique(anyString());
  }

  @Test
  public void execute_missingWorkingDir_doesNotCallAdd() throws IOException {
    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of("wrong_path\\.sonar"));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(temp.newFolder()));

    verify(analysisWarningsMock, never()).addUnique(anyString());
    assertThat(logTester.logs()).containsExactly(
      "Searching for analysis warnings in wrong_path",
      "Error occurred while loading analysis analysis warnings");
  }

  @Test
  public void execute_workingDirWithNoMatchingFiles_doesNotCallAdd() throws IOException {
    File unrelatedFile = new File(basePath, "otherFile.json");
    unrelatedFile.createNewFile();

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    verify(analysisWarningsMock, never()).addUnique(anyString());
    assertThat(logTester.logs()).containsExactly(String.format("Searching for analysis warnings in %s", absoluteBasePath));
  }

  @Test
  public void execute_workingDirWithWithMatchingFile_addWarnings() throws IOException {
    copyFile("AnalysisWarnings.AutoScan.json");

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    verify(analysisWarningsMock, times(1)).addUnique("First message");
    verify(analysisWarningsMock, times(1)).addUnique("Second message");
    verify(analysisWarningsMock, times(2)).addUnique(anyString());
    assertThat(logTester.logs()).containsExactly(
      String.format("Searching for analysis warnings in %s", absoluteBasePath),
      String.format("Loading analysis warnings from %s\\AnalysisWarnings.AutoScan.json", absoluteBasePath));
  }

  @Test
  public void execute_workingDirWithWithMatchingFiles_addWarnings() throws IOException {
    copyFile("AnalysisWarnings.AutoScan.json");
    copyFile("AnalysisWarnings.Scanner.json");

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    verify(analysisWarningsMock, times(1)).addUnique("First message");
    verify(analysisWarningsMock, times(1)).addUnique("Second message");
    verify(analysisWarningsMock, times(1)).addUnique("Scanner message");
    verify(analysisWarningsMock, times(3)).addUnique(anyString());
    assertThat(logTester.logs()).containsExactly(
      String.format("Searching for analysis warnings in %s", absoluteBasePath),
      String.format("Loading analysis warnings from %s\\AnalysisWarnings.AutoScan.json", absoluteBasePath),
      String.format("Loading analysis warnings from %s\\AnalysisWarnings.Scanner.json", absoluteBasePath));
  }

  @Test
  public void execute_errorWhenCallingService_addWarnings_logsError() throws IOException {
    copyFile("AnalysisWarnings.AutoScan.json");

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));
    doThrow(RuntimeException.class).when(analysisWarningsMock).addUnique(anyString());

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    assertThat(logTester.logs()).containsExactly(
      String.format("Searching for analysis warnings in %s", absoluteBasePath),
      String.format("Loading analysis warnings from %s\\AnalysisWarnings.AutoScan.json", absoluteBasePath),
      "Error occurred while publishing analysis warnings");
  }

  private void copyFile(String fileName) throws IOException {
    Path sourcePath = Paths.get("src/test/resources/analysisWarnings/" + fileName);
    Path targetPath = Paths.get(basePath.getAbsolutePath(), fileName);
    Files.copy(sourcePath, targetPath, StandardCopyOption.REPLACE_EXISTING);
  }
}
