/*
 * SonarQube .NET Tests Library
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
package org.sonar.plugins.dotnet.tests;

import com.google.common.collect.ImmutableMap;
import java.io.File;
import java.util.HashSet;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.mockito.Mockito;
import org.sonar.api.SonarQubeVersion;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.DefaultSensorDescriptor;
import org.sonar.api.batch.sensor.internal.SensorContextTester;

import static java.util.Arrays.asList;
import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.*;

public class CoverageReportImportSensorTest {

  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @Test
  public void coverage() {
    CoverageConfiguration coverageConf = new CoverageConfiguration("", "", "", "", "");

    CoverageAggregator coverageAggregator = mock(CoverageAggregator.class);

    SonarQubeVersion sonarQubeVersion = new SonarQubeVersion(SonarQubeVersion.V5_6);
    new CoverageReportImportSensor(coverageConf, coverageAggregator, "cs", "C#", sonarQubeVersion, false)
        .describe(new DefaultSensorDescriptor());
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

  private SensorContextTester computeCoverageMeasures(boolean isIntegrationTest) throws Exception {
    File baseDir = temp.newFolder();

    Coverage coverage = mock(Coverage.class);
    String fooPath = new File(baseDir, "Foo.cs").getCanonicalPath();
    String bazPath = new File(baseDir, "Baz.java").getCanonicalPath();
    String barPath = new File(baseDir, "Bar.cs").getCanonicalPath();
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

    CoverageAggregator coverageAggregator = mock(CoverageAggregator.class);

    SensorContextTester context = SensorContextTester.create(baseDir);

    DefaultInputFile inputFile = new TestInputFileBuilder("foo", baseDir, new File(baseDir, "Foo.cs"))
      .setLanguage("cs")
      .initMetadata("a\na\na\na\na\na\na\na\na\na\n")
      .build();
    context.fileSystem().add(inputFile);
    context.fileSystem().add(new TestInputFileBuilder("foo", "Baz.java").setLanguage("java").build());

    CoverageConfiguration coverageConf = new CoverageConfiguration("cs", "", "", "", "");

    SonarQubeVersion sonarQubeVersion = new SonarQubeVersion(SonarQubeVersion.V5_6);
    new CoverageReportImportSensor(coverageConf, coverageAggregator,"cs", "C#", sonarQubeVersion, isIntegrationTest)
        .analyze(context, coverage);

    verify(coverageAggregator).aggregate(Mockito.any(WildcardPatternFileProvider.class), Mockito.eq(coverage));

    return context;
  }

}
