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

public class JumpStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.jumpStatement);
    g.breakStatement.mock();
    g.continueStatement.mock();
    g.gotoStatement.mock();
    g.returnStatement.mock();
    g.throwStatement.mock();

  }

  @Test
  public void testOk() {
    assertThat(p, parse("breakStatement"));
    assertThat(p, parse("continueStatement"));
    assertThat(p, parse("gotoStatement"));
    assertThat(p, parse("returnStatement"));
    assertThat(p, parse("throwStatement"));
  }

}
