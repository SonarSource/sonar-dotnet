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
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.mockito.ArgumentCaptor;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.SensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Settings;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.mockito.Mockito.when;

public class FileTypeSensorTest {
  public static final String REPO_KEY = "REPO_KEY";
  public static final String LANG_KEY = "LANG_KEY";
  public static final String LANG_NAME = "LANG_NAME";

  @Rule
  public LogTester logTester = new LogTester();
  @Rule
  public ExpectedException thrown = ExpectedException.none();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private Path workDir;

  private Settings settingsMock;
  private SensorContextTester tester;
  private ProjectTypeCollector projectTypeCollectorMock;
  private DotNetPluginMetadata pluginMetadataMock;
  private FileTypeSensor sensor;

  @Before
  public void prepare() throws Exception {
    workDir = temp.newFolder().toPath();
    settingsMock = mock(Settings.class);
    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir);
    tester.setSettings(settingsMock);

    projectTypeCollectorMock = mock(ProjectTypeCollector.class);

    pluginMetadataMock = mock(DotNetPluginMetadata.class);
    when(pluginMetadataMock.languageKey()).thenReturn(LANG_KEY);

    sensor = new FileTypeSensor(projectTypeCollectorMock, pluginMetadataMock);
  }


  @Test
  public void should_describe() {

    SensorDescriptor desc = mock(SensorDescriptor.class);
    sensor.describe(desc);

    verify(desc).name("Verify what types of files (MAIN, TEST) are in LANG_KEY projects.");
  }

  @Test
  public void whenProjectNameNotPresent_shouldNotAddProjectInfo() {
    when(settingsMock.getString("sonar.projectName")).thenReturn(null);

    sensor.execute(tester);

    assertThat(logTester.logs()).isEmpty();
    verifyZeroInteractions(projectTypeCollectorMock);
  }

  @Test
  public void whenLanguageKeyIsPresent_logsCsOutputFile() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");
    when(settingsMock.getString("sonar.projectBaseDir")).thenReturn("BASE DIR");
    when(settingsMock.getString("sonar.cs.analyzer.projectOutPath")).thenReturn("CS PATH");
    when(settingsMock.getString("sonar.vbnet.analyzer.projectOutPath")).thenReturn("VB PATH");

    when(pluginMetadataMock.languageKey()).thenReturn("cs");
    sensor.execute(tester);
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'false') for project 'FOO_PROJ' (base dir 'BASE DIR'). For debug info, see ProjectInfo.xml in 'CS PATH'.");

    logTester.clear();
    when(pluginMetadataMock.languageKey()).thenReturn("vbnet");
    sensor.execute(tester);
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'false') for project 'FOO_PROJ' (base dir 'BASE DIR'). For debug info, see ProjectInfo.xml in 'VB PATH'.");
  }

  @Test
  public void whenProjectNamePresent_andHasNoFiles_shouldAddCorrectInfo() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");

    sensor.execute(tester);

    assertThat(logTester.logs()).hasSize(1);
    // we haven't mocked the base dir and project out settings
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'false') for project 'FOO_PROJ' (base dir ''). For debug info, see ProjectInfo.xml in ''.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo( false, false);
  }

  @Test
  public void whenProjectNamePresent_andHasOnlyTestFiles_shouldAddCorrectInfo() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");
    addFileToFileSystem("foo.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs()).hasSize(1);
    // we haven't mocked the base dir and project out settings
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'true') for project 'FOO_PROJ' (base dir ''). For debug info, see ProjectInfo.xml in ''.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo( false, true);
  }

  @Test
  public void whenProjectNamePresent_andHasOnlyMainFiles_shouldAddCorrectInfo() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");
    addFileToFileSystem("foo.language", Type.MAIN);

    sensor.execute(tester);

    verify(projectTypeCollectorMock, times(1)).addProjectInfo( true, false);
  }

  @Test
  public void whenProjectNamePresent_andHasBothMainAndTestFiles_shouldAddCorrectInfo() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");
    addFileToFileSystem("foo.language", Type.MAIN);
    addFileToFileSystem("bar.language", Type.TEST);

    sensor.execute(tester);

    verify(projectTypeCollectorMock, times(1)).addProjectInfo(true, true);
  }

  @Test
  public void whenInvokedMultipleTimes_shouldAddInformationForEachInvocation() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("foo.proj");
    addFileToFileSystem("foo.language", Type.MAIN);
    sensor.execute(tester);

    when(settingsMock.getString("sonar.projectName")).thenReturn("bar.proj");
    // the file system still has 'foo.language', too - so 2 files for 'bar.proj'
    addFileToFileSystem("bar.language", Type.TEST);
    sensor.execute(tester);

    ArgumentCaptor<Boolean> mainFilesCaptor = ArgumentCaptor.forClass(Boolean.class);
    ArgumentCaptor<Boolean> testFilesCaptor = ArgumentCaptor.forClass(Boolean.class);

    verify(projectTypeCollectorMock, times(2)).addProjectInfo(mainFilesCaptor.capture(), testFilesCaptor.capture());

    assertThat(mainFilesCaptor.getAllValues()).containsExactly(true, true);
    assertThat(testFilesCaptor.getAllValues()).containsExactly(false, true);
  }

  private void addFileToFileSystem(String fileName, Type fileType) {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", fileName)
      .setLanguage(LANG_KEY)
      .setType(fileType)
      .build();
    tester.fileSystem().add(inputFile);
  }
}