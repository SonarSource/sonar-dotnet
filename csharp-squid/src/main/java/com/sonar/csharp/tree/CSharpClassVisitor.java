/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import java.util.Stack;

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
  private Stack<String> classesNameStack = new Stack<String>();

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
      classesNameStack.push(className);
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
      classesNameStack.pop();
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
    StringBuilder className = new StringBuilder();
    if ( !classesNameStack.isEmpty()) {
      className.append(classesNameStack.peek());
      className.append(".");
    }
    className.append(astNode.findFirstChild(CSharpKeyword.CLASS).nextSibling().getTokenValue());
    return className.toString();
  }

  private String extractNamespaceSignature(AstNode astNode) {
    AstNode qualifiedIdentifierNode = astNode.findFirstChild(CSharpKeyword.NAMESPACE).nextSibling();
    StringBuilder name = new StringBuilder();
    for (AstNode child : qualifiedIdentifierNode.getChildren()) {
      name.append(child.getTokenValue());
    }
    return name.toString();
  }

}
