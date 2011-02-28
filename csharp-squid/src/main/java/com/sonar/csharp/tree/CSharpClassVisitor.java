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

  private String namespaceName;

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(getCSharpGrammar().namespaceDeclaration, getCSharpGrammar().classDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    if (astNode.is(getCSharpGrammar().namespaceDeclaration)) {
      namespaceName = extractNamespaceSignature(astNode);
    } else {
      String className = extractClassName(astNode);
      SourceClass clazz = new SourceClass(extractClassSignature(className), className);
      clazz.setMeasure(CSharpMetric.CLASSES, 1);
      addSourceCode(clazz);
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    if (astNode.is(getCSharpGrammar().classDeclaration)) {
      popSourceCode();
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveFile(AstNode astNode) {
    namespaceName = null;
  }

  private String extractClassSignature(String className) {
    StringBuilder signature = new StringBuilder();
    if (namespaceName != null) {
      signature.append(namespaceName);
      signature.append(".");
    }
    signature.append(className);
    return signature.toString();
  }

  private String extractClassName(AstNode astNode) {
    return astNode.findFirstChild(CSharpKeyword.CLASS).nextSibling().getTokenValue();
  }

  private String extractNamespaceSignature(AstNode astNode) {
    return astNode.findFirstChild(CSharpKeyword.NAMESPACE).nextSibling().getTokenValue();
  }

}
