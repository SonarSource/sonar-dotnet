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

public class EmbeddedStatementTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammar.EMBEDDED_STATEMENT));
    p.getGrammar().rule(CSharpGrammar.BLOCK).override("block");
    p.getGrammar().rule(CSharpGrammar.EXPRESSION_STATEMENT).override("expressionStatement");
    p.getGrammar().rule(CSharpGrammar.SELECTION_STATEMENT).override("selectionStatement");
    p.getGrammar().rule(CSharpGrammar.ITERATION_STATEMENT).override("iterationStatement");
    p.getGrammar().rule(CSharpGrammar.JUMP_STATEMENT).override("jumpStatement");
    p.getGrammar().rule(CSharpGrammar.TRY_STATEMENT).override("tryStatement");
    p.getGrammar().rule(CSharpGrammar.CHECKED_STATEMENT).override("checkedStatement");
    p.getGrammar().rule(CSharpGrammar.UNCHECKED_STATEMENT).override("uncheckedStatement");
    p.getGrammar().rule(CSharpGrammar.LOCK_STATEMENT).override("lockStatement");
    p.getGrammar().rule(CSharpGrammar.USING_STATEMENT).override("usingStatement");
    p.getGrammar().rule(CSharpGrammar.YIELD_STATEMENT).override("yieldStatement");

  }

  @Test
  public void ok() {
    assertThat(p)
        .matches("block")
        .matches(";")
        .matches("expressionStatement")
        .matches("selectionStatement")
        .matches("iterationStatement")
        .matches("jumpStatement")
        .matches("tryStatement")
        .matches("checkedStatement")
        .matches("uncheckedStatement")
        .matches("lockStatement")
        .matches("usingStatement")
        .matches("yieldStatement");
  }

}
