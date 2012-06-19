/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class UnaryExpressionTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.unaryExpression);
  }

  @Test
  public void testOk() {
    g.primaryExpression.mock();
    g.type.mock();
    assertThat(p, parse("primaryExpression"));
    assertThat(p, parse("+primaryExpression"));
    assertThat(p, parse("-primaryExpression"));
    assertThat(p, parse("!primaryExpression"));
    assertThat(p, parse("~primaryExpression"));
    assertThat(p, parse("++primaryExpression"));
    assertThat(p, parse("--primaryExpression"));
    assertThat(p, parse("( type ) primaryExpression"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("(Level)info.GetValue(\"Level\", typeof(Level))"));
  }

}
