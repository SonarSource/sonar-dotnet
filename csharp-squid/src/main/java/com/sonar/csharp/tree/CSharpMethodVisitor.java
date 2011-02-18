/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.csharp.api.squid.CSharpClass;
import com.sonar.csharp.api.squid.CSharpMethod;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates method resources and computes the number of methods.
 */
public class CSharpMethodVisitor extends CSharpAstVisitor {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(getCSharpGrammar().methodDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    String methodSignature = extractMethodSignature(astNode);
    CSharpMethod method = new CSharpMethod((CSharpClass) peekLogicalSourceCode(), methodSignature, astNode.getTokenLine());
    method.setMeasure(CSharpMetric.METHODS, 1);
    addLogicalSourceCode(method);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    popLogicalSourceCode();
  }

  private String extractMethodSignature(AstNode astNode) {
    return astNode.findFirstChild(getCSharpGrammar().memberName).getTokenValue() + ":" + astNode.getToken().getLine();
  }

}
