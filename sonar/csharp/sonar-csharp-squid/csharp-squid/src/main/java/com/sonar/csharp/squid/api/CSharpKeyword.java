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

public enum CSharpKeyword implements TokenType {
  ABSTRACT("abstract"),
  AS("as"),
  BASE("base"),
  BOOL("bool"),
  BREAK("break"),
  BYTE("byte"),
  CASE("case"),
  CATCH("catch"),
  CHAR("char"),
  CHECKED("checked"),
  CLASS("class"),
  CONST("const"),
  CONTINUE("continue"),
  DECIMAL("decimal"),
  DEFAULT("default"),
  DELEGATE("delegate"),
  DO("do"),
  DOUBLE("double"),
  ELSE("else"),
  ENUM("enum"),
  EVENT("event"),
  EXPLICIT("explicit"),
  EXTERN("extern"),
  FALSE("false"),
  FINALLY("finally"),
  FIXED("fixed"),
  FLOAT("float"),
  FOR("for"),
  FOREACH("foreach"),
  GOTO("goto"),
  IF("if"),
  IMPLICIT("implicit"),
  IN("in"),
  INT("int"),
  INTERFACE("interface"),
  INTERNAL("internal"),
  IS("is"),
  LOCK("lock"),
  LONG("long"),
  NAMESPACE("namespace"),
  NEW("new"),
  NULL("null"),
  OBJECT("object"),
  OPERATOR("operator"),
  OUT("out"),
  OVERRIDE("override"),
  PARAMS("params"),
  PRIVATE("private"),
  PROTECTED("protected"),
  PUBLIC("public"),
  READONLY("readonly"),
  REF("ref"),
  RETURN("return"),
  SBYTE("sbyte"),
  SEALED("sealed"),
  SHORT("short"),
  SIZEOF("sizeof"),
  STACKALLOC("stackalloc"),
  STATIC("static"),
  STRING("string"),
  STRUCT("struct"),
  SWITCH("switch"),
  THIS("this"),
  THROW("throw"),
  TRUE("true"),
  TRY("try"),
  TYPEOF("typeof"),
  UINT("uint"),
  ULONG("ulong"),
  UNCHECKED("unchecked"),
  UNSAFE("unsafe"),
  USHORT("ushort"),
  USING("using"),
  VIRTUAL("virtual"),
  VOID("void"),
  VOLATILE("volatile"),
  WHILE("while");

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
