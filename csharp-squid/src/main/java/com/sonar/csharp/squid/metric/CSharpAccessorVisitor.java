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
 * Visitor that computes the number of accessors.
 */
public class CSharpAccessorVisitor extends SquidAstVisitor<CSharpGrammar> {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    CSharpGrammar g = getContext().getGrammar();
    subscribeTo(g.getAccessorDeclaration, g.setAccessorDeclaration, g.addAccessorDeclaration, g.removeAccessorDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode node) {
    getContext().peekSourceCode().add(CSharpMetric.ACCESSORS, 1);
  }

}
