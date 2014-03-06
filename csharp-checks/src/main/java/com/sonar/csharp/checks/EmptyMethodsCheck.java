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
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.checks.SquidCheck;
import org.sonar.check.BelongsToProfile;
import org.sonar.check.Priority;
import org.sonar.check.Rule;

@Rule(
  key = "S1186",
  priority = Priority.MAJOR)
@BelongsToProfile(title = CheckList.SONAR_WAY_PROFILE, priority = Priority.MAJOR)
public class EmptyMethodsCheck extends SquidCheck<Grammar> {

  @Override
  public void init() {
    subscribeTo(CSharpGrammar.CLASS_DECLARATION);
  }

  @Override
  public void visitNode(AstNode node) {
    AstNode classBody = node.getFirstChild(CSharpGrammar.CLASS_BODY);
    if (isAbstract(node)) {
      return;
    }

    for (AstNode classMember : classBody.getChildren(CSharpGrammar.CLASS_MEMBER_DECLARATION)) {
      AstNode memberType = classMember.getFirstChild();

      if (memberType.is(CSharpGrammar.METHOD_DECLARATION) && !isConstructor(node, memberType) && isEmptyMethod(memberType)) {
        getContext().createLineViolation(this,
          "Add a nested comment explaining why this method is empty, throw an NotSupportedException or complete the implementation.",
          // To not report issue on method's attributes line
          memberType.getFirstChild(CSharpGrammar.MEMBER_NAME));
      }
    }
  }

  private boolean isAbstract(AstNode classDeclaration) {
    for (AstNode modifier : classDeclaration.getChildren(CSharpGrammar.CLASS_MODIFIER)) {
      if (modifier.getFirstChild().is(CSharpKeyword.ABSTRACT)) {
        return true;
      }
    }
    return false;
  }

  private boolean isEmptyMethod(AstNode methodDeclaration) {
    AstNode block = methodDeclaration.getFirstChild(CSharpGrammar.METHOD_BODY).getFirstChild(CSharpGrammar.BLOCK);
    if (block == null) {
      return false;
    }
    AstNode rightCurlyBrace = block.getFirstChild(CSharpPunctuator.RCURLYBRACE);
    return rightCurlyBrace.getPreviousAstNode().is(CSharpPunctuator.LCURLYBRACE) && !hasComment(rightCurlyBrace);

  }

  private boolean isConstructor(AstNode classDeclaration, AstNode methodDeclaration) {
    String className = classDeclaration.getFirstChild(GenericTokenType.IDENTIFIER).getTokenValue();
    String methodName = methodDeclaration.getFirstChild(CSharpGrammar.MEMBER_NAME).getTokenValue();
    return className.equals(methodName);
  }

  private static boolean hasComment(AstNode node) {
    return node.getToken().hasTrivia();
  }

}
