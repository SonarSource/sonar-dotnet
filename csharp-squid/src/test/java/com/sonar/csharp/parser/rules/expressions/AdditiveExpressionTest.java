/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.parser.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class AdditiveExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.additiveExpression);
    g.multiplicativeExpression.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("multiplicativeExpression"));
    assertThat(p, parse("multiplicativeExpression - multiplicativeExpression "));
    assertThat(p, parse("multiplicativeExpression + multiplicativeExpression + multiplicativeExpression - multiplicativeExpression"));
    assertThat(p, parse("multiplicativeExpression - multiplicativeExpression - multiplicativeExpression - multiplicativeExpression"));
  }

}
