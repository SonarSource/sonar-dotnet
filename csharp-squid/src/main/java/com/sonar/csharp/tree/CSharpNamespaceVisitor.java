/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import org.sonar.squid.api.SourcePackage;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates namespace (<=>package) resources and computes the number of namespace.
 */
public class CSharpNamespaceVisitor extends CSharpAstVisitor {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(getCSharpGrammar().namespaceDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    String namespaceSignature = extractNamespaceSignature(astNode);
    SourcePackage namespace = new SourcePackage(namespaceSignature);
    namespace.setMeasure(CSharpMetric.NAMESPACES, 1);
    addSourceCode(namespace);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    popSourceCode();
  }

  private String extractNamespaceSignature(AstNode astNode) {
    return astNode.findFirstChild(CSharpKeyword.NAMESPACE).nextSibling().getTokenValue();
  }

}
