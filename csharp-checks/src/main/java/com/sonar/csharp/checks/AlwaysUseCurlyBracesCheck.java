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

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import org.sonar.squidbridge.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;
import org.sonar.sslr.grammar.GrammarRuleKey;

import java.util.Map;

@Rule(
  key = "S121",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class AlwaysUseCurlyBracesCheck extends SquidCheck<Grammar> {

  private static final Map<GrammarRuleKey, String> LABELS_FOR_MESSAGE = Maps.newHashMap();

  static {
    LABELS_FOR_MESSAGE.put(CSharpGrammar.IF_STATEMENT, CSharpKeyword.IF.getValue());
    LABELS_FOR_MESSAGE.put(CSharpGrammar.FOR_STATEMENT, CSharpKeyword.FOR.getValue());
    LABELS_FOR_MESSAGE.put(CSharpGrammar.FOREACH_STATEMENT, CSharpKeyword.FOREACH.getValue());
    LABELS_FOR_MESSAGE.put(CSharpGrammar.DO_STATEMENT, CSharpKeyword.DO.getValue());
    LABELS_FOR_MESSAGE.put(CSharpGrammar.WHILE_STATEMENT, CSharpKeyword.WHILE.getValue());
  }

  @Override
  public void init() {
    subscribeTo(
      CSharpGrammar.IF_STATEMENT,
      CSharpGrammar.FOR_STATEMENT,
      CSharpGrammar.FOREACH_STATEMENT,
      CSharpGrammar.DO_STATEMENT,
      CSharpGrammar.WHILE_STATEMENT,
      CSharpKeyword.ELSE);
  }

  @Override
  public void visitNode(AstNode node) {
    if (!isElseIf(node) && !hasBlock(node)) {
      getContext().createLineViolation(this, "Add curly braces around the nested statement(s) in this \"{0}\" block.", node,
        getLabelForMessage(node));
    }
  }

  private static boolean hasBlock(AstNode node) {
    AstNode statement = node.getFirstChild(CSharpGrammar.EMBEDDED_STATEMENT);
    return node.is(CSharpKeyword.ELSE) ? isBlock(node.getNextAstNode()) : isBlock(statement);
  }

  private static boolean isBlock(AstNode statement) {
    return statement.getFirstChild().is(CSharpGrammar.BLOCK);

  }

  private static boolean isElseIf(AstNode node) {
    if (node.is(CSharpKeyword.ELSE)) {
      AstNode statementChild = node.getNextAstNode().getFirstChild();
      return statementChild.is(CSharpGrammar.SELECTION_STATEMENT) && statementChild.getFirstChild().is(CSharpGrammar.IF_STATEMENT);
    }
    return false;
  }

  private static String getLabelForMessage(AstNode node) {
    return node.is(CSharpKeyword.ELSE) ? CSharpKeyword.ELSE.getValue() : LABELS_FOR_MESSAGE.get(node.getType());
  }

}
