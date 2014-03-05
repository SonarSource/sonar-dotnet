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

import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

@Rule(
  key = "S1125",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class BooleanEqualityComparisonCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(
      CSharpGrammar.UNARY_EXPRESSION,
      CSharpGrammar.EQUALITY_EXPRESSION,
      CSharpGrammar.CONDITIONAL_AND_EXPRESSION,
      CSharpGrammar.CONDITIONAL_OR_EXPRESSION);
  }

  @Override
  public void visitNode(AstNode node) {
    AstNode booleanLiteral = getBooleanLiteralFromExpression(node);

    if (booleanLiteral != null) {
      getContext().createLineViolation(this, "Remove the literal \"{0}\" boolean value.", booleanLiteral, booleanLiteral.getTokenValue());
    }
  }

  private AstNode getBooleanLiteralFromExpression(AstNode expression) {
    if (expression.is(CSharpGrammar.UNARY_EXPRESSION)) {
      return getBooleanLiteralFromUnaryExpression(expression);
    }

    AstNode leftExpr = expression.getFirstChild();
    AstNode rightExpr = expression.getLastChild();

    if (isBooleanLiteral(leftExpr)) {
      return leftExpr;
    } else if (isBooleanLiteral(rightExpr)) {
      return rightExpr;
    } else {
      return null;
    }
  }

  private AstNode getBooleanLiteralFromUnaryExpression(AstNode unaryExpression) {
    AstNode boolLiteral = null;

    if (unaryExpression.getFirstChild().is(CSharpPunctuator.EXCLAMATION)) {
      AstNode expr = unaryExpression.getLastChild();

      if (isBooleanLiteral(expr)) {
        boolLiteral = expr;
      }
    }
    return boolLiteral;
  }

  private boolean isBooleanLiteral(AstNode equalityExpr) {
    AstNode exprChild = equalityExpr.getFirstChild();
    if (!exprChild.getToken().equals(exprChild.getLastToken())) {
      return false;
    }

    String tokenValue = exprChild.getTokenValue();
    return CSharpKeyword.TRUE.getValue().equals(tokenValue)
      || CSharpKeyword.FALSE.getValue().equals(tokenValue);
  }

}
