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
package com.sonar.csharp.squid.parser.rules.statements;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class StatementTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.STATEMENT));

  }

  @Test
  public void ok() {
    p.getGrammar().rule(CSharpGrammar.LABELED_STATEMENT).override("labeledStatement");
    p.getGrammar().rule(CSharpGrammar.DECLARATION_STATEMENT).override("declarationStatement");
    p.getGrammar().rule(CSharpGrammar.EMBEDDED_STATEMENT).override("embeddedStatement");

    assertThat(p)
        .matches("labeledStatement")
        .matches("declarationStatement")
        .matches("embeddedStatement");
  }

  @Test
  public void reallife() {
    assertThat(p)
        .matches("Integer i = 15;")
        .matches("frameIndex++;")
        .matches("loggingEvent.GetProperties()[\"log4jmachinename\"] = loggingEvent.LookupProperty(LoggingEvent.HostNameProperty);")
        .matches("buf.Append(\"Exception during StringFormat: \").Append(formatException.Message);")
        .matches("m_headFilter = m_tailFilter = filter;")
        .matches("var query = from user in db.Users select new { user.Name, RoleName = user.Role.Name };");
  }

}
