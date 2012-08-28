/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
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
