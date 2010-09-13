/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

public enum CSharpTokenType implements TokenType {
  DECIMAL_CONSTANT, OCTAL_CONSTANT,CHARACTER_CONSTANT, HEXADECIMAL_CONSTANT, HEXADECIMAL_FLOATING_CONSTANT, DECIMAL_FLOATING_CONSTANT;

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
