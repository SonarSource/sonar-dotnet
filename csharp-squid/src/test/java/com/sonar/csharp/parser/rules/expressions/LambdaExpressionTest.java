/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class LambdaExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.lambdaExpression);
  }

  @Test
  public void testOk() {
    g.anonymousFunctionSignature.mock();
    g.anonymousFunctionBody.mock();
    assertThat(p, parse("anonymousFunctionSignature => anonymousFunctionBody"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }
  
  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("(x,y)=>String.Compare(x, y, true)"));
    assertThat(p, parse("item => item.Id == prdId"));
  }

}
