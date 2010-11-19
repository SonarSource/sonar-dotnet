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

  public Rule literal;
  public Rule identifier;

  // A.2.1 Basic concepts
  public Rule compilationUnit; // OK
  public Rule namespaceName; // OK
  public Rule typeName; // OK
  public Rule namespaceOrTypeName; // OK

  // A.2.2 Types
  public Rule type;
  public Rule valueType;
  public Rule referenceType;
  public Rule structType;
  public Rule enumType; // OK
  public Rule simpleType; // OK
  public Rule nullableType; // OK
  public Rule numericType; // OK
  public Rule integralType; // OK
  public Rule floatingPointType; // OK
  public Rule nonNullableValueType; // OK
  public Rule classType; // OK
  public Rule interfaceType; // NONEED
  public Rule arrayType; // OK
  public Rule nonArrayType; // NONEED
  public Rule rankSpecifier; // OK
  public Rule delegateType; // NONEED
  
  // A.2.3 Variables
  public Rule variableReference; // NONEED
  
  // A.2.4 Expressions
  public Rule argumentList; // NONEED
  public Rule argument; // OK
  public Rule primaryExpression; // NONEED
  public Rule primaryNoArrayCreationExpression; // NONEED
  public Rule simpleName; // OK
  public Rule parenthesizedExpression; // OK
  public Rule memberAccess; // OK
  public Rule predefinedType; // OK
  public Rule invocationExpression; // OK
  public Rule elementAccess; // OK
  public Rule expressionList; // NONEED
  public Rule thisAccess; // NONEED
  public Rule baseAccess; // OK
  public Rule postIncrementExpression; // NONEED
  public Rule postDecrementExpression; // NONEED
  public Rule objectCreationExpression; // OK
  public Rule arrayCreationExpression; // OK
  public Rule delegateCreationExpression; // OK
  public Rule typeOfExpression; // OK
  public Rule unboundTypeName; // OK
  public Rule genericDimensionSpecifier; // OK
  public Rule checkedExpression; // OK
  public Rule uncheckedExpression; // OK
  public Rule defaultValueExpression; // OK
  public Rule anonymousMethodExpression; // OK
  public Rule anonymousMethodSignature; // OK
  public Rule anonymousMethodParameter; // OK
  public Rule unaryExpression; // OK
  public Rule preIncrementExpression; // OK - tested via UnaryExpressionTest
  public Rule preDecrementExpression; // OK - tested via UnaryExpressionTest
  public Rule castExpression; // OK - tested via UnaryExpressionTest
  public Rule multiplicativeExpression; // OK
  public Rule additiveExpression; // OK
  public Rule shiftExpression; // OK
  public Rule relationalExpression; // OK
  public Rule equalityExpression; // OK
  public Rule andExpression; // OK
  public Rule exclusiveOrExpression; // OK
  public Rule inclusiveOrExpression; // OK
  public Rule conditionalAndExpression; // OK
  public Rule conditionalOrExpression; // OK
  public Rule nullCoalescingExpression; // OK
  public Rule conditionalExpression; // OK
  public Rule assignment; // OK
  public Rule expression; // OK 
  public Rule constantExpression; // NONEED
  public Rule booleanExpression; // NONEED

  // A.2.5 Statement
  public Rule statement; // OK
  public Rule embeddedStatement; // OK
  public Rule block; // BRIDGE TEST ONLY
  public Rule labeledStatement; // OK
  public Rule declarationStatement; // OK
  public Rule localVariableDeclaration; // OK
  public Rule localVariableDeclarator; // OK - tested via LocalVariableDeclarationTest
  public Rule localVariableInitializer; // OK
  public Rule localConstantDeclaration; // OK
  public Rule constantDeclarator; // OK
  public Rule expressionStatement; // OK
  public Rule statementExpression; // OK - tested via ExpressionStatementTest
  public Rule selectionStatement; // NO NEED
  public Rule ifStatement;
  public Rule switchStatement;
  public Rule switchSection;
  public Rule switchLabel;
  public Rule iterationStatement;
  public Rule whileStatement;
  public Rule doStatement;
  public Rule forStatement;
  public Rule forInitializer;
  public Rule forCondition;
  public Rule forIterator;
  public Rule statementExpressionList;
  public Rule foreachStatement;
  public Rule jumpStatement;
  public Rule breakStatement;
  public Rule continueStatement;
  public Rule gotoStatement;
  public Rule returnStatement;
  public Rule throwStatement;
  public Rule tryStatement;
  public Rule catchClauses;
  public Rule specificCatchClause;
  public Rule generalCatchClause;
  public Rule finallyClause;
  public Rule checkedStatement;
  public Rule uncheckedStatement;
  public Rule lockStatement;
  public Rule usingStatement;
  public Rule resourceAcquisition;
  public Rule yieldStatement;
  public Rule namespaceDeclaration;
  public Rule qualifiedIdentifier;
  public Rule namespaceBody;
  public Rule externAliasDirective;
  public Rule usingDirective;
  public Rule usingAliasDirective;
  public Rule usingNamespaceDirective;
  public Rule namespaceMemberDeclaration;
  public Rule typeDeclaration;
  public Rule qualifiedAliasMember; // OK

  // A.2.6 Classes
  public Rule classDeclaration; // OK
  public Rule attributes;
  public Rule classModifier; // OK
  public Rule typeParameterList;
  public Rule classBase;
  public Rule typeParameterConstraintsClauses;
  public Rule classBody; // OK
  public Rule classMemberDeclaration;
  public Rule parameterModifier;
  
  // A.2.7 Structc
  public Rule structDeclaration;
  
  // A.2.8 Arrays
  public Rule arrayInitializer;
  
  // A.2.9 Interfaces
  public Rule interfaceDeclaration;
  
  // A.2.10 Enums
  public Rule enumDeclaration;

  // A.2.11 Delegates
  public Rule delegateDeclaration;

  // A.2.12 Attributes
  public Rule globalAttributes;

  // A.2.13 Generics
  public Rule typeParameter;
  public Rule typeArgumentList; // OK
  

  /**
   * ${@inheritDoc}
   */
  public Rule getRootRule() {
    return compilationUnit;
  }
}
