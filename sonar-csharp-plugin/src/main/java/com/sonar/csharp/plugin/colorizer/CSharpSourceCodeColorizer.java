/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin.colorizer;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.web.CodeColorizerFormat;
import org.sonar.colorizer.CDocTokenizer;
import org.sonar.colorizer.CppDocTokenizer;
import org.sonar.colorizer.KeywordsTokenizer;
import org.sonar.colorizer.LiteralTokenizer;
import org.sonar.colorizer.RegexpTokenizer;
import org.sonar.colorizer.Tokenizer;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.plugin.CSharpConstants;

public class CSharpSourceCodeColorizer extends CodeColorizerFormat {

  public CSharpSourceCodeColorizer() {
    super(CSharpConstants.LANGUAGE_KEY);
  }

  @Override
  public List<Tokenizer> getTokenizers() {
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
