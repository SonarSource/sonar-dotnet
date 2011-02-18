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
import com.sonar.csharp.api.squid.CSharpClass;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates classes resources and computes the number of classes.
 */
public class CSharpClassVisitor extends CSharpAstVisitor {

  private Map<String, CSharpClass> classesMap = Maps.newHashMap();

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
    CSharpClass cSharpClass = classesMap.get(classSignature);
    if (cSharpClass == null) {
      cSharpClass = new CSharpClass(classSignature, classSignature);
      cSharpClass.setMeasure(CSharpMetric.CLASSES, 1);
      classesMap.put(classSignature, cSharpClass);
    }
    addLogicalSourceCode(cSharpClass);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    popLogicalSourceCode();
  }

  /**
   * {@inheritDoc}
   */
  private String extractClassSignature(AstNode astNode) {
    return astNode.findFirstChild(CSharpKeyword.CLASS).nextSibling().getTokenValue();
  }

}
