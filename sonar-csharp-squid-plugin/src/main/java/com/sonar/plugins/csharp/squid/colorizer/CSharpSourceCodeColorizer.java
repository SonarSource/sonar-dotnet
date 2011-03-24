/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.colorizer;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.web.CodeColorizerFormat;
import org.sonar.colorizer.CDocTokenizer;
import org.sonar.colorizer.CppDocTokenizer;
import org.sonar.colorizer.KeywordsTokenizer;
import org.sonar.colorizer.LiteralTokenizer;
import org.sonar.colorizer.RegexpTokenizer;
import org.sonar.colorizer.Tokenizer;
import org.sonar.plugins.csharp.api.CSharpConstants;

import com.sonar.csharp.squid.api.CSharpKeyword;

public class CSharpSourceCodeColorizer extends CodeColorizerFormat {

  private static final String SPAN = "</span>";

  public CSharpSourceCodeColorizer() {
    super(CSharpConstants.LANGUAGE_KEY);
  }

  @Override
  public List<Tokenizer> getTokenizers() {
    List<Tokenizer> tokenizers = new ArrayList<Tokenizer>();
    tokenizers.add(new CDocTokenizer("<span class=\"cd\">", SPAN));
    tokenizers.add(new CppDocTokenizer("<span class=\"cppd\">", SPAN));
    tokenizers.add(new KeywordsTokenizer("<span class=\"k\">", SPAN, CSharpKeyword.keywordValues()));
    tokenizers.add(new LiteralTokenizer("<span class=\"s\">", SPAN));
    tokenizers.add(new RegexpTokenizer("<span class=\"j\">", SPAN, "#[^\\n\\r]*+")); // preprocessor directives
    tokenizers.add(new RegexpTokenizer("<span class=\"c\">", SPAN, "[+-]?[0-9]++(\\.[0-9]*+)?")); // decimal constant
    return tokenizers;
  }
}
