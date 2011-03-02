/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ConditionalExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.conditionalExpression);
  }

  @Test
  public void testOk() {
    g.nullCoalescingExpression.mock();
    g.expression.mock();
    assertThat(p, parse("nullCoalescingExpression"));
    assertThat(p, parse("nullCoalescingExpression ? expression : expression"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("arg is string ? \"{0}\" : \"{1}\""));
    assertThat(p, parse("RightContext is CollectionOperator ? base.LeftPrecedence + 10 : base.LeftPrecedence"));
    assertThat(p, parse("arg is double ? true : false"));
  }

}
