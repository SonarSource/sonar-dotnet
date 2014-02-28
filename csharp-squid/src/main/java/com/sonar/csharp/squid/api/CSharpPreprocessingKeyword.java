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
