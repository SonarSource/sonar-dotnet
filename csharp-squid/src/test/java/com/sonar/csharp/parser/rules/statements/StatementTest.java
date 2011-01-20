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

public class StatementTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.statement);

  }

  @Test
  public void testOk() {
    g.labeledStatement.mock();
    g.declarationStatement.mock();
    g.embeddedStatement.mock();
    assertThat(p, parse("labeledStatement"));
    assertThat(p, parse("declarationStatement"));
    assertThat(p, parse("embeddedStatement"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("Integer i = 15;"));
    assertThat(p, parse("frameIndex++;"));
    assertThat(p, parse("loggingEvent.GetProperties()[\"log4jmachinename\"] = loggingEvent.LookupProperty(LoggingEvent.HostNameProperty);"));
    assertThat(p, parse("buf.Append(\"Exception during StringFormat: \").Append(formatException.Message);"));
    assertThat(p, parse("m_headFilter = m_tailFilter = filter;"));
  }

}
