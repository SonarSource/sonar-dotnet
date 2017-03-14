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

import com.google.common.collect.ImmutableSet;
import java.io.File;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.ArgumentCaptor;
import org.mockito.Mockito;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.config.Settings;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CoverageAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void hasCoverageProperty() {
    Settings settings = mock(Settings.class);

    CoverageConfiguration coverageConf = new CoverageConfiguration("", "ncover", "opencover", "dotcover", "visualstudio");

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(false);
    when(settings.hasKey("dotcover")).thenReturn(false);
    when(settings.hasKey("visualstudio")).thenReturn(false);
    assertThat(new CoverageAggregator(coverageConf, settings).hasCoverageProperty()).isFalse();

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(true);
    when(settings.hasKey("dotcover")).thenReturn(false);
    when(settings.hasKey("visualstudio")).thenReturn(false);
    assertThat(new CoverageAggregator(coverageConf, settings).hasCoverageProperty()).isTrue();

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(false);
    when(settings.hasKey("dotcover")).thenReturn(true);
    when(settings.hasKey("visualstudio")).thenReturn(false);
    assertThat(new CoverageAggregator(coverageConf, settings).hasCoverageProperty()).isTrue();

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(false);
    when(settings.hasKey("dotcover")).thenReturn(false);
    when(settings.hasKey("visualstudio")).thenReturn(true);
    assertThat(new CoverageAggregator(coverageConf, settings).hasCoverageProperty()).isTrue();

    when(settings.hasKey("ncover")).thenReturn(true);
    when(settings.hasKey("opencover")).thenReturn(true);
    when(settings.hasKey("dotcover")).thenReturn(true);
    when(settings.hasKey("visualstudio")).thenReturn(true);
    assertThat(new CoverageAggregator(coverageConf, settings).hasCoverageProperty()).isTrue();

    coverageConf = new CoverageConfiguration("", "ncover2", "opencover2", "dotcover2", "visualstudio2");
    when(settings.hasKey("ncover")).thenReturn(true);
    when(settings.hasKey("opencover")).thenReturn(true);
    when(settings.hasKey("dotcover")).thenReturn(true);
    when(settings.hasKey("visualstudio")).thenReturn(true);
    assertThat(new CoverageAggregator(coverageConf, settings).hasCoverageProperty()).isFalse();
  }

  @Test
  public void aggregate() {
    CoverageCache cache = mock(CoverageCache.class);
    when(cache.readCoverageFromCacheOrParse(Mockito.any(CoverageParser.class), Mockito.any(File.class))).thenAnswer(
      new Answer<Coverage>() {
        @Override
        public Coverage answer(InvocationOnMock invocation) throws Throwable {
          CoverageParser parser = (CoverageParser)invocation.getArguments()[0];
          File reportFile = (File)invocation.getArguments()[1];
          Coverage coverage = new Coverage();
          parser.accept(reportFile, coverage);
          return coverage;
        }
      }
    );

    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);

    CoverageConfiguration coverageConf = new CoverageConfiguration("", "ncover", "opencover", "dotcover", "visualstudio");
    Settings settings = mock(Settings.class);

    when(settings.hasKey("ncover")).thenReturn(true);
    when(settings.getString("ncover")).thenReturn("foo.nccov");
    when(wildcardPatternFileProvider.listFiles("foo.nccov")).thenReturn(ImmutableSet.of(new File("foo.nccov")));
    when(settings.hasKey("opencover")).thenReturn(false);
    when(settings.hasKey("dotcover")).thenReturn(false);
    when(settings.hasKey("visualstudio")).thenReturn(false);
    NCover3ReportParser ncoverParser = mock(NCover3ReportParser.class);
    OpenCoverReportParser openCoverParser = mock(OpenCoverReportParser.class);
    DotCoverReportsAggregator dotCoverParser = mock(DotCoverReportsAggregator.class);
    VisualStudioCoverageXmlReportParser visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    Coverage coverage = mock(Coverage.class);
    ArgumentCaptor<Coverage> captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings, cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser).accept(Mockito.eq(new File("foo.nccov")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(ncoverParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(true);
    when(settings.getString("opencover")).thenReturn("bar.xml");
    when(wildcardPatternFileProvider.listFiles("bar.xml")).thenReturn(ImmutableSet.of(new File("bar.xml")));
    when(settings.hasKey("dotcover")).thenReturn(false);
    when(settings.hasKey("visualstudio")).thenReturn(false);
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings, cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser).accept(Mockito.eq(new File("bar.xml")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(openCoverParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(false);
    when(settings.hasKey("dotcover")).thenReturn(true);
    when(settings.getString("dotcover")).thenReturn("baz.html");
    when(wildcardPatternFileProvider.listFiles("baz.html")).thenReturn(ImmutableSet.of(new File("baz.html")));
    when(settings.hasKey("visualstudio")).thenReturn(false);
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings, cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser).accept(Mockito.eq(new File("baz.html")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(dotCoverParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());
    verify(visualStudioCoverageXmlReportParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));

    when(settings.hasKey("ncover")).thenReturn(false);
    when(settings.hasKey("opencover")).thenReturn(false);
    when(settings.hasKey("dotcover")).thenReturn(false);
    when(settings.hasKey("visualstudio")).thenReturn(true);
    when(settings.getString("visualstudio")).thenReturn("qux.coveragexml");
    when(wildcardPatternFileProvider.listFiles("qux.coveragexml")).thenReturn(ImmutableSet.of(new File("qux.coveragexml")));
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);
    new CoverageAggregator(coverageConf, settings, cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
      .aggregate(wildcardPatternFileProvider, coverage);
    verify(ncoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(openCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(dotCoverParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(Coverage.class));
    verify(visualStudioCoverageXmlReportParser).accept(Mockito.eq(new File("qux.coveragexml")), captor.capture());
    verify(cache).readCoverageFromCacheOrParse(Mockito.eq(visualStudioCoverageXmlReportParser), Mockito.any(File.class));
    verify(coverage).mergeWith(captor.getValue());

    Mockito.reset(wildcardPatternFileProvider);

    when(settings.hasKey("ncover")).thenReturn(true);
    when(settings.getString("ncover")).thenReturn(",*.nccov  ,bar.nccov");
    when(wildcardPatternFileProvider.listFiles("*.nccov")).thenReturn(ImmutableSet.of(new File("foo.nccov")));
    when(wildcardPatternFileProvider.listFiles("bar.nccov")).thenReturn(ImmutableSet.of(new File("bar.nccov")));
    when(settings.hasKey("opencover")).thenReturn(true);
    when(settings.getString("opencover")).thenReturn("bar.xml");
    when(wildcardPatternFileProvider.listFiles("bar.xml")).thenReturn(ImmutableSet.of(new File("bar.xml")));
    when(settings.hasKey("dotcover")).thenReturn(true);
    when(settings.getString("dotcover")).thenReturn("baz.html");
    when(wildcardPatternFileProvider.listFiles("baz.html")).thenReturn(ImmutableSet.of(new File("baz.html")));
    when(settings.hasKey("visualstudio")).thenReturn(true);
    when(settings.getString("visualstudio")).thenReturn("qux.coveragexml");
    when(wildcardPatternFileProvider.listFiles("qux.coveragexml")).thenReturn(ImmutableSet.of(new File("qux.coveragexml")));
    ncoverParser = mock(NCover3ReportParser.class);
    openCoverParser = mock(OpenCoverReportParser.class);
    dotCoverParser = mock(DotCoverReportsAggregator.class);
    visualStudioCoverageXmlReportParser = mock(VisualStudioCoverageXmlReportParser.class);
    coverage = mock(Coverage.class);
    captor = ArgumentCaptor.forClass(Coverage.class);

    new CoverageAggregator(coverageConf, settings, cache, ncoverParser, openCoverParser, dotCoverParser, visualStudioCoverageXmlReportParser)
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

    for (Coverage capturedCoverage: captor.getAllValues()) {
      verify(coverage).mergeWith(capturedCoverage);
    }
  }

}
