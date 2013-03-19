/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.squid.tree;

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceClass;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.SquidAstVisitor;

import java.util.Map;
import java.util.Stack;

/**
 * Visitor that creates type resources and computes the number of types. <br/>
 * Types can be: classes, interfaces, delegates, enumerations and structures
 */
public class CSharpTypeVisitor extends SquidAstVisitor<Grammar> {

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
    subscribeTo(
        CSharpGrammar.NAMESPACE_DECLARATION,
        CSharpGrammar.CLASS_DECLARATION,
        CSharpGrammar.INTERFACE_DECLARATION,
        CSharpGrammar.DELEGATE_DECLARATION,
        CSharpGrammar.STRUCT_DECLARATION,
        CSharpGrammar.ENUM_DECLARATION);

    keywordMap.put(CSharpGrammar.CLASS_DECLARATION, CSharpKeyword.CLASS);
    metricMap.put(CSharpGrammar.CLASS_DECLARATION, CSharpMetric.CLASSES);
    keywordMap.put(CSharpGrammar.INTERFACE_DECLARATION, CSharpKeyword.INTERFACE);
    metricMap.put(CSharpGrammar.INTERFACE_DECLARATION, CSharpMetric.INTERFACES);
    keywordMap.put(CSharpGrammar.DELEGATE_DECLARATION, CSharpKeyword.DELEGATE);
    metricMap.put(CSharpGrammar.DELEGATE_DECLARATION, CSharpMetric.DELEGATES);
    keywordMap.put(CSharpGrammar.STRUCT_DECLARATION, CSharpKeyword.STRUCT);
    metricMap.put(CSharpGrammar.STRUCT_DECLARATION, CSharpMetric.STRUCTS);
    keywordMap.put(CSharpGrammar.ENUM_DECLARATION, CSharpKeyword.ENUM);
    metricMap.put(CSharpGrammar.ENUM_DECLARATION, CSharpMetric.ENUMS);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    currentNodeType = astNode.getType();
    if (astNode.is(CSharpGrammar.NAMESPACE_DECLARATION)) {
      namespaceName = extractNamespaceSignature(astNode);
    } else {
      String typeName = extractTypeName(astNode);
      typeNameStack.push(typeName);
      SourceType type = null;
      if (currentNodeType.equals(CSharpGrammar.CLASS_DECLARATION)) {
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
    if (!astNode.is(CSharpGrammar.NAMESPACE_DECLARATION)) {
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
    if (currentNodeType.equals(CSharpGrammar.DELEGATE_DECLARATION)) {
      typeName.append(astNode.getFirstChild(keywordMap.get(currentNodeType)).getNextSibling().getNextSibling().getTokenValue());
    } else {
      typeName.append(astNode.getFirstChild(keywordMap.get(currentNodeType)).getNextSibling().getTokenValue());
    }
    return typeName.toString();
  }

  private String extractNamespaceSignature(AstNode astNode) {
    AstNode qualifiedIdentifierNode = astNode.getFirstChild(CSharpKeyword.NAMESPACE).getNextSibling();
    StringBuilder name = new StringBuilder();
    for (AstNode child : qualifiedIdentifierNode.getChildren()) {
      name.append(child.getTokenValue());
    }
    return name.toString();
  }

}
