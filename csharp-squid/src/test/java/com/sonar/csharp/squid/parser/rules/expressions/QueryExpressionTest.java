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

public class QueryExpressionTest {

  CSharpParser p = new CSharpParser();
  CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.queryExpression);
  }

  @Test
  public void testOk() {
    g.fromClause.mock();
    g.queryBody.mock();
    assertThat(p, parse("fromClause queryBody"));
  }

  @Test
  public void testKo() {
    assertThat(p, notParse(""));
  }

  @Test
  public void testRealLife() throws Exception {
    assertThat(p, parse("from c in customers let d = c where d != null "
        + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() "
        + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() into e " + "group c by c.Country"));
    assertThat(p, parse("from c in customers let d = c where d != null "
        + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() "
        + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() into e " + "group c by c.Country " + "into g "
        + "orderby g.Count() ascending orderby g.Key descending " + "select new { Country = g.Key, CustCount = g.Count() }"));
    assertThat(p, parse("from user in db.Users select new { user.Name, RoleName = user.Role.Name }"));
  }

}
