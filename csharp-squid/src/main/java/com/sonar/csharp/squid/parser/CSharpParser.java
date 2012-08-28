/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.lexer.CSharpLexer;
import com.sonar.sslr.impl.Parser;
import com.sonar.sslr.impl.events.ParsingEventListener;

/**
 * Parser for the C# language.
 */
public final class CSharpParser {

  private CSharpParser() {
  }

  public static Parser<CSharpGrammar> create(ParsingEventListener... parsingEventListeners) {
    return create(new CSharpConfiguration(), parsingEventListeners);
  }

  public static Parser<CSharpGrammar> create(CSharpConfiguration conf, ParsingEventListener... parsingEventListeners) {
    return Parser.builder((CSharpGrammar) new CSharpGrammarImpl()).withLexer(CSharpLexer.create(conf))
        .setParsingEventListeners(parsingEventListeners).build();
  }

}
