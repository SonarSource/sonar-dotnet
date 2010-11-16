/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Rule;

/**
 * Listing of the syntactic elements of the C# grammar
 */
public class CSharpGrammar implements Grammar {

  // TODO Code de Freddy
  public Rule compilationUnit;
  public Rule compilationUnitLevel;
  public Rule block;
  public Rule line;

  // XXX Beginning of my code
  public Rule identifier;

  // A.2.1 Basic concepts
  public Rule namespaceName;
  public Rule typeName;
  public Rule namespaceOrTypeName;

  // A.2.2 Types
  public Rule type;
  public Rule valueType;
  public Rule referenceType;
  public Rule typeParameter;
  public Rule structType;
  public Rule enumType;
  public Rule simpleType;
  public Rule nullableType;
  public Rule numericType;
  public Rule integralType;
  public Rule floatingPointType;
  public Rule nonNullableValueType;
  public Rule classType;
  public Rule interfaceType;
  public Rule arrayType;
  public Rule nonArrayType;
  public Rule rankSpecifier;
  public Rule delegateType;

  // A.2.5 Statements
  public Rule qualifiedAliasMember;

  // A.2.6 Classes
  public Rule classDeclaration;
  public Rule attributes;
  public Rule classModifier;
  public Rule typeParameterList;
  public Rule classBase;
  public Rule typeParameterConstraintsClauses;
  public Rule classBody;
  public Rule classMemberDeclaration;

  // A.2.13 Generics
  public Rule typeArgumentList;

  /**
   * ${@inheritDoc}
   */
  public Rule getRootRule() {
    return compilationUnit;
  }
}
