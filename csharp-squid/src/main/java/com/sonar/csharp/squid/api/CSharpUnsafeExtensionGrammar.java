/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api;

import com.sonar.sslr.api.LeftRecursiveRule;

/**
 * Listing of the syntactic elements of the C# grammar extension for unsafe code
 */
public class CSharpUnsafeExtensionGrammar {

  public LeftRecursiveRule destructorDeclaration; // OK
  public LeftRecursiveRule unsafeStatement; // OK
  public LeftRecursiveRule pointerType; // OK
  public LeftRecursiveRule pointerIndirectionExpression; // OK
  public LeftRecursiveRule pointerMemberAccess; // OK
  public LeftRecursiveRule pointerElementAccess; // OK
  public LeftRecursiveRule addressOfExpression; // OK
  public LeftRecursiveRule sizeOfExpression; // OK
  public LeftRecursiveRule fixedStatement; // OK
  public LeftRecursiveRule fixedPointerDeclarator; // OK
  public LeftRecursiveRule fixedPointerInitializer; // OK
  public LeftRecursiveRule fixedSizeBufferDeclaration; // OK
  public LeftRecursiveRule fixedSizeBufferModifier; // OK
  public LeftRecursiveRule fixedSizeBufferDeclarator; // OK
  public LeftRecursiveRule stackallocInitializer; // OK

}
