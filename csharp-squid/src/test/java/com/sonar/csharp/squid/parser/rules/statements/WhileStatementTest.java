/*
 * Copyright (C) 2009-2012 SonarSource SA
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

public class WhileStatementTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.whileStatement);
  }

  @Test
  public void testOk() {
    g.expression.mock();
    g.embeddedStatement.mock();
    assertThat(p, parse("while ( expression ) embeddedStatement"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("while (frameIndex < st.FrameCount) { Integer i = 15; }"));
    assertThat(p, parse("while (frameIndex < st.FrameCount) { Integer i = 15;  frameIndex++; }"));
  }

}
