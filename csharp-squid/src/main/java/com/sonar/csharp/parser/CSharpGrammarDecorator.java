/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import static com.sonar.csharp.api.CSharpPunctuator.LCURLYBRACE;
import static com.sonar.csharp.api.CSharpPunctuator.RCURLYBRACE;
import static com.sonar.csharp.api.CSharpPunctuator.SEMICOLON;
import static com.sonar.sslr.api.GenericTokenType.EOF;
import static com.sonar.sslr.impl.matcher.Matchers.not;
import static com.sonar.sslr.impl.matcher.Matchers.o2n;
import static com.sonar.sslr.impl.matcher.Matchers.one2n;
import static com.sonar.sslr.impl.matcher.Matchers.opt;
import static com.sonar.sslr.impl.matcher.Matchers.or;

import com.sonar.sslr.api.GrammarDecorator;
import com.sonar.sslr.impl.GrammarFieldsInitializer;
/**
 * 
 * CLASSE ORIGINALE A MODIFIER
 *
 */
public class CSharpGrammarDecorator implements GrammarDecorator<CSharpGrammar> {

  public void decorate(CSharpGrammar cs) {
    GrammarFieldsInitializer.initializeRuleFields(cs, CSharpGrammar.class);

    // External Definitions
    cs.compilationUnit.is(opt(cs.compilationUnitLevel), EOF);
    cs.compilationUnitLevel.is(one2n(or(cs.line, cs.block)));
    cs.block.is(one2n(not(or(SEMICOLON, LCURLYBRACE, EOF))), LCURLYBRACE, opt(cs.compilationUnitLevel), RCURLYBRACE);
    cs.line.is(o2n(not(or(SEMICOLON, LCURLYBRACE, EOF))), SEMICOLON);
  }
}
