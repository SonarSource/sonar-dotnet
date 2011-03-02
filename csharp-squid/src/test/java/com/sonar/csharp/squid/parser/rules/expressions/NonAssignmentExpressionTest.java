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

public class NonAssignmentExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.nonAssignmentExpression);
    g.conditionalExpression.mock();
    g.lambdaExpression.mock();
    g.queryExpression.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("conditionalExpression"));
    assertThat(p, parse("lambdaExpression"));
    assertThat(p, parse("queryExpression"));
  }

}
