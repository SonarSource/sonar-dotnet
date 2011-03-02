/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class WhileStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.whileStatement);
  }

  @Test
  public void testOk() {
    g.booleanExpression.mock();
    g.embeddedStatement.mock();
    assertThat(p, parse("while ( booleanExpression ) embeddedStatement"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("while (frameIndex < st.FrameCount) { Integer i = 15; }"));
    assertThat(p, parse("while (frameIndex < st.FrameCount) { Integer i = 15;  frameIndex++; }"));
  }
  
}
