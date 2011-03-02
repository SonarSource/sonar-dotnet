/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import org.sonar.squid.api.SourceFile;

import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates file resources and computes the number of files.
 */
public class CSharpFileVisitor extends CSharpAstVisitor {

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitFile(AstNode astNode) {
    SourceFile cSharpFile = new SourceFile(getFile().getAbsolutePath().replace('\\', '/'), getFile().getName());
    addSourceCode(cSharpFile);
    peekSourceCode().setMeasure(CSharpMetric.FILES, 1);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveFile(AstNode astNode) {
    popSourceCode();
  }

}
