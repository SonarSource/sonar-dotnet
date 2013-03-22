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
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

@Rule(
  key = "AssignmentInsideSubExpression",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class AssignmentInsideSubExpressionCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(CSharpGrammar.ASSIGNMENT);
  }

  @Override
  public void visitNode(AstNode node) {
    if (isInsideSubExpression(node)) {
      getContext().createLineViolation(this, "Extract this assignment outside of the sub-expression.", node);
    }
  }

  private boolean isInsideSubExpression(AstNode node) {
    AstNode subExpression = node.getFirstAncestor(CSharpGrammar.EXPRESSION);
    AstNode expression = subExpression.getFirstAncestor(CSharpGrammar.EXPRESSION);

    return expression != null &&
      !isLambdaExpression(expression) &&
      !isDelegateExpression(expression);
  }

  private boolean isLambdaExpression(AstNode node) {
    return node.hasDirectChildren(CSharpGrammar.LAMBDA_EXPRESSION);
  }

  private boolean isDelegateExpression(AstNode node) {
    return node.getNumberOfChildren() == 1 &&
      node.hasDirectChildren(CSharpGrammar.PRIMARY_EXPRESSION) &&
      node.getFirstChild(CSharpGrammar.PRIMARY_EXPRESSION).hasDirectChildren(CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION);
  }

}
