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
package com.sonar.csharp.squid.metric;

import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.parser.CSharpGrammarImpl;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.SquidAstVisitor;

/**
 * Visitor that computes the McCabe complexity.
 */
public class CSharpComplexityVisitor extends SquidAstVisitor<Grammar> {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(
        CSharpGrammarImpl.ifStatement,
        CSharpGrammarImpl.switchStatement,
        CSharpGrammarImpl.labeledStatement,
        CSharpGrammarImpl.whileStatement,
        CSharpGrammarImpl.doStatement,
        CSharpGrammarImpl.forStatement,
        CSharpGrammarImpl.returnStatement,
        CSharpGrammarImpl.methodBody,
        CSharpGrammarImpl.accessorBody,
        CSharpGrammarImpl.addAccessorDeclaration,
        CSharpGrammarImpl.removeAccessorDeclaration,
        CSharpGrammarImpl.operatorBody,
        CSharpGrammarImpl.constructorBody,
        CSharpGrammarImpl.destructorBody,
        CSharpGrammarImpl.staticConstructorBody,
        CSharpPunctuator.AND_OP,
        CSharpPunctuator.OR_OP,
        CSharpKeyword.CASE);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode node) {
    if (node.hasChildren() && node.getChild(0).is(CSharpPunctuator.SEMICOLON)) {
      // this is an empty declaration
      return;
    }
    if (node.is(CSharpGrammarImpl.returnStatement) && isLastReturnStatement(node)) {
      // last return of a block, do not count +1
      return;
    }
    getContext().peekSourceCode().add(CSharpMetric.COMPLEXITY, 1);
  }

  private boolean isLastReturnStatement(AstNode node) {
    AstNode currentNode = node;
    AstNode parent = currentNode.getParent();
    while (!parent.is(CSharpGrammarImpl.block)) {
      currentNode = parent;
      parent = currentNode.getParent();
    }
    // here, parent is a block
    if (!currentNode.getNextSibling().is(CSharpPunctuator.RCURLYBRACE)) {
      return false;
    }
    if (isMemberBloc(parent.getParent())) {
      return true;
    }
    return false;
  }

  private boolean isMemberBloc(AstNode parent) {
    return parent.is(
        CSharpGrammarImpl.methodBody,
        CSharpGrammarImpl.accessorBody,
        CSharpGrammarImpl.addAccessorDeclaration,
        CSharpGrammarImpl.removeAccessorDeclaration,
        CSharpGrammarImpl.operatorBody,
        CSharpGrammarImpl.constructorBody,
        CSharpGrammarImpl.destructorBody,
        CSharpGrammarImpl.staticConstructorBody);
  }

}
