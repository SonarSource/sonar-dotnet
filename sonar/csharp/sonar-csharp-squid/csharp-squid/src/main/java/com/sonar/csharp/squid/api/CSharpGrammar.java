/*
 * Sonar C# Plugin :: C# Squid :: Squid
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package com.sonar.csharp.squid.api;

import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.Rule;

/**
 * Listing of the syntactic elements of the C# grammar
 */
public abstract class CSharpGrammar extends Grammar {

  public Rule literal;
  public Rule rightShift;
  public Rule rightShiftAssignment;

  // A.2.1 Basic concepts
  public Rule compilationUnit;
  public Rule namespaceName;
  public Rule typeName;
  public Rule namespaceOrTypeName;

  // A.2.2 Types
  public Rule simpleType;
  public Rule numericType;
  public Rule integralType;
  public Rule floatingPointType;

  public Rule rankSpecifier;
  public Rule rankSpecifiers;

  public Rule typePrimary;
  public Rule nullableType;
  public Rule pointerType;
  public Rule arrayType;
  public Rule type;

  public Rule nonArrayType;
  public Rule nonNullableValueType;

  public Rule classType;
  public Rule interfaceType;
  public Rule enumType;
  public Rule delegateType;

  // A.2.3 Variables
  public Rule variableReference;

  // A.2.4 Expressions
  public Rule primaryExpressionPrimary;
  public Rule primaryNoArrayCreationExpression;
  public Rule postElementAccess;
  public Rule postMemberAccess;

  public Rule postInvocation;
  public Rule postIncrement;
  public Rule postDecrement;
  public Rule postPointerMemberAccess;

  public Rule postfixExpression;
  public Rule primaryExpression;

  public Rule argumentList;
  public Rule argument;
  public Rule argumentName;
  public Rule argumentValue;
  public Rule simpleName;
  public Rule parenthesizedExpression;
  public Rule memberAccess;
  public Rule predefinedType;
  public Rule thisAccess;
  public Rule baseAccess;
  public Rule objectCreationExpression;
  public Rule objectOrCollectionInitializer;
  public Rule objectInitializer;
  public Rule memberInitializer;
  public Rule initializerValue;
  public Rule collectionInitializer;
  public Rule elementInitializer;
  public Rule expressionList;
  public Rule arrayCreationExpression;
  public Rule delegateCreationExpression;
  public Rule anonymousObjectCreationExpression;
  public Rule anonymousObjectInitializer;
  public Rule memberDeclarator;
  public Rule typeOfExpression;
  public Rule unboundTypeName;
  public Rule genericDimensionSpecifier;
  public Rule checkedExpression;
  public Rule uncheckedExpression;
  public Rule defaultValueExpression;
  public Rule unaryExpression;
  public Rule multiplicativeExpression;
  public Rule additiveExpression;
  public Rule shiftExpression;
  public Rule relationalExpression;
  public Rule equalityExpression;
  public Rule andExpression;
  public Rule exclusiveOrExpression;
  public Rule inclusiveOrExpression;
  public Rule conditionalAndExpression;
  public Rule conditionalOrExpression;
  public Rule nullCoalescingExpression;
  public Rule conditionalExpression;
  public Rule lambdaExpression;
  public Rule anonymousMethodExpression;
  public Rule anonymousFunctionSignature;
  public Rule explicitAnonymousFunctionSignature;
  public Rule explicitAnonymousFunctionParameter;
  public Rule anonymousFunctionParameterModifier;
  public Rule implicitAnonymousFunctionSignature;
  public Rule implicitAnonymousFunctionParameter;
  public Rule anonymousFunctionBody;
  public Rule queryExpression;
  public Rule fromClause;
  public Rule queryBody;
  public Rule queryBodyClause;
  public Rule letClause;
  public Rule whereClause;
  public Rule joinClause;
  public Rule joinIntoClause;
  public Rule orderByClause;
  public Rule ordering;
  public Rule orderingDirection;
  public Rule selectOrGroupClause;
  public Rule selectClause;
  public Rule groupClause;
  public Rule queryContinuation;
  public Rule assignment;
  public Rule expression;
  public Rule nonAssignmentExpression;

