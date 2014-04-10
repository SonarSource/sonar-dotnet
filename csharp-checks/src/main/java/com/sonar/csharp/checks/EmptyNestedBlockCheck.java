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

@Rule(
  key = "S108",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class EmptyNestedBlockCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(CSharpGrammar.BLOCK, CSharpGrammar.SWITCH_STATEMENT);
  }

  @Override
  public void visitNode(AstNode astNode) {
    if (isNested(astNode) && isEmptyBlock(astNode)) {
      getContext().createLineViolation(this, "Either remove or fill this block of code.", astNode);
    }
  }

  private static boolean isNested(AstNode node) {
    return node.getParent().isNot(
      CSharpGrammar.UNSAFE_STATEMENT,
      CSharpGrammar.ANONYMOUS_METHOD_EXPRESSION,
      CSharpGrammar.ANONYMOUS_FUNCTION_BODY,
      CSharpGrammar.METHOD_BODY,
      CSharpGrammar.CONSTRUCTOR_BODY,
      CSharpGrammar.DESTRUCTOR_BODY);
  }

  private static boolean isEmptyBlock(AstNode node) {
    if (node.is(CSharpGrammar.SWITCH_STATEMENT)) {
      return !node.hasDirectChildren(CSharpGrammar.SWITCH_SECTION);
    }

    AstNode rightCurlyBrace = node.getFirstChild(CSharpPunctuator.RCURLYBRACE);
    return rightCurlyBrace.getPreviousAstNode().is(CSharpPunctuator.LCURLYBRACE) && !hasComment(rightCurlyBrace);
  }

  private static boolean hasComment(AstNode node) {
    return node.getToken().hasTrivia();
  }

}
