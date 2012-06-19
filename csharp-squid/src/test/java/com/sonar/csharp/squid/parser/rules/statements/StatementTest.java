/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.statements;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class StatementTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

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
    assertThat(p, parse("var query = from user in db.Users select new { user.Name, RoleName = user.Role.Name };"));
  }
}
