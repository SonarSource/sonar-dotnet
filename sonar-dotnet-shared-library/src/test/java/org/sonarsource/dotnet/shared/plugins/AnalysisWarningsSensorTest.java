/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2022 SonarSource SA
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

import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.utils.log.LogTester;

import java.io.File;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.anyString;
import static org.mockito.Mockito.doThrow;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class AnalysisWarningsSensorTest {
  private static final String LANG_KEY = "LANG_KEY";
  private static final String SHORT_LANG_NAME = "SHORT_LANG_NAME";

  private static File basePath;
  private static File sonarFolder;
  private static AnalysisWarnings analysisWarningsMock;
  private static DotNetPluginMetadata pluginMetadataMock;
  private static Configuration configurationMock;

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void before() throws IOException {
    basePath = temp.newFolder();
    sonarFolder = new File(basePath, ".sonar");
    configurationMock = mock(Configuration.class);
    analysisWarningsMock = mock(AnalysisWarnings.class);
    pluginMetadataMock = mock(DotNetPluginMetadata.class);
    when(pluginMetadataMock.languageKey()).thenReturn(LANG_KEY);
    when(pluginMetadataMock.shortLanguageName()).thenReturn(SHORT_LANG_NAME);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.describe(sensorDescriptor);

    assertThat(sensorDescriptor.name()).isEqualTo(SHORT_LANG_NAME + " Analysis Warnings import");
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void execute_noWorkingDir_doesNotCallAdd() throws IOException {
    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(temp.newFolder()));

    verify(analysisWarningsMock, never()).addUnique(anyString());
  }

  @Test
  public void execute_workingDirWithoutSonarSuffix_doesNotCallAdd() throws IOException {
    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of("wrong"));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(temp.newFolder()));

    verify(analysisWarningsMock, never()).addUnique(anyString());
  }

  @Test
  public void execute_missingWorkingDir_doesNotCallAdd() throws IOException {
    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of("wrong\\.sonar"));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(temp.newFolder()));

    verify(analysisWarningsMock, never()).addUnique(anyString());
    assertThat(logTester.logs()).containsExactly("Error occurred while loading analysis analysis warnings");
  }

  @Test
  public void execute_workingDirWithNoMatchingFiles_doesNotCallAdd() throws IOException {
    File unrelatedFile = new File(basePath, "otherFile.json");
    unrelatedFile.createNewFile();

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    verify(analysisWarningsMock, never()).addUnique(anyString());
    assertThat(logTester.logs()).isEmpty();
  }

  @Test
  public void execute_workingDirWithWithMatchingFile_addWarnings() throws IOException {
    copyFile("AnalysisWarnings.AutoScan.json");

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    verify(analysisWarningsMock, times(1)).addUnique("First message");
    verify(analysisWarningsMock, times(1)).addUnique("Second message");
    verify(analysisWarningsMock, times(2)).addUnique(anyString());
    assertThat(logTester.logs()).isEmpty();
  }

  @Test
  public void execute_workingDirWithWithMatchingFiles_addWarnings() throws IOException {
    copyFile("AnalysisWarnings.AutoScan.json");
    copyFile("AnalysisWarnings.Scanner.json");

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    verify(analysisWarningsMock, times(1)).addUnique("First message");
    verify(analysisWarningsMock, times(1)).addUnique("Second message");
    verify(analysisWarningsMock, times(1)).addUnique("Scanner message");
    verify(analysisWarningsMock, times(3)).addUnique(anyString());
    assertThat(logTester.logs()).isEmpty();
  }

  @Test
  public void execute_errorWhenCallingService_addWarnings_logsError() throws IOException {
    copyFile("AnalysisWarnings.AutoScan.json");

    when(configurationMock.get("sonar.working.directory")).thenReturn(Optional.of(sonarFolder.getAbsolutePath()));
    doThrow(RuntimeException.class).when(analysisWarningsMock).addUnique(anyString());

    AnalysisWarningsSensor sensor = new AnalysisWarningsSensor(configurationMock, pluginMetadataMock, analysisWarningsMock);
    sensor.execute(SensorContextTester.create(basePath));

    assertThat(logTester.logs()).containsExactly("Error occurred while publishing analysis warnings");
  }

  private void copyFile(String fileName) throws IOException {
    Path sourcePath = Paths.get("src/test/resources/analysisWarnings/" + fileName);
    Path targetPath = Paths.get(basePath.getAbsolutePath(), fileName);
    Files.copy(sourcePath, targetPath, StandardCopyOption.REPLACE_EXISTING);
  }
}
