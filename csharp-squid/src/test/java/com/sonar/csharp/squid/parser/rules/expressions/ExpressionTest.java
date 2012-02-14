/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;

public class ExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.expression);
  }

  @Test
  public void testOk() {
    g.nonAssignmentExpression.mock();
    g.assignment.mock();
    assertThat(p, parse("nonAssignmentExpression"));
    assertThat(p, parse("assignment"));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("CurrentDomain.GetAssemblies()"));
    assertThat(p, parse("dbCommand.Dispose()"));
    assertThat(p, parse("p.field++.ToString()"));
    assertThat(p, parse("this.Id++"));
    assertThat(p, parse("a++.ToString().ToString()"));
    assertThat(p, parse("int.Parse(\"42\")"));
    assertThat(p, parse("int.Parse(\"42\").ToString()"));
    assertThat(p, parse("int.MaxValue"));
    assertThat(p, parse("new []{12, 13}"));
    assertThat(p, parse("new []{12, 13}.ToString()"));
    assertThat(p, parse("new[] { 12, 13 }.Length"));
    assertThat(p, parse("new[] { 12, 13 }[0]"));
  }

}