  // A.2.5 Statement
  public Rule statement;
  public Rule embeddedStatement;
  public Rule block;
  public Rule labeledStatement;
  public Rule declarationStatement;
  public Rule localVariableDeclaration;
  public Rule localVariableDeclarator;
  public Rule localVariableInitializer;
  public Rule localConstantDeclaration;
  public Rule constantDeclarator;
  public Rule expressionStatement;
  public Rule selectionStatement;
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
  public Rule qualifiedAliasMember;

  // A.2.6 Classes
  public Rule classDeclaration;
  public Rule classModifier;
  public Rule classBase;
  public Rule interfaceTypeList;
  public Rule classBody;
  public Rule classMemberDeclaration;
  public Rule constantDeclaration;
  public Rule constantModifier;
  public Rule fieldDeclaration;
  public Rule fieldModifier;
  public Rule variableDeclarator;
  public Rule variableInitializer;
  public Rule methodDeclaration;
  public Rule methodHeader;
  public Rule methodModifier;
  public Rule returnType;
  public Rule memberName;
  public Rule methodBody;
  public Rule formalParameterList;
  public Rule fixedParameters;
  public Rule fixedParameter;
  public Rule parameterModifier;
  public Rule parameterArray;
  public Rule propertyDeclaration;
  public Rule propertyModifier;
  public Rule accessorDeclarations;
  public Rule getAccessorDeclaration;
  public Rule setAccessorDeclaration;
  public Rule accessorModifier;
  public Rule accessorBody;
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
  public Rule destructorDeclaration;
  public Rule destructorBody;

  // A.2.7 Struct
  public Rule structDeclaration;
  public Rule structModifier;
  public Rule structInterfaces;
  public Rule structBody;
  public Rule structMemberDeclaration;

  // A.2.8 Arrays
  public Rule arrayInitializer;
  public Rule variableInitializerList;

  // A.2.9 Interfaces
  public Rule interfaceDeclaration;
  public Rule interfaceModifier;
  public Rule variantTypeParameterList;
  public Rule variantTypeParameter;
  public Rule varianceAnnotation;
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
  public Rule globalAttributeTarget;
  public Rule attributes;
  public Rule attributeSection;
  public Rule attributeTargetSpecifier;
  public Rule attributeTarget;
  public Rule attributeList;
  public Rule attribute;
  public Rule attributeName;
  public Rule attributeArguments;
  public Rule positionalArgument;
  public Rule namedArgument;
  public Rule attributeArgumentExpression;

  // A.2.13 Generics
  public Rule typeParameterList;
  public Rule typeParameters;
  public Rule typeParameter;
  public Rule typeArgumentList;
  public Rule typeArgument;
  public Rule typeParameterConstraintsClauses;
  public Rule typeParameterConstraintsClause;
  public Rule typeParameterConstraints;
  public Rule primaryConstraint;
  public Rule secondaryConstraints;
  public Rule constructorConstraint;

  // A.3 Unsafe code
  public Rule unsafeStatement;
  public Rule pointerIndirectionExpression;
  public Rule pointerElementAccess;
  public Rule addressOfExpression;
  public Rule sizeOfExpression;
  public Rule fixedStatement;
  public Rule fixedPointerDeclarator;
  public Rule fixedPointerInitializer;
  public Rule fixedSizeBufferDeclaration;
  public Rule fixedSizeBufferModifier;
  public Rule fixedSizeBufferDeclarator;
  public Rule stackallocInitializer;

  /**
   * ${@inheritDoc}
   */
  @Override
  public Rule getRootRule() {
    return compilationUnit;
  }

}
