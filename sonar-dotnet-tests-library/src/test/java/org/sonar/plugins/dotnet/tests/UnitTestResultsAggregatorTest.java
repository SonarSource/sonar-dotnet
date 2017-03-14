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
import org.mockito.Mockito;
import org.sonar.api.config.Settings;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class UnitTestResultsAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void hasUnitTestResultsProperty() {
    Settings settings = mock(Settings.class);

    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");

    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, settings).hasUnitTestResultsProperty()).isFalse();

    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, settings).hasUnitTestResultsProperty()).isTrue();

    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, settings).hasUnitTestResultsProperty()).isTrue();

    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, settings).hasUnitTestResultsProperty()).isTrue();

    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, settings).hasUnitTestResultsProperty()).isTrue();

    unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile2", "nunit2", "xunit2");
    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, settings).hasUnitTestResultsProperty()).isFalse();
  }

  @Test
  public void aggregate() {
    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);

    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");
    Settings settings = mock(Settings.class);

    // Visual Studio test results only
    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(settings.getString("visualStudioTestResultsFile")).thenReturn("foo.trx");
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(ImmutableSet.of(new File("foo.trx")));
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(false);
    VisualStudioTestResultsFileParser visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    NUnitTestResultsFileParser nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    XUnitTestResultsFileParser xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    UnitTestResults results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings, visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser).accept(new File("foo.trx"), results);
    verify(nunitTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));

    // NUnit test results only
    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(settings.getString("nunitTestResultsFile")).thenReturn("foo.xml");
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(false);
    when(wildcardPatternFileProvider.listFiles("foo.xml")).thenReturn(ImmutableSet.of(new File("foo.xml")));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings, visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(nunitTestResultsFileParser).accept(new File("foo.xml"), results);


    // XUnit test results only
    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(true);
    when(settings.getString("xunitTestResultsFile")).thenReturn("foo.xml");
    when(wildcardPatternFileProvider.listFiles("foo.xml")).thenReturn(ImmutableSet.of(new File("foo.xml")));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings, visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(xunitTestResultsFileParser).accept(new File("foo.xml"), results);

    // All configured
    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(settings.getString("visualStudioTestResultsFile")).thenReturn("foo.trx");
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(ImmutableSet.of(new File("foo.trx")));
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(settings.getString("nunitTestResultsFile")).thenReturn("foo.xml");
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(true);
    when(settings.getString("xunitTestResultsFile")).thenReturn("foo.xml");
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(ImmutableSet.of(new File("foo.trx")));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings, visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser).accept(new File("foo.trx"), results);
    verify(nunitTestResultsFileParser).accept(new File("foo.xml"), results);
    verify(xunitTestResultsFileParser).accept(new File("foo.xml"), results);

    // None configured
    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(false);
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings, visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(nunitTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(xunitTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));

    // Multiple files configured
    Mockito.reset(wildcardPatternFileProvider);

    when(settings.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(settings.getString("visualStudioTestResultsFile")).thenReturn(",*.trx  ,bar.trx");
    when(wildcardPatternFileProvider.listFiles("*.trx")).thenReturn(ImmutableSet.of(new File("foo.trx")));
    when(wildcardPatternFileProvider.listFiles("bar.trx")).thenReturn(ImmutableSet.of(new File("bar.trx")));
    when(settings.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(settings.getString("nunitTestResultsFile")).thenReturn(",foo.xml  ,bar.xml");
    when(wildcardPatternFileProvider.listFiles("foo.xml")).thenReturn(ImmutableSet.of(new File("foo.xml")));
    when(wildcardPatternFileProvider.listFiles("bar.xml")).thenReturn(ImmutableSet.of(new File("bar.xml")));
    when(settings.hasKey("xunitTestResultsFile")).thenReturn(true);
    when(settings.getString("xunitTestResultsFile")).thenReturn(",foo2.xml  ,bar2.xml");
    when(wildcardPatternFileProvider.listFiles("foo2.xml")).thenReturn(ImmutableSet.of(new File("foo2.xml")));
    when(wildcardPatternFileProvider.listFiles("bar2.xml")).thenReturn(ImmutableSet.of(new File("bar2.xml")));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);

    new UnitTestResultsAggregator(unitTestConf, settings, visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);

    verify(wildcardPatternFileProvider).listFiles("*.trx");
    verify(wildcardPatternFileProvider).listFiles("bar.trx");
    verify(wildcardPatternFileProvider).listFiles("foo.xml");
    verify(wildcardPatternFileProvider).listFiles("bar.xml");

    verify(visualStudioTestResultsFileParser).accept(new File("foo.trx"), results);
    verify(visualStudioTestResultsFileParser).accept(new File("bar.trx"), results);
    verify(nunitTestResultsFileParser).accept(new File("foo.xml"), results);
    verify(nunitTestResultsFileParser).accept(new File("bar.xml"), results);
    verify(xunitTestResultsFileParser).accept(new File("foo2.xml"), results);
    verify(xunitTestResultsFileParser).accept(new File("bar2.xml"), results);
  }

}
