/*
 * Sonar C# Plugin :: C# Squid :: Sonar Plugin
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
package com.sonar.plugins.csharp.squid.cpd;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Preprocessor;
import com.sonar.sslr.api.PreprocessorAction;
import com.sonar.sslr.api.RecognitionException;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.Trivia;
import com.sonar.sslr.impl.Parser;

import java.util.ArrayList;
import java.util.List;

public class IgnoreUsingDirectivePreprocessor extends Preprocessor {

  private final Parser<CSharpGrammar> parser;

  public IgnoreUsingDirectivePreprocessor(CSharpConfiguration conf) {
    this.parser = CSharpParser.create(conf);
    this.parser.setRootRule(this.parser.getGrammar().usingDirective);
  }

  @Override
  public PreprocessorAction process(List<Token> tokens) {
    if (tokens.get(0).getType() == CSharpKeyword.USING) {
      try {
        AstNode usingDirectiveNode = this.parser.parse(tokens);
        return new PreprocessorAction(usingDirectiveNode.getToIndex(), new ArrayList<Trivia>(), new ArrayList<Token>());
      } catch (RecognitionException re) {
        return PreprocessorAction.NO_OPERATION;
      }
    } else {
      return PreprocessorAction.NO_OPERATION;
    }
  }

}
