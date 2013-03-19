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

public class JoinClauseTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.JOIN_CLAUSE));
    p.getGrammar().rule(CSharpGrammar.TYPE).override("type");
    p.getGrammar().rule(CSharpGrammar.EXPRESSION).override("expression");
  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("join id in expression on expression equals expression")
        .matches("join type id in expression on expression equals expression");
  }

  @Test
  public void ko() {
    assertThat(p)
        .notMatches("");
  }

}
