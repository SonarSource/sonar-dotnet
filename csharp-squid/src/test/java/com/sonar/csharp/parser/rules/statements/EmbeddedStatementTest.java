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

public class EmbeddedStatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.embeddedStatement);
    g.block.mock();
    g.expressionStatement.mock();
    g.selectionStatement.mock();
    g.iterationStatement.mock();
    g.jumpStatement.mock();
    g.tryStatement.mock();
    g.checkedStatement.mock();
    g.uncheckedStatement.mock();
    g.lockStatement.mock();
    g.usingStatement.mock();
    g.yieldStatement.mock();

  }

  @Test
  public void testOk() {
    assertThat(p, parse("block"));
    assertThat(p, parse(";"));
    assertThat(p, parse("expressionStatement"));
    assertThat(p, parse("selectionStatement"));
    assertThat(p, parse("iterationStatement"));
    assertThat(p, parse("jumpStatement"));
    assertThat(p, parse("tryStatement"));
    assertThat(p, parse("checkedStatement"));
    assertThat(p, parse("uncheckedStatement"));
    assertThat(p, parse("lockStatement"));
    assertThat(p, parse("usingStatement"));
    assertThat(p, parse("yieldStatement"));
  }

}
