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

import java.io.File;
import java.util.Collections;
import java.util.HashSet;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.Mockito;
import org.sonar.api.config.Configuration;
import org.sonar.api.config.internal.MapSettings;
import org.sonar.api.utils.log.LogTester;
import org.sonar.api.utils.log.LoggerLevel;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.ArgumentMatchers.notNull;
import static org.mockito.Mockito.doThrow;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class UnitTestResultsAggregatorTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Rule
  public LogTester logTester = new LogTester();

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
  public void aggregate_visual_studio_only() {
    AggregateTestContext context = new AggregateTestContext("visualStudioTestResultsFile", "foo.trx");
    context.run();

    verify(context.visualStudio).accept(eq(new File("foo.trx")), notNull());
    verify(context.nunit, Mockito.never()).accept(any(), any());
    verify(context.xunit, Mockito.never()).accept(any(), any());
  }

  @Test
  public void aggregate_nunit_only() {
    AggregateTestContext context = new AggregateTestContext("nunitTestResultsFile", "foo.xml");
    context.run();

    verify(context.visualStudio, Mockito.never()).accept(any(), any());
    verify(context.nunit).accept(eq(new File("foo.xml")), notNull());
    verify(context.xunit, Mockito.never()).accept(any(), any());
  }

  @Test
  public void aggregate_xunit_only() {
    AggregateTestContext context = new AggregateTestContext("xunitTestResultsFile", "foo.xml");
    context.run();

    verify(context.visualStudio, Mockito.never()).accept(any(), any());
    verify(context.nunit, Mockito.never()).accept(any(), any());
    verify(context.xunit).accept(eq(new File("foo.xml")), notNull());
  }

  @Test
  public void aggregate_all_formats_configured() {
    AggregateTestContext context = new AggregateTestContext();
    context.add("visualStudioTestResultsFile", "foo.trx");
    context.add("nunitTestResultsFile", "foo.xml");
    context.add("xunitTestResultsFile", "foo.xml");
    context.run();

    verify(context.visualStudio).accept(eq(new File("foo.trx")), notNull());
    verify(context.nunit).accept(eq(new File("foo.xml")), notNull());
    verify(context.xunit).accept(eq(new File("foo.xml")), notNull());
  }

  @Test
  public void aggregate_none_formats_configured() {
    AggregateTestContext context = new AggregateTestContext();
    context.run();

    verify(context.visualStudio, Mockito.never()).accept(any(), any());
    verify(context.nunit, Mockito.never()).accept(any(), any());
    verify(context.xunit, Mockito.never()).accept(any(), any());
  }

  @Test
  public void aggregate_multiple_files_configured() {
    AggregateTestContext context = new AggregateTestContext();
    context.settings.setProperty("visualStudioTestResultsFile", ", *.trx, bar.trx");
    when(context.fileProvider.listFiles("*.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.trx"))));
    when(context.fileProvider.listFiles("bar.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.trx"))));
    context.settings.setProperty("nunitTestResultsFile", ", foo.xml, bar.xml");
    when(context.fileProvider.listFiles("foo.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.xml"))));
    when(context.fileProvider.listFiles("bar.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar.xml"))));
    context.settings.setProperty("xunitTestResultsFile", ", foo2.xml, bar2.xml");
    when(context.fileProvider.listFiles("foo2.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo2.xml"))));
    when(context.fileProvider.listFiles("bar2.xml")).thenReturn(new HashSet<>(Collections.singletonList(new File("bar2.xml"))));
    context.run();

    verify(context.fileProvider).listFiles("*.trx");
    verify(context.fileProvider).listFiles("bar.trx");
    verify(context.fileProvider).listFiles("foo.xml");
    verify(context.fileProvider).listFiles("bar.xml");

    verify(context.visualStudio).accept(eq(new File("foo.trx")), notNull());
    verify(context.visualStudio).accept(eq(new File("bar.trx")), notNull());
    verify(context.nunit).accept(eq(new File("foo.xml")), notNull());
    verify(context.nunit).accept(eq(new File("bar.xml")), notNull());
    verify(context.xunit).accept(eq(new File("foo2.xml")), notNull());
    verify(context.xunit).accept(eq(new File("bar2.xml")), notNull());
  }

  @Test
  public void aggregate_logs_warning_on_exception() {
    UnitTestConfiguration unitTestConf = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");
    MapSettings settings = new MapSettings();
    settings.setProperty("visualStudioTestResultsFile", "foo.trx");

    WildcardPatternFileProvider wildcardPatternFileProvider = mock(WildcardPatternFileProvider.class);
    when(wildcardPatternFileProvider.listFiles("foo.trx")).thenReturn(new HashSet<>(Collections.singletonList(new File("foo.trx"))));

    VisualStudioTestResultsFileParser visualStudioTestResultsFileParser = mock(VisualStudioTestResultsFileParser.class);
    doThrow(IllegalStateException.class).when(visualStudioTestResultsFileParser).accept(notNull(), notNull());

    new UnitTestResultsAggregator(unitTestConf, settings.asConfig(), visualStudioTestResultsFileParser, null, null)
      .aggregate(wildcardPatternFileProvider);

    assertThat(logTester.logs(LoggerLevel.WARN)).containsOnly("Could not import unit test report 'foo.trx'");
  }

  private class AggregateTestContext {

    public final WildcardPatternFileProvider fileProvider = mock(WildcardPatternFileProvider.class);
    public final MapSettings settings = new MapSettings();
    public final VisualStudioTestResultsFileParser visualStudio = mock(VisualStudioTestResultsFileParser.class);
    public final NUnitTestResultsFileParser nunit = mock(NUnitTestResultsFileParser.class);
    public final XUnitTestResultsFileParser xunit = mock(XUnitTestResultsFileParser.class);
    public final UnitTestConfiguration config = new UnitTestConfiguration("visualStudioTestResultsFile", "nunitTestResultsFile", "xunitTestResultsFile");;

    public AggregateTestContext() {
    }

    public AggregateTestContext(String propertyName, String fileName) {
      add(propertyName, fileName);
    }

    public void add(String propertyName, String fileName) {
      settings.setProperty(propertyName, fileName);
      when(fileProvider.listFiles(fileName)).thenReturn(new HashSet<>(Collections.singletonList(new File(fileName))));
    }

    public void run() {
      new UnitTestResultsAggregator(config, settings.asConfig(), visualStudio, nunit, xunit).aggregate(fileProvider);
    }
  }
}
