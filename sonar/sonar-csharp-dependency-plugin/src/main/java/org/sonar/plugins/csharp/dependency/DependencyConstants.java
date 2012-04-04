/*
 * Sonar C# Plugin :: Dependency
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

package org.sonar.plugins.csharp.dependency;

public final class DependencyConstants {

  private DependencyConstants() {
  }

  public static final String DEPENDENCYPARSER_REPORT_XML = "dependencyparser-report.xml";

  public static final String INSTALL_DIR_KEY = "sonar.dependencyparser.installDirectory";

  public static final String TIMEOUT_MINUTES_KEY = "sonar.dependencyparser.timeoutMinutes";
  public static final int TIMEOUT_MINUTES_DEFVALUE = 10;

  public static final String MODE = "sonar.dependencyparser.mode";

  public static final String REPORTS_PATH_KEY = "sonar.dependencyparser.reports.path";
}
