/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin;

public final class CSharpConstants {

  private CSharpConstants() {
  }

  public static final String PLUGIN_KEY = "cs";
  public static final String PLUGIN_NAME = "C#";

  public static final String REPOSITORY_KEY = PLUGIN_KEY;
  public static final String REPOSITORY_NAME = PLUGIN_NAME;

  public static final String LANGUAGE_KEY = "cs";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FILE_SUFFIXES_KEY = "sonar.csharp.file.suffixes";
  public static final String FILE_SUFFIXES_DEFVALUE = ".cs";

  public static final String CPD_MINIMUM_TOKENS_PROPERTY = "sonar.cpd.cs.minimumTokens";

  public static final String CPD_IGNORE_LITERALS_PROPERTY = "sonar.cpd.cs.ignoreLiteral";
  public static final boolean CPD_IGNORE_LITERALS_DEFVALUE = true;

  public static final String SONAR_CSHARP_WAY_PROFILE_KEY = "Sonar C# Way";

  // TODO property found in the DotNet plugin -> Need to push it to the Plugin
  public static final String SONAR_EXCLUDE_GEN_CODE_KEY = "sonar.dotnet.excludeGeneratedCode";

}
