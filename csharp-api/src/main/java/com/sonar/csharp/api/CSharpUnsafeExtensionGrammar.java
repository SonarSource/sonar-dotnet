/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.Rule;

/**
 * Listing of the syntactic elements of the C# grammar extension for unsafe code
 */
public class CSharpUnsafeExtensionGrammar {

  public Rule destructorDeclaration; // OK
  public Rule unsafeStatement; // OK
  public Rule pointerType; // OK
  public Rule pointerIndirectionExpression; // OK
  public Rule pointerMemberAccess; // OK
  public Rule pointerElementAccess; // OK
  public Rule addressOfExpression; // OK
  public Rule sizeOfExpression; // OK
  public Rule fixedStatement; // OK
  public Rule fixedPointerDeclarator; // OK
  public Rule fixedPointerInitializer; // OK
  public Rule fixedSizeBufferDeclaration; // OK
  public Rule fixedSizeBufferModifier; // OK
  public Rule fixedSizeBufferDeclarator; // OK
  public Rule stackallocInitializer; // OK

}
