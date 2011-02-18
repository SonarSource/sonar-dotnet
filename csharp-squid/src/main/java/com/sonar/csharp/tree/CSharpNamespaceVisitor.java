/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import java.util.Map;

import com.google.common.collect.Maps;
import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.csharp.api.squid.CSharpNamespace;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates namespace (<=>package) resources and computes the number of namespace.
 */
public class CSharpNamespaceVisitor extends CSharpAstVisitor {

  private Map<String, CSharpNamespace> namespacesMap = Maps.newHashMap();

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
    CSharpNamespace namespace = namespacesMap.get(namespaceSignature);
    if (namespace == null) {
      namespace = new CSharpNamespace(namespaceSignature, namespaceSignature);
      namespace.setMeasure(CSharpMetric.NAMESPACES, 1);
      namespacesMap.put(namespaceSignature, namespace);
    }
    addLogicalSourceCode(namespace);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    popLogicalSourceCode();
  }

  private String extractNamespaceSignature(AstNode astNode) {
    return astNode.findFirstChild(CSharpKeyword.NAMESPACE).nextSibling().getTokenValue();
  }

}
