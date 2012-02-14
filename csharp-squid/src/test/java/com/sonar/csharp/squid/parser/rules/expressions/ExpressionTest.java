/*
 * Copyright (C) 2010 SonarSource SA
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

public class ExpressionTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

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
    assertThat(p, parse("db.Users"));
    assertThat(p, parse("new { name }"));
    assertThat(p, parse("new { name, foo }"));
    assertThat(p, parse("new { user.Name, user.Role.Name }"));
    assertThat(p, parse("from user in db.Users select new { user.Name, RoleName = user.Role.Name }"));
  }

}
