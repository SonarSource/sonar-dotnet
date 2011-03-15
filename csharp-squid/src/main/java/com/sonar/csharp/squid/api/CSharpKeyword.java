/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api;

import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.TokenType;

/**
 * Keywords for the C# language. <br>
 * <br>
 * Note: the ECMA specification (part #9.4.3) tells that the following words are <b>not</b> C# keywords, also they have a specific meaning
 * in the C# language:
 * <ul>
 * <li>add</li>
 * <li>alias</li>
 * <li>get</li>
 * <li>global</li>
 * <li>partial</li>
 * <li>remove</li>
 * <li>set</li>
 * <li>value</li>
 * <li>where</li>
 * <li>yield</li>
 * </ul>
 */
public enum CSharpKeyword implements TokenType {
  ABSTRACT("abstract"), AS("as"), BASE("base"), BOOL("bool"), BREAK("break"), BYTE("byte"), CASE("case"), CATCH("catch"), CHAR("char"), CHECKED(
      "checked"), CLASS("class"), CONST("const"), CONTINUE("continue"), DECIMAL("decimal"), DEFAULT("default"), DELEGATE("delegate"), DO(
      "do"), DOUBLE("double"), ELSE("else"), ENUM("enum"), EVENT("event"), EXPLICIT("explicit"), EXTERN("extern"), FALSE("false"), FINALLY(
      "finally"), FIXED("fixed"), FLOAT("float"), FOR("for"), FOREACH("foreach"), GOTO("goto"), IF("if"), IMPLICIT("implicit"), IN("in"), INT(
      "int"), INTERFACE("interface"), INTERNAL("internal"), IS("is"), LOCK("lock"), LONG("long"), NAMESPACE("namespace"), NEW("new"), NULL(
      "null"), OBJECT("object"), OPERATOR("operator"), OUT("out"), OVERRIDE("override"), PARAMS("params"), PRIVATE("private"), PROTECTED(
      "protected"), PUBLIC("public"), READONLY("readonly"), REF("ref"), RETURN("return"), SBYTE("sbyte"), SEALED("sealed"), SHORT("short"), SIZEOF(
      "sizeof"), STACKALLOC("stackalloc"), STATIC("static"), STRING("string"), STRUCT("struct"), SWITCH("switch"), THIS("this"), THROW(
      "throw"), TRUE("true"), TRY("try"), TYPEOF("typeof"), UINT("uint"), ULONG("ulong"), UNCHECKED("unchecked"), UNSAFE("unsafe"), USHORT(
      "ushort"), USING("using"), VIRTUAL("virtual"), VOID("void"), VOLATILE("volatile"), WHILE("while");

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
