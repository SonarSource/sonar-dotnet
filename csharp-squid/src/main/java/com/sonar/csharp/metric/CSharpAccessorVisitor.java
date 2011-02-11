/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.metric;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that computes the number of lines of code of a file.
 */
public class CSharpAccessorVisitor extends CSharpAstVisitor {

  @Override
  public void init() {
    CSharpGrammar g = getCSharpGrammar();
    subscribeTo(g.getAccessorDeclaration, g.setAccessorDeclaration);
  }

  @Override
  public void visitNode(AstNode node) {
    peekSourceCode().add(CSharpMetric.ACCESSORS, 1);
  }

}
