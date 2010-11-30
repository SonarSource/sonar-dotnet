/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.tree;

import org.sonar.squid.api.SourceFile;

import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstNode;

public class CSharpFileVisitor extends CSharpAstVisitor {

  @Override
  public void visitFile(AstNode astNode) {
    SourceFile cSharpFile = new SourceFile(getFile().getAbsolutePath().replace('\\', '/'), getFile().getName());
    addSourceCode(cSharpFile);
    peekSourceCode().setMeasure(CSharpMetric.FILES, 1);
  }

  @Override
  public void leaveFile(AstNode astNode) {
    popSourceCode();
  }

}
