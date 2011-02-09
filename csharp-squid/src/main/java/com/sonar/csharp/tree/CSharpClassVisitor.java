/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import org.sonar.squid.api.SourceClass;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates classes resources and computes the number of classes.
 */
public class CSharpClassVisitor extends CSharpAstVisitor {

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(getCSharpGrammar().classDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    String classSignature = extractClassSignature(astNode);
    SourceClass clazz = new SourceClass(classSignature);
    clazz.setMeasure(CSharpMetric.CLASSES, 1);
    addSourceCode(clazz);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    popSourceCode();
  }

  /**
   * {@inheritDoc}
   */
  private String extractClassSignature(AstNode astNode) {
    return astNode.findFirstChild(CSharpKeyword.CLASS).nextSibling().getTokenValue();
  }

}
