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

import com.google.common.collect.Sets;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

import javax.annotation.Nullable;
import java.util.Collections;
import java.util.Set;

@Rule(
  key = "S127",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class ForLoopCounterChangedCheck extends SquidCheck<Grammar> {

  private Set<String> counters = Sets.newHashSet();
  private Set<String> pendingCounters = Sets.newHashSet();

  @Override
  public void init() {
    subscribeTo(
      CSharpGrammar.FOR_STATEMENT,
      CSharpGrammar.STATEMENT,

      CSharpGrammar.ASSIGNMENT,
      CSharpPunctuator.INC_OP,
      CSharpPunctuator.DEC_OP);
  }

  @Override
  public void visitFile(@Nullable AstNode astNode) {
    counters.clear();
  }

  @Override
  public void visitNode(AstNode astNode) {
    if (astNode.is(CSharpGrammar.FOR_STATEMENT)) {
      pendingCounters = getLoopsCounters(astNode);
    } else if (astNode.is(CSharpGrammar.STATEMENT)) {
      counters.addAll(pendingCounters);
      pendingCounters = Collections.emptySet();
    } else if (!counters.isEmpty() && isAssignmentOrUnaryExpression(astNode)) {
      check(astNode);
    }
  }

  @Override
  public void leaveNode(AstNode astNode) {
    if (astNode.is(CSharpGrammar.FOR_STATEMENT)) {
      counters.removeAll(getLoopsCounters(astNode));
      pendingCounters = Collections.emptySet();
    }
  }

  private void check(AstNode node) {
    String modifiedVar;

    if (node.is(CSharpGrammar.ASSIGNMENT)) {
      modifiedVar = getAssignmentTargetName(node);
    } else if (isPostExpression(node)) {
      modifiedVar = getModifiedVarByPostfixOp(node);
    } else {
      // Prefix expr
      modifiedVar = node.getNextAstNode().getTokenValue();
    }

    if (modifiedVar != null && counters.contains(modifiedVar)) {
      reportIssue(node, modifiedVar);
    }
  }

  private void reportIssue(AstNode astNode, String counter) {
    getContext().createLineViolation(this, "Refactor the code to avoid updating the loop counter \"{0}\" within the loop body.", astNode, counter);
  }

  private Set<String> getLoopsCounters(AstNode astNode) {
    Set<String> counterList = Sets.newHashSet();
    AstNode initializer = astNode.getFirstChild(CSharpGrammar.FOR_INITIALIZER);

    if (initializer != null) {
      AstNode initializerChild = initializer.getFirstChild();

      if (initializerChild.is(CSharpGrammar.LOCAL_VARIABLE_DECLARATION)) {
        for (AstNode varDeclarator : initializerChild.getChildren(CSharpGrammar.LOCAL_VARIABLE_DECLARATOR)) {
          counterList.add(varDeclarator.getFirstChild(GenericTokenType.IDENTIFIER).getTokenValue());
        }
      } else {
        // Statement expression list
        for (AstNode expr : initializerChild.getChildren(CSharpGrammar.EXPRESSION)) {
          AstNode exprChild = expr.getFirstChild();
          if (exprChild.is(CSharpGrammar.ASSIGNMENT)) {
            counterList.add(getAssignmentTargetName(exprChild));
          }
        }
      }
    }
    return counterList;
  }

  private static String getAssignmentTargetName(AstNode assignment) {
    AstNode primaryExpression = assignment.getFirstChild(CSharpGrammar.ASSIGNMENT_TARGET).getFirstChild(CSharpGrammar.PRIMARY_EXPRESSION);
    String targetName = null;

    if (primaryExpression == null) {
      return targetName;
    }

    AstNode primaryChild = primaryExpression.getFirstChild();

    if (primaryChild.is(CSharpGrammar.SIMPLE_NAME)) {
      targetName = primaryExpression.getTokenValue();

    } else if (primaryChild.is(CSharpGrammar.POSTFIX_EXPRESSION)) {
      StringBuilder builder = new StringBuilder();

      for (AstNode varMember : primaryChild.getChildren()) {
        if (varMember.is(CSharpGrammar.SIMPLE_NAME)) {
          builder.append(varMember.getTokenValue());

        } else if (varMember.is(CSharpGrammar.POST_MEMBER_ACCESS)) {
          builder.append(varMember.getFirstChild(CSharpPunctuator.DOT).getTokenValue());
          builder.append(varMember.getFirstChild(GenericTokenType.IDENTIFIER).getTokenValue());

        } else if (varMember.is(CSharpGrammar.POST_ELEMENT_ACCESS)) {
          builder.append(CSharpPunctuator.LBRACKET);
          builder.append(varMember.getFirstChild(CSharpGrammar.ARGUMENT_LIST).getTokenValue());
          builder.append(CSharpPunctuator.RBRACKET);
        }
      }
      targetName = builder.toString();

    }
    return targetName;
  }

  private static String getModifiedVarByPostfixOp(AstNode unaryOperator) {
    AstNode parentPreviousNode = unaryOperator.getParent().getPreviousAstNode();

    if (parentPreviousNode.is(CSharpGrammar.SIMPLE_NAME)) {
      return parentPreviousNode.getTokenValue();
    }
    return null;
  }

  private static boolean isAssignmentOrUnaryExpression(AstNode node) {
    return node.is(CSharpGrammar.ASSIGNMENT, CSharpPunctuator.INC_OP, CSharpPunctuator.DEC_OP);
  }

  private static boolean isPostExpression(AstNode incOrDecOperator) {
    return incOrDecOperator.getParent().is(CSharpGrammar.POST_DECREMENT, CSharpGrammar.POST_INCREMENT);
  }

}
