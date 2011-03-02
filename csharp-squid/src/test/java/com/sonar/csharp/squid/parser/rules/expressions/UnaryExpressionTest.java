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

public class UnaryExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.unaryExpression);
  }

  @Test
  public void testOk() {
    g.primaryExpression.mock();
    g.type.mock();
    assertThat(p, parse("primaryExpression"));
    assertThat(p, parse("+primaryExpression"));
    assertThat(p, parse("-primaryExpression"));
    assertThat(p, parse("!primaryExpression"));
    assertThat(p, parse("~primaryExpression"));
    assertThat(p, parse("++primaryExpression"));
    assertThat(p, parse("--primaryExpression"));
    assertThat(p, parse("( type ) primaryExpression"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("(Level)info.GetValue(\"Level\", typeof(Level))"));
  }

}
