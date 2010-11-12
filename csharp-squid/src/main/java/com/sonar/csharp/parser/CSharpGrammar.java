/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Rule;
/**
 * 
 * CLASSE ORIGINALE A MODIFIER
 *
 */
public class CSharpGrammar implements Grammar {

  public Rule compilationUnit;
  public Rule compilationUnitLevel;
  public Rule block;
  public Rule line;

  public Rule getRootRule() {
    return compilationUnit;
  }
}
