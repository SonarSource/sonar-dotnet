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

import org.sonar.api.BatchExtension;
import org.sonar.api.config.Settings;
import org.sonar.api.utils.SonarException;

import java.io.File;

public class CoverageProviderFactory implements BatchExtension {

  private final Settings settings;

  public CoverageProviderFactory(Settings settings) {
    this.settings = settings;
  }

  public boolean hasCoverageProperty() {
    return settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY) ||
      settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY);
  }

  public CoverageProvider coverageProvider() {
    CoverageProvider coverageProvider;

    if (settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY) && !settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)) {
      coverageProvider = new NCover3ReportParser(new File(settings.getString(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)));
    } else if (settings.hasKey(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY) && !settings.hasKey(TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY)) {
      coverageProvider = new OpenCoverReportParser(new File(settings.getString(TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY)));
    } else {
      // In case both are not set, this method is not supposed to be called
      throw new SonarException("The properties \"" + TestsPlugin.NCOVER3_REPORT_PATH_PROPERTY +
        "\" and \"" + TestsPlugin.OPEN_COVER_REPORT_PATH_PROPERTY +
        "\" are mutually exclusive, specify either none or just one of them, but not both.");
    }

    return coverageProvider;
  }

}
