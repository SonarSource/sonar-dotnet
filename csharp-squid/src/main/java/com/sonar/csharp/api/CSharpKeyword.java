/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

public enum CSharpKeyword implements TokenType {
  AUTO("auto"), BOOL("_Bool"), BREAK("break"), CASE("case"), CHAR("char"), COMPLEX("_Complex"), CONST("const"), CONTINUE("continue"), DEFAULT(
      "default"), DO("do"), DOUBLE("double"), ELSE("else"), ENUM("enum"), EXTERN("extern"), FLOAT("float"), FOR("for"), GOTO("goto"), IF(
      "if"), IMAGINARY("_Imaginary"), INLINE("inline"), INT("int"), LONG("long"), REGISTER("register"), RESTRICT("restrict"), RETURN(
      "return"), SHORT("short"), SIGNED("signed"), SIZEOF("sizeof"), STATIC("static"), STRUCT("struct"), SWITCH("switch"), TYPEDEF(
      "typedef"), UNION("union"), UNSIGNED("unsigned"), VOID("void"), VOLATILE("volatile"), WHILE("while");

  private final String value;

  private CSharpKeyword(String value) {
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
    CSharpKeyword[] keywordsEnum = CSharpKeyword.values();
    String[] keywords = new String[keywordsEnum.length];
    for (int i = 0; i < keywords.length; i++) {
      keywords[i] = keywordsEnum[i].getValue();
    }
    return keywords;
  }
}
