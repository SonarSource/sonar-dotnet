/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.fxcop;

/**
 * Constants of the FxCop plugin.
 */
public final class FxCopConstants {

  private FxCopConstants() {
  }

  public static final String PLUGIN_KEY = "fxcop";
  public static final String PLUGIN_NAME = "FxCop";

  public static final String REPOSITORY_KEY = PLUGIN_KEY;
  public static final String REPOSITORY_NAME = PLUGIN_NAME;

  public static final String LANGUAGE_KEY = "cs";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FXCOP_REPORT_XML = "fxcop-report.xml";
  public static final String SL_FXCOP_REPORT_XML = "silverlight-fxcop-report.xml";

  // ----------- Plugin Configuration Properties ----------- //
  public static final String EXECUTABLE_KEY = "sonar.fxcop.executable";
  public static final String EXECUTABLE_DEFVALUE = "C:/Program Files/Microsoft FxCop 10.0/FxCopCmd.exe";

  public static final String ASSEMBLIES_TO_SCAN_KEY = "sonar.fxcop.assemblies";
  public static final String ASSEMBLIES_TO_SCAN_DEFVALUE = "";

  public static final String ASSEMBLY_DEPENDENCY_DIRECTORIES_KEY = "sonar.fxcop.assemblyDependencyDirectories";
  public static final String ASSEMBLY_DEPENDENCY_DIRECTORIES_DEFVALUE = "";
  
  public static final String IGNORE_GENERATED_CODE_KEY = "sonar.fxcop.ignoreGeneratedCode";
  public static final boolean IGNORE_GENERATED_CODE_DEFVALUE = true;

  public static final String TIMEOUT_MINUTES_KEY = "sonar.fxcop.timeoutMinutes";
  public static final int TIMEOUT_MINUTES_DEFVALUE = 10;

}
