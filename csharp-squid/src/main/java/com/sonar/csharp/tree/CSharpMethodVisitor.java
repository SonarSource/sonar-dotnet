/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import org.sonar.squid.api.SourceFile;
import org.sonar.squid.api.SourceFunction;

import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

public class CSharpMethodVisitor extends CSharpAstVisitor {

  @Override
  public void init() {
    subscribeTo(getCSharpGrammar().methodDeclaration);
  }

  @Override
  public void visitNode(AstNode astNode) {
    String methodSignature = extractMethodSignature(astNode);
    SourceFunction method = new SourceFunction((SourceFile) peekSourceCode(), methodSignature, astNode.getTokenLine());
    method.setMeasure(CSharpMetric.METHODS, 1);
    addSourceCode(method);
  }

  @Override
  public void leaveNode(AstNode astNode) {
    popSourceCode();
  }

  private String extractMethodSignature(AstNode astNode) {
    return astNode.findFirstChild(getCSharpGrammar().memberName).getTokenValue() + ":" + astNode.getToken().getLine();
  }

}
