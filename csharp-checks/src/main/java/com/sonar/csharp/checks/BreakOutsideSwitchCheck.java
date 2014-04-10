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
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.Grammar;
import org.sonar.squidbridge.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

@Rule(
  key = "BreakOutsideSwitch",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class BreakOutsideSwitchCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(CSharpGrammar.BREAK_STATEMENT);
  }

  @Override
  public void visitNode(AstNode node) {
    if (!isInSwitch(node)) {
      getContext().createLineViolation(this, "Refactor the code in order to remove this break statement.", node);
    }
  }

  private boolean isInSwitch(AstNode node) {
    AstNode ancestor = getFirstAncestor(node, CSharpGrammar.SWITCH_STATEMENT, CSharpGrammar.ITERATION_STATEMENT);

    return ancestor != null &&
      ancestor.is(CSharpGrammar.SWITCH_STATEMENT);
  }

  private AstNode getFirstAncestor(AstNode node, AstNodeType t1, AstNodeType t2) {
    AstNode result = node;

    while (result != null && !result.is(t1, t2)) {
      result = result.getParent();
    }

    return result;
  }

}
