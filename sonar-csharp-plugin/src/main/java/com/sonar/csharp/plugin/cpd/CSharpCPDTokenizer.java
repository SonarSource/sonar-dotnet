/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin.cpd;

import java.io.File;
import java.nio.charset.Charset;

import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.TokenEntry;
import net.sourceforge.pmd.cpd.Tokenizer;
import net.sourceforge.pmd.cpd.Tokens;

import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.api.CSharpTokenType;
import com.sonar.csharp.lexer.CSharpLexer;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.TokenType;

public class CSharpCPDTokenizer implements Tokenizer {

  private final boolean ignoreLiterals;
  private final Charset charset;

  public CSharpCPDTokenizer(boolean ignoreLiterals, Charset charset) {
    this.charset = charset;
    this.ignoreLiterals = ignoreLiterals;
  }

  public final void tokenize(SourceCode source, Tokens cpdTokens) {
    CSharpLexer lexer = new CSharpLexer(new CSharpConfiguration(charset));
    String fileName = source.getFileName();
    LexerOutput tokens = lexer.lex(new File(fileName));
    for (Token token : tokens.getTokens()) {
      if (isElligibleForDuplicationDetection(token)) {
        TokenEntry cpdToken = new TokenEntry(getTokenImage(token), fileName, token.getLine());
        cpdTokens.add(cpdToken);
      }
    }
    cpdTokens.add(TokenEntry.getEOF());
  }

  private boolean isElligibleForDuplicationDetection(Token token) {
    TokenType type = token.getType();
    return type != GenericTokenType.COMMENT && type != GenericTokenType.EOL && type != GenericTokenType.EOF;
  }

  private String getTokenImage(Token token) {
    if (ignoreLiterals && token.getType() == CSharpTokenType.STRING_LITERAL) {
      return CSharpTokenType.STRING_LITERAL.getValue();
    }
    return token.getValue();
  }

}
