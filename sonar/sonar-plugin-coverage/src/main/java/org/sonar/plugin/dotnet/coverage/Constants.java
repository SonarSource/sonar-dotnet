/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.coverage;

public class Constants {

  public static final String COVERAGE_MODE_KEY = "sonar.dotnet.coverage";
  public static final String COVERAGE_DEFAULT_MODE = "enable";
  public static final String COVERAGE_SKIP_MODE = "skip";
  public static final String COVERAGE_REUSE_MODE = "reuseReport";
  public static final String COVERAGE_REPORT_KEY = "sonar.dotnet.coverage.reportPath";

  public static final String COVERAGE_REPORT_XML = "coverage-report.xml";
  
}
