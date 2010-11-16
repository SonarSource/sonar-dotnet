/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

public enum CSharpPreprocessingKeyword implements TokenType {
  SHARP("#"), DEFINE("define"), UNDEF("undef"), IF("if"), ELIF("elif"), ELSE("else"), ENDIF("endif"), LINE("line"), ERROR("error"), WARNING(
      "warning"), REGION("region"), ENDREGION("endregion"), PRAGMA("pragma");

  private final String value;

  private CSharpPreprocessingKeyword(String value) {
    this.value = value;
  }

  public boolean hasToBeSkippedFromAst(AstNode node) {
    return false;
  }

  public String getName() {
    return name();
  }

  public String getValue() {
    return value;
  }

  public static String[] keywordValues() {
    CSharpPreprocessingKeyword[] keywordsEnum = CSharpPreprocessingKeyword.values();
    String[] keywords = new String[keywordsEnum.length];
    for (int i = 0; i < keywords.length; i++) {
      keywords[i] = keywordsEnum[i].getValue();
    }
    return keywords;
  }
}
