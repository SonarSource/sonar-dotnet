/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
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
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.junit.rules.TemporaryFolder;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.rule.internal.ActiveRulesBuilder;
import org.sonar.api.batch.rule.internal.NewActiveRule;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.rule.RuleKey;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyNoInteractions;
import static org.mockito.Mockito.when;

public class DotNetSensorTest {

  private static final String REPO_KEY = "REPO_KEY";
  private static final String LANG_KEY = "LANG_KEY";
  private static final String A_DIFFERENT_LANG_KEY = "another language than the tested plugin";
  private static final String SHORT_LANG_NAME = "SHORT_LANG_NAME";
  private static final String READ_MORE_LOG = "Read more about how the SonarScanner for .NET detects test projects: https://github.com/SonarSource/sonar-scanner-msbuild/wiki/Analysis-of-product-projects-vs.-test-projects";

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
  private ProjectTypeCollector projectTypeCollector = mock(ProjectTypeCollector.class);
  private AnalysisWarnings analysisWarnings = mock(AnalysisWarnings.class);

  private SensorContextTester tester;
  private DotNetSensor sensor;
  private Path workDir;

  @Before
  public void prepare() throws Exception {
    logTester.setLevel(Level.DEBUG);
    workDir = temp.newFolder().toPath();
    reportPaths = Collections.singletonList(workDir.getRoot());
    tester = SensorContextTester.create(new File("src/test/resources"));
    tester.fileSystem().setWorkDir(workDir);
    when(reportPathCollector.protobufDirs()).thenReturn(reportPaths);
    when(projectTypeCollector.getSummary(SHORT_LANG_NAME)).thenReturn(Optional.of("TEST PROJECTS SUMMARY"));
    when(projectTypeCollector.hasProjects()).thenReturn(true);
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn(LANG_KEY);
    when(pluginMetadata.repositoryKey()).thenReturn(REPO_KEY);
    when(pluginMetadata.shortLanguageName()).thenReturn(SHORT_LANG_NAME);
    sensor = new DotNetSensor(pluginMetadata, reportPathCollector, projectTypeCollector, protobufDataImporter, roslynDataImporter, analysisWarnings);
  }

  @Test
  public void checkDescriptor() {
    DefaultSensorDescriptor sensorDescriptor = new DefaultSensorDescriptor();
    sensor.describe(sensorDescriptor);
    assertThat(sensorDescriptor.languages()).containsOnly(LANG_KEY);
    assertThat(sensorDescriptor.name()).isEqualTo(SHORT_LANG_NAME);
  }

  @Test
  public void isProjectSensor() {
    assertThat(ProjectSensor.class.isAssignableFrom(sensor.getClass())).isTrue();
  }

