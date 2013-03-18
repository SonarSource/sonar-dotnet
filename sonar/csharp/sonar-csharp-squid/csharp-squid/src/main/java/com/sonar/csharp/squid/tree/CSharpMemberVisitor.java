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

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.api.source.SourceMember;
import com.sonar.csharp.squid.api.source.SourceType;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.squid.SquidAstVisitor;

/**
 * Visitor that creates member resources (= methods, property accessors, event accessors, indexer accessors, operators, constructors,
 * finalizers) and computes the number of members.
 */
public class CSharpMemberVisitor extends SquidAstVisitor<CSharpGrammar> {

  private CSharpGrammar g;

  /**
   * {@inheritDoc}
   */
  @Override
  public void init() {
    g = getContext().getGrammar();

    subscribeTo(
        g.methodDeclaration,
        g.constructorBody,
        g.staticConstructorBody,
        g.destructorBody,
        g.accessorBody,
        g.addAccessorDeclaration,
        g.removeAccessorDeclaration,
        g.operatorBody);
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

    String memberSignature = defineMemberSignature(astNode);
    SourceMember member = new SourceMember((SourceType) getContext().peekSourceCode(), memberSignature, astNode.getTokenLine());
    member.setMeasure(CSharpMetric.METHODS, 1);
    getContext().addSourceCode(member);
  }

  private String defineMemberSignature(AstNode astNode) {
    String memberSignature = "";
    if (astNode.is(g.methodDeclaration)) {
      memberSignature = extractMethodSignature(astNode.getFirstChild(g.methodBody));
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
    return memberSignature;
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
    getContext().popSourceCode();
  }

  private String extractMethodSignature(AstNode astNode) {
    return astNode.getParent().getFirstChild(g.memberName).getTokenValue() + ":" + astNode.getTokenLine();
  }

  private String extractPropertySignature(AstNode astNode) {
    StringBuilder signature = new StringBuilder(astNode.getPreviousSibling().getLastToken().getValue());
    signature.append("_");
    AstNode delcarationNode = astNode.getParent().getParent().getParent();
    if (delcarationNode.is(g.indexerDeclaration)) {
      signature.append("Item");
    } else {
      signature.append(delcarationNode.getFirstChild(g.memberName).getTokenValue());
    }
    signature.append(":");
    signature.append(astNode.getTokenLine());
    return signature.toString();
  }

  private String extractEventSignature(String accessor, AstNode astNode) {
    StringBuilder signature = new StringBuilder(accessor);
    signature.append("_");
    AstNode delcarationNode = astNode.getParent().getParent();
    signature.append(delcarationNode.getFirstChild(g.memberName).getTokenValue());
    signature.append(":");
    signature.append(astNode.getTokenLine());
    return signature.toString();
  }

}
