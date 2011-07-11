/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.toolkit.squid;

import java.util.ArrayList;
import java.util.List;

import org.sonar.colorizer.CDocTokenizer;
import org.sonar.colorizer.CppDocTokenizer;
import org.sonar.colorizer.KeywordsTokenizer;
import org.sonar.colorizer.LiteralTokenizer;
import org.sonar.colorizer.RegexpTokenizer;
import org.sonar.colorizer.Tokenizer;

import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonarsource.sdk.SsdkGui;

public class CSharpSsdkGui {

  public static void main(String[] args) {
    System.setProperty("com.apple.mrj.application.apple.menu.about.name", "SSDK");
    SsdkGui cSsdkGui = new SsdkGui(new CSharpParser(), getCTokenizers());
    cSsdkGui.setVisible(true);
    cSsdkGui.setSize(1000, 800);
    cSsdkGui.setTitle("C : SonarSource Development Kit");
  }

  public static List<Tokenizer> getCTokenizers() {
    List<Tokenizer> tokenizers = new ArrayList<Tokenizer>();
    tokenizers.add(new CDocTokenizer("<span class=\"cd\">", "</span>"));
    tokenizers.add(new CppDocTokenizer("<span class=\"cppd\">", "</span>"));
    tokenizers.add(new KeywordsTokenizer("<span class=\"k\">", "</span>", CSharpKeyword.keywordValues()));
    tokenizers.add(new LiteralTokenizer("<span class=\"s\">", "</span>"));
    tokenizers.add(new RegexpTokenizer("<span class=\"p\">", "</span>", "#[^\\n\\r/]*+")); // preprocessor directives
    tokenizers.add(new RegexpTokenizer("<span class=\"c\">", "</span>", "[+-]?[0-9]++(\\.[0-9]*+)?")); // decimal constant
    return tokenizers;
  }

}
