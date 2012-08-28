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

import static com.sonar.sslr.test.parser.ParserMatchers.*;
import static org.junit.Assert.*;

import java.nio.charset.Charset;

import org.junit.Before;
import org.junit.Test;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;

public class QueryExpressionTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

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
