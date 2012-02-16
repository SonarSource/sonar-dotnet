/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceClass;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.squid.SquidAstVisitor;

import java.util.Map;
import java.util.Stack;

/**
 * Visitor that creates type resources and computes the number of types. <br/>
 * Types can be: classes, interfaces, delegates, enumerations and structures
 */
public class CSharpTypeVisitor extends SquidAstVisitor<CSharpGrammar> {

  private CSharpGrammar g;

  private String namespaceName;
  private final Stack<String> typeNameStack = new Stack<String>();
  private AstNodeType currentNodeType;
  private final Map<AstNodeType, CSharpKeyword> keywordMap = Maps.newHashMap();
  private final Map<AstNodeType, CSharpMetric> metricMap = Maps.newHashMap();

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    g = getContext().getGrammar();

    subscribeTo(g.namespaceDeclaration, g.classDeclaration, g.interfaceDeclaration,
        g.delegateDeclaration, g.structDeclaration, g.enumDeclaration);
    keywordMap.put(g.classDeclaration, CSharpKeyword.CLASS);
    metricMap.put(g.classDeclaration, CSharpMetric.CLASSES);
    keywordMap.put(g.interfaceDeclaration, CSharpKeyword.INTERFACE);
    metricMap.put(g.interfaceDeclaration, CSharpMetric.INTERFACES);
    keywordMap.put(g.delegateDeclaration, CSharpKeyword.DELEGATE);
    metricMap.put(g.delegateDeclaration, CSharpMetric.DELEGATES);
    keywordMap.put(g.structDeclaration, CSharpKeyword.STRUCT);
    metricMap.put(g.structDeclaration, CSharpMetric.STRUCTS);
    keywordMap.put(g.enumDeclaration, CSharpKeyword.ENUM);
    metricMap.put(g.enumDeclaration, CSharpMetric.ENUMS);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    currentNodeType = astNode.getType();
    if (astNode.is(g.namespaceDeclaration)) {
      namespaceName = extractNamespaceSignature(astNode);
    } else {
      String typeName = extractTypeName(astNode);
      typeNameStack.push(typeName);
      SourceType type = null;
      if (currentNodeType.equals(g.classDeclaration)) {
        type = new SourceClass(extractTypeSignature(typeName), typeName);
      } else {
        type = new SourceType(extractTypeSignature(typeName), typeName);
      }
      type.setMeasure(metricMap.get(currentNodeType), 1);
      getContext().addSourceCode(type);
    }
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    if (!astNode.is(g.namespaceDeclaration)) {
      getContext().popSourceCode();
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
    if (!typeNameStack.isEmpty()) {
      typeName.append(typeNameStack.peek());
      typeName.append(".");
    }
    if (currentNodeType.equals(g.delegateDeclaration)) {
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
