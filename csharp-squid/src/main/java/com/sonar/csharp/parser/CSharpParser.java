/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.parser;

import com.sonar.csharp.CSharpConfiguration;
import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.lexer.CSharpLexer;
import com.sonar.sslr.impl.Parser;

/**
 * Parser for the C# language.
 */
public class CSharpParser extends Parser<CSharpGrammar> {

  @SuppressWarnings("unchecked")
  public CSharpParser() {
    super(new CSharpGrammar(), new CSharpLexer(), new CSharpGrammarDecorator());
  }

  @SuppressWarnings("unchecked")
  public CSharpParser(CSharpConfiguration configuration) {
    super(new CSharpGrammar(), new CSharpLexer(configuration), new CSharpGrammarDecorator());
  }
  
}
