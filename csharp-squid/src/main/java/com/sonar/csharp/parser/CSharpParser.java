/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.parser;

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
}
