/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.squid.SquidAstVisitor;

/**
 * Visitor that creates namespace (<=>package) resources and computes the number of namespace.
 */
public class CSharpNamespaceVisitor extends SquidAstVisitor<CSharpGrammar> {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(getContext().getGrammar().namespaceDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    getContext().peekSourceCode().setMeasure(CSharpMetric.NAMESPACES, 1);
  }

}
