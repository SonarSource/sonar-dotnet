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

import java.io.File;
import java.util.Collections;
import java.util.HashSet;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.Mockito;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.internal.MapSettings;

import static java.util.Arrays.asList;
import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class UnitTestResultsAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void hasUnitTestResultsProperty() {
    Configuration configuration = mock(Configuration.class);

    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isFalse();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(false);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(false);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isTrue();

    unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile2", "nunit2", "xunit2");
    when(configuration.hasKey("visualStudioTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("nunitTestResultsFile")).thenReturn(true);
    when(configuration.hasKey("xunitTestResultsFile")).thenReturn(true);
    assertThat(new UnitTestResultsAggregator(unitTestConf, configuration).hasUnitTestResultsProperty()).isFalse();
  }

  @Test
  public void aggregate() {
    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);

    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");
    MapSettings settings = new MapSettings();

    // Visual Studio test results only
    settings.setProperty("visualStudioTestResultsFile", "foo.trx");
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.trx"))));
    VisualStudioTestResultsFileParser visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    NUnitTestResultsFileParser nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    XUnitTestResultsFileParser xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    UnitTestResults results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser).accept(new File("foo.trx"), results);
    verify(nunitTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));

    // NUnit test results only
    settings.clear();
    settings.setProperty("nunitTestResultsFile", "foo.xml");
    when(wildcardPatternFileProvider.listFiles("foo.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.xml"))));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(nunitTestResultsFileParser).accept(new File("foo.xml"), results);

    // XUnit test results only
    settings.clear();
    settings.setProperty("xunitTestResultsFile", "foo.xml");
    when(wildcardPatternFileProvider.listFiles("foo.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.xml"))));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(xunitTestResultsFileParser).accept(new File("foo.xml"), results);

    // All configured
    settings.clear();
    settings.setProperty("visualStudioTestResultsFile", "foo.trx");
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(new HashSet<>(asList(new File("foo.trx"))));
    settings.setProperty("nunitTestResultsFile", "foo.xml");
    settings.setProperty("xunitTestResultsFile", "foo.xml");
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(new HashSet<>(asList(new File("foo.trx"))));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser).accept(new File("foo.trx"), results);
    verify(nunitTestResultsFileParser).accept(new File("foo.xml"), results);
    verify(xunitTestResultsFileParser).accept(new File("foo.xml"), results);

    // None configured
    settings.clear();
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);
    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
      .aggregate(wildcardPatternFileProvider, results);
    verify(visualStudioTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(nunitTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));
    verify(xunitTestResultsFileParser, Mockito.never()).accept(Mockito.any(File.class), Mockito.any(UnitTestResults.class));

    // Multiple files configured
    Mockito.reset(wildcardPatternFileProvider);
    settings.clear();
    settings.setProperty("visualStudioTestResultsFile", ",*.trx  ,bar.trx");
    when(wildcardPatternFileProvider.listFiles("*.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.trx"))));
    when(wildcardPatternFileProvider.listFiles("bar.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.trx"))));
    settings.setProperty("nunitTestResultsFile", ",foo.xml  ,bar.xml");
    when(wildcardPatternFileProvider.listFiles("foo.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.xml"))));
    when(wildcardPatternFileProvider.listFiles("bar.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.xml"))));
    settings.setProperty("xunitTestResultsFile", ",foo2.xml  ,bar2.xml");
    when(wildcardPatternFileProvider.listFiles("foo2.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo2.xml"))));
    when(wildcardPatternFileProvider.listFiles("bar2.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar2.xml"))));
    visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    nunitTestResultsFileParser = mock(NUnitTestResultsFileParser.class);
    xunitTestResultsFileParser = mock(XUnitTestResultsFileParser.class);
    results = mock(UnitTestResults.class);

    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, nunitTestResultsFileParser, xunitTestResultsFileParser)
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
