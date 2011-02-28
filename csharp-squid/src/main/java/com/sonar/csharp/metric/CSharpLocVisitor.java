/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.metric;

import static com.sonar.sslr.api.GenericTokenType.EOF;

import com.sonar.csharp.api.ast.CSharpAstVisitor;
import com.sonar.csharp.api.metric.CSharpMetric;
import com.sonar.sslr.api.AstAndTokenVisitor;
import com.sonar.sslr.api.AstNode;
import com.sonar.sslr.api.Token;

/**
 * Visitor that computes the number of lines of code of a file.
 */
public class CSharpLocVisitor extends CSharpAstVisitor implements AstAndTokenVisitor {

  private int lastTokenLine;

  /**
   * {@inheritDoc}
   */
  @Override
  public void visitFile(AstNode node) {
    lastTokenLine = -1;
  }

  /**
   * {@inheritDoc}
   */
  public void visitToken(Token token) {
    if (token.getType() != EOF && lastTokenLine != token.getLine()) {
      peekSourceCode().add(CSharpMetric.LINES_OF_CODE, 1);
      lastTokenLine = token.getLine();
    }
  }
}
