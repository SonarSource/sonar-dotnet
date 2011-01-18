/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.lexer;

import java.io.File;
import java.nio.charset.Charset;
import java.util.HashSet;
import java.util.Set;

import com.sonar.csharp.CSharpConfiguration;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.api.Token;

public class CSharpNoSonarScanner {

  private final CSharpLexer lexer;
  private static final String NOSONAR_TAG = "NOSONAR";

  public CSharpNoSonarScanner() {
    this.lexer = new CSharpLexer();
  }

  public CSharpNoSonarScanner(Charset charset) {
    this.lexer = new CSharpLexer(new CSharpConfiguration(charset));
  }

  public Set<Integer> scan(File csharpFile) {
    Set<Integer> noSonarTags = new HashSet<Integer>();
    LexerOutput lexerOutput = lexer.lex(csharpFile);
    for (Integer line : lexerOutput.getCommentTokens().keySet()) {
      Token comment = lexerOutput.getCommentTokens().get(line);
      if (comment.getValue().indexOf(NOSONAR_TAG) != -1) {
        noSonarTags.add(line);
      }
    }
    return noSonarTags;
  }

}
