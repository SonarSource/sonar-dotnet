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
import com.sonar.sslr.api.Token;

/**
 * Visitor that computes the number of lines of a file.
 */
public class CSharpLineVisitor extends CSharpAstVisitor implements AstAndTokenVisitor {

  /**
   * {@inheritDoc}
   */
  public void visitToken(Token token) {
    if (token.getType() == EOF) {
      peekPhysicalSourceCode().setMeasure(CSharpMetric.LINES, token.getLine());
    }
  }
}
