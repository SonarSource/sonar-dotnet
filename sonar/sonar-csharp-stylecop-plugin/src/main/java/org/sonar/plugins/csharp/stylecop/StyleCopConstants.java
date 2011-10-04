/*
 * Sonar C# Plugin :: StyleCop
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

package org.sonar.plugins.csharp.stylecop;

/**
 * Constants of the StyleCop plugin.
 */
public final class StyleCopConstants {

  private StyleCopConstants() {
  }

  public static final String REPOSITORY_KEY = "stylecop";
  public static final String REPOSITORY_NAME = "StyleCop";

  public static final String STYLECOP_RULES_FILE = "sonar.StyleCop";
  public static final String STYLECOP_REPORT_XML = "stylecop-report.xml";

  // ----------- Plugin Configuration Properties ----------- //
  public static final String INSTALL_DIR_KEY = "sonar.stylecop.installDirectory";
  public static final String INSTALL_DIR_DEFVALUE = "C:/Program Files/Microsoft StyleCop 4.4.0.14";

  public static final String TIMEOUT_MINUTES_KEY = "sonar.stylecop.timeoutMinutes";
  public static final int TIMEOUT_MINUTES_DEFVALUE = 10;

  public static final String MODE = "sonar.stylecop.mode";

  public static final String REPORTS_PATH_KEY = "sonar.stylecop.reports.path";

}
