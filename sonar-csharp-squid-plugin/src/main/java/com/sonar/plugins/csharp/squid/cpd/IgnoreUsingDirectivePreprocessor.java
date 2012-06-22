/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.cpd;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.api.*;
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
