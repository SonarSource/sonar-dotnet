/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme;

/**
 * Constants of the Gendarme plugin.
 */
public final class GendarmeConstants {

  private GendarmeConstants() {
  }

  public static final String REPOSITORY_KEY = "gendarme";
  public static final String REPOSITORY_NAME = "Gendarme";

  public static final String GENDARME_RULES_FILE = "sonar.Gendarme";
  public static final String GENDARME_REPORT_XML = "gendarme-report.xml";

  // ----------- Plugin Configuration Properties ----------- //
  public static final String INSTALL_DIR_KEY = "sonar.gendarme.installDirectory";
  public static final String INSTALL_DIR_DEFVALUE = "C:/Program Files/gendarme-2.10-bin";

  public static final String ASSEMBLIES_TO_SCAN_KEY = "sonar.dotnet.assemblies";
  public static final String ASSEMBLIES_TO_SCAN_DEFVALUE = "";

  public static final String GENDARME_CONFIDENCE_KEY = "sonar.gendarme.confidence";
  public static final String GENDARME_CONFIDENCE_DEFVALUE = "normal+";

  public static final String TIMEOUT_MINUTES_KEY = "sonar.gendarme.timeoutMinutes";
  public static final int TIMEOUT_MINUTES_DEFVALUE = 10;

  public static final String MODE = "sonar.gendarme.mode";
  public static final String MODE_SKIP = "skip";
  public static final String MODE_REUSE_REPORT = "reuseReport";

  public static final String REPORTS_PATH_KEY = "sonar.gendarme.reports.path";

}
