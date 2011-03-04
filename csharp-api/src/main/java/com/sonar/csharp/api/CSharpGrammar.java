/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.api;

import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.api.LeftRecursiveRule;
import com.sonar.sslr.api.Rule;

/**
 * Listing of the syntactic elements of the C# grammar
 */
public class CSharpGrammar implements Grammar {

  public LeftRecursiveRule literal;
  public LeftRecursiveRule rightShift;
  public LeftRecursiveRule rightShiftAssignment;

  // A.2.1 Basic concepts
  public LeftRecursiveRule compilationUnit; // OK
  public LeftRecursiveRule namespaceName; // OK
  public LeftRecursiveRule typeName; // OK
  public LeftRecursiveRule namespaceOrTypeName; // OK

  // A.2.2 Types
  public LeftRecursiveRule type; // OK
  public LeftRecursiveRule valueType; // LATER // NOT NECESSARY
  public LeftRecursiveRule structType; // OK // NOT NECESSARY
  public LeftRecursiveRule enumType; // LATER // NOT NECESSARY
  public LeftRecursiveRule simpleType; // OK // NOT NECESSARY
  public LeftRecursiveRule numericType; // OK - tested via SimpleTypeTest // NOT NECESSARY
  public LeftRecursiveRule integralType; // OK - tested via SimpleTypeTest
  public LeftRecursiveRule floatingPointType; // OK - tested via SimpleTypeTest // NOT NECESSARY
  public LeftRecursiveRule nullableType; // OK // NOT NECESSARY
  public LeftRecursiveRule nonNullableValueType; // OK - tested via NullableTypeTest // NOT NECESSARY
  public LeftRecursiveRule referenceType; // OK // NOT NECESSARY
  public LeftRecursiveRule classType; // OK
  public LeftRecursiveRule interfaceType; // OK
  public LeftRecursiveRule arrayType; // OK
  public LeftRecursiveRule nonArrayType; // LATER
  public LeftRecursiveRule rankSpecifier; // OK
  public LeftRecursiveRule delegateType; // LATER

  // A.2.3 Variables
  public LeftRecursiveRule variableReference; // LATER

  // A.2.4 Expressions
  public LeftRecursiveRule argumentList; // LATER
  public LeftRecursiveRule argument; // OK
  public LeftRecursiveRule argumentName; // OK
  public LeftRecursiveRule argumentValue; // OK
  public LeftRecursiveRule primaryExpression; // OK
  public LeftRecursiveRule primaryNoArrayCreationExpression; // LATER
  public LeftRecursiveRule simpleName; // OK
  public LeftRecursiveRule parenthesizedExpression; // OK
  public LeftRecursiveRule memberAccess; // OK
  public LeftRecursiveRule predefinedType; // OK
  public LeftRecursiveRule invocationExpression; // OK
  public LeftRecursiveRule elementAccess; // OK
  public LeftRecursiveRule thisAccess; // LATER
  public LeftRecursiveRule baseAccess; // OK
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
  public LeftRecursiveRule unboundTypeName; // OK
  public LeftRecursiveRule genericDimensionSpecifier; // OK
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
  public LeftRecursiveRule anonymousFunctionSignature; // OK
  public LeftRecursiveRule explicitAnonymousFunctionSignature; // OK
  public LeftRecursiveRule explicitAnonymousFunctionParameter; // OK
  public LeftRecursiveRule anonymousFunctionParameterModifier; // OK
  public LeftRecursiveRule implicitAnonymousFunctionSignature; // OK
  public LeftRecursiveRule implicitAnonymousFunctionParameter; // OK
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
  public LeftRecursiveRule statement; // OK
  public LeftRecursiveRule embeddedStatement; // OK
  public LeftRecursiveRule block; // BRIDGE TEST ONLY
  public LeftRecursiveRule labeledStatement; // OK
  public LeftRecursiveRule declarationStatement; // OK
  public LeftRecursiveRule localVariableDeclaration; // OK
  public LeftRecursiveRule localVariableDeclarator; // OK - tested via LocalVariableDeclarationTest
  public LeftRecursiveRule localVariableInitializer; // OK
  public LeftRecursiveRule localConstantDeclaration; // OK
  public LeftRecursiveRule constantDeclarator; // OK
  public LeftRecursiveRule expressionStatement; // OK
  public LeftRecursiveRule statementExpression; // OK - tested via ExpressionStatementTest
  public LeftRecursiveRule selectionStatement; // NO NEED
  public LeftRecursiveRule ifStatement; // OK
  public LeftRecursiveRule switchStatement; // OK
  public LeftRecursiveRule switchSection; // OK
  public LeftRecursiveRule switchLabel; // OK
  public LeftRecursiveRule iterationStatement; // OK
  public LeftRecursiveRule whileStatement; // OK
  public LeftRecursiveRule doStatement; // OK
  public LeftRecursiveRule forStatement; // OK
  public LeftRecursiveRule forInitializer; // OK
  public LeftRecursiveRule forCondition; // NO NEED
  public LeftRecursiveRule forIterator; // NO NEED
  public LeftRecursiveRule statementExpressionList; // OK
  public LeftRecursiveRule foreachStatement; // OK
  public LeftRecursiveRule jumpStatement; // OK
  public LeftRecursiveRule breakStatement; // OK
  public LeftRecursiveRule continueStatement; // OK
  public LeftRecursiveRule gotoStatement; // OK
  public LeftRecursiveRule returnStatement; // OK
  public LeftRecursiveRule throwStatement; // OK
  public LeftRecursiveRule tryStatement; // OK
  public LeftRecursiveRule catchClauses; // OK
  public LeftRecursiveRule specificCatchClause; // OK
  public LeftRecursiveRule generalCatchClause; // OK
  public LeftRecursiveRule finallyClause; // OK
  public LeftRecursiveRule checkedStatement; // OK
  public LeftRecursiveRule uncheckedStatement; // OK
  public LeftRecursiveRule lockStatement; // OK
  public LeftRecursiveRule usingStatement; // OK
  public LeftRecursiveRule resourceAcquisition; // OK - tested via UsingStatementTest
  public LeftRecursiveRule yieldStatement; // OK
  public LeftRecursiveRule namespaceDeclaration; // OK
  public LeftRecursiveRule qualifiedIdentifier; // OK
  public LeftRecursiveRule namespaceBody; // OK
  public LeftRecursiveRule externAliasDirective; // OK
  public LeftRecursiveRule usingDirective; // OK
  public LeftRecursiveRule usingAliasDirective; // OK - tested via UsingDirectiveTest
  public LeftRecursiveRule usingNamespaceDirective; // OK - tested via UsingDirectiveTest
  public LeftRecursiveRule namespaceMemberDeclaration; // OK
  public LeftRecursiveRule typeDeclaration; // OK
  public LeftRecursiveRule qualifiedAliasMember; // OK

