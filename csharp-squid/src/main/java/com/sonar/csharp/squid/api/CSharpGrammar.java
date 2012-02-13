/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.api;

import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.LeftRecursiveRule;
import com.sonar.sslr.api.Rule;

/**
 * Listing of the syntactic elements of the C# grammar
 */
public class CSharpGrammar implements Grammar {

  public Rule literal;
  public Rule rightShift;
  public Rule rightShiftAssignment;

  // A.2.1 Basic concepts
  public Rule compilationUnit; // OK
  public Rule namespaceName; // OK
  public Rule typeName; // OK
  public LeftRecursiveRule namespaceOrTypeName; // OK

  // A.2.2 Types
  public LeftRecursiveRule type; // OK
  public LeftRecursiveRule valueType; // LATER // NOT NECESSARY
  public LeftRecursiveRule structType; // OK // NOT NECESSARY
  public LeftRecursiveRule enumType; // LATER // NOT NECESSARY
  public Rule simpleType; // OK // NOT NECESSARY
  public Rule numericType; // OK - tested via SimpleTypeTest // NOT NECESSARY
  public Rule integralType; // OK - tested via SimpleTypeTest
  public Rule floatingPointType; // OK - tested via SimpleTypeTest // NOT NECESSARY
  public LeftRecursiveRule nullableType; // OK // NOT NECESSARY
  public LeftRecursiveRule nonNullableValueType; // OK - tested via NullableTypeTest // NOT NECESSARY
  public LeftRecursiveRule referenceType; // OK // NOT NECESSARY
  public LeftRecursiveRule classType; // OK
  public LeftRecursiveRule interfaceType; // OK
  public LeftRecursiveRule arrayType; // OK
  public LeftRecursiveRule nonArrayType; // LATER
  public Rule rankSpecifier; // OK
  public LeftRecursiveRule delegateType; // LATER

  // A.2.3 Variables
  public Rule variableReference; // LATER

  // A.2.4 Expressions
  public LeftRecursiveRule argumentList; // LATER
  public LeftRecursiveRule argument; // OK
  public LeftRecursiveRule argumentName; // OK
  public LeftRecursiveRule argumentValue; // OK
  public LeftRecursiveRule primaryExpression; // OK
  public LeftRecursiveRule primaryNoArrayCreationExpression; // LATER
  public Rule simpleName; // OK
  public LeftRecursiveRule parenthesizedExpression; // OK
  public LeftRecursiveRule memberAccess; // OK
  public Rule predefinedType; // OK
  public LeftRecursiveRule invocationExpression; // OK
  public LeftRecursiveRule elementAccess; // OK
  public Rule thisAccess; // LATER
  public Rule baseAccess; // OK
  public LeftRecursiveRule postIncrementExpression; // OK
  public LeftRecursiveRule postDecrementExpression; // OK
  public LeftRecursiveRule objectCreationExpression; // OK
  public LeftRecursiveRule objectOrCollectionInitializer; // OK
  public LeftRecursiveRule objectInitializer; // OK
  public LeftRecursiveRule memberInitializer; // OK
  public LeftRecursiveRule initializerValue; // OK
  public LeftRecursiveRule collectionInitializer; // OK
  public LeftRecursiveRule elementInitializer; // OK
  public LeftRecursiveRule expressionList; // LATER
  public LeftRecursiveRule arrayCreationExpression; // OK
  public LeftRecursiveRule delegateCreationExpression; // OK
  public LeftRecursiveRule anonymousObjectCreationExpression; // OK
  public LeftRecursiveRule anonymousObjectInitializer; // OK
  public LeftRecursiveRule memberDeclarator; // OK
  public LeftRecursiveRule typeOfExpression; // OK
  public Rule unboundTypeName; // OK
  public Rule genericDimensionSpecifier; // OK
  public LeftRecursiveRule checkedExpression; // OK
  public LeftRecursiveRule uncheckedExpression; // OK
  public LeftRecursiveRule defaultValueExpression; // OK
  public LeftRecursiveRule unaryExpression; // OK
  public LeftRecursiveRule preIncrementExpression; // OK - tested via UnaryExpressionTest
  public LeftRecursiveRule preDecrementExpression; // OK - tested via UnaryExpressionTest
  public LeftRecursiveRule castExpression; // OK - tested via UnaryExpressionTest
  public LeftRecursiveRule multiplicativeExpression; // OK
  public LeftRecursiveRule additiveExpression; // OK
  public LeftRecursiveRule shiftExpression; // OK
  public LeftRecursiveRule relationalExpression; // OK
  public LeftRecursiveRule equalityExpression; // OK
  public LeftRecursiveRule andExpression; // OK
  public LeftRecursiveRule exclusiveOrExpression; // OK
  public LeftRecursiveRule inclusiveOrExpression; // OK
  public LeftRecursiveRule conditionalAndExpression; // OK
  public LeftRecursiveRule conditionalOrExpression; // OK
  public LeftRecursiveRule nullCoalescingExpression; // OK
  public LeftRecursiveRule conditionalExpression; // OK
  public LeftRecursiveRule lambdaExpression; // OK
  public LeftRecursiveRule anonymousMethodExpression; // OK
  public Rule anonymousFunctionSignature; // OK
  public Rule explicitAnonymousFunctionSignature; // OK
  public Rule explicitAnonymousFunctionParameter; // OK
  public Rule anonymousFunctionParameterModifier; // OK
  public Rule implicitAnonymousFunctionSignature; // OK
  public Rule implicitAnonymousFunctionParameter; // OK
  public LeftRecursiveRule anonymousFunctionBody; // OK
  public LeftRecursiveRule queryExpression; // OK
  public LeftRecursiveRule fromClause; // OK
  public LeftRecursiveRule queryBody; // OK
  public LeftRecursiveRule queryBodyClause; // OK
  public LeftRecursiveRule letClause; // OK
  public LeftRecursiveRule whereClause; // OK
  public LeftRecursiveRule joinClause; // OK
  public LeftRecursiveRule joinIntoClause; // OK
  public LeftRecursiveRule orderByClause; // OK
  public LeftRecursiveRule ordering; // OK
  public LeftRecursiveRule orderingDirection; // OK
  public LeftRecursiveRule selectOrGroupClause; // OK
  public LeftRecursiveRule selectClause; // OK
  public LeftRecursiveRule groupClause; // OK
  public LeftRecursiveRule queryContinuation; // OK
  public LeftRecursiveRule assignment; // OK
  public LeftRecursiveRule expression; // OK
  public LeftRecursiveRule nonAssignmentExpression; // OK
  public LeftRecursiveRule constantExpression; // LATER
  public LeftRecursiveRule booleanExpression; // LATER

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
  public Rule forInitializer; // OK
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
  public Rule variableInitializer; // OK
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
  public Rule eventDeclaration; // OK
  public Rule eventModifier; // OK - tested via EventDeclarationTest
  public Rule eventAccessorDeclarations; // OK
  public Rule addAccessorDeclaration; // OK - tested via EventAccessorDeclarationTest
  public Rule removeAccessorDeclaration; // OK - tested via EventAccessorDeclarationTest
  public Rule indexerDeclaration; // OK
  public Rule indexerModifier; // OK - tested via IndexerDeclarationTest
  public Rule indexerDeclarator; // OK
  public Rule operatorDeclaration; // OK
  public Rule operatorModifier; // OK - tested via OperatorDeclarationTest
  public Rule operatorDeclarator; // OK
  public Rule unaryOperatorDeclarator; // OK
  public Rule overloadableUnaryOperator; // OK - tested via UnaryOperatorDeclaration
  public Rule binaryOperatorDeclarator; // OK
  public Rule overloadableBinaryOperator; // OK - tested via BinaryOperatorDeclaration
  public Rule conversionOperatorDeclarator; // OK
  public Rule operatorBody; // LATER
  public Rule constructorDeclaration; // OK
  public Rule constructorModifier; // OK - tested via ConstructorDeclarationTest
  public Rule constructorDeclarator; // OK
  public Rule constructorInitializer; // OK
  public Rule constructorBody; // OK
  public Rule staticConstructorDeclaration; // OK
  public Rule staticConstructorModifiers; // OK
  public Rule staticConstructorBody; // LATER
  public Rule destructorDeclaration; // OK
  public Rule destructorBody; // LATER

