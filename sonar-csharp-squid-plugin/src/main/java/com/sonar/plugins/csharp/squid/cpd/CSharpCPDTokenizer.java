/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.plugins.csharp.squid.cpd;

import java.io.File;
import java.nio.charset.Charset;

import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.TokenEntry;
import net.sourceforge.pmd.cpd.Tokenizer;
import net.sourceforge.pmd.cpd.Tokens;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.csharp.squid.lexer.CSharpLexer;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.TokenType;
import com.sonar.sslr.impl.Lexer;

public class CSharpCPDTokenizer implements Tokenizer {

  private final boolean ignoreLiterals;
  private final Charset charset;

  public CSharpCPDTokenizer(boolean ignoreLiterals, Charset charset) {
    this.charset = charset;
    this.ignoreLiterals = ignoreLiterals;
  }

  public final void tokenize(SourceCode source, Tokens cpdTokens) {
    Lexer lexer = CSharpLexer.create(new CSharpConfiguration(charset));
    String fileName = source.getFileName();
    for (Token token : lexer.lex(new File(fileName))) {
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
