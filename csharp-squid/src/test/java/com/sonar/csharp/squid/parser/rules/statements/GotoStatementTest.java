/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.statements;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;
import org.junit.Before;
import org.junit.Test;

import java.nio.charset.Charset;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

public class GotoStatementTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.gotoStatement);
    g.expression.mock();

  }

  @Test
  public void testOk() {
    assertThat(p, parse("goto id;"));
    assertThat(p, parse("goto case expression;"));
    assertThat(p, parse("goto default;"));
  }

}
