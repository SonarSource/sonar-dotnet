/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.notParse;
import static com.sonar.sslr.test.parser.ParserMatchers.parse;
import static org.junit.Assert.assertThat;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.parser.CSharpParser;

public class AssignmentTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.assignment);
  }

  @Test
  public void testOk() {
    g.unaryExpression.mock();
    g.expression.mock();
    assertThat(p, parse("unaryExpression = expression"));
    assertThat(p, parse("unaryExpression += expression"));
    assertThat(p, parse("unaryExpression -= expression"));
    assertThat(p, parse("unaryExpression *= expression"));
    assertThat(p, parse("unaryExpression /= expression"));
    assertThat(p, parse("unaryExpression %= expression"));
    assertThat(p, parse("unaryExpression &= expression"));
    assertThat(p, parse("unaryExpression |= expression"));
    assertThat(p, parse("unaryExpression ^= expression"));
    assertThat(p, parse("unaryExpression <<= expression"));
    assertThat(p, parse("unaryExpression >>= expression"));
  }

  @Test
  public void testKo() {
    g.unaryExpression.mock();
    g.expression.mock();
    assertThat(p, notParse("unaryExpression != expression"));
    assertThat(p, notParse("unaryExpression == expression"));
    assertThat(p, notParse("unaryExpression >> expression"));
    assertThat(p, notParse("unaryExpression + expression"));
  }

  @Test
  public void testRealLife() {
    assertThat(p, parse("message = \"Hello World\""));
    assertThat(p, parse("frameworkAssemblyInitialized = true"));
    assertThat(p, parse("GetProperties(true)[key] = value"));
    assertThat(p, parse("loggingEvent.GetProperties()[\"log4jmachinename\"] = loggingEvent.LookupProperty(LoggingEvent.HostNameProperty)"));
    assertThat(p, parse("m_headFilter = m_tailFilter = filter"));
  }

}
