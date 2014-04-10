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
package com.sonar.csharp.squid.metric;

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.parser.CSharpGrammar;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Trivia;
import org.sonar.squidbridge.SquidAstVisitor;

import java.util.List;
import java.util.Map;

/**
 * Visitor that computes the number of statements.
 */
public class CSharpPublicApiVisitor extends SquidAstVisitor<Grammar> {

  private final Map<AstNodeType, AstNodeType> modifiersMap = Maps.newHashMap();

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    modifiersMap.put(CSharpGrammar.CLASS_DECLARATION, CSharpGrammar.CLASS_MODIFIER);
    modifiersMap.put(CSharpGrammar.STRUCT_DECLARATION, CSharpGrammar.STRUCT_MODIFIER);
    modifiersMap.put(CSharpGrammar.INTERFACE_DECLARATION, CSharpGrammar.INTERFACE_MODIFIER);
    modifiersMap.put(CSharpGrammar.ENUM_DECLARATION, CSharpGrammar.ENUM_MODIFIER);
    modifiersMap.put(CSharpGrammar.DELEGATE_DECLARATION, CSharpGrammar.DELEGATE_MODIFIER);
    modifiersMap.put(CSharpGrammar.CONSTANT_DECLARATION, CSharpGrammar.CONSTANT_MODIFIER);
    modifiersMap.put(CSharpGrammar.FIELD_DECLARATION, CSharpGrammar.FIELD_MODIFIER);
    modifiersMap.put(CSharpGrammar.METHOD_DECLARATION, CSharpGrammar.METHOD_MODIFIER);
    modifiersMap.put(CSharpGrammar.PROPERTY_DECLARATION, CSharpGrammar.PROPERTY_MODIFIER);
    modifiersMap.put(CSharpGrammar.EVENT_DECLARATION, CSharpGrammar.EVENT_MODIFIER);
    modifiersMap.put(CSharpGrammar.INDEXER_DECLARATOR, CSharpGrammar.INDEXER_MODIFIER);
    modifiersMap.put(CSharpGrammar.OPERATOR_DECLARATION, CSharpGrammar.OPERATOR_MODIFIER);

    subscribeTo(modifiersMap.keySet().toArray(new AstNodeType[modifiersMap.keySet().size()]));
    // and we need to add interface members that are special cases (they do not have modifiers, they inherit the visibility of their
    // enclosing interface definition)
    subscribeTo(CSharpGrammar.INTERFACE_METHOD_DECLARATION, CSharpGrammar.INTERFACE_PROPERTY_DECLARATION, CSharpGrammar.INTERFACE_EVENT_DECLARATION,
        CSharpGrammar.INTERFACE_INDEXER_DECLARATION);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode node) {
    AstNodeType nodeType = node.getType();
    boolean isPublicApi = false;
    if (node.getType().equals(CSharpGrammar.INTERFACE_METHOD_DECLARATION) || node.getType().equals(CSharpGrammar.INTERFACE_PROPERTY_DECLARATION)
      || node.getType().equals(CSharpGrammar.INTERFACE_EVENT_DECLARATION) || node.getType().equals(CSharpGrammar.INTERFACE_INDEXER_DECLARATION)) {
      // then we must look at the visibility of the enclosing interface definition
      isPublicApi = checkNodeForPublicModifier(node.getFirstAncestor(CSharpGrammar.INTERFACE_DECLARATION), CSharpGrammar.INTERFACE_MODIFIER);
    } else {
      isPublicApi = checkNodeForPublicModifier(node, modifiersMap.get(nodeType));
    }
    if (isPublicApi) {
      // let's see if it's documented
      checkNodeForPreviousComments(node);
    }
  }

  private boolean checkNodeForPublicModifier(AstNode currentNode, AstNodeType wantedChildrenType) {
    List<AstNode> modifiers = currentNode.getChildren(wantedChildrenType);
    for (AstNode astNode : modifiers) {
      if (astNode.getToken().getType().equals(CSharpKeyword.PUBLIC)) {
        getContext().peekSourceCode().add(CSharpMetric.PUBLIC_API, 1);
        return true;
      }
    }
    return false;
  }

  private void checkNodeForPreviousComments(AstNode node) {
    for (Trivia trivia : node.getToken().getTrivia()) {
      if (trivia.isComment()) {
        getContext().peekSourceCode().add(CSharpMetric.PUBLIC_DOC_API, 1);
        break;
      }
    }
  }

}
