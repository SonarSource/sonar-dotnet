/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2019 SonarSource SA
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
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.fs.InputFile.Type;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.config.Configuration;
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
  public TemporaryFolder temp = new TemporaryFolder();

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
  public void describe_global_sensor() {
    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .describe(descriptor);

    assertThat(descriptor.isGlobal()).isTrue();
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

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsOnly("No coverage property. Skip Sensor");
  }

  @Test
  public void execute_warn_about_deprecated_integration_tests() throws IOException {
    when(coverageAggregator.hasCoverageProperty()).thenReturn(true);

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", true)
      .execute(context);

    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("Starting with SonarQube 6.2 separation between Unit Tests and Integration Tests "
      + "Coverage reports is deprecated. Please move all reports specified from *.it.reportPaths into *.reportPaths.");
  }

  @Test
  public void analyze() throws Exception {
    SensorContextTester context = computeCoverageMeasures(false);
    assertThat(context.lineHits("foo:Foo.cs", 2)).isEqualTo(1);
    assertThat(context.lineHits("foo:Foo.cs", 4)).isEqualTo(0);
  }

  @Test
  public void analyzeIntegrationTests() throws Exception {
    SensorContextTester context = computeCoverageMeasures(true);
    assertThat(context.lineHits("foo:Foo.cs", 2)).isEqualTo(1);
    assertThat(context.lineHits("foo:Foo.cs", 4)).isEqualTo(0);
  }

  @Test
  public void execute_coverage_no_main_file() throws IOException {
    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getCanonicalPath();
    when(coverage.files()).thenReturn(new HashSet<>(Collections.singletonList(fooPath)));

    context.fileSystem().add(new TestInputFileBuilder("foo", "Foo.cs").setLanguage("cs")
      .setType(Type.TEST).build());

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .analyze(context, coverage);

    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("The Code Coverage report doesn't contain any coverage "
      + "data for the included files. For troubleshooting hints, please refer to https://docs.sonarqube.org/x/CoBh");
  }

  @Test
  public void execute_coverage_not_indexed_file() throws IOException {
    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getCanonicalPath();
    when(coverage.files()).thenReturn(new HashSet<>(Collections.singletonList(fooPath)));

    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", false)
      .analyze(context, coverage);

    assertThat(logTester.logs(LoggerLevel.DEBUG)).containsOnly("The file '" + fooPath + "' is either excluded or outside of "
      + "your solution folder therefore Code Coverage will not be imported.");
  }

  private SensorContextTester computeCoverageMeasures(boolean isIntegrationTest) throws Exception {
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

}
