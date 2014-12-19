/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp;

import com.sonar.csharp.squid.api.CSharpKeyword;
import org.sonar.api.web.CodeColorizerFormat;
import org.sonar.colorizer.CDocTokenizer;
import org.sonar.colorizer.CppDocTokenizer;
import org.sonar.colorizer.KeywordsTokenizer;
import org.sonar.colorizer.LiteralTokenizer;
import org.sonar.colorizer.RegexpTokenizer;
import org.sonar.colorizer.Tokenizer;

import java.util.ArrayList;
import java.util.List;

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
    tokenizers.add(new RegexpTokenizer("<span class=\"j\">", SPAN, "#[^\\n\\r]*+"));
    tokenizers.add(new RegexpTokenizer("<span class=\"c\">", SPAN, "[+-]?[0-9]++(\\.[0-9]*+)?"));
    return tokenizers;
  }

}