  // A.2.6 Classes
  public LeftRecursiveRule classDeclaration; // OK
  public LeftRecursiveRule classModifier; // OK
  public LeftRecursiveRule classBase; // OK
  public LeftRecursiveRule interfaceTypeList; // LATER
  public LeftRecursiveRule classBody; // OK
  public LeftRecursiveRule classMemberDeclaration; // LATER
  public LeftRecursiveRule constantDeclaration; // OK
  public LeftRecursiveRule constantModifier; // OK - tested via ConstantDeclarationTest
  public LeftRecursiveRule fieldDeclaration; // OK
  public LeftRecursiveRule fieldModifier; // OK - tested via FieldDeclarationTest
  public LeftRecursiveRule variableDeclarator; // OK
  public LeftRecursiveRule variableInitializer; // OK
  public LeftRecursiveRule methodDeclaration; // LATER
  public LeftRecursiveRule methodHeader; // OK
  public LeftRecursiveRule methodModifier; // OK - tested via MethodHeaderTest
  public LeftRecursiveRule returnType; // OK
  public LeftRecursiveRule memberName; // OK
  public LeftRecursiveRule methodBody; // LATER
  public LeftRecursiveRule formalParameterList; // OK
  public LeftRecursiveRule fixedParameters; // LATER
  public LeftRecursiveRule fixedParameter; // OK
  public LeftRecursiveRule parameterModifier; // OK - tested via FixedParameterTest
  public LeftRecursiveRule parameterArray; // OK
  public LeftRecursiveRule propertyDeclaration; // OK
  public LeftRecursiveRule propertyModifier; // OK - tested via PropertyDeclarationTest
  public LeftRecursiveRule accessorDeclarations; // OK
  public LeftRecursiveRule getAccessorDeclaration; // OK
  public LeftRecursiveRule setAccessorDeclaration; // OK
  public LeftRecursiveRule accessorModifier; // OK
  public LeftRecursiveRule accessorBody; // LATER
  public LeftRecursiveRule eventDeclaration; // OK
  public LeftRecursiveRule eventModifier; // OK - tested via EventDeclarationTest
  public LeftRecursiveRule eventAccessorDeclarations; // OK
  public LeftRecursiveRule addAccessorDeclaration; // OK - tested via EventAccessorDeclarationTest
  public LeftRecursiveRule removeAccessorDeclaration; // OK - tested via EventAccessorDeclarationTest
  public LeftRecursiveRule indexerDeclaration; // OK
  public LeftRecursiveRule indexerModifier; // OK - tested via IndexerDeclarationTest
  public LeftRecursiveRule indexerDeclarator; // OK
  public LeftRecursiveRule operatorDeclaration; // OK
  public LeftRecursiveRule operatorModifier; // OK - tested via OperatorDeclarationTest
  public LeftRecursiveRule operatorDeclarator; // OK
  public LeftRecursiveRule unaryOperatorDeclarator; // OK
  public LeftRecursiveRule overloadableUnaryOperator; // OK - tested via UnaryOperatorDeclaration
  public LeftRecursiveRule binaryOperatorDeclarator; // OK
  public LeftRecursiveRule overloadableBinaryOperator; // OK - tested via BinaryOperatorDeclaration
  public LeftRecursiveRule conversionOperatorDeclarator; // OK
  public LeftRecursiveRule operatorBody; // LATER
  public LeftRecursiveRule constructorDeclaration; // OK
  public LeftRecursiveRule constructorModifier; // OK - tested via ConstructorDeclarationTest
  public LeftRecursiveRule constructorDeclarator; // OK
  public LeftRecursiveRule constructorInitializer; // OK
  public LeftRecursiveRule constructorBody; // OK
  public LeftRecursiveRule staticConstructorDeclaration; // OK
  public LeftRecursiveRule staticConstructorModifiers; // OK
  public LeftRecursiveRule staticConstructorBody; // LATER
  public LeftRecursiveRule destructorDeclaration; // OK
  public LeftRecursiveRule destructorBody; // LATER

