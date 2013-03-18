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

import com.sonar.csharp.squid.parser.CSharpGrammarImpl;
import com.sonar.csharp.squid.parser.RuleTest;
import org.junit.Before;
import org.junit.Test;

import static org.sonar.sslr.tests.Assertions.assertThat;

public class EmbeddedStatementTest extends RuleTest {

  @Before
  public void init() {
    p.setRootRule(p.getGrammar().rule(CSharpGrammarImpl.embeddedStatement));
    p.getGrammar().rule(CSharpGrammarImpl.block).mock();
    p.getGrammar().rule(CSharpGrammarImpl.expressionStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.selectionStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.iterationStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.jumpStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.tryStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.checkedStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.uncheckedStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.lockStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.usingStatement).mock();
    p.getGrammar().rule(CSharpGrammarImpl.yieldStatement).mock();

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
