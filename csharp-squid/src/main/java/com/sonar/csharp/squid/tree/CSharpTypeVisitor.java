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

import com.google.common.base.Joiner;
import com.google.common.collect.ImmutableMap;
import com.google.common.collect.Lists;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceClass;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.SquidAstVisitor;

import java.util.List;
import java.util.Map;

/**
 * Visitor that creates type resources and computes the number of types. <br/>
 * Types can be: classes, interfaces, delegates, enumerations and structures
 */
public class CSharpTypeVisitor extends SquidAstVisitor<Grammar> {

  private static final Map<AstNodeType, CSharpMetric> METRIC_MAP = ImmutableMap.<AstNodeType, CSharpMetric>of(
    CSharpGrammar.CLASS_DECLARATION, CSharpMetric.CLASSES,
    CSharpGrammar.INTERFACE_DECLARATION, CSharpMetric.INTERFACES,
    CSharpGrammar.DELEGATE_DECLARATION, CSharpMetric.DELEGATES,
    CSharpGrammar.STRUCT_DECLARATION, CSharpMetric.STRUCTS,
    CSharpGrammar.ENUM_DECLARATION, CSharpMetric.ENUMS);

  private final List<String> signatures = Lists.newArrayList();

  @Override
  public void init() {
    subscribeTo(
      CSharpGrammar.NAMESPACE_DECLARATION,
      CSharpGrammar.CLASS_DECLARATION,
      CSharpGrammar.INTERFACE_DECLARATION,
      CSharpGrammar.DELEGATE_DECLARATION,
      CSharpGrammar.STRUCT_DECLARATION,
      CSharpGrammar.ENUM_DECLARATION);
  }

  @Override
  public void visitNode(AstNode astNode) {
    if (astNode.is(CSharpGrammar.NAMESPACE_DECLARATION)) {
      String namespaceName = extractNamespaceSignature(astNode);
      signatures.add(namespaceName);
    } else {
      String signature = extractTypeSignature(astNode);
      signatures.add(signature);

      SourceType type;
      if (astNode.getType().equals(CSharpGrammar.CLASS_DECLARATION)) {
        type = new SourceClass(currentQualifiedIdentifier(), signature);
      } else {
        type = new SourceType(currentQualifiedIdentifier(), signature);
      }
      type.setMeasure(METRIC_MAP.get(astNode.getType()), 1);
      getContext().addSourceCode(type);
    }
  }

  @Override
  public void leaveNode(AstNode astNode) {
    if (!astNode.is(CSharpGrammar.NAMESPACE_DECLARATION)) {
      getContext().popSourceCode();
    }
    signatures.remove(signatures.size() - 1);
  }

  @Override
  public void leaveFile(AstNode astNode) {
    signatures.clear();
  }

  private String currentQualifiedIdentifier() {
    return Joiner.on('.').join(signatures);
  }

  private String extractTypeSignature(AstNode astNode) {
    String typeName = astNode.getFirstChild(GenericTokenType.IDENTIFIER).getTokenValue();

    String signature;
    AstNode typeParameterList = astNode.getFirstChild(CSharpGrammar.TYPE_PARAMETER_LIST);
    if (typeParameterList != null) {
      int numberOfParameters = typeParameterList.getFirstChild(CSharpGrammar.TYPE_PARAMETERS).getChildren(CSharpGrammar.TYPE_PARAMETER).size();
      signature = typeName + "<" + numberOfParameters + ">";
    } else {
      signature = typeName;
    }

    return signature;
  }

  private String extractNamespaceSignature(AstNode astNode) {
    AstNode qualifiedIdentifierNode = astNode.getFirstChild(CSharpGrammar.QUALIFIED_IDENTIFIER);
    StringBuilder sb = new StringBuilder();
    for (AstNode child : qualifiedIdentifierNode.getChildren()) {
      sb.append(child.getTokenValue());
    }
    return sb.toString();
  }

}
