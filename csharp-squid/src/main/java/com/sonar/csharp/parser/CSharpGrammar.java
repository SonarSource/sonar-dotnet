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
  public Rule type; // LATER
  public Rule valueType; // LATER
  public Rule structType; // LATER
  public Rule enumType; // LATER
  public Rule simpleType; // OK
  public Rule numericType; // OK - tested via SimpleTypeTest
  public Rule integralType; // OK - tested via SimpleTypeTest
  public Rule floatingPointType; // OK - tested via SimpleTypeTest
  public Rule nullableType; // OK
  public Rule nonNullableValueType; // LATER
  public Rule referenceType; // LATER
  public Rule classType; // OK
  public Rule interfaceType; // LATER
  public Rule arrayType; // OK
  public Rule nonArrayType; // LATER
  public Rule rankSpecifier; // OK
  public Rule delegateType; // LATER

  // A.2.3 Variables
  public Rule variableReference; // LATER

  // A.2.4 Expressions
  public Rule argumentList; // LATER
  public Rule argument; // OK
  public Rule primaryExpression; // LATER
  public Rule primaryNoArrayCreationExpression; // LATER
  public Rule simpleName; // OK
  public Rule parenthesizedExpression; // OK
  public Rule memberAccess; // OK
  public Rule predefinedType; // OK
  public Rule invocationExpression; // OK
  public Rule elementAccess; // OK
  public Rule expressionList; // LATER
  public Rule thisAccess; // LATER
  public Rule baseAccess; // OK
  public Rule postIncrementExpression; // LATER
  public Rule postDecrementExpression; // LATER
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
  public Rule constantExpression; // LATER
  public Rule booleanExpression; // LATER

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
  public Rule ifStatement; // OK
  public Rule switchStatement; // OK
  public Rule switchSection; // OK
  public Rule switchLabel; // OK
  public Rule iterationStatement; // OK
  public Rule whileStatement; // OK
  public Rule doStatement; // OK
  public Rule forStatement; // OK
  public Rule forInitializer; // NO NEED
  public Rule forCondition; // NO NEED
  public Rule forIterator; // NO NEED
  public Rule statementExpressionList; // OK
  public Rule foreachStatement; // OK
  public Rule jumpStatement; // OK
  public Rule breakStatement; // OK
  public Rule continueStatement; // OK
  public Rule gotoStatement; // OK
  public Rule returnStatement; // OK
  public Rule throwStatement; // OK
  public Rule tryStatement; // OK
  public Rule catchClauses; // OK
  public Rule specificCatchClause; // OK
  public Rule generalCatchClause; // OK
  public Rule finallyClause; // OK
  public Rule checkedStatement; // OK
  public Rule uncheckedStatement; // OK
  public Rule lockStatement; // OK
  public Rule usingStatement; // OK
  public Rule resourceAcquisition; // OK - tested via UsingStatementTest
  public Rule yieldStatement; // OK
  public Rule namespaceDeclaration; // OK
  public Rule qualifiedIdentifier; // OK
  public Rule namespaceBody; // OK
  public Rule externAliasDirective; // OK
  public Rule usingDirective; // OK
  public Rule usingAliasDirective; // OK - tested via UsingDirectiveTest
  public Rule usingNamespaceDirective; // OK - tested via UsingDirectiveTest
  public Rule namespaceMemberDeclaration; // OK
  public Rule typeDeclaration; // OK
  public Rule qualifiedAliasMember; // OK

  // A.2.6 Classes
  public Rule classDeclaration; // OK
  public Rule classModifier; // OK
  public Rule classBase; // OK
  public Rule interfaceTypeList; // LATER
  public Rule classBody; // OK
  public Rule classMemberDeclaration; // LATER
  public Rule constantDeclaration; // OK
  public Rule constantModifier; // OK - tested via ConstantDeclarationTest
  public Rule fieldDeclaration; // OK
  public Rule fieldModifier; // OK - tested via FieldDeclarationTest
  public Rule variableDeclarator; // OK
  public Rule variableInitializer; // LATER
  public Rule methodDeclaration; // LATER
  public Rule methodHeader; // OK
  public Rule methodModifier; // OK - tested via MethodHeaderTest
  public Rule returnType; // OK
  public Rule memberName; // OK
  public Rule methodBody; // LATER
  public Rule formalParameterList; // OK
  public Rule fixedParameters; // LATER
  public Rule fixedParameter; // OK
  public Rule parameterModifier; // OK - tested via FixedParameterTest
  public Rule parameterArray; // OK
  public Rule propertyDeclaration; // OK
  public Rule propertyModifier; // OK - tested via PropertyDeclarationTest
  public Rule accessorDeclarations; // OK
  public Rule getAccessorDeclaration; // OK
  public Rule setAccessorDeclaration; // OK
  public Rule accessorModifier; // OK
  public Rule accessorBody; // LATER
  public Rule eventDeclaration;
  public Rule eventModifier;
  public Rule eventAccessorDeclarations;
  public Rule addAccessorDeclaration;
  public Rule removeAccessorDeclaration;
  public Rule indexerDeclaration;
  public Rule indexerModifier;
  public Rule indexerDeclarator;
  public Rule operatorDeclaration;
  public Rule operatorModifier;
  public Rule operatorDeclarator;
  public Rule unaryOperatorDeclarator;
  public Rule overloadableUnaryOperator;
  public Rule binaryOperatorDeclarator;
  public Rule overloadableBinaryOperator;
  public Rule conversionOperatorDeclarator;
  public Rule operatorBody;
  public Rule constructorDeclaration;
  public Rule constructorModifier;
  public Rule constructorDeclarator;
  public Rule constructorInitializer;
  public Rule constructorBody;
  public Rule staticConstructorDeclaration;
  public Rule staticConstructorModifiers;
  public Rule staticConstructorBody;
  public Rule finalizerDeclaration;
  public Rule finalizerBody;

  // A.2.7 Struct
  public Rule structDeclaration;
  public Rule structModifier;
  public Rule structInterfaces;
  public Rule structBody;
  public Rule structMemberDeclaration;

  // A.2.8 Arrays  
  public Rule arrayInitializer; // OK
  public Rule variableInitializerList; // OK

  // A.2.9 Interfaces
  public Rule interfaceDeclaration;
  public Rule interfaceModifier;
  public Rule interfaceBase;
  public Rule interfaceBody;
  public Rule interfaceMemberDeclaration;
  public Rule interfaceMethodDeclaration;
  public Rule interfacePropertyDeclaration;
  public Rule interfaceAccessors;
  public Rule interfaceEventDeclaration;
  public Rule interfaceIndexerDeclaration;

  // A.2.10 Enums
  public Rule enumDeclaration;
  public Rule enumBase;
  public Rule enumBody;
  public Rule enumModifier;
  public Rule enumMemberDeclarations;
  public Rule enumMemberDeclaration;

  // A.2.11 Delegates
  public Rule delegateDeclaration;
  public Rule delegateModifier;

  // A.2.12 Attributes
  public Rule globalAttributes;
  public Rule globalAttributeSection;
  public Rule globalAttributeTargetSpecifier;
  public Rule globalAttributeTarget; // OK
  public Rule attributes;
  public Rule attributeSection;
  public Rule attributeTargetSpecifier;
  public Rule attributeTarget;
  public Rule attributeList;
  public Rule attribute;
  public Rule attributeName;
  public Rule attributeArguments;
  public Rule positionalArgumentList;
  public Rule positionalArgument;
  public Rule namedArgumentList;
  public Rule namedArgument;
  public Rule attributeArgumentExpression;

  // A.2.13 Generics
  public Rule typeParameterList;
  public Rule typeParameters;
  public Rule typeParameter;
  public Rule typeArgumentList; // OK
  public Rule typeArgument;
  public Rule typeParameterConstraintsClauses;
  public Rule typeParameterConstraintClause;
  public Rule typeParamterConstraints;
  public Rule primaryConstraint;
  public Rule secondaryConstraints;
  public Rule constructorConstraint;

  /**
   * ${@inheritDoc}
   */
  public Rule getRootRule() {
    return compilationUnit;
  }
}
