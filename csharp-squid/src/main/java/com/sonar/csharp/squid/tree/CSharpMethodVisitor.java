/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import org.sonar.plugins.csharp.api.CSharpMetric;
import org.sonar.plugins.csharp.api.source.SourceMember;
import org.sonar.plugins.csharp.api.source.SourceType;

import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
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
    SourceMember method = new SourceMember((SourceType) peekSourceCode(), methodSignature, astNode.getTokenLine());
    method.setMeasure(CSharpMetric.METHODS, 1);
    addSourceCode(method);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    popSourceCode();
  }

  private String extractMethodSignature(AstNode astNode) {
    return astNode.findFirstChild(getCSharpGrammar().memberName).getTokenValue() + ":" + astNode.getTokenLine();
  }

}
