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

public class QueryBodyClauseTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.queryBodyClause);
    g.fromClause.mock();
    g.letClause.mock();
    g.whereClause.mock();
    g.joinClause.mock();
    g.joinIntoClause.mock();
    g.orderByClause.mock();
  }

  @Test
  public void testOk() {
    assertThat(p, parse("fromClause"));
    assertThat(p, parse("letClause"));
    assertThat(p, parse("whereClause"));
    assertThat(p, parse("joinClause"));
    assertThat(p, parse("joinIntoClause"));
    assertThat(p, parse("orderByClause"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

}
