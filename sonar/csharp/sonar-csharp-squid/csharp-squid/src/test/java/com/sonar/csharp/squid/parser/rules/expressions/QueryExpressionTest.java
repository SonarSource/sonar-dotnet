/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.squid.parser.rules.expressions;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class QueryExpressionTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.QUERY_EXPRESSION));
  }

  @Test
  public void ok() {
    p.getGrammar().rule(CSharpGrammar.FROM_CLAUSE).override("fromClause");
    p.getGrammar().rule(CSharpGrammar.QUERY_BODY).override("queryBody");

    assertThat(p)
        .matches("fromClause queryBody");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("");
  }

  @Test
  public void reallife() {
    assertThat(p)
        .matches("from c in customers let d = c where d != null "
          + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() "
          + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() into e " + "group c by c.Country")
        .matches("from c in customers let d = c where d != null "
          + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() "
          + "join c1 in customers on c1.GetHashCode() equals c.GetHashCode() into e " + "group c by c.Country " + "into g "
          + "orderby g.Count() ascending orderby g.Key descending " + "select new { Country = g.Key, CustCount = g.Count() }")
        .matches("from user in db.Users select new { user.Name, RoleName = user.Role.Name }");
  }

}
