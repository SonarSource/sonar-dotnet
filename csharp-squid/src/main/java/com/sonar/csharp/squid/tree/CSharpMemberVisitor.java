/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.tree;

import org.sonar.plugins.csharp.api.CSharpMetric;
import org.sonar.plugins.csharp.api.source.SourceMember;
import org.sonar.plugins.csharp.api.source.SourceType;

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.api.ast.CSharpAstVisitor;
import com.sonar.sslr.api.AstNode;

/**
 * Visitor that creates member resources (= methods, property accessors, event accessors, indexer accessors, operators, constructors,
 * finalizers) and computes the number of members.
 */
public class CSharpMemberVisitor extends CSharpAstVisitor {

  private CSharpGrammar g;

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    g = getCSharpGrammar();
    subscribeTo(g.methodBody, g.constructorBody, g.staticConstructorBody, g.destructorBody, g.accessorBody, g.addAccessorDeclaration,
        g.removeAccessorDeclaration, g.operatorBody);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitNode(AstNode astNode) {
    if (astNode.getChild(0).is(CSharpPunctuator.SEMICOLON)) {
      // this is an empty declaration
      return;
    }

    String memberSignature = "";
    if (astNode.is(g.methodBody)) {
      memberSignature = extractMethodSignature(astNode);
    } else if (astNode.is(g.accessorBody)) {
      memberSignature = extractPropertySignature(astNode);
    } else if (astNode.is(g.addAccessorDeclaration)) {
      memberSignature = extractEventSignature("add", astNode);
    } else if (astNode.is(g.removeAccessorDeclaration)) {
      memberSignature = extractEventSignature("remove", astNode);
    } else if (astNode.is(g.constructorBody)) {
      memberSignature = ".ctor:" + astNode.getTokenLine();
    } else if (astNode.is(g.staticConstructorBody)) {
      memberSignature = ".cctor():" + astNode.getTokenLine();
    } else if (astNode.is(g.destructorBody)) {
      memberSignature = "Finalize:" + astNode.getTokenLine();
    } else if (astNode.is(g.operatorBody)) {
      // call it "op", but should be more precise: for instance, "+" => "op_Addition"
      memberSignature = "op:" + astNode.getTokenLine();
    } else {
      throw new IllegalStateException("The current AST node is not supported by this visitor.");
    }

    SourceMember member = new SourceMember((SourceType) peekSourceCode(), memberSignature, astNode.getTokenLine());
    member.setMeasure(CSharpMetric.METHODS, 1);
    addSourceCode(member);
  }

  /**
   * {@inheritDoc}
   */
  @Override
  public void leaveNode(AstNode astNode) {
    if (astNode.getChild(0).is(CSharpPunctuator.SEMICOLON)) {
      // this was an empty declaration
      return;
    }
    popSourceCode();
  }

  private String extractMethodSignature(AstNode astNode) {
    return astNode.getParent().findFirstChild(g.memberName).getTokenValue() + ":" + astNode.getTokenLine();
  }

  private String extractPropertySignature(AstNode astNode) {
    StringBuilder signature = new StringBuilder(astNode.getToken().getPreviousToken().getValue());
    signature.append("_");
    AstNode delcarationNode = astNode.getParent().getParent().getParent();
    if (delcarationNode.is(g.indexerDeclaration)) {
      signature.append("Item");
    } else {
      signature.append(delcarationNode.findFirstChild(g.memberName).getTokenValue());
    }
    signature.append(":");
    signature.append(astNode.getTokenLine());
    return signature.toString();
  }

  private String extractEventSignature(String accessor, AstNode astNode) {
    StringBuilder signature = new StringBuilder(accessor);
    signature.append("_");
    AstNode delcarationNode = astNode.getParent().getParent();
    signature.append(delcarationNode.findFirstChild(g.memberName).getTokenValue());
    signature.append(":");
    signature.append(astNode.getTokenLine());
    return signature.toString();
  }

}
