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

public class ForeachStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.foreachStatement);
  }

  @Test
  public void testOk() {
    g.type.mock();
    g.expression.mock();
    g.embeddedStatement.mock();
    assertThat(p, parse("foreach ( type id in expression ) embeddedStatement"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("foreach (Assembly assembly in AppDomain) {}"));
    assertThat(p, parse("foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {}"));
  }
  
}
