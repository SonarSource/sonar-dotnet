/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.devkit;

import com.sonar.csharp.squid.api.CSharpKeyword;
import org.sonar.colorizer.*;

import java.util.ArrayList;
import java.util.List;

public final class CSharpSourceCodeColorizer {

  private CSharpSourceCodeColorizer() {
  }

  public static List<Tokenizer> getTokenizers() {
    List<Tokenizer> tokenizers = new ArrayList<Tokenizer>();
    tokenizers.add(new CDocTokenizer("<span class=\"cd\">", "</span>"));
    tokenizers.add(new CppDocTokenizer("<span class=\"cppd\">", "</span>"));
    tokenizers.add(new KeywordsTokenizer("<span class=\"k\">", "</span>", CSharpKeyword.keywordValues()));
    tokenizers.add(new LiteralTokenizer("<span class=\"s\">", "</span>"));
    tokenizers.add(new RegexpTokenizer("<span class=\"j\">", "</span>", "#[^\\n\\r]*+")); // preprocessor directives
    tokenizers.add(new RegexpTokenizer("<span class=\"c\">", "</span>", "[+-]?[0-9]++(\\.[0-9]*+)?")); // decimal constant
    return tokenizers;
  }

}
