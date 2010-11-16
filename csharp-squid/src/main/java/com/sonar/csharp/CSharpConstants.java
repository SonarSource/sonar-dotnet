/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp;

public final class CSharpConstants {

  public final static String PLUGIN_KEY = "csharp";
  public final static String PLUGIN_NAME = "C#";

  public final static String REPOSITORY_KEY = PLUGIN_KEY;
  public final static String REPOSITORY_NAME = PLUGIN_NAME;

  public final static String LANGUAGE_KEY = "csharp";
  public final static String LANGUAGE_NAME = "C#";

  public final static String FILE_SUFFIXES_KEY = "sonar.csharp.file.suffixes";
  public final static String FILE_SUFFIXES_DEFVALUE = ".cs";

  public final static String CPD_MINIMUM_TOKENS_PROPERTY = "sonar.cpd.csharp.minimumTokens";

  public final static String CPD_IGNORE_LITERALS_PROPERTY = "sonar.cpd.csharp.ignoreLiteral";
  public final static boolean CPD_IGNORE_LITERALS_DEFVALUE = true;

  public final static String SONAR_CSHARP_WAY_PROFILE_KEY = "Sonar C# Way";
}
