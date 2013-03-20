/*
 * Sonar C# Plugin :: C# Squid :: Checks
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
package com.sonar.csharp.checks;

import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

import java.util.List;

@Rule(
  key = "NonEmptyCaseWithoutBreak",
  priority = Priority.MAJOR)
public class NonEmptyCaseWithoutBreakCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(CSharpGrammar.SWITCH_SECTION);
  }

  @Override
  public void visitNode(AstNode node) {
    List<AstNode> statements = node.getChildren(CSharpGrammar.STATEMENT);
    AstNode lastStatement = statements.get(statements.size() - 1);

    if (!isBreakStatement(lastStatement)) {
      List<AstNode> switchLabels = node.getChildren(CSharpGrammar.SWITCH_LABEL);
      AstNode lastSwitchLabel = switchLabels.get(switchLabels.size() - 1);

      getContext().createLineViolation(this, "Add a break; statement at the end of this switch case.", lastSwitchLabel);
    }
  }

  private boolean isBreakStatement(AstNode node) {
    AstNode embeddedStatement = node.getFirstChild(CSharpGrammar.EMBEDDED_STATEMENT);

    return embeddedStatement != null &&
      embeddedStatement.hasDirectChildren(CSharpGrammar.JUMP_STATEMENT) &&
      embeddedStatement.getFirstChild(CSharpGrammar.JUMP_STATEMENT).hasDirectChildren(CSharpGrammar.BREAK_STATEMENT);
  }

}
