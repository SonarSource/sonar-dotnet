/*
 * Copyright (C) 2009-2012 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.squid.SquidAstVisitor;

/**
 * Visitor that computes the McCabe complexity.
 */
public class CSharpComplexityVisitor extends SquidAstVisitor<CSharpGrammar> {

  private CSharpGrammar g;

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    g = getContext().getGrammar();
    subscribeTo(g.ifStatement, g.switchStatement, g.labeledStatement, g.whileStatement, g.doStatement, g.forStatement, g.returnStatement,
        g.methodBody, g.accessorBody, g.addAccessorDeclaration, g.removeAccessorDeclaration, g.operatorBody, g.constructorBody,
        g.destructorBody, g.staticConstructorBody, CSharpPunctuator.AND_OP, CSharpPunctuator.OR_OP, CSharpKeyword.CASE);
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
    if (node.is(g.returnStatement) && isLastReturnStatement(node)) {
      // last return of a block, do not count +1
      return;
    }
    getContext().peekSourceCode().add(CSharpMetric.COMPLEXITY, 1);
  }

  private boolean isLastReturnStatement(AstNode node) {
    AstNode currentNode = node;
    AstNode parent = currentNode.getParent();
    while (!parent.is(g.block)) {
      currentNode = parent;
      parent = currentNode.getParent();
    }
    // here, parent is a block
    if (!currentNode.nextSibling().is(CSharpPunctuator.RCURLYBRACE)) {
      return false;
    }
    if (isMemberBloc(parent.getParent())) {
      return true;
    }
    return false;
  }

  private boolean isMemberBloc(AstNode parent) {
    return parent.is(g.methodBody) || parent.is(g.accessorBody) || parent.is(g.addAccessorDeclaration)
      || parent.is(g.removeAccessorDeclaration) || parent.is(g.operatorBody) || parent.is(g.constructorBody)
      || parent.is(g.destructorBody) || parent.is(g.staticConstructorBody);
  }
}
