/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.lexer.preprocessors;

import java.util.ArrayList;
import java.util.List;

import com.google.common.collect.Lists;
import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.sslr.api.Preprocessor;
import com.sonar.sslr.api.PreprocessorAction;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.Trivia;

public class StandardPreprocessorLinePreprocessor extends Preprocessor {

  @Override
  public PreprocessorAction process(List<Token> tokens) {
    Token token = tokens.get(0);

    if (token.getType() == CSharpTokenType.PREPROCESSOR) {
      return new PreprocessorAction(1, Lists.newArrayList(Trivia.createSkippedText(token)), new ArrayList<Token>());
    } else {
      return PreprocessorAction.NO_OPERATION;
    }
  }

}
