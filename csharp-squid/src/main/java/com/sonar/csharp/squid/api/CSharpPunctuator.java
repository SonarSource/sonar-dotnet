/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

public enum CSharpPunctuator implements TokenType {
  SEMICOLON(";"), EQUAL("="), STAR("*"), LCURLYBRACE("{"), LPARENTHESIS("("), LBRACKET("["), RBRACKET("]"), RPARENTHESIS(")"), RCURLYBRACE(
      "}"), COLON(":"), COMMA(","), DOT("."), EXCLAMATION("!"), SUPERIOR(">"), INFERIOR("<"), PLUS("+"), MINUS("-"), SLASH("/"), MODULO("%"), AND(
      "&"), XOR("^"), OR("|"), QUESTION("?"), TILDE("~"), DOUBLE_COLON("::"), DOUBLE_QUESTION("??"), EQ_OP("=="), NE_OP("!="), LEFT_ASSIGN(
      "<<="), ADD_ASSIGN("+="), SUB_ASSIGN("-="), MUL_ASSIGN("*="), DIV_ASSIGN("/="), MOD_ASSIGN("%="), AND_ASSIGN("&="), XOR_ASSIGN("^="), OR_ASSIGN(
      "|="), LEFT_OP("<<"), INC_OP("++"), DEC_OP("--"), PTR_OP("->"), AND_OP("&&"), OR_OP("||"), LE_OP("<="), GE_OP(">="), LAMBDA("=>");

  // 4 étaient en plus Intialement ajoutés par Freddy:
  /*
   * SHARP("#") DOUBLE_SHARP_OP("##") ELLIPSIS("...") BACKSLASH("\\")
   */

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
