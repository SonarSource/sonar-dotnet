/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid;

public final class CSharpSquidConstants {

  private CSharpSquidConstants() {
  }

  public static final String REPOSITORY_KEY = "csharpsquid";
  public static final String REPOSITORY_NAME = "Sonar";

  public static final String CPD_MINIMUM_TOKENS_PROPERTY = "sonar.cpd.cs.minimumTokens";
  public static final String CPD_IGNORE_LITERALS_PROPERTY = "sonar.cpd.cs.ignoreLiteral";
  public static final boolean CPD_IGNORE_LITERALS_DEFVALUE = true;
  public static final String IGNORE_HEADER_COMMENTS = "sonar.cs.ignoreHeaderComments";

}
