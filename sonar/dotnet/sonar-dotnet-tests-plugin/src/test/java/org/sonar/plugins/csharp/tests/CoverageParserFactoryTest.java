/*
 * Sonar .NET Plugin :: Tests
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp.tests;

import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.sonar.api.config.Settings;
import org.sonar.api.utils.SonarException;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CoverageParserFactoryTest {

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  @Test
  public void hasCoverageProperty() {
    Settings settings = mock(Settings.class);

    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(false);
    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(false);
    assertThat(new CoverageParserFactory(settings).hasCoverageProperty()).isFalse();

    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(false);
    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(true);
    assertThat(new CoverageParserFactory(settings).hasCoverageProperty()).isTrue();

    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(true);
    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(false);
    assertThat(new CoverageParserFactory(settings).hasCoverageProperty()).isTrue();

    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(true);
    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(true);
    assertThat(new CoverageParserFactory(settings).hasCoverageProperty()).isTrue();
  }

  @Test
  public void coverageProvider() {
    Settings settings = mock(Settings.class);

    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(true);
    when(settings.getString(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn("");
    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(false);
    assertThat(new CoverageParserFactory(settings).coverageProvider()).isInstanceOf(NCover3ReportParser.class);

    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(true);
    when(settings.getString(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn("");
    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(false);
    assertThat(new CoverageParserFactory(settings).coverageProvider()).isInstanceOf(OpenCoverReportParser.class);
  }

  @Test
  public void coverageProvider_should_fail_when_several_reports_are_provided() {
    thrown.expect(SonarException.class);
    thrown.expectMessage("The properties \"" + TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY +
      "\" and \"" + TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY +
      "\" are mutually exclusive, specify either none or just one of them, but not both.");

    Settings settings = mock(Settings.class);

    when(settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)).thenReturn(true);
    when(settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)).thenReturn(true);
    new CoverageParserFactory(settings).coverageProvider();
  }

}
