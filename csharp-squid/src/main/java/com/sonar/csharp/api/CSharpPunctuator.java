/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

public enum CSharpPunctuator implements TokenType {
  SEMICOLON(";"), EQUAL("="), STAR("*"), LCURLYBRACE("{"), LPARENTHESIS("("), LBRACKET("["), RBRACKET("]"), RPARENTHESIS(")"), RCURLYBRACE(
      "}"), COLON(":"), COMMA(","), DOT("."), EXCLAMATION("!"), SUPERIOR(">"), INFERIOR("<"), PLUS("+"), MINUS("-"), SLASH("/"), BACKSLASH("\\"),
  MODULO("%"), AND("&"), XOR("^"), OR("|"), QUESTION("?"), TILDE("~"), SHARP("#"), DOUBLE_SHARP_OP("##"), EQ_OP("=="), NE_OP("!="), ELLIPSIS("..."), RIGHT_ASSIGN(">>="), 
  LEFT_ASSIGN("<<="), ADD_ASSIGN("+="), SUB_ASSIGN("-="), MUL_ASSIGN("*="), DIV_ASSIGN("/="), MOD_ASSIGN(
          "%="), AND_ASSIGN("&="), XOR_ASSIGN("^="), OR_ASSIGN("|="), RIGHT_OP(">>"), LEFT_OP("<<"), INC_OP("++"), DEC_OP("--"), PTR_OP("->"), AND_OP("&&"), OR_OP("||"),
      LE_OP("<="), GE_OP(">=");;

  private final String value;

  private CSharpPunctuator(String word) {
    this.value = word;
  }

  public String getName() {
    return name();
  }

  public String getValue() {
    return value;
  }

  public boolean hasToBeSkippedFromAst(AstNode node) {
    return false;
  }
}
