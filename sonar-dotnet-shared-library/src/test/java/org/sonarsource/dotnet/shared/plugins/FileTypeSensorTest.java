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
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.ArgumentCaptor;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
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
  private static final String LANG_KEY = "LANG_KEY";
  private static final String SHORT_LANG_NAME = "SHORT_LANG_NAME";

  @Rule
  public LogTester logTester = new LogTester();
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  private Settings settingsMock;
  private SensorContextTester tester;
  private ProjectTypeCollector projectTypeCollectorMock;
  private DotNetPluginMetadata pluginMetadataMock;
  private FileTypeSensor sensor;

  @Before
  public void prepare() throws Exception {
    settingsMock = mock(Settings.class);
    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(temp.newFolder().toPath());
    tester.setSettings(settingsMock);

    projectTypeCollectorMock = mock(ProjectTypeCollector.class);

    pluginMetadataMock = mock(DotNetPluginMetadata.class);
    when(pluginMetadataMock.languageKey()).thenReturn(LANG_KEY);
    when(pluginMetadataMock.shortLanguageName()).thenReturn(SHORT_LANG_NAME);

    sensor = new FileTypeSensor(projectTypeCollectorMock, pluginMetadataMock);
  }

  @Test
  public void should_describe() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.name()).isEqualTo(SHORT_LANG_NAME + " Project Type Information");
    // should not filter per language
    assertThat(sensorDescriptor.languages()).isEmpty();
  }

  @Test
  public void whenProjectOutPathsNotPresent_shouldNotAddProjectInfo_shouldNotLog() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");
    when(settingsMock.getString("sonar.projectKey")).thenReturn("FOO_PROJ:GUID");
    when(settingsMock.getString("sonar.projectBaseDir")).thenReturn("BASE DIR");
    // note that below it's the singular - Path
    when(settingsMock.getString("sonar.cs.analyzer.projectOutPath")).thenReturn("CS OutPath");
    when(settingsMock.getString("sonar.vbnet.analyzer.projectOutPath")).thenReturn("VB OutPath");
    // should query the `getStringArray` method, not the `getString` method like below
    when(settingsMock.getString("sonar.cs.analyzer.projectOutPaths")).thenReturn("CS OutPaths");
    when(settingsMock.getString("sonar.vbnet.analyzer.projectOutPaths")).thenReturn("VB OutPaths");
    addFileToFileSystem("foo.language", Type.MAIN);
    addFileToFileSystem("bar.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs()).isEmpty();
    verifyZeroInteractions(projectTypeCollectorMock);
  }

  @Test
  public void whenProjectOutPaths_returnsNull_shouldNotAddProjectInfo_shouldNotLog() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(null);
    addFileToFileSystem("foo.language", Type.MAIN);
    addFileToFileSystem("bar.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs()).isEmpty();
    verifyZeroInteractions(projectTypeCollectorMock);
  }

  @Test
  public void whenProjectOutPaths_returnsEmpty_shouldNotAddProjectInfo_shouldNotLog() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(new String[]{});
    addFileToFileSystem("foo.language", Type.MAIN);
    addFileToFileSystem("bar.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs()).isEmpty();
    verifyZeroInteractions(projectTypeCollectorMock);
  }

  @Test
  public void whenProjectOutPaths_andLanguageKey_ArePresent_logsAnalyzerWorkDir() {
    when(settingsMock.getString("sonar.projectName")).thenReturn("FOO_PROJ");
    when(settingsMock.getString("sonar.projectKey")).thenReturn("FOO_PROJ:GUID");
    when(settingsMock.getString("sonar.projectBaseDir")).thenReturn("BASE DIR");
    // the following property specifies the analyzer work dir
    when(settingsMock.getStringArray("sonar.cs.analyzer.projectOutPaths")).thenReturn(arrayOf("CS PATH"));
    when(settingsMock.getStringArray("sonar.vbnet.analyzer.projectOutPaths")).thenReturn(arrayOf("VB PATH"));

    when(pluginMetadataMock.languageKey()).thenReturn("cs");
    sensor.execute(tester);
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'false') for project 'FOO_PROJ' (project key 'FOO_PROJ:GUID', base dir 'BASE DIR'). For debug info, see ProjectInfo.xml in 'CS PATH'.");

    logTester.clear();
    when(pluginMetadataMock.languageKey()).thenReturn("vbnet");
    sensor.execute(tester);
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'false') for project 'FOO_PROJ' (project key 'FOO_PROJ:GUID', base dir 'BASE DIR'). For debug info, see ProjectInfo.xml in 'VB PATH'.");
  }

  @Test
  public void whenProjectOutPathsPresent_andHasNoFiles_shouldAddCorrectInfo() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(arrayOf("foo\\.sonarqube\\out\\0"));

    sensor.execute(tester);

    assertThat(logTester.logs()).hasSize(1);
    // we haven't mocked the base dir and project out settings
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'false') for project '' (project key '', base dir ''). For debug info, see ProjectInfo.xml in 'foo\\.sonarqube\\out\\0'.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo(false, false);
  }

  @Test
  public void whenProjectOutPathsPresent_andHasOnlyTestFiles_shouldAddCorrectInfo() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(arrayOf("foo\\.sonarqube\\out\\0"));
    addFileToFileSystem("foo.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs()).hasSize(1);
    // we haven't mocked the base dir and project out settings
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'false', has TEST 'true') for project '' (project key '', base dir ''). For debug info, see ProjectInfo.xml in 'foo\\.sonarqube\\out\\0'.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo(false, true);
  }

  @Test
  public void whenProjectOutPathsPresent_andHasOnlyMainFiles_shouldAddCorrectInfo() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(arrayOf("foo\\.sonarqube\\out\\0"));
    addFileToFileSystem("foo.language", Type.MAIN);

    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'true', has TEST 'false') for project '' (project key '', base dir ''). For debug info, see ProjectInfo.xml in 'foo\\.sonarqube\\out\\0'.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo(true, false);
  }

  @Test
  public void whenProjectOutPathsPresent_andHasBothMainAndTestFiles_shouldAddCorrectInfo() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(arrayOf("foo\\.sonarqube\\out\\0"));
    addFileToFileSystem("foo.language", Type.MAIN);
    addFileToFileSystem("bar.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'true', has TEST 'true') for project '' (project key '', base dir ''). For debug info, see ProjectInfo.xml in 'foo\\.sonarqube\\out\\0'.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo(true, true);
  }

  @Test
  public void whenInvokedMultipleTimes_shouldAddInformationForEachInvocation() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(arrayOf("foo\\.sonarqube\\out\\0"));
    addFileToFileSystem("foo.language", Type.MAIN);
    sensor.execute(tester);

    when(settingsMock.getString("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn("foo\\.sonarqube\\out\\1");
    // the file system still has 'foo.language', too - so 2 files for 'bar.proj'
    addFileToFileSystem("bar.language", Type.TEST);
    sensor.execute(tester);

    ArgumentCaptor<Boolean> mainFilesCaptor = ArgumentCaptor.forClass(Boolean.class);
    ArgumentCaptor<Boolean> testFilesCaptor = ArgumentCaptor.forClass(Boolean.class);

    verify(projectTypeCollectorMock, times(2)).addProjectInfo(mainFilesCaptor.capture(), testFilesCaptor.capture());

    assertThat(mainFilesCaptor.getAllValues()).containsExactly(true, true);
    assertThat(testFilesCaptor.getAllValues()).containsExactly(false, true);
  }

  @Test
  public void whenGetStringArray_returnsMultiplePaths_shouldLogConcatenatedValues() {
    when(settingsMock.getStringArray("sonar.LANG_KEY.analyzer.projectOutPaths")).thenReturn(new String[]{"foo\\.sonarqube\\out\\0", "BAR", "QUIX\\FOO"});
    addFileToFileSystem("foo.language", Type.MAIN);
    addFileToFileSystem("bar.language", Type.TEST);

    sensor.execute(tester);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsExactly("Adding file type information (has MAIN 'true', has TEST 'true') for project '' (project key '', base dir ''). For debug info, see ProjectInfo.xml in 'foo\\.sonarqube\\out\\0, BAR, QUIX\\FOO'.");
    verify(projectTypeCollectorMock, times(1)).addProjectInfo(true, true);
  }

  private void addFileToFileSystem(String fileName, Type fileType) {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", fileName)
      .setLanguage(LANG_KEY)
      .setType(fileType)
      .build();
    tester.fileSystem().add(inputFile);
  }

  private static String[] arrayOf(String input) {
    return new String[]{input};
  }
}
