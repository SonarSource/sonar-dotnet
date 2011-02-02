/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp;

public final class CSharpConstants {

  private CSharpConstants() {
  }

  public static final String PLUGIN_KEY = "csharp";
  public static final String PLUGIN_NAME = "C#";

  public static final String REPOSITORY_KEY = PLUGIN_KEY;
  public static final String REPOSITORY_NAME = PLUGIN_NAME;

  public static final String LANGUAGE_KEY = "csharp";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FILE_SUFFIXES_KEY = "sonar.csharp.file.suffixes";
  public static final String FILE_SUFFIXES_DEFVALUE = ".cs";

  public static final String CPD_MINIMUM_TOKENS_PROPERTY = "sonar.cpd.csharp.minimumTokens";

  public static final String CPD_IGNORE_LITERALS_PROPERTY = "sonar.cpd.csharp.ignoreLiteral";
  public static final boolean CPD_IGNORE_LITERALS_DEFVALUE = true;

  public static final String SONAR_CSHARP_WAY_PROFILE_KEY = "Sonar C# Way";
}
