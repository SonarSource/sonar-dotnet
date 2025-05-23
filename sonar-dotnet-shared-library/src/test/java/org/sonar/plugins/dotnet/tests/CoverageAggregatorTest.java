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
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.util.Collections;
import java.util.HashSet;
import java.util.function.Predicate;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.ArgumentCaptor;
import org.mockito.Mockito;
import org.sonar.api.batch.fs.FileSystem;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;
import org.slf4j.event.Level;
import org.sonarsource.dotnet.shared.plugins.CodeCoverageProvider;
import org.sonarsource.dotnet.shared.plugins.DotNetPluginMetadata;

import static java.util.Arrays.asList;
import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.doThrow;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CoverageAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

  @Test
  public void hasCoverageProperty() {
    Configuration configuration = mock(Configuration.class);

    CoverageConfiguration coverageConf = new CoverageConfiguration("", "ncover", "opencover", "dotcover", "visualstudio");

    when(configuration.hasKey("ncover")).thenReturn(false);
    when(configuration.hasKey("opencover")).thenReturn(false);
    when(configuration.hasKey("dotcover")).thenReturn(false);
    when(configuration.hasKey("visualstudio")).thenReturn(false);
    assertThat(getCoverageAggregator(configuration, coverageConf).hasCoverageProperty()).isFalse();

    when(configuration.hasKey("ncover")).thenReturn(false);
    when(configuration.hasKey("opencover")).thenReturn(true);
    when(configuration.hasKey("dotcover")).thenReturn(false);
    when(configuration.hasKey("visualstudio")).thenReturn(false);
    assertThat(getCoverageAggregator(configuration, coverageConf).hasCoverageProperty()).isTrue();

    when(configuration.hasKey("ncover")).thenReturn(false);
    when(configuration.hasKey("opencover")).thenReturn(false);
    when(configuration.hasKey("dotcover")).thenReturn(true);
    when(configuration.hasKey("visualstudio")).thenReturn(false);
    assertThat(getCoverageAggregator(configuration, coverageConf).hasCoverageProperty()).isTrue();

    when(configuration.hasKey("ncover")).thenReturn(false);
    when(configuration.hasKey("opencover")).thenReturn(false);
    when(configuration.hasKey("dotcover")).thenReturn(false);
    when(configuration.hasKey("visualstudio")).thenReturn(true);
    assertThat(getCoverageAggregator(configuration, coverageConf).hasCoverageProperty()).isTrue();

    when(configuration.hasKey("ncover")).thenReturn(true);
    when(configuration.hasKey("opencover")).thenReturn(true);
    when(configuration.hasKey("dotcover")).thenReturn(true);
    when(configuration.hasKey("visualstudio")).thenReturn(true);
    assertThat(getCoverageAggregator(configuration, coverageConf).hasCoverageProperty()).isTrue();

    coverageConf = new CoverageConfiguration("", "ncover2", "opencover2", "dotcover2", "visualstudio2");
    when(configuration.hasKey("ncover")).thenReturn(true);
    when(configuration.hasKey("opencover")).thenReturn(true);
    when(configuration.hasKey("dotcover")).thenReturn(true);
    when(configuration.hasKey("visualstudio")).thenReturn(true);
    assertThat(getCoverageAggregator(configuration, coverageConf).hasCoverageProperty()).isFalse();
  }

  private CoverageAggregator getCoverageAggregator(Configuration configuration, CoverageConfiguration coverageConf) {
    ScannerFileService alwaysTrue = mock(ScannerFileService.class);
    when(alwaysTrue.isSupportedAbsolute(anyString())).thenReturn(true);
    return new CoverageAggregator(coverageConf, configuration, alwaysTrue, mock(AnalysisWarnings.class));
  }

  @Test
  public void aggregate() {
    CoverageCache cache = mock(CoverageCache.class);
    when(cache.readCoverageFromCacheOrParse(Mockito.any(CoverageParser.class), Mockito.any(File.class))).thenAnswer(
      invocation -> {
        CoverageParser parser = (CoverageParser) invocation.getArguments()[0];
        File reportFile = (File) invocation.getArguments()[1];
        Coverage coverage = new Coverage();
        parser.accept(reportFile, coverage);
        return coverage;
      });

    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);

    CoverageConfiguration coverageConf = new CoverageConfiguration("", "ncover", "opencover", "dotcover", "visualstudio");
    MapSettings settings = new MapSettings();

    settings.setProperty("ncover", "foo.nccov");
    when(wildcardPatternFileProvider.listFiles("foo.nccov")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.nccov"))));
    NCover3ReportParser ncoverParser = mock(NCover3ReportParser.class);
    OpenCoverReportParser openCoverParser = mock(OpenCoverReportParser.class);
    DotCoverReportsAggregator dotCoverParser = mock(DotCoverReportsAggregator.class);
    VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    Coverage coverage = mock(Coverage.class);
    ArgumentCaptor<Coverage> captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings.asConfig(), cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser).accept(Mockito.eq(new File("foo.nccov")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(ncoverParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    settings.clear();

    settings.setProperty("opencover", "bar.xml");
    when(wildcardPatternFileProvider.listFiles("bar.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.xml"))));
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings.asConfig(), cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser).accept(Mockito.eq(new File("bar.xml")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(openCoverParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    settings.clear();
    settings.setProperty("dotcover", "baz.html");
    when(wildcardPatternFileProvider.listFiles("baz.html")).thenReturn(new HashSet<>(Collections.singletonList(new File("baz.html"))));
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings.asConfig(), cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser).accept(Mockito.eq(new File("baz.html")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(dotCoverParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    settings.clear();
    settings.setProperty("visualstudio", "qux.coveragexml");
    when(wildcardPatternFileProvider.listFiles("qux.coveragexml")).thenReturn(new HashSet<>(Collections.singletonList(new File("qux.coveragexml"))));
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings.asConfig(), cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser).accept(Mockito.eq(new File("qux.coveragexml")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(visualStudioCoverageXmlReportParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());

    Mockito.reset(wildcardPatternFileProvider);

    settings.clear();
    settings.setProperty("ncover", ",*.nccov  ,bar.nccov");
    when(wildcardPatternFileProvider.listFiles("*.nccov")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.nccov"))));
    when(wildcardPatternFileProvider.listFiles("bar.nccov")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.nccov"))));
    settings.setProperty("opencover", "bar.xml");
    when(wildcardPatternFileProvider.listFiles("bar.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.xml"))));
    settings.setProperty("dotcover", "baz.html");
    when(wildcardPatternFileProvider.listFiles("baz.html")).thenReturn(new HashSet<>(asList(new File("baz.html"))));
    settings.setProperty("visualstudio", "qux.coveragexml");
    when(wildcardPatternFileProvider.listFiles("qux.coveragexml")).thenReturn(new HashSet<>(asList(new File("qux.coveragexml"))));
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);

    new CoverageAggregator(coverageConf, settings.asConfig(), cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);

    verify(wildcardPatternFileProvider).listFiles("*.nccov");
    verify(wildcardPatternFileProvider).listFiles("bar.nccov");
    verify(wildcardPatternFileProvider).listFiles("bar.xml");
    verify(wildcardPatternFileProvider).listFiles("baz.html");
    verify(wildcardPatternFileProvider).listFiles("qux.coveragexml");

    verify(ncoverParser).accept(Mockito.eq(new File("foo.nccov")), captor.capture());
    verify(ncoverParser).accept(Mockito.eq(new File("bar.nccov")), captor.capture());
    verify(openCoverParser).accept(Mockito.eq(new File("bar.xml")), captor.capture());
    verify(dotCoverParser).accept(Mockito.eq(new File("baz.html")), captor.capture());
    verify(visualStudioCoverageXmlReportParser).accept(Mockito.eq(new File("qux.coveragexml")), captor.capture());

    for (Coverage capturedCoverage : captor.getAllValues()) {
      verify(coverage).mergeWith(capturedCoverage);
    }
  }

  @Test
  public void aggregate_logs_warning_on_exception() {
    MapSettings settings = new MapSettings();
    settings.setProperty("ncover", "foo.nccov");
    CoverageConfiguration coverageConf = new CoverageConfiguration("", "ncover", "opencover", "dotcover", "visualstudio");

    CoverageCache cache = mock(CoverageCache.class);
    when(cache.readCoverageFromCacheOrParse(Mockito.any(CoverageParser.class), Mockito.any(File.class))).thenReturn(new Coverage());

    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);
    when(wildcardPatternFileProvider.listFiles("foo.nccov")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.nccov"))));

    Coverage coverage = mock(Coverage.class);
    doThrow(new IllegalStateException("TEST EXCEPTION MESSAGE")).when(coverage).mergeWith(any());

    new CoverageAggregator(coverageConf, settings.asConfig(), cache, null, null, null, null)
      .aggregate(wildcardPatternFileProvider, coverage);

    assertThat(logTester.logs(Level.WARN)).containsOnly("Could not import coverage report 'foo.nccov' because 'TEST EXCEPTION MESSAGE'. Troubleshooting guide: https://community.sonarsource.com/t/37151");
  }

  @Test
  public void aggregate_ncover_report_does_not_exist() {
    aggregate_report_does_not_exist("ncover", ".nccov");
  }

  @Test
  public void aggregate_opencover_report_does_not_exist() {
    aggregate_report_does_not_exist("opencover", ".xml");
  }

  @Test
  public void aggregate_dotcover_report_does_not_exist() {
    aggregate_report_does_not_exist("dotcover", ".html");
  }

  @Test
  public void aggregate_visualstudio_report_does_not_exist() {
    aggregate_report_does_not_exist("visualstudio", ".coveragexml");
  }

  // this method needs to be here as the test needs to be in the same package with CoverageAggregator
  @Test
  public void when_UnitTestCoverageAggregator_is_created_from_CodeCoverageProvider_calls_uses_properties() {
    // setup
    CodeCoverageProvider provider = createTestProvider();
    CoverageAggregator sut = provider.new UnitTestCoverageAggregator(
      mock(Configuration.class),
      mock(FileSystem.class),
      mock(AnalysisWarnings.class)
    );

    // act
    Predicate<String> mockPredicate = mock(Predicate.class);
    sut.hasCoverageProperty(mockPredicate);

    // verify
    ArgumentCaptor<String> argument = ArgumentCaptor.forClass(String.class);
    verify(mockPredicate, times(4)).test(argument.capture());
    assertThat(argument.getAllValues()).containsExactlyInAnyOrder(
      "sonar.KEY.ncover3.reportsPaths",
      "sonar.KEY.opencover.reportsPaths",
      "sonar.KEY.dotcover.reportsPaths",
      "sonar.KEY.vscoveragexml.reportsPaths"
    );
  }

  private static CodeCoverageProvider createTestProvider() {
    DotNetPluginMetadata pluginMetadata = mock(DotNetPluginMetadata.class);
    when(pluginMetadata.languageKey()).thenReturn("KEY");
    return new CodeCoverageProvider(pluginMetadata);
  }

  private void aggregate_report_does_not_exist(String propertyName, String reportFileExtension) {
    // Arrange
    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);

    CoverageConfiguration coverageConf = new CoverageConfiguration("", "ncover", "opencover", "dotcover", "visualstudio");
    MapSettings settings = new MapSettings();

    String reportFile = "file-that-does-not-exist" + reportFileExtension;

    settings.setProperty(propertyName, reportFile);
    when(wildcardPatternFileProvider.listFiles(reportFile)).thenReturn(new HashSet<>());

    NCover3ReportParser ncoverParser = mock(NCover3ReportParser.class);
    OpenCoverReportParser openCoverParser = mock(OpenCoverReportParser.class);
    DotCoverReportsAggregator dotCoverParser = mock(DotCoverReportsAggregator.class);
    VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);

    // Act
    new CoverageAggregator(coverageConf, settings.asConfig(), mock(CoverageCache.class), ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, mock(Coverage.class));

    // Assert
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    assertThat(logTester.logs(Level.WARN)).containsOnly("Could not find any coverage report file matching the pattern '" + reportFile + "'. Troubleshooting guide: https://community.sonarsource.com/t/37151");
  }
}
