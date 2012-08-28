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
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.Trivia;
import com.sonar.sslr.squid.SquidAstVisitor;

import java.util.List;
import java.util.Map;

/**
 * Visitor that computes the number of statements.
 */
public class CSharpPublicApiVisitor extends SquidAstVisitor<CSharpGrammar> {

  private final Map<AstNodeType, AstNodeType> modifiersMap = Maps.newHashMap();

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    CSharpGrammar g = getContext().getGrammar();
    modifiersMap.put(g.classDeclaration, g.classModifier);
    modifiersMap.put(g.structDeclaration, g.structModifier);
    modifiersMap.put(g.interfaceDeclaration, g.interfaceModifier);
    modifiersMap.put(g.enumDeclaration, g.enumModifier);
    modifiersMap.put(g.delegateDeclaration, g.delegateModifier);
    modifiersMap.put(g.constantDeclaration, g.constantModifier);
    modifiersMap.put(g.fieldDeclaration, g.fieldModifier);
    modifiersMap.put(g.methodDeclaration, g.methodModifier);
    modifiersMap.put(g.propertyDeclaration, g.propertyModifier);
    modifiersMap.put(g.eventDeclaration, g.eventModifier);
    modifiersMap.put(g.indexerDeclarator, g.indexerModifier);
    modifiersMap.put(g.operatorDeclaration, g.operatorModifier);

    subscribeTo(modifiersMap.keySet().toArray(new AstNodeType[modifiersMap.keySet().size()]));
    // and we need to add interface members that are special cases (they do not have modifiers, they inherit the visibility of their
    // enclosing interface definition)
    subscribeTo(g.interfaceMethodDeclaration, g.interfacePropertyDeclaration, g.interfaceEventDeclaration, g.interfaceIndexerDeclaration);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode node) {
    CSharpGrammar g = getContext().getGrammar();
    AstNodeType nodeType = node.getType();
    boolean isPublicApi = false;
    if (node.getType().equals(g.interfaceMethodDeclaration) || node.getType().equals(g.interfacePropertyDeclaration)
      || node.getType().equals(g.interfaceEventDeclaration) || node.getType().equals(g.interfaceIndexerDeclaration)) {
      // then we must look at the visibility of the enclosing interface definition
      isPublicApi = checkNodeForPublicModifier(node.findFirstParent(g.interfaceDeclaration), g.interfaceModifier);
    } else {
      isPublicApi = checkNodeForPublicModifier(node, modifiersMap.get(nodeType));
    }
    if (isPublicApi) {
      // let's see if it's documented
      checkNodeForPreviousComments(node);
    }
  }

  private boolean checkNodeForPublicModifier(AstNode currentNode, AstNodeType wantedChildrenType) {
    List<AstNode> modifiers = currentNode.findDirectChildren(wantedChildrenType);
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
