/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

public enum CSharpTokenType implements TokenType {
  INTEGER_DEC_LITERAL, INTEGER_HEX_LITERAL, REAL_LITERAL, CHARACTER_LITERAL, STRING_LITERAL, PREPROCESSOR;
   

  public String getName() {
    return name();
  }

  public String getValue() {
    return name();
  }

  public boolean hasToBeSkippedFromAst(AstNode node) {
    return false;
  }
}
