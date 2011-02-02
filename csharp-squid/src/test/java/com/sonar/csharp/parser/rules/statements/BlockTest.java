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

public class BlockTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.block);
  }

  @Test
  public void testOk() {
    g.statement.mock();
    assertThat(p, parse("{}"));
    assertThat(p, parse("{ statement }"));
    assertThat(p, parse("{ statement statement}"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("{ Integer i = 15; }"));
    assertThat(p, parse("{ Integer i = 15;  frameIndex++; }"));
    assertThat(p,
        parse("{ loggingEvent.GetProperties()[\"log4jmachinename\"] = loggingEvent.LookupProperty(LoggingEvent.HostNameProperty); }"));
  }

}