  // A.2.7 Struct
  public LeftRecursiveRule structDeclaration; // OK
  public LeftRecursiveRule structModifier; // OK - tested via StructDeclarationTest
  public LeftRecursiveRule structInterfaces; // OK
  public LeftRecursiveRule structBody; // OK
  public LeftRecursiveRule structMemberDeclaration; // OK - tested via StructBodyTest

  // A.2.8 Arrays
  public LeftRecursiveRule arrayInitializer; // OK
  public LeftRecursiveRule variableInitializerList; // OK

  // A.2.9 Interfaces
  public LeftRecursiveRule interfaceDeclaration; // OK
  public LeftRecursiveRule interfaceModifier; // OK - tested via InterfaceDeclarationTest
  public LeftRecursiveRule variantTypeParameterList; // OK
  public LeftRecursiveRule variantTypeParameter; // OK
  public LeftRecursiveRule varianceAnnotation; // OK
  public LeftRecursiveRule interfaceBase; // OK
  public LeftRecursiveRule interfaceBody; // OK
  public LeftRecursiveRule interfaceMemberDeclaration; // OK - tested via InterfaceBodyDeclaration
  public LeftRecursiveRule interfaceMethodDeclaration; // OK
  public LeftRecursiveRule interfacePropertyDeclaration; // OK
  public LeftRecursiveRule interfaceAccessors; // OK
  public LeftRecursiveRule interfaceEventDeclaration; // OK
  public LeftRecursiveRule interfaceIndexerDeclaration; // OK

  // A.2.10 Enums
  public LeftRecursiveRule enumDeclaration; // OK
  public LeftRecursiveRule enumBase; // OK
  public LeftRecursiveRule enumBody; // OK
  public LeftRecursiveRule enumModifier; // OK - tested via EnumDeclarationTest
  public LeftRecursiveRule enumMemberDeclarations; // OK
  public LeftRecursiveRule enumMemberDeclaration; // OK

  // A.2.11 Delegates
  public LeftRecursiveRule delegateDeclaration; // OK
  public LeftRecursiveRule delegateModifier; // OK - tested via DelegateDeclarationTest

  // A.2.12 Attributes
  public LeftRecursiveRule globalAttributes; // LATER
  public LeftRecursiveRule globalAttributeSection; // OK
  public LeftRecursiveRule globalAttributeTargetSpecifier; // OK
  public LeftRecursiveRule globalAttributeTarget; // OK
  public LeftRecursiveRule attributes; // LATER
  public LeftRecursiveRule attributeSection; // OK
  public LeftRecursiveRule attributeTargetSpecifier; // OK
  public LeftRecursiveRule attributeTarget; // OK
  public LeftRecursiveRule attributeList; // OK
  public LeftRecursiveRule attribute; // OK
  public LeftRecursiveRule attributeName; // LATER
  public LeftRecursiveRule attributeArguments; // OK
  public LeftRecursiveRule positionalArgument; // LATER
  public LeftRecursiveRule namedArgument; // OK
  public LeftRecursiveRule attributeArgumentExpression; // OK

  // A.2.13 Generics
  public LeftRecursiveRule typeParameterList; // OK
  public LeftRecursiveRule typeParameters; // OK
  public LeftRecursiveRule typeParameter; // OK - tested via TypeParametersTest
  public LeftRecursiveRule typeArgumentList; // OK
  public LeftRecursiveRule typeArgument; // OK - tested via TypeArgumentListTest
  public LeftRecursiveRule typeParameterConstraintsClauses; // LATER
  public LeftRecursiveRule typeParameterConstraintsClause; // OK
  public LeftRecursiveRule typeParameterConstraints; // OK
  public LeftRecursiveRule primaryConstraint; // OK
  public LeftRecursiveRule secondaryConstraints; // OK
  public LeftRecursiveRule constructorConstraint; // OK

  /** Grammar extensions **/
  public CSharpUnsafeExtensionGrammar unsafe;

  /**
   * ${@inheritDoc}
   */
  public Rule getRootRule() {
    return compilationUnit;
  }
}
