/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.squid.lexer.preprocessors;

import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.api.Preprocessor;
import com.sonar.sslr.api.Token;

public class StandardPreprocessorLinePreprocessor extends Preprocessor {

  public boolean process(Token token, LexerOutput output) {
    if (token.getType() == CSharpTokenType.PREPROCESSOR) {
      output.addPreprocessingToken(token);
      return true;
    }
    return false;
  }

}
