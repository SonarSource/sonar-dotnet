/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
