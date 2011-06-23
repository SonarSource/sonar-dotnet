/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import java.util.Map;
import java.util.Stack;

import org.sonar.plugins.csharp.api.squid.CSharpMetric;
import org.sonar.plugins.csharp.api.squid.source.SourceClass;
import org.sonar.plugins.csharp.api.squid.source.SourceType;

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;

/**
 * Visitor that creates type resources and computes the number of types. <br/>
 * Types can be: classes, interfaces, delegates, enumerations and structures
 */
public class CSharpTypeVisitor extends CSharpAstVisitor {

  private String namespaceName;
  private Stack<String> typeNameStack = new Stack<String>();
  private AstNodeType currentNodeType;
  private Map<AstNodeType, CSharpKeyword> keywordMap = Maps.newHashMap();
  private Map<AstNodeType, CSharpMetric> metricMap = Maps.newHashMap();

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    subscribeTo(getCSharpGrammar().namespaceDeclaration, getCSharpGrammar().classDeclaration, getCSharpGrammar().interfaceDeclaration,
        getCSharpGrammar().delegateDeclaration, getCSharpGrammar().structDeclaration, getCSharpGrammar().enumDeclaration);
    keywordMap.put(getCSharpGrammar().classDeclaration, CSharpKeyword.CLASS);
    metricMap.put(getCSharpGrammar().classDeclaration, CSharpMetric.CLASSES);
    keywordMap.put(getCSharpGrammar().interfaceDeclaration, CSharpKeyword.INTERFACE);
    metricMap.put(getCSharpGrammar().interfaceDeclaration, CSharpMetric.INTERFACES);
    keywordMap.put(getCSharpGrammar().delegateDeclaration, CSharpKeyword.DELEGATE);
    metricMap.put(getCSharpGrammar().delegateDeclaration, CSharpMetric.DELEGATES);
    keywordMap.put(getCSharpGrammar().structDeclaration, CSharpKeyword.STRUCT);
    metricMap.put(getCSharpGrammar().structDeclaration, CSharpMetric.STRUCTS);
    keywordMap.put(getCSharpGrammar().enumDeclaration, CSharpKeyword.ENUM);
    metricMap.put(getCSharpGrammar().enumDeclaration, CSharpMetric.ENUMS);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    currentNodeType = astNode.getType();
    if (astNode.is(getCSharpGrammar().namespaceDeclaration)) {
      namespaceName = extractNamespaceSignature(astNode);
    } else {
      String typeName = extractTypeName(astNode);
      typeNameStack.push(typeName);
      SourceType type = null;
      if (currentNodeType.equals(getCSharpGrammar().classDeclaration)) {
        type = new SourceClass(extractTypeSignature(typeName), typeName);
      } else {
        type = new SourceType(extractTypeSignature(typeName), typeName);
      }
      type.setMeasure(metricMap.get(currentNodeType), 1);
      addSourceCode(type);
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    if ( !astNode.is(getCSharpGrammar().namespaceDeclaration)) {
      popSourceCode();
      typeNameStack.pop();
    } else {
      namespaceName = null;
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveFile(AstNode astNode) {
    namespaceName = null;
    currentNodeType = null;
    typeNameStack.clear();
  }

  private String extractTypeSignature(String className) {
    StringBuilder signature = new StringBuilder();
    if (namespaceName != null) {
      signature.append(namespaceName);
      signature.append(".");
    }
    signature.append(className);
    return signature.toString();
  }

  private String extractTypeName(AstNode astNode) {
    StringBuilder typeName = new StringBuilder();
    if ( !typeNameStack.isEmpty()) {
      typeName.append(typeNameStack.peek());
      typeName.append(".");
    }
    if (currentNodeType.equals(getCSharpGrammar().delegateDeclaration)) {
      typeName.append(astNode.findFirstChild(keywordMap.get(currentNodeType)).nextSibling().nextSibling().getTokenValue());
    } else {
      typeName.append(astNode.findFirstChild(keywordMap.get(currentNodeType)).nextSibling().getTokenValue());
    }
    return typeName.toString();
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
