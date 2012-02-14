/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.metric;

import java.util.List;
import java.util.Map;

import com.google.common.collect.Maps;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.AstNodeType;
import com.sonar.sslr.api.Trivia;

/**
 * Visitor that computes the number of statements.
 */
public class CSharpPublicApiVisitor extends CSharpAstVisitor {

  private final Map<AstNodeType, AstNodeType> modifiersMap = Maps.newHashMap();

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    CSharpGrammar g = getCSharpGrammar();
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
    CSharpGrammar g = getCSharpGrammar();
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
        peekSourceCode().add(CSharpMetric.PUBLIC_API, 1);
        return true;
      }
    }
    return false;
  }

  private void checkNodeForPreviousComments(AstNode node) {
    for (Trivia trivia : node.getToken().getTrivia()) {
      if (trivia.isComment()) {
        peekSourceCode().add(CSharpMetric.PUBLIC_DOC_API, 1);
        break;
      }
    }
  }

}