  // A.2.7 Struct
  public Rule structDeclaration; // OK
  public Rule structModifier; // OK - tested via StructDeclarationTest
  public Rule structInterfaces; // OK
  public Rule structBody; // OK
  public Rule structMemberDeclaration; // OK - tested via StructBodyTest

  // A.2.8 Arrays
  public Rule arrayInitializer; // OK
  public Rule variableInitializerList; // OK

  // A.2.9 Interfaces
  public Rule interfaceDeclaration; // OK
  public Rule interfaceModifier; // OK - tested via InterfaceDeclarationTest
  public Rule variantTypeParameterList; // OK
  public Rule variantTypeParameter; // OK
  public Rule varianceAnnotation; // OK
  public Rule interfaceBase; // OK
  public Rule interfaceBody; // OK
  public Rule interfaceMemberDeclaration; // OK - tested via InterfaceBodyDeclaration
  public Rule interfaceMethodDeclaration; // OK
  public Rule interfacePropertyDeclaration; // OK
  public Rule interfaceAccessors; // OK
  public Rule interfaceEventDeclaration; // OK
  public Rule interfaceIndexerDeclaration; // OK

  // A.2.10 Enums
  public Rule enumDeclaration; // OK
  public Rule enumBase; // OK
  public Rule enumBody; // OK
  public Rule enumModifier; // OK - tested via EnumDeclarationTest
  public Rule enumMemberDeclarations; // OK
  public Rule enumMemberDeclaration; // OK

  // A.2.11 Delegates
  public Rule delegateDeclaration; // OK
  public Rule delegateModifier; // OK - tested via DelegateDeclarationTest

  // A.2.12 Attributes
  public Rule globalAttributes; // LATER
  public Rule globalAttributeSection; // OK
  public Rule globalAttributeTargetSpecifier; // OK
  public Rule globalAttributeTarget; // OK
  public Rule attributes; // LATER
  public Rule attributeSection; // OK
  public Rule attributeTargetSpecifier; // OK
  public Rule attributeTarget; // OK
  public Rule attributeList; // OK
  public Rule attribute; // OK
  public Rule attributeName; // LATER
  public Rule attributeArguments; // OK
  public Rule positionalArgument; // LATER
  public Rule namedArgument; // OK
  public Rule attributeArgumentExpression; // OK

  // A.2.13 Generics
  public Rule typeParameterList; // OK
  public Rule typeParameters; // OK
  public Rule typeParameter; // OK - tested via TypeParametersTest
  public Rule typeArgumentList; // OK
  public Rule typeArgument; // OK - tested via TypeArgumentListTest
  public Rule typeParameterConstraintsClauses; // LATER
  public Rule typeParameterConstraintsClause; // OK
  public Rule typeParameterConstraints; // OK
  public Rule primaryConstraint; // OK
  public Rule secondaryConstraints; // OK
  public Rule constructorConstraint; // OK

  /** Grammar extensions **/
  public CSharpUnsafeExtensionGrammar unsafe;

  /**
   * ${@inheritDoc}
   */
  public Rule getRootRule() {
    return compilationUnit;
  }
}
