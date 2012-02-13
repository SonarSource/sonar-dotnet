/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.squid.parser;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.lexer.CSharpLexer;
import com.sonar.sslr.impl.Parser;

/**
 * Parser for the C# language.
 */
public class CSharpParser extends Parser<CSharpGrammar> {

  public CSharpParser() {
    this(new CSharpConfiguration());
  }

  @SuppressWarnings("unchecked")
  public CSharpParser(CSharpConfiguration configuration) {
    super(new CSharpGrammar(), new CSharpLexer(configuration), new CSharpGrammarDecorator());
  }

}
