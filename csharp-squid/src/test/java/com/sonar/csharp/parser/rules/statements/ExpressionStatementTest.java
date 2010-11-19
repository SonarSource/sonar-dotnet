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

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class ExpressionStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.expressionStatement);
    g.invocationExpression.mock();
    g.objectCreationExpression.mock();
    g.assignment.mock();
    g.postIncrementExpression.mock();
    g.postDecrementExpression.mock();
    g.preIncrementExpression.mock();
    g.preDecrementExpression.mock();
    
  }

  @Test
  public void testOk() {
    assertThat(p, parse("invocationExpression;"));
    assertThat(p, parse("objectCreationExpression;"));
    assertThat(p, parse("assignment;"));
    assertThat(p, parse("postIncrementExpression;"));
    assertThat(p, parse("postDecrementExpression;"));
    assertThat(p, parse("preIncrementExpression;"));
    assertThat(p, parse("preDecrementExpression;"));
  }

}
