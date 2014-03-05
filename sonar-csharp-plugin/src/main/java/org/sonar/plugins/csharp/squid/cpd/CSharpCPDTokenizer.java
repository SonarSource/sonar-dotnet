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
package org.sonar.plugins.csharp.squid.cpd;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.csharp.squid.lexer.CSharpLexer;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.impl.Lexer;
import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.TokenEntry;
import net.sourceforge.pmd.cpd.Tokenizer;
import net.sourceforge.pmd.cpd.Tokens;

import java.io.File;
import java.nio.charset.Charset;

import static com.sonar.sslr.api.GenericTokenType.EOF;

public class CSharpCPDTokenizer implements Tokenizer {

  private final Charset charset;

  public CSharpCPDTokenizer(Charset charset) {
    this.charset = charset;
  }

  @Override
  public final void tokenize(SourceCode source, Tokens cpdTokens) {
    CSharpConfiguration conf = new CSharpConfiguration(charset);
    Lexer lexer = CSharpLexer.create(conf, new IgnoreUsingDirectivePreprocessor(conf));

    String fileName = source.getFileName();
    for (Token token : lexer.lex(new File(fileName))) {
      if (token.getType() == EOF) {
        break;
      }

      TokenEntry cpdToken = new TokenEntry(getTokenImage(token), fileName, token.getLine());
      cpdTokens.add(cpdToken);
    }
    cpdTokens.add(TokenEntry.getEOF());
  }

  private String getTokenImage(Token token) {
    if (token.getType() == CSharpTokenType.STRING_LITERAL) {
      return CSharpTokenType.STRING_LITERAL.getValue();
    }
    return token.getValue();
  }

}
