/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop;

public final class Constants {

  private Constants() {
  }

  public static final String PLUGIN_KEY = "fxcop";
  public static final String PLUGIN_NAME = "FxCop";

  public static final String REPOSITORY_KEY = PLUGIN_KEY;
  public static final String REPOSITORY_NAME = PLUGIN_NAME;

  public static final String LANGUAGE_KEY = "cs";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FXCOP_MODE_KEY = "sonar.dotnet.fxcop";
  public static final String FXCOP_DEFAULT_MODE = "enable";
  public static final String FXCOP_SKIP_MODE = "skip";
  public static final String FXCOP_REUSE_MODE = "reuseReport";

  public static final String FXCOP_REPORT_KEY = "sonar.dotnet.fxcop.reportsPath";
  public static final String FXCOP_REPORT_XML = "fxcop-report.xml";
  public static final String SL_FXCOP_REPORT_XML = "silverlight-fxcop-report.xml";

  public static final String FXCOP_TRANSFO_XSL = "fxcop-transformation.xsl";

  public static final String FXCOP_PROCESSED_REPORT_SUFFIX = ".processed";

}
