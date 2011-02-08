/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class ReturnStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.returnStatement);

  }

  @Test
  public void testOk() {
    g.expression.mock();
    assertThat(p, parse("return;"));
    assertThat(p, parse("return expression;"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("return RightContext is CollectionOperator ? base.LeftPrecedence + 10 : base.LeftPrecedence;"));
  }
  
}
