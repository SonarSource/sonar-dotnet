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

import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import org.sonar.squidbridge.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.check.RuleProperty;
import org.sonar.sslr.grammar.GrammarRuleKey;

import javax.annotation.Nullable;
import java.util.ArrayDeque;
import java.util.Deque;

@Rule(
  key = "S1067",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class ExpressionComplexityCheck extends SquidCheck<Grammar> {

  public static final int DEFAULT = 3;
  private Deque<ExpressionComplexity> scope = new ArrayDeque<ExpressionComplexity>();
  private static final GrammarRuleKey[] LOGICAL_AND_CONDITIONAL_EXPRS = {
    CSharpGrammar.CONDITIONAL_EXPRESSION,
    CSharpGrammar.CONDITIONAL_AND_EXPRESSION,
    CSharpGrammar.CONDITIONAL_OR_EXPRESSION
  };

  private static final GrammarRuleKey[] REQUIRES_NEW_SCOPE = {
    CSharpGrammar.LAMBDA_EXPRESSION,
    CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION,
    CSharpGrammar.EXPRESSION_LIST,
    CSharpGrammar.ARGUMENT_LIST,
    CSharpGrammar.MEMBER_INITIALIZER};

  @RuleProperty(defaultValue = "" + DEFAULT)
  public int max = DEFAULT;


  public static class ExpressionComplexity {
    private int nestedLevel = 0;
    private int counterOperator = 0;

    public void increaseOperatorCounter(int nbOperator) {
      counterOperator += nbOperator;
    }

    public void incrementNestedExprLevel() {
      nestedLevel++;
    }

    public void decrementNestedExprLevel() {
      nestedLevel--;
    }

    public boolean isOnFirstExprLevel() {
      return nestedLevel == 0;
    }

    public int getExprNumberOfOperator() {
      return counterOperator;
    }

    public void resetExprOperatorCounter() {
      counterOperator = 0;
    }
  }

  @Override
  public void visitFile(@Nullable AstNode astNode) {
    scope.clear();
    scope.push(new ExpressionComplexity());
  }

  @Override
  public void init() {
    subscribeTo(LOGICAL_AND_CONDITIONAL_EXPRS);
    subscribeTo(CSharpGrammar.EXPRESSION);
    subscribeTo(REQUIRES_NEW_SCOPE);
  }

  @Override
  public void visitNode(AstNode astNode) {
    if (isExpression(astNode)) {
      scope.peek().incrementNestedExprLevel();
    }
    if (astNode.is(LOGICAL_AND_CONDITIONAL_EXPRS)) {
      scope.peek().increaseOperatorCounter(
        astNode.getChildren(CSharpPunctuator.AND_OP, CSharpPunctuator.OR_OP, CSharpPunctuator.QUESTION).size());
    }
    if (astNode.is(REQUIRES_NEW_SCOPE)) {
      scope.push(new ExpressionComplexity());
    }
  }

  @Override
  public void leaveNode(AstNode astNode) {
    if (isExpression(astNode)) {
      ExpressionComplexity currentExpression = scope.peek();
      currentExpression.decrementNestedExprLevel();

      if (currentExpression.isOnFirstExprLevel()) {
        if (currentExpression.getExprNumberOfOperator() > max) {
          getContext().createLineViolation(this,
            "Reduce the number of conditional operators (" + currentExpression.getExprNumberOfOperator() + ") used in the expression (maximum allowed " + max + ").",
            astNode);
        }
        currentExpression.resetExprOperatorCounter();
      }
    } else if (astNode.is(REQUIRES_NEW_SCOPE)) {
      scope.pop();
    }
  }

  public static boolean isExpression(AstNode node) {
    return node.is(CSharpGrammar.EXPRESSION) || node.is(LOGICAL_AND_CONDITIONAL_EXPRS);
  }
}
