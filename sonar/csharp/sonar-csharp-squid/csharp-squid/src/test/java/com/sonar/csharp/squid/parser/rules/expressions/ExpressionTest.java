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

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.parser.CSharpParser;
import com.sonar.sslr.impl.Parser;
import org.junit.Before;
import org.junit.Test;

import java.nio.charset.Charset;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class ExpressionTest {

  private final Parser<CSharpGrammar> p = CSharpParser.create(new CSharpConfiguration(Charset.forName("UTF-8")));
  private final CSharpGrammar g = p.getGrammar();

  @Before
  public void init() {
    p.setRootRule(g.expression);
  }

  @Test
  public void ok() {
    g.nonAssignmentExpression.mock();
    g.assignment.mock();

    assertThat(p)
        .matches("nonAssignmentExpression")
        .matches("assignment");
  }

  @Test
  public void reallife() {
    assertThat(p)
        .matches("CurrentDomain.GetAssemblies()")
        .matches("dbCommand.Dispose()")
        .matches("p.field++.ToString()")
        .matches("this.Id++")
        .matches("a++.ToString().ToString()")
        .matches("int.Parse(\"42\")")
        .matches("int.Parse(\"42\").ToString()")
        .matches("int.MaxValue")
        .matches("new []{12, 13}")
        .matches("new []{12, 13}.ToString()")
        .matches("new[] { 12, 13 }.Length")
        .matches("new[] { 12, 13 }[0]")
        .matches("db.Users")
        .matches("new { name }")
        .matches("new { name, foo }")
        .matches("new { user.Name, user.Role.Name }")
        .matches("from user in db.Users select new { user.Name, RoleName = user.Role.Name }");
  }

}
