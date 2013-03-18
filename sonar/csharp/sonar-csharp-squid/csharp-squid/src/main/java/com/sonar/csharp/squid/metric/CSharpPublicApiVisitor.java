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
import com.sonar.csharp.squid.parser.CSharpGrammarImpl;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Trivia;
import com.sonar.sslr.squid.SquidAstVisitor;

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
    modifiersMap.put(CSharpGrammarImpl.classDeclaration, CSharpGrammarImpl.classModifier);
    modifiersMap.put(CSharpGrammarImpl.structDeclaration, CSharpGrammarImpl.structModifier);
    modifiersMap.put(CSharpGrammarImpl.interfaceDeclaration, CSharpGrammarImpl.interfaceModifier);
    modifiersMap.put(CSharpGrammarImpl.enumDeclaration, CSharpGrammarImpl.enumModifier);
    modifiersMap.put(CSharpGrammarImpl.delegateDeclaration, CSharpGrammarImpl.delegateModifier);
    modifiersMap.put(CSharpGrammarImpl.constantDeclaration, CSharpGrammarImpl.constantModifier);
    modifiersMap.put(CSharpGrammarImpl.fieldDeclaration, CSharpGrammarImpl.fieldModifier);
    modifiersMap.put(CSharpGrammarImpl.methodDeclaration, CSharpGrammarImpl.methodModifier);
    modifiersMap.put(CSharpGrammarImpl.propertyDeclaration, CSharpGrammarImpl.propertyModifier);
    modifiersMap.put(CSharpGrammarImpl.eventDeclaration, CSharpGrammarImpl.eventModifier);
    modifiersMap.put(CSharpGrammarImpl.indexerDeclarator, CSharpGrammarImpl.indexerModifier);
    modifiersMap.put(CSharpGrammarImpl.operatorDeclaration, CSharpGrammarImpl.operatorModifier);

    subscribeTo(modifiersMap.keySet().toArray(new AstNodeType[modifiersMap.keySet().size()]));
    // and we need to add interface members that are special cases (they do not have modifiers, they inherit the visibility of their
    // enclosing interface definition)
    subscribeTo(CSharpGrammarImpl.interfaceMethodDeclaration, CSharpGrammarImpl.interfacePropertyDeclaration, CSharpGrammarImpl.interfaceEventDeclaration,
        CSharpGrammarImpl.interfaceIndexerDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode node) {
    AstNodeType nodeType = node.getType();
    boolean isPublicApi = false;
    if (node.getType().equals(CSharpGrammarImpl.interfaceMethodDeclaration) || node.getType().equals(CSharpGrammarImpl.interfacePropertyDeclaration)
      || node.getType().equals(CSharpGrammarImpl.interfaceEventDeclaration) || node.getType().equals(CSharpGrammarImpl.interfaceIndexerDeclaration)) {
      // then we must look at the visibility of the enclosing interface definition
      isPublicApi = checkNodeForPublicModifier(node.getFirstAncestor(CSharpGrammarImpl.interfaceDeclaration), CSharpGrammarImpl.interfaceModifier);
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