  @Test
  public void whenNoProtobufFiles_shouldNotFail() {
    addMainFileToFileSystem();
    when(reportPathCollector.protobufDirs()).thenReturn(Collections.emptyList());
    when(reportPathCollector.roslynReports()).thenReturn(Collections.singletonList(new RoslynReport(null, workDir.getRoot())));
    tester.setActiveRules(new ActiveRulesBuilder()
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "S1186")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of(REPO_KEY, "[parameters_key]")).build())
      .addRule(new NewActiveRule.Builder().setRuleKey(RuleKey.of("roslyn.foo", "custom-roslyn")).build())
      .build());

    sensor.execute(tester);

    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
    assertThat(logTester.logs(Level.INFO)).containsExactly("TEST PROJECTS SUMMARY");
    assertThat(logTester.logs(Level.WARN)).containsExactly("No protobuf reports found. The " + SHORT_LANG_NAME + " files will not have highlighting and metrics. You can get help on the community forum: https://community.sonarsource.com");
    verify(analysisWarnings, never()).addUnique(any());
    verify(reportPathCollector).protobufDirs();
    verifyNoInteractions(protobufDataImporter);
    Map<String, List<RuleKey>> expectedMap = new HashMap<String, List<RuleKey>>() {{
      put("sonaranalyzer-" + LANG_KEY, Arrays.asList(RuleKey.of(REPO_KEY, "S1186"), RuleKey.of(REPO_KEY, "[parameters_key]")));
      put("foo", Collections.singletonList(RuleKey.of("roslyn.foo", "custom-roslyn")));
    }};
    verify(roslynDataImporter).importRoslynReports(eq(Collections.singletonList(new RoslynReport(null, workDir.getRoot()))), eq(tester), eq(expectedMap),
      any(RealPathProvider.class));
  }

  @Test
  public void whenNoRoslynReport_shouldNotFail() {
    addMainFileToFileSystem();

    sensor.execute(tester);

    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    verifyNoInteractions(roslynDataImporter);
    assertThat(logTester.logs(Level.WARN)).containsExactly("No Roslyn issue reports were found. The " + SHORT_LANG_NAME + " files have not been analyzed. You can get help on the community forum: https://community.sonarsource.com");
    verify(analysisWarnings, never()).addUnique(any());
    assertThat(logTester.logs(Level.INFO)).containsExactly("TEST PROJECTS SUMMARY");
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void whenReportsArePresent_thereAreNoWarnings() {
    addMainFileToFileSystem();
    addRoslynReports();

    sensor.execute(tester);

    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    assertThat(logTester.logs(Level.WARN)).isEmpty();
    verify(analysisWarnings, never()).addUnique(any());
    assertThat(logTester.logs(Level.INFO)).containsExactly("TEST PROJECTS SUMMARY");
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void whereThereIsNoSummary_doNoLogSummary() {
    addMainFileToFileSystem();
    addRoslynReports();
    when(projectTypeCollector.getSummary(SHORT_LANG_NAME)).thenReturn(Optional.empty());
    sensor.execute(tester);
    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.INFO)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void whenThereAreBothMainAndTestFiles_doNotLog() {
    addMainFileToFileSystem();
    addTestFileToFileSystem();
    // add roslyn reports to avoid warnings in the logs for this test
    addRoslynReports();

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
    verify(analysisWarnings, never()).addUnique(any());
  }

  @Test
  public void whenThereAreOnlyTestFilesInAnotherLanguage_logOnlySkipSensor() {
    addFileToFileSystem("qix", Type.TEST, A_DIFFERENT_LANG_KEY);

    sensor.execute(tester);
    assertThat(logTester.logs(Level.WARN)).isEmpty();
    verify(analysisWarnings, never()).addUnique(any());
    assertThat(logTester.logs(Level.DEBUG)).containsExactlyInAnyOrder("No files to analyze. Skip Sensor.");
  }

  @Test
  public void whenThereAreOnlyMainFilesInAnotherLanguage_logOnlySkipSensor() {
    addFileToFileSystem("qix", Type.MAIN, A_DIFFERENT_LANG_KEY);

    sensor.execute(tester);
    assertThat(logTester.logs(Level.WARN)).isEmpty();
    verify(analysisWarnings, never()).addUnique(any());
    assertThat(logTester.logs(Level.DEBUG)).containsExactlyInAnyOrder("No files to analyze. Skip Sensor.");
  }

  @Test
  public void whenThereAreOnlyTestFilesInPluginLanguage_andNoMainFilesInAnyLanguage_resultsAreImportedAndLogsConsoleAndAnalysisWarnings() {
    addTestFileToFileSystem();
    addRoslynReports();

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN))
      .containsExactly("SonarScanner for .NET detected only TEST files and no MAIN files for " + SHORT_LANG_NAME + " in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " + READ_MORE_LOG);
    verify(analysisWarnings).addUnique("Your project contains only TEST-code for language " + SHORT_LANG_NAME +
      " and no MAIN-code for any language, so only TEST-code related results are imported. " +
      "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " + READ_MORE_LOG);
    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    assertThat(logTester.logs(Level.INFO)).containsExactly("TEST PROJECTS SUMMARY");
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void whenThereAreOnlyTestFilesInPluginLanguage_andMainFilesInAnotherLanguage_resultsAreImportedWithWarningsOnlyInConsole() {
    addTestFileToFileSystem();
    addFileToFileSystem("qix", Type.MAIN, A_DIFFERENT_LANG_KEY);
    addRoslynReports();

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN))
      .containsExactly("SonarScanner for .NET detected only TEST files and no MAIN files for " + SHORT_LANG_NAME + " in the current solution. " +
        "Only TEST-code related results will be imported to your SonarQube/SonarCloud project. " +
        "Many of our rules (e.g. vulnerabilities) are raised only on MAIN-code. " + READ_MORE_LOG);
    verify(reportPathCollector).protobufDirs();
    verify(protobufDataImporter).importResults(eq(tester), eq(reportPaths), any(RealPathProvider.class));
    assertThat(logTester.logs(Level.INFO)).containsExactly("TEST PROJECTS SUMMARY");
    assertThat(logTester.logs(Level.DEBUG)).isEmpty();
  }

  @Test
  public void whenThereAreNoFiles_logDebug() {
    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    verify(analysisWarnings, never()).addUnique(any());
    assertThat(logTester.logs(Level.DEBUG)).containsExactly("No files to analyze. Skip Sensor.");
  }

  @Test
  public void whenThereAreMainFiles_andNoProjects_logToUseScannerForNet() {
    addMainFileToFileSystem();
    when(projectTypeCollector.hasProjects()).thenReturn(false);

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN)).containsExactly("Your project contains " + SHORT_LANG_NAME + " files which cannot be analyzed with the scanner you are using." +
      " To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html");
    verify(analysisWarnings, never()).addUnique(any());
  }

  @Test
  public void whenThereAreTestFiles_andNoProjects_logToUseScannerForNet() {
    addTestFileToFileSystem();
    when(projectTypeCollector.hasProjects()).thenReturn(false);

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN)).containsExactly("Your project contains " + SHORT_LANG_NAME + " files which cannot be analyzed with the scanner you are using." +
      " To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html");
    verify(analysisWarnings, never()).addUnique(any());
  }

  @Test
  public void whenThereAreMainAndTestFiles_andNoProjects_logToUseScannerForNet() {
    addMainFileToFileSystem();
    addTestFileToFileSystem();
    when(projectTypeCollector.hasProjects()).thenReturn(false);

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN)).containsExactly("Your project contains " + SHORT_LANG_NAME + " files which cannot be analyzed with the scanner you are using." +
      " To analyze C# or VB.NET, you must use the SonarScanner for .NET 5.x or higher, see https://redirect.sonarsource.com/doc/install-configure-scanner-msbuild.html");
    verify(analysisWarnings, never()).addUnique(any());
  }

  @Test
  public void whenThereAreNoFiles_andNoProjects_logDebug() {
    when(projectTypeCollector.hasProjects()).thenReturn(false);

    sensor.execute(tester);

    assertThat(logTester.logs(Level.WARN)).isEmpty();
    verify(analysisWarnings, never()).addUnique(any());
    assertThat(logTester.logs(Level.DEBUG)).containsExactly("No files to analyze. Skip Sensor.");
  }

  private void addMainFileToFileSystem() {
    addFileToFileSystem("foo.language", Type.MAIN, LANG_KEY);
  }

  private void addTestFileToFileSystem() {
    addFileToFileSystem("bar.language", Type.TEST, LANG_KEY);
  }

  private void addFileToFileSystem(String fileName, Type fileType, String language) {
    DefaultInputFile inputFile = new TestInputFileBuilder("mod", fileName)
      .setLanguage(language)
      .setType(fileType)
      .build();
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
