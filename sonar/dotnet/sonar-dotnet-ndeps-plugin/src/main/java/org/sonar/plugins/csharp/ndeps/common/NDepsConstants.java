/*
 * Sonar .NET Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps.common;

import com.google.common.collect.Lists;
import org.sonar.api.measures.Metric;
import org.sonar.api.measures.Metrics;

import java.util.List;

public final class NDepsConstants implements Metrics {

  public static final String DEPENDENCYPARSER_REPORT_XML = "ndeps-report.xml";

  public static final String INSTALL_DIR_KEY = "sonar.ndeps.installDirectory";

  public static final String TIMEOUT_MINUTES_KEY = "sonar.ndeps.timeoutMinutes";
  public static final int TIMEOUT_MINUTES_DEFVALUE = 10;

  public static final String MODE = "sonar.ndeps.mode";

  public static final String REPORTS_PATH_KEY = "sonar.ndeps.reports.path";

  public static final String[] SUPPORTED_LANGUAGES = new String[] {"cs", "vbnet"};

  public static final String REPOSITORY_KEY = "ndeps";

  public static final Metric MERGED_TYPES = new Metric.Builder("ndeps_merged_types", "Merged Types", Metric.ValueType.DATA)
      .setDescription("Types from a single source files")
      .setDirection(0)
      .setQualitative(false)
      .setDomain("Design")
      .setHidden(true)
      .setDeleteHistoricalData(true)
      .create();

  public static final String IGNORABLE_FIELDS_KEY = "sonar.ndeps.ignorable.fields";

  //
  // Utility method
  //

  public static boolean isLanguageSupported(String languageKey) {
    for (String key : SUPPORTED_LANGUAGES) {
      if (key.equals(languageKey)) {
        return true;
      }
    }
    return false;
  }

  public List<Metric> getMetrics() {
    return Lists.newArrayList(MERGED_TYPES);
  }
}
