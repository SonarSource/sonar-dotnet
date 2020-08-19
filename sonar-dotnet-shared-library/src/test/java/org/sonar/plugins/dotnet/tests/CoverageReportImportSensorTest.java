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
package org.sonar.plugins.dotnet.tests;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableMap;
import java.io.File;
import java.io.IOException;
import java.util.Collections;
import java.util.HashSet;
import java.util.function.Predicate;
import org.junit.Before;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.Mockito;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
import org.sonar.api.scanner.sensor.ProjectSensor;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static java.util.Arrays.asList;
import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CoverageReportImportSensorTest {

  @Rule
  public TemporaryFolder temp = createTempFolder();

  @Rule
  public LogTester logTester = new LogTester();

  private CoverageConfiguration coverageConf = new CoverageConfiguration("cs", "", "", "", "");
  private CoverageAggregator coverageAggregator = mock(CoverageAggregator.class);
  private File baseDir;
  private SensorContextTester context;
  private DefaultSensorDescriptor descriptor = new DefaultSensorDescriptor();

  @Before
  public void setUp() throws IOException {
    baseDir = temp.newFolder();
    context = SensorContextTester.create(baseDir);
  }

  @Test
  public void describe_unit_test() {
    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .describe(descriptor);

    assertThat(descriptor.name()).isEqualTo("C# Tests Coverage Report Import");
  }

  @Test
  public void describe_integration_test() {
    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", true)
      .describe(descriptor);

    assertThat(descriptor.name()).isEqualTo("[Deprecated] C# Integration Tests Coverage Report Import");
  }

  @Test
  public void isProjectSensor() {
    assertThat(ProjectSensor.class.isAssignableFrom(CoverageReportImportSensor.class)).isTrue();
  }

  @Test
  public void describe_execute_only_when_key_present() {
    Configuration configWithKey = mock(Configuration.class);
    when(configWithKey.hasKey("expectedKey")).thenReturn(true);

    Configuration configWithoutKey = mock(Configuration.class);

    when(coverageAggregator.hasCoverageProperty(any(Predicate.class))).thenAnswer((invocationOnMock) -> {
      Predicate<String> pr = invocationOnMock.getArgument(0);
      return pr.test("expectedKey");
    });

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .describe(descriptor);

    assertThat(descriptor.configurationPredicate()).accepts(configWithKey);
    assertThat(descriptor.configurationPredicate()).rejects(configWithoutKey);
  }

  @Test
  public void execute_no_coverage_property() throws Exception {
    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .execute(context);

    assertThat(logTester.logs(LoggerLevel.INFO)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsOnly("No coverage property. Skip Sensor");
  }

  @Test
  public void execute_warn_about_deprecated_integration_tests() throws IOException {
    when(coverageAggregator.hasCoverageProperty()).thenReturn(true);

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", true)
      .execute(context);

    assertThat(logTester.logs(LoggerLevel.INFO)).isEmpty();
    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("Starting with SonarQube 6.2 separation between Unit Tests and Integration Tests "
      + "Coverage reports is deprecated. Please move all reports specified from *.it.reportPaths into *.reportPaths.");
  }

  @Test
  public void analyze() throws Exception {
    SensorContextTester context = computeCoverageMeasures(false);
    assertThat(context.lineHits("foo:Foo.cs", 2)).isEqualTo(1);
    assertThat(context.lineHits("foo:Foo.cs", 4)).isEqualTo(0);
    assertThat(context.coveredConditions("foo:Foo.cs", 1)).isNull();
    assertThat(context.coveredConditions("foo:Foo.cs", 4)).isNull();
    assertThat(context.coveredConditions("foo:Foo.cs", 5)).isEqualTo(1);
    assertThat(context.coveredConditions("foo:Foo.cs", 6)).isEqualTo(2);
  }

  @Test
  public void analyzeIntegrationTests() throws Exception {
    SensorContextTester context = computeCoverageMeasures(true);
    assertThat(context.lineHits("foo:Foo.cs", 2)).isEqualTo(1);
    assertThat(context.lineHits("foo:Foo.cs", 4)).isEqualTo(0);
    assertThat(context.coveredConditions("foo:Foo.cs", 1)).isNull();
    assertThat(context.coveredConditions("foo:Foo.cs", 4)).isNull();
    assertThat(context.coveredConditions("foo:Foo.cs", 5)).isEqualTo(1);
    assertThat(context.coveredConditions("foo:Foo.cs", 6)).isEqualTo(2);
  }

  @Test
  public void execute_coverage_no_main_file() throws IOException {
    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getAbsolutePath();
    when(coverage.files()).thenReturn(new HashSet<>(Collections.singletonList(fooPath)));

    context.fileSystem().add(new TestInputFileBuilder("foo", "Foo.cs").setLanguage("cs")
      .setType(Type.TEST).build());

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .analyze(context, coverage);

    assertThat(logTester.logs(LoggerLevel.INFO)).containsOnly("Coverage Report Statistics: " +
      "1 files, 0 main files, 0 main files with coverage, 1 test files, 0 project excluded files, 0 other language files.");
    assertThat(logTester.logs(LoggerLevel.WARN)).contains("The Code Coverage report doesn't contain any coverage "
      + "data for the included files. For troubleshooting hints, please refer to https://docs.sonarqube.org/x/CoBh");
    assertThat(logTester.logs(LoggerLevel.DEBUG)).contains(
      "Analyzing coverage with wildcardPatternFileProvider with base dir '" + CoverageReportImportSensor.BASE_DIR.getAbsolutePath() + "' and file separator '\\'.",
      "Analyzing coverage after aggregate found '1' coverage files.",
      "Skipping '" + fooPath + "' as it is a test file.",
      "The total number of file count statistics is '1'.");
    assertThat(logTester.logs(LoggerLevel.TRACE)).contains("Counting statistics for '" + fooPath + "'.");
  }

  @Test
  public void execute_coverage_main_file_no_coverage_for_file() throws IOException {
    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getAbsolutePath();
    when(coverage.files()).thenReturn(new HashSet<>(Collections.singletonList(fooPath)));

    context.fileSystem().add(new TestInputFileBuilder("foo", "Foo.cs").setLanguage("cs")
      .setType(Type.MAIN).build());

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .analyze(context, coverage);

    assertThat(logTester.logs(LoggerLevel.INFO)).containsOnly("Coverage Report Statistics: " +
      "1 files, 1 main files, 0 main files with coverage, 0 test files, 0 project excluded files, 0 other language files.");
    assertThat(logTester.logs(LoggerLevel.WARN)).contains("The Code Coverage report doesn't contain any coverage "
      + "data for the included files. For troubleshooting hints, please refer to https://docs.sonarqube.org/x/CoBh");
    assertThat(logTester.logs(LoggerLevel.DEBUG)).contains(
      "Analyzing coverage with wildcardPatternFileProvider with base dir '" + CoverageReportImportSensor.BASE_DIR.getAbsolutePath() + "' and file separator '\\'.",
      "Analyzing coverage after aggregate found '1' coverage files.",
      "No coverage info found for the file '" + fooPath + "'.",
      "The total number of file count statistics is '1'.");
    assertThat(logTester.logs(LoggerLevel.TRACE)).contains(
      "Counting statistics for '" + fooPath + "'.",
      "Checking main file coverage for '" + fooPath + "'.");
  }

  @Test
  public void execute_coverage_not_indexed_file() throws IOException {
    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getCanonicalPath();
    when(coverage.files()).thenReturn(new HashSet<>(Collections.singletonList(fooPath)));

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .analyze(context, coverage);

    assertThat(logTester.logs(LoggerLevel.INFO)).containsOnly("Coverage Report Statistics: " +
      "1 files, 0 main files, 0 main files with coverage, 0 test files, 1 project excluded files, 0 other language files.");
    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsOnly(
      "Analyzing coverage with wildcardPatternFileProvider with base dir '" + CoverageReportImportSensor.BASE_DIR.getAbsolutePath() + "' and file separator '\\'.",
      "Analyzing coverage after aggregate found '1' coverage files.",
      "The file '" + fooPath + "' is either excluded or outside of "
        + "your solution folder therefore Code Coverage will not be imported.",
      "The total number of file count statistics is '1'.");
    assertThat(logTester.logs(LoggerLevel.TRACE)).contains("Counting statistics for '" + fooPath + "'.");
  }

  private SensorContextTester computeCoverageMeasures(boolean isIntegrationTest) {
    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getAbsolutePath();
    String bazPath = new File(baseDir, "Baz.java").getAbsolutePath();
    String barPath = new File(baseDir, "Bar.cs").getAbsolutePath();
    when(coverage.files()).thenReturn(new HashSet<>(asList(fooPath, barPath, bazPath)));
    when(coverage.hits(fooPath)).thenReturn(ImmutableMap.<Integer, Integer>builder()
      .put(2, 1)
      .put(4, 0)
      .build());
    when(coverage.hits(barPath)).thenReturn(ImmutableMap.<Integer, Integer>builder()
      .put(42, 1)
      .build());
    when(coverage.hits(bazPath)).thenReturn(ImmutableMap.<Integer, Integer>builder()
      .put(42, 1)
      .build());
    when(coverage.getBranchCoverage(fooPath)).thenReturn(ImmutableList.<BranchCoverage>builder()
      .add(new BranchCoverage(5, 2, 1))
      .add(new BranchCoverage(6, 3, 2))
      .build());

    DefaultInputFile inputFile = new TestInputFileBuilder("foo", baseDir, new File(baseDir, "Foo.cs"))
      .setLanguage("cs")
      .initMetadata("a\na\na\na\na\na\na\na\na\na\n")
      .build();
    context.fileSystem().add(inputFile);
    context.fileSystem().add(new TestInputFileBuilder("foo", "Baz.java").setLanguage("java").build());

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", isIntegrationTest)
      .analyze(context, coverage);

    verify(coverageAggregator).aggregate(Mockito.any(WildcardPatternFileProvider.class), Mockito.eq(coverage));

    return context;
  }

  // This method has been taken from SonarSource/sonar-scanner-msbuild
  private static TemporaryFolder createTempFolder() {
    // If the test is being run under VSTS then the Scanner will
    // expect the project to be under the VSTS sources directory
    File baseDirectory = null;

    if (VstsUtils.isRunningUnderVsts()) {
      String vstsSourcePath = VstsUtils.getSourcesDirectory();
      baseDirectory = new File(vstsSourcePath);
    }

    return new TemporaryFolder(baseDirectory);
  }
}
