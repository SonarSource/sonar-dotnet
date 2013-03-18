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
package com.sonar.csharp.squid.parser;

import org.sonar.sslr.grammar.GrammarRuleKey;
import org.sonar.sslr.grammar.LexerfulGrammarBuilder;

import static com.sonar.csharp.squid.api.CSharpKeyword.ABSTRACT;
import static com.sonar.csharp.squid.api.CSharpKeyword.AS;
import static com.sonar.csharp.squid.api.CSharpKeyword.BASE;
import static com.sonar.csharp.squid.api.CSharpKeyword.BOOL;
import static com.sonar.csharp.squid.api.CSharpKeyword.BREAK;
import static com.sonar.csharp.squid.api.CSharpKeyword.BYTE;
import static com.sonar.csharp.squid.api.CSharpKeyword.CASE;
import static com.sonar.csharp.squid.api.CSharpKeyword.CATCH;
import static com.sonar.csharp.squid.api.CSharpKeyword.CHAR;
import static com.sonar.csharp.squid.api.CSharpKeyword.CHECKED;
import static com.sonar.csharp.squid.api.CSharpKeyword.CLASS;
import static com.sonar.csharp.squid.api.CSharpKeyword.CONST;
import static com.sonar.csharp.squid.api.CSharpKeyword.CONTINUE;
import static com.sonar.csharp.squid.api.CSharpKeyword.DECIMAL;
import static com.sonar.csharp.squid.api.CSharpKeyword.DEFAULT;
import static com.sonar.csharp.squid.api.CSharpKeyword.DELEGATE;
import static com.sonar.csharp.squid.api.CSharpKeyword.DO;
import static com.sonar.csharp.squid.api.CSharpKeyword.DOUBLE;
import static com.sonar.csharp.squid.api.CSharpKeyword.ELSE;
import static com.sonar.csharp.squid.api.CSharpKeyword.ENUM;
import static com.sonar.csharp.squid.api.CSharpKeyword.EVENT;
import static com.sonar.csharp.squid.api.CSharpKeyword.EXPLICIT;
import static com.sonar.csharp.squid.api.CSharpKeyword.EXTERN;
import static com.sonar.csharp.squid.api.CSharpKeyword.FALSE;
import static com.sonar.csharp.squid.api.CSharpKeyword.FINALLY;
import static com.sonar.csharp.squid.api.CSharpKeyword.FIXED;
import static com.sonar.csharp.squid.api.CSharpKeyword.FLOAT;
import static com.sonar.csharp.squid.api.CSharpKeyword.FOR;
import static com.sonar.csharp.squid.api.CSharpKeyword.FOREACH;
import static com.sonar.csharp.squid.api.CSharpKeyword.GOTO;
import static com.sonar.csharp.squid.api.CSharpKeyword.IF;
import static com.sonar.csharp.squid.api.CSharpKeyword.IMPLICIT;
import static com.sonar.csharp.squid.api.CSharpKeyword.IN;
import static com.sonar.csharp.squid.api.CSharpKeyword.INT;
import static com.sonar.csharp.squid.api.CSharpKeyword.INTERFACE;
import static com.sonar.csharp.squid.api.CSharpKeyword.INTERNAL;
import static com.sonar.csharp.squid.api.CSharpKeyword.IS;
import static com.sonar.csharp.squid.api.CSharpKeyword.LOCK;
import static com.sonar.csharp.squid.api.CSharpKeyword.LONG;
import static com.sonar.csharp.squid.api.CSharpKeyword.NAMESPACE;
import static com.sonar.csharp.squid.api.CSharpKeyword.NEW;
import static com.sonar.csharp.squid.api.CSharpKeyword.NULL;
import static com.sonar.csharp.squid.api.CSharpKeyword.OBJECT;
import static com.sonar.csharp.squid.api.CSharpKeyword.OPERATOR;
import static com.sonar.csharp.squid.api.CSharpKeyword.OUT;
import static com.sonar.csharp.squid.api.CSharpKeyword.OVERRIDE;
import static com.sonar.csharp.squid.api.CSharpKeyword.PARAMS;
import static com.sonar.csharp.squid.api.CSharpKeyword.PRIVATE;
import static com.sonar.csharp.squid.api.CSharpKeyword.PROTECTED;
import static com.sonar.csharp.squid.api.CSharpKeyword.PUBLIC;
import static com.sonar.csharp.squid.api.CSharpKeyword.READONLY;
import static com.sonar.csharp.squid.api.CSharpKeyword.REF;
import static com.sonar.csharp.squid.api.CSharpKeyword.RETURN;
import static com.sonar.csharp.squid.api.CSharpKeyword.SBYTE;
import static com.sonar.csharp.squid.api.CSharpKeyword.SEALED;
import static com.sonar.csharp.squid.api.CSharpKeyword.SHORT;
import static com.sonar.csharp.squid.api.CSharpKeyword.SIZEOF;
import static com.sonar.csharp.squid.api.CSharpKeyword.STACKALLOC;
import static com.sonar.csharp.squid.api.CSharpKeyword.STATIC;
import static com.sonar.csharp.squid.api.CSharpKeyword.STRING;
import static com.sonar.csharp.squid.api.CSharpKeyword.STRUCT;
import static com.sonar.csharp.squid.api.CSharpKeyword.SWITCH;
import static com.sonar.csharp.squid.api.CSharpKeyword.THIS;
import static com.sonar.csharp.squid.api.CSharpKeyword.THROW;
import static com.sonar.csharp.squid.api.CSharpKeyword.TRUE;
import static com.sonar.csharp.squid.api.CSharpKeyword.TRY;
import static com.sonar.csharp.squid.api.CSharpKeyword.TYPEOF;
import static com.sonar.csharp.squid.api.CSharpKeyword.UINT;
import static com.sonar.csharp.squid.api.CSharpKeyword.ULONG;
import static com.sonar.csharp.squid.api.CSharpKeyword.UNCHECKED;
import static com.sonar.csharp.squid.api.CSharpKeyword.UNSAFE;
import static com.sonar.csharp.squid.api.CSharpKeyword.USHORT;
import static com.sonar.csharp.squid.api.CSharpKeyword.USING;
import static com.sonar.csharp.squid.api.CSharpKeyword.VIRTUAL;
import static com.sonar.csharp.squid.api.CSharpKeyword.VOID;
import static com.sonar.csharp.squid.api.CSharpKeyword.VOLATILE;
import static com.sonar.csharp.squid.api.CSharpKeyword.WHILE;
import static com.sonar.csharp.squid.api.CSharpPunctuator.ADD_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.AND;
import static com.sonar.csharp.squid.api.CSharpPunctuator.AND_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.AND_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.COLON;
import static com.sonar.csharp.squid.api.CSharpPunctuator.COMMA;
import static com.sonar.csharp.squid.api.CSharpPunctuator.DEC_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.DIV_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.DOT;
import static com.sonar.csharp.squid.api.CSharpPunctuator.DOUBLE_COLON;
import static com.sonar.csharp.squid.api.CSharpPunctuator.DOUBLE_QUESTION;
import static com.sonar.csharp.squid.api.CSharpPunctuator.EQUAL;
import static com.sonar.csharp.squid.api.CSharpPunctuator.EQ_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.EXCLAMATION;
import static com.sonar.csharp.squid.api.CSharpPunctuator.GE_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.INC_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.INFERIOR;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LAMBDA;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LBRACKET;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LCURLYBRACE;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LEFT_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LEFT_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LE_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.LPARENTHESIS;
import static com.sonar.csharp.squid.api.CSharpPunctuator.MINUS;
import static com.sonar.csharp.squid.api.CSharpPunctuator.MODULO;
import static com.sonar.csharp.squid.api.CSharpPunctuator.MOD_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.MUL_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.NE_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.OR;
import static com.sonar.csharp.squid.api.CSharpPunctuator.OR_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.OR_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.PLUS;
import static com.sonar.csharp.squid.api.CSharpPunctuator.PTR_OP;
import static com.sonar.csharp.squid.api.CSharpPunctuator.QUESTION;
import static com.sonar.csharp.squid.api.CSharpPunctuator.RBRACKET;
import static com.sonar.csharp.squid.api.CSharpPunctuator.RCURLYBRACE;
import static com.sonar.csharp.squid.api.CSharpPunctuator.RPARENTHESIS;
import static com.sonar.csharp.squid.api.CSharpPunctuator.SEMICOLON;
import static com.sonar.csharp.squid.api.CSharpPunctuator.SLASH;
import static com.sonar.csharp.squid.api.CSharpPunctuator.STAR;
import static com.sonar.csharp.squid.api.CSharpPunctuator.SUB_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpPunctuator.SUPERIOR;
import static com.sonar.csharp.squid.api.CSharpPunctuator.TILDE;
import static com.sonar.csharp.squid.api.CSharpPunctuator.XOR;
import static com.sonar.csharp.squid.api.CSharpPunctuator.XOR_ASSIGN;
import static com.sonar.csharp.squid.api.CSharpTokenType.CHARACTER_LITERAL;
import static com.sonar.csharp.squid.api.CSharpTokenType.INTEGER_DEC_LITERAL;
import static com.sonar.csharp.squid.api.CSharpTokenType.INTEGER_HEX_LITERAL;
import static com.sonar.csharp.squid.api.CSharpTokenType.REAL_LITERAL;
import static com.sonar.csharp.squid.api.CSharpTokenType.STRING_LITERAL;
import static com.sonar.sslr.api.GenericTokenType.EOF;
import static com.sonar.sslr.api.GenericTokenType.IDENTIFIER;

/**
 * Definition of each element of the C# grammar, based on the C# language specification 4.0
 */
public enum CSharpGrammarImpl implements GrammarRuleKey {

  literal,
  rightShift,
  rightShiftAssignment,

  // A.2.1 Basic concepts
  compilationUnit,
  namespaceName,
  typeName,
  namespaceOrTypeName,

  // A.2.2 Types
  simpleType,
  numericType,
  integralType,
  floatingPointType,

  rankSpecifier,
  rankSpecifiers,

  typePrimary,
  nullableType,
  pointerType,
  arrayType,
  type,

  nonArrayType,
  nonNullableValueType,

  classType,
  interfaceType,
  enumType,
  delegateType,

  // A.2.3 Variables
  variableReference,

  // A.2.4 Expressions
  primaryExpressionPrimary,
  primaryNoArrayCreationExpression,
  postElementAccess,
  postMemberAccess,

  postInvocation,
  postIncrement,
  postDecrement,
  postPointerMemberAccess,

  postfixExpression,
  primaryExpression,

  argumentList,
  argument,
  argumentName,
  argumentValue,
  simpleName,
  parenthesizedExpression,
  memberAccess,
  predefinedType,
  thisAccess,
  baseAccess,
  objectCreationExpression,
  objectOrCollectionInitializer,
  objectInitializer,
  memberInitializer,
  initializerValue,
  collectionInitializer,
  elementInitializer,
  expressionList,
  arrayCreationExpression,
  delegateCreationExpression,
  anonymousObjectCreationExpression,
  anonymousObjectInitializer,
  memberDeclarator,
  typeOfExpression,
  unboundTypeName,
  genericDimensionSpecifier,
  checkedExpression,
  uncheckedExpression,
  defaultValueExpression,
  unaryExpression,
  multiplicativeExpression,
  additiveExpression,
  shiftExpression,
  relationalExpression,
  equalityExpression,
  andExpression,
  exclusiveOrExpression,
  inclusiveOrExpression,
  conditionalAndExpression,
  conditionalOrExpression,
  nullCoalescingExpression,
  conditionalExpression,
  lambdaExpression,
  anonymousMethodExpression,
  anonymousFunctionSignature,
  explicitAnonymousFunctionSignature,
  explicitAnonymousFunctionParameter,
  anonymousFunctionParameterModifier,
  implicitAnonymousFunctionSignature,
  implicitAnonymousFunctionParameter,
  anonymousFunctionBody,
  queryExpression,
  fromClause,
  queryBody,
  queryBodyClause,
  letClause,
  whereClause,
  joinClause,
  joinIntoClause,
  orderByClause,
  ordering,
  orderingDirection,
  selectOrGroupClause,
  selectClause,
  groupClause,
  queryContinuation,
  assignment,
  expression,
  nonAssignmentExpression,

  // A.2.5 Statement
  statement,
  embeddedStatement,
  block,
  labeledStatement,
  declarationStatement,
  localVariableDeclaration,
  localVariableDeclarator,
  localVariableInitializer,
  localConstantDeclaration,
  constantDeclarator,
  expressionStatement,
  selectionStatement,
  ifStatement,
  switchStatement,
  switchSection,
  switchLabel,
  iterationStatement,
  whileStatement,
  doStatement,
  forStatement,
  forInitializer,
  forCondition,
  forIterator,
  statementExpressionList,
  foreachStatement,
  jumpStatement,
  breakStatement,
  continueStatement,
  gotoStatement,
  returnStatement,
  throwStatement,
  tryStatement,
  catchClauses,
  specificCatchClause,
  generalCatchClause,
  finallyClause,
  checkedStatement,
  uncheckedStatement,
  lockStatement,
  usingStatement,
  resourceAcquisition,
  yieldStatement,
  namespaceDeclaration,
  qualifiedIdentifier,
  namespaceBody,
  externAliasDirective,
  usingDirective,
  usingAliasDirective,
  usingNamespaceDirective,
  namespaceMemberDeclaration,
  typeDeclaration,
  qualifiedAliasMember,

  // A.2.6 Classes
  classDeclaration,
  classModifier,
  classBase,
  interfaceTypeList,
  classBody,
  classMemberDeclaration,
  constantDeclaration,
  constantModifier,
  fieldDeclaration,
  fieldModifier,
  variableDeclarator,
  variableInitializer,
  methodDeclaration,
  methodHeader,
  methodModifier,
  returnType,
  memberName,
  methodBody,
  formalParameterList,
  fixedParameters,
  fixedParameter,
  parameterModifier,
  parameterArray,
  propertyDeclaration,
  propertyModifier,
  accessorDeclarations,
  getAccessorDeclaration,
  setAccessorDeclaration,
  accessorModifier,
  accessorBody,
  eventDeclaration,
  eventModifier,
  eventAccessorDeclarations,
  addAccessorDeclaration,
  removeAccessorDeclaration,
  indexerDeclaration,
  indexerModifier,
  indexerDeclarator,
  operatorDeclaration,
  operatorModifier,
  operatorDeclarator,
  unaryOperatorDeclarator,
  overloadableUnaryOperator,
  binaryOperatorDeclarator,
  overloadableBinaryOperator,
  conversionOperatorDeclarator,
  operatorBody,
  constructorDeclaration,
  constructorModifier,
  constructorDeclarator,
  constructorInitializer,
  constructorBody,
  staticConstructorDeclaration,
  staticConstructorModifiers,
  staticConstructorBody,
  destructorDeclaration,
  destructorBody,

  // A.2.7 Struct
  structDeclaration,
  structModifier,
  structInterfaces,
  structBody,
  structMemberDeclaration,

  // A.2.8 Arrays
  arrayInitializer,
  variableInitializerList,

  // A.2.9 Interfaces
  interfaceDeclaration,
  interfaceModifier,
  variantTypeParameterList,
  variantTypeParameter,
  varianceAnnotation,
  interfaceBase,
  interfaceBody,
  interfaceMemberDeclaration,
  interfaceMethodDeclaration,
  interfacePropertyDeclaration,
  interfaceAccessors,
  interfaceEventDeclaration,
  interfaceIndexerDeclaration,

  // A.2.10 Enums
  enumDeclaration,
  enumBase,
  enumBody,
  enumModifier,
  enumMemberDeclarations,
  enumMemberDeclaration,

  // A.2.11 Delegates
  delegateDeclaration,
  delegateModifier,

  // A.2.12 Attributes
  globalAttributes,
  globalAttributeSection,
  globalAttributeTargetSpecifier,
  globalAttributeTarget,
  attributes,
  attributeSection,
  attributeTargetSpecifier,
  attributeTarget,
  attributeList,
  attribute,
  attributeName,
  attributeArguments,
  positionalArgument,
  namedArgument,
  attributeArgumentExpression,

  // A.2.13 Generics
  typeParameterList,
  typeParameters,
  typeParameter,
  typeArgumentList,
  typeArgument,
  typeParameterConstraintsClauses,
  typeParameterConstraintsClause,
  typeParameterConstraints,
  primaryConstraint,
  secondaryConstraints,
  constructorConstraint,

  // A.3 Unsafe code
  unsafeStatement,
  pointerIndirectionExpression,
  pointerElementAccess,
  addressOfExpression,
  sizeOfExpression,
  fixedStatement,
  fixedPointerDeclarator,
  fixedPointerInitializer,
  fixedSizeBufferDeclaration,
  fixedSizeBufferModifier,
  fixedSizeBufferDeclarator,
  stackallocInitializer;

  private static final String SET = "set";
  private static final String GET = "get";
  private static final String PARTIAL = "partial";

  public static LexerfulGrammarBuilder create() {
    LexerfulGrammarBuilder b = LexerfulGrammarBuilder.create();

    b.rule(literal).is(
        b.firstOf(
            TRUE,
            FALSE,
            INTEGER_DEC_LITERAL,
            INTEGER_HEX_LITERAL,
            REAL_LITERAL,
            CHARACTER_LITERAL,
            STRING_LITERAL,
            NULL));
    b.rule(rightShift).is(SUPERIOR, SUPERIOR);
    b.rule(rightShiftAssignment).is(SUPERIOR, GE_OP);

    // A.2.1 Basic concepts
    basicConcepts(b);

    // A.2.2 Types
    types(b);

    // A.2.3 Variables
    variables(b);

    // A.2.4 Expressions
    expressions(b);

    // A.2.5 Statements
    statements(b);

    // A.2.6 Classes
    classes(b);

    // A.2.7 Struct
    structs(b);

    // A.2.8 Arrays
    arrays(b);

    // A.2.9 Interfaces
    interfaces(b);

    // A.2.10 Enums
    enums(b);

    // A.2.11 Delegates
    delegates(b);

    // A.2.12 Attributes
    attributes(b);

    // A.2.13 Generics
    generics(b);

    // A.3 Unsafe code
    unsafe(b);

    b.setRootRule(compilationUnit);

    return b;
  }

  private static void basicConcepts(LexerfulGrammarBuilder b) {
    b.rule(compilationUnit).is(b.zeroOrMore(externAliasDirective), b.zeroOrMore(usingDirective), b.optional(globalAttributes), b.zeroOrMore(namespaceMemberDeclaration), EOF);
    b.rule(namespaceName).is(namespaceOrTypeName);
    b.rule(typeName).is(namespaceOrTypeName);
    b.rule(namespaceOrTypeName).is(
        b.firstOf(
            qualifiedAliasMember,
            b.sequence(IDENTIFIER, b.optional(typeArgumentList))),
        b.zeroOrMore(DOT, IDENTIFIER, b.optional(typeArgumentList)));
  }

  private static void types(LexerfulGrammarBuilder b) {
    b.rule(simpleType).is(
        b.firstOf(
            numericType,
            BOOL));
    b.rule(numericType).is(
        b.firstOf(
            integralType,
            floatingPointType,
            DECIMAL));
    b.rule(integralType).is(
        b.firstOf(
            SBYTE,
            BYTE,
            SHORT,
            USHORT,
            INT,
            UINT,
            LONG,
            ULONG,
            CHAR));
    b.rule(floatingPointType).is(
        b.firstOf(
            FLOAT,
            DOUBLE));

    b.rule(rankSpecifier).is(LBRACKET, b.zeroOrMore(COMMA), RBRACKET);
    b.rule(rankSpecifiers).is(b.oneOrMore(rankSpecifier));

    b.rule(typePrimary).is(
        b.firstOf(
            simpleType,
            "dynamic",
            OBJECT,
            STRING,
            typeName)).skip();
    b.rule(nullableType).is(typePrimary, QUESTION, b.nextNot(b.sequence(expression, COLON)));
    b.rule(pointerType).is(
        b.firstOf(
            nullableType,
            typePrimary,
            VOID),
        STAR);
    b.rule(arrayType).is(
        b.firstOf(
            pointerType,
            nullableType,
            typePrimary),
        rankSpecifiers);
    b.rule(type).is(
        b.firstOf(
            arrayType,
            pointerType,
            nullableType,
            typePrimary));

    b.rule(nonNullableValueType).is(b.nextNot(nullableType), type);
    b.rule(nonArrayType).is(b.nextNot(arrayType), type);

    b.rule(classType).is(
        b.firstOf(
            "dynamic",
            OBJECT,
            STRING,
            typeName));
    b.rule(interfaceType).is(typeName);
    b.rule(enumType).is(typeName);
    b.rule(delegateType).is(typeName);
  }

  private static void variables(LexerfulGrammarBuilder b) {
    b.rule(variableReference).is(expression);
  }

  private static void expressions(LexerfulGrammarBuilder b) {
    b.rule(primaryExpressionPrimary).is(
        b.firstOf(
            arrayCreationExpression,
            primaryNoArrayCreationExpression)).skip();
    b.rule(primaryNoArrayCreationExpression).is(
        b.firstOf(
            parenthesizedExpression,
            memberAccess,
            thisAccess,
            baseAccess,
            objectCreationExpression,
            delegateCreationExpression,
            anonymousObjectCreationExpression,
            typeOfExpression,
            checkedExpression,
            uncheckedExpression,
            defaultValueExpression,
            anonymousMethodExpression,
            literal,
            simpleName,
            unsafe(sizeOfExpression))).skip();

    b.rule(postMemberAccess).is(DOT, IDENTIFIER, b.optional(typeArgumentList));
    b.rule(postElementAccess).is(LBRACKET, argumentList, RBRACKET);
    b.rule(postPointerMemberAccess).is(PTR_OP, IDENTIFIER);
    b.rule(postIncrement).is(INC_OP);
    b.rule(postDecrement).is(DEC_OP);
    b.rule(postInvocation).is(LPARENTHESIS, b.optional(argumentList), RPARENTHESIS);

    b.rule(postfixExpression).is(
        primaryExpressionPrimary,
        b.zeroOrMore(
            b.firstOf(
                postMemberAccess,
                postElementAccess,
                postPointerMemberAccess,
                postIncrement,
                postDecrement,
                postInvocation))).skipIfOneChild();
    b.rule(primaryExpression).is(postfixExpression);

    b.rule(argumentList).is(argument, b.zeroOrMore(COMMA, argument));
    b.rule(argument).is(b.optional(argumentName), argumentValue);
    b.rule(argumentName).is(IDENTIFIER, COLON);
    b.rule(argumentValue).is(
        b.firstOf(
            expression,
            b.sequence(REF, variableReference),
            b.sequence(OUT, variableReference)));
    b.rule(simpleName).is(IDENTIFIER, b.optional(typeArgumentList));
    b.rule(parenthesizedExpression).is(LPARENTHESIS, expression, RPARENTHESIS);
    b.rule(memberAccess).is(
        b.firstOf(
            b.sequence(qualifiedAliasMember, DOT, IDENTIFIER),
            b.sequence(predefinedType, DOT, IDENTIFIER, b.optional(typeArgumentList))));
    b.rule(predefinedType).is(
        b.firstOf(
            BOOL,
            BYTE,
            CHAR,
            DECIMAL,
            DOUBLE,
            FLOAT,
            INT,
            LONG,
            OBJECT,
            SBYTE,
            SHORT,
            STRING,
            UINT,
            ULONG,
            USHORT));
    b.rule(thisAccess).is(THIS);
    b.rule(baseAccess).is(
        BASE,
        b.firstOf(
            b.sequence(DOT, IDENTIFIER, b.optional(typeArgumentList)),
            b.sequence(LBRACKET, argumentList, RBRACKET)));
    b.rule(objectCreationExpression).is(
        NEW, type,
        b.firstOf(
            b.sequence(LPARENTHESIS, b.optional(argumentList), RPARENTHESIS, b.optional(objectOrCollectionInitializer)),
            objectOrCollectionInitializer));
    b.rule(objectOrCollectionInitializer).is(
        b.firstOf(
            objectInitializer,
            collectionInitializer));
    b.rule(objectInitializer).is(LCURLYBRACE, b.optional(memberInitializer), b.zeroOrMore(COMMA, memberInitializer), b.optional(COMMA), RCURLYBRACE);
    b.rule(memberInitializer).is(IDENTIFIER, EQUAL, initializerValue);
    b.rule(initializerValue).is(
        b.firstOf(
            expression,
            objectOrCollectionInitializer));
    b.rule(collectionInitializer).is(LCURLYBRACE, elementInitializer, b.zeroOrMore(COMMA, elementInitializer), b.optional(COMMA), RCURLYBRACE);
    b.rule(elementInitializer).is(
        b.firstOf(
            nonAssignmentExpression,
            b.sequence(LCURLYBRACE, expressionList, RCURLYBRACE)));
    b.rule(expressionList).is(expression, b.zeroOrMore(COMMA, expression));
    b.rule(arrayCreationExpression).is(
        b.firstOf(
            b.sequence(NEW, nonArrayType, LBRACKET, expressionList, RBRACKET, b.zeroOrMore(rankSpecifier), b.optional(arrayInitializer)),
            b.sequence(NEW, arrayType, arrayInitializer), b.sequence(NEW, rankSpecifier, arrayInitializer)));
    b.rule(delegateCreationExpression).is(NEW, delegateType, LPARENTHESIS, expression, RPARENTHESIS);
    b.rule(anonymousObjectCreationExpression).is(NEW, anonymousObjectInitializer);
    b.rule(anonymousObjectInitializer).is(LCURLYBRACE, b.optional(memberDeclarator), b.zeroOrMore(COMMA, memberDeclarator), b.optional(COMMA), RCURLYBRACE);
    b.rule(memberDeclarator).is(b.optional(IDENTIFIER, EQUAL), expression);
    b.rule(typeOfExpression).is(TYPEOF, b.bridge(LPARENTHESIS, RPARENTHESIS));
    b.rule(unboundTypeName).is(
        b.oneOrMore(
            IDENTIFIER, b.optional(DOUBLE_COLON, IDENTIFIER), b.optional(genericDimensionSpecifier),
            b.optional(DOT, IDENTIFIER, b.optional(genericDimensionSpecifier))), b.optional(DOT, IDENTIFIER, b.optional(genericDimensionSpecifier)));
    b.rule(genericDimensionSpecifier).is(INFERIOR, b.zeroOrMore(COMMA), SUPERIOR);
    b.rule(checkedExpression).is(CHECKED, LPARENTHESIS, expression, RPARENTHESIS);
    b.rule(uncheckedExpression).is(UNCHECKED, LPARENTHESIS, expression, RPARENTHESIS);
    b.rule(defaultValueExpression).is(DEFAULT, LPARENTHESIS, type, RPARENTHESIS);

    b.rule(unaryExpression).is(
        b.firstOf(
            b.sequence(LPARENTHESIS, type, RPARENTHESIS, unaryExpression),
            primaryExpression,
            b.sequence(
                b.firstOf(
                    MINUS,
                    EXCLAMATION,
                    INC_OP,
                    DEC_OP,
                    TILDE,
                    PLUS),
                unaryExpression),
            unsafe(
            b.firstOf(
                pointerIndirectionExpression,
                addressOfExpression)))).skipIfOneChild();

    b.rule(multiplicativeExpression).is(
        unaryExpression,
        b.zeroOrMore(
            b.firstOf(
                STAR,
                SLASH,
                MODULO),
            unaryExpression)).skipIfOneChild();
    b.rule(additiveExpression).is(
        multiplicativeExpression,
        b.zeroOrMore(
            b.firstOf(
                PLUS,
                MINUS),
            multiplicativeExpression)).skipIfOneChild();
    b.rule(shiftExpression).is(
        additiveExpression,
        b.zeroOrMore(
            b.firstOf(
                LEFT_OP,
                rightShift),
            additiveExpression)).skipIfOneChild();
    b.rule(relationalExpression).is(
        shiftExpression,
        b.zeroOrMore(
            b.firstOf(
                b.sequence(
                    b.firstOf(
                        INFERIOR,
                        SUPERIOR,
                        LE_OP,
                        GE_OP),
                    shiftExpression),
                b.sequence(
                    b.firstOf(
                        IS,
                        AS),
                    type)))).skipIfOneChild();
    b.rule(equalityExpression).is(
        relationalExpression,
        b.zeroOrMore(
            b.firstOf(
                EQ_OP,
                NE_OP),
            relationalExpression)).skipIfOneChild();
    b.rule(andExpression).is(equalityExpression, b.zeroOrMore(AND, equalityExpression)).skipIfOneChild();
    b.rule(exclusiveOrExpression).is(andExpression, b.zeroOrMore(XOR, andExpression)).skipIfOneChild();
    b.rule(inclusiveOrExpression).is(exclusiveOrExpression, b.zeroOrMore(OR, exclusiveOrExpression)).skipIfOneChild();
    b.rule(conditionalAndExpression).is(inclusiveOrExpression, b.zeroOrMore(AND_OP, inclusiveOrExpression)).skipIfOneChild();
    b.rule(conditionalOrExpression).is(conditionalAndExpression, b.zeroOrMore(OR_OP, conditionalAndExpression)).skipIfOneChild();
    b.rule(nullCoalescingExpression).is(conditionalOrExpression, b.optional(DOUBLE_QUESTION, nullCoalescingExpression)).skipIfOneChild();
    b.rule(conditionalExpression).is(nullCoalescingExpression, b.optional(QUESTION, expression, COLON, expression)).skipIfOneChild();
    b.rule(lambdaExpression).is(anonymousFunctionSignature, LAMBDA, anonymousFunctionBody);
    b.rule(anonymousMethodExpression).is(DELEGATE, b.optional(explicitAnonymousFunctionSignature), block);
    b.rule(anonymousFunctionSignature).is(
        b.firstOf(
            explicitAnonymousFunctionSignature,
            implicitAnonymousFunctionSignature));
    b.rule(explicitAnonymousFunctionSignature).is(LPARENTHESIS, b.optional(explicitAnonymousFunctionParameter, b.zeroOrMore(COMMA, explicitAnonymousFunctionParameter)),
        RPARENTHESIS);
    b.rule(explicitAnonymousFunctionParameter).is(b.optional(anonymousFunctionParameterModifier), type, IDENTIFIER);
    b.rule(anonymousFunctionParameterModifier).is(
        b.firstOf(
            "ref",
            "out"));
    b.rule(implicitAnonymousFunctionSignature).is(
        b.firstOf(
            implicitAnonymousFunctionParameter,
            b.sequence(LPARENTHESIS, b.optional(implicitAnonymousFunctionParameter, b.zeroOrMore(COMMA, implicitAnonymousFunctionParameter)), RPARENTHESIS)));
    b.rule(implicitAnonymousFunctionParameter).is(IDENTIFIER);
    b.rule(anonymousFunctionBody).is(
        b.firstOf(
            expression,
            block));
    b.rule(queryExpression).is(fromClause, queryBody);
    b.rule(fromClause).is(
        "from",
        b.firstOf(
            b.sequence(type, IDENTIFIER),
            IDENTIFIER),
        IN, expression);
    b.rule(queryBody).is(b.zeroOrMore(queryBodyClause), selectOrGroupClause, b.optional(queryContinuation));
    b.rule(queryBodyClause).is(
        b.firstOf(
            fromClause,
            letClause,
            whereClause,
            joinIntoClause,
            joinClause,
            orderByClause));
    b.rule(letClause).is("let", IDENTIFIER, EQUAL, expression);
    b.rule(whereClause).is("where", expression);
    b.rule(joinClause).is(
        "join",
        b.firstOf(
            b.sequence(type, IDENTIFIER),
            IDENTIFIER),
        IN, expression, "on", expression, "equals", expression);
    b.rule(joinIntoClause).is(
        "join",
        b.firstOf(
            b.sequence(type, IDENTIFIER),
            IDENTIFIER),
        IN, expression, "on", expression, "equals", expression,
        "into", IDENTIFIER);
    b.rule(orderByClause).is("orderby", ordering, b.zeroOrMore(COMMA, ordering));
    b.rule(ordering).is(expression, b.optional(orderingDirection));
    b.rule(orderingDirection).is(
        b.firstOf(
            "ascending",
            "descending"));
    b.rule(selectOrGroupClause).is(
        b.firstOf(
            selectClause,
            groupClause));
    b.rule(selectClause).is("select", expression);
    b.rule(groupClause).is("group", expression, "by", expression);
    b.rule(queryContinuation).is("into", IDENTIFIER, queryBody);
    b.rule(assignment).is(
        unaryExpression,
        b.firstOf(
            EQUAL,
            ADD_ASSIGN,
            SUB_ASSIGN,
            MUL_ASSIGN,
            DIV_ASSIGN,
            MOD_ASSIGN,
            AND_ASSIGN,
            OR_ASSIGN,
            XOR_ASSIGN,
            LEFT_ASSIGN,
            rightShiftAssignment),
        expression);
    b.rule(nonAssignmentExpression).is(
        b.firstOf(
            lambdaExpression,
            queryExpression,
            conditionalExpression)).skip();
    b.rule(expression).is(
        b.firstOf(
            assignment,
            nonAssignmentExpression));
  }

  private static void statements(LexerfulGrammarBuilder b) {
    b.rule(statement).is(
        b.firstOf(
            labeledStatement,
            declarationStatement,
            embeddedStatement));
    b.rule(embeddedStatement).is(
        b.firstOf(
            block,
            SEMICOLON,
            expressionStatement,
            selectionStatement,
            iterationStatement,
            jumpStatement,
            tryStatement,
            checkedStatement,
            uncheckedStatement,
            lockStatement,
            usingStatement,
            yieldStatement));
    b.rule(block).is(LCURLYBRACE, b.zeroOrMore(statement), RCURLYBRACE);
    b.rule(labeledStatement).is(IDENTIFIER, COLON, statement);
    b.rule(declarationStatement).is(
        b.firstOf(
            localVariableDeclaration,
            localConstantDeclaration),
        SEMICOLON);
    b.rule(localVariableDeclaration).is(type, localVariableDeclarator, b.zeroOrMore(COMMA, localVariableDeclarator));
    b.rule(localVariableDeclarator).is(IDENTIFIER, b.optional(EQUAL, localVariableInitializer));
    b.rule(localVariableInitializer).is(
        b.firstOf(
            expression,
            arrayInitializer,
            unsafe(stackallocInitializer)));
    b.rule(localConstantDeclaration).is(CONST, type, constantDeclarator, b.zeroOrMore(COMMA, constantDeclarator));
    b.rule(constantDeclarator).is(IDENTIFIER, EQUAL, expression);
    b.rule(expressionStatement).is(expression, SEMICOLON);
    b.rule(selectionStatement).is(
        b.firstOf(
            ifStatement,
            switchStatement));
    b.rule(ifStatement).is(IF, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement, b.optional(ELSE, embeddedStatement));
    b.rule(switchStatement).is(SWITCH, LPARENTHESIS, expression, RPARENTHESIS, LCURLYBRACE, b.zeroOrMore(switchSection), RCURLYBRACE);
    b.rule(switchSection).is(b.oneOrMore(switchLabel), b.oneOrMore(statement));
    b.rule(switchLabel).is(
        b.firstOf(
            b.sequence(CASE, expression, COLON),
            b.sequence(DEFAULT, COLON)));
    b.rule(iterationStatement).is(
        b.firstOf(
            whileStatement,
            doStatement,
            forStatement,
            foreachStatement));
    b.rule(whileStatement).is(WHILE, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement);
    b.rule(doStatement).is(DO, embeddedStatement, WHILE, LPARENTHESIS, expression, RPARENTHESIS, SEMICOLON);
    b.rule(forStatement)
        .is(FOR, LPARENTHESIS, b.optional(forInitializer), SEMICOLON, b.optional(forCondition), SEMICOLON, b.optional(forIterator), RPARENTHESIS, embeddedStatement);
    b.rule(forInitializer).is(
        b.firstOf(
            localVariableDeclaration,
            statementExpressionList));
    b.rule(forCondition).is(expression);
    b.rule(forIterator).is(statementExpressionList);
    b.rule(statementExpressionList).is(expression, b.zeroOrMore(COMMA, expression));
    b.rule(foreachStatement).is(FOREACH, LPARENTHESIS, type, IDENTIFIER, IN, expression, RPARENTHESIS, embeddedStatement);
    b.rule(jumpStatement).is(
        b.firstOf(
            breakStatement,
            continueStatement,
            gotoStatement,
            returnStatement,
            throwStatement));
    b.rule(breakStatement).is(BREAK, SEMICOLON);
    b.rule(continueStatement).is(CONTINUE, SEMICOLON);
    b.rule(gotoStatement).is(
        GOTO,
        b.firstOf(
            IDENTIFIER,
            b.sequence(CASE, expression),
            DEFAULT),
        SEMICOLON);
    b.rule(returnStatement).is(RETURN, b.optional(expression), SEMICOLON);
    b.rule(throwStatement).is(THROW, b.optional(expression), SEMICOLON);
    b.rule(tryStatement).is(
        TRY, block,
        b.firstOf(
            b.sequence(b.optional(catchClauses), finallyClause),
            catchClauses));
    b.rule(catchClauses).is(
        b.firstOf(
            b.sequence(b.zeroOrMore(specificCatchClause), generalCatchClause),
            b.oneOrMore(specificCatchClause)));
    b.rule(specificCatchClause).is(CATCH, LPARENTHESIS, classType, b.optional(IDENTIFIER), RPARENTHESIS, block);
    b.rule(generalCatchClause).is(CATCH, block);
    b.rule(finallyClause).is(FINALLY, block);
    b.rule(checkedStatement).is(CHECKED, block);
    b.rule(uncheckedStatement).is(UNCHECKED, block);
    b.rule(lockStatement).is(LOCK, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement);
    b.rule(usingStatement).is(USING, LPARENTHESIS, resourceAcquisition, RPARENTHESIS, embeddedStatement);
    b.rule(resourceAcquisition).is(
        b.firstOf(
            localVariableDeclaration,
            expression));
    b.rule(yieldStatement).is(
        "yield",
        b.firstOf(
            b.sequence(RETURN, expression),
            BREAK),
        SEMICOLON);
    b.rule(namespaceDeclaration).is(NAMESPACE, qualifiedIdentifier, namespaceBody, b.optional(SEMICOLON));
    b.rule(qualifiedIdentifier).is(IDENTIFIER, b.zeroOrMore(DOT, IDENTIFIER));
    b.rule(namespaceBody).is(LCURLYBRACE, b.zeroOrMore(externAliasDirective), b.zeroOrMore(usingDirective), b.zeroOrMore(namespaceMemberDeclaration), RCURLYBRACE);
    b.rule(externAliasDirective).is(EXTERN, "alias", IDENTIFIER, SEMICOLON);
    b.rule(usingDirective).is(
        b.firstOf(
            usingAliasDirective,
            usingNamespaceDirective));
    b.rule(usingAliasDirective).is(USING, IDENTIFIER, EQUAL, namespaceOrTypeName, SEMICOLON);
    b.rule(usingNamespaceDirective).is(USING, namespaceName, SEMICOLON);
    b.rule(namespaceMemberDeclaration).is(
        b.firstOf(
            namespaceDeclaration,
            typeDeclaration));
    b.rule(typeDeclaration).is(
        b.firstOf(
            classDeclaration,
            structDeclaration,
            interfaceDeclaration,
            enumDeclaration,
            delegateDeclaration));
    b.rule(qualifiedAliasMember).is(IDENTIFIER, DOUBLE_COLON, IDENTIFIER, b.optional(typeArgumentList));
  }

  private static void classes(LexerfulGrammarBuilder b) {
    b.rule(classDeclaration).is(
        b.optional(attributes), b.zeroOrMore(classModifier), b.optional(PARTIAL),
        CLASS, IDENTIFIER, b.optional(typeParameterList), b.optional(classBase), b.optional(typeParameterConstraintsClauses),
        classBody,
        b.optional(SEMICOLON));
    b.rule(classModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            ABSTRACT,
            SEALED,
            STATIC,
            UNSAFE));
    b.rule(classBase).is(
        COLON,
        b.firstOf(
            b.sequence(classType, COMMA, interfaceTypeList),
            classType,
            interfaceTypeList));
    b.rule(interfaceTypeList).is(interfaceType, b.zeroOrMore(COMMA, interfaceType));
    b.rule(classBody).is(LCURLYBRACE, b.zeroOrMore(classMemberDeclaration), RCURLYBRACE);
    b.rule(classMemberDeclaration).is(
        b.firstOf(
            constantDeclaration,
            fieldDeclaration,
            methodDeclaration,
            propertyDeclaration,
            eventDeclaration,
            indexerDeclaration,
            operatorDeclaration,
            constructorDeclaration,
            destructorDeclaration,
            staticConstructorDeclaration,
            typeDeclaration));
    b.rule(constantDeclaration).is(
        b.optional(attributes), b.zeroOrMore(constantModifier),
        CONST, type,
        constantDeclarator, b.zeroOrMore(COMMA, constantDeclarator),
        SEMICOLON);
    b.rule(constantModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE));
    b.rule(fieldDeclaration).is(
        b.optional(attributes), b.zeroOrMore(fieldModifier),
        type,
        variableDeclarator, b.zeroOrMore(COMMA, variableDeclarator),
        SEMICOLON);
    b.rule(fieldModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            STATIC,
            READONLY,
            VOLATILE,
            UNSAFE));
    b.rule(variableDeclarator).is(IDENTIFIER, b.optional(EQUAL, variableInitializer));
    b.rule(variableInitializer).is(
        b.firstOf(
            expression,
            arrayInitializer));
    b.rule(methodDeclaration).is(methodHeader, methodBody);
    b.rule(methodHeader).is(
        b.optional(attributes), b.zeroOrMore(methodModifier), b.optional(PARTIAL),
        returnType, memberName,
        b.optional(typeParameterList),
        LPARENTHESIS, b.optional(formalParameterList), RPARENTHESIS,
        b.optional(typeParameterConstraintsClauses)).skip();
    b.rule(methodModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            STATIC,
            VIRTUAL,
            SEALED,
            OVERRIDE,
            ABSTRACT,
            EXTERN,
            UNSAFE));
    b.rule(returnType).is(
        b.firstOf(
            type,
            VOID));
    b.rule(memberName).is(
        b.zeroOrMore(
            b.firstOf(
                qualifiedAliasMember,
                b.sequence(
                    b.firstOf(
                        THIS,
                        IDENTIFIER),
                    b.optional(typeArgumentList))),
            DOT),
        b.firstOf(
            THIS,
            IDENTIFIER),
        b.optional(typeArgumentList));
    b.rule(methodBody).is(
        b.firstOf(
            block,
            SEMICOLON));
    b.rule(formalParameterList).is(
        b.firstOf(
            b.sequence(fixedParameters, b.optional(COMMA, parameterArray)),
            parameterArray));
    b.rule(fixedParameters).is(fixedParameter, b.zeroOrMore(COMMA, fixedParameter));
    b.rule(fixedParameter).is(b.optional(attributes), b.optional(parameterModifier), type, IDENTIFIER, b.optional(EQUAL, expression));
    b.rule(parameterModifier).is(
        b.firstOf(
            REF,
            OUT,
            THIS));
    b.rule(parameterArray).is(b.optional(attributes), PARAMS, arrayType, IDENTIFIER);
    b.rule(propertyDeclaration).is(b.optional(attributes), b.zeroOrMore(propertyModifier), type, memberName, LCURLYBRACE, accessorDeclarations, RCURLYBRACE);
    b.rule(propertyModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            STATIC,
            VIRTUAL,
            SEALED,
            OVERRIDE,
            ABSTRACT,
            EXTERN,
            UNSAFE));
    b.rule(accessorDeclarations).is(
        b.firstOf(
            b.sequence(getAccessorDeclaration, b.optional(setAccessorDeclaration)),
            b.sequence(setAccessorDeclaration, b.optional(getAccessorDeclaration))));
    b.rule(getAccessorDeclaration).is(b.optional(attributes), b.zeroOrMore(accessorModifier), GET, accessorBody);
    b.rule(setAccessorDeclaration).is(b.optional(attributes), b.zeroOrMore(accessorModifier), SET, accessorBody);
    b.rule(accessorModifier).is(
        b.firstOf(
            b.sequence(PROTECTED, INTERNAL),
            b.sequence(INTERNAL, PROTECTED),
            PROTECTED,
            INTERNAL,
            PRIVATE));
    b.rule(accessorBody).is(
        b.firstOf(
            block,
            SEMICOLON));
    b.rule(eventDeclaration).is(
        b.optional(attributes),
        b.zeroOrMore(eventModifier),
        EVENT, type,
        b.firstOf(
            b.sequence(variableDeclarator, b.zeroOrMore(COMMA, variableDeclarator), SEMICOLON),
            b.sequence(memberName, LCURLYBRACE, eventAccessorDeclarations, RCURLYBRACE)));
    b.rule(eventModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            STATIC,
            VIRTUAL,
            SEALED,
            OVERRIDE,
            ABSTRACT,
            EXTERN,
            UNSAFE));
    b.rule(eventAccessorDeclarations).is(
        b.firstOf(
            b.sequence(addAccessorDeclaration, removeAccessorDeclaration),
            b.sequence(removeAccessorDeclaration, addAccessorDeclaration)));
    b.rule(addAccessorDeclaration).is(b.optional(attributes), "add", block);
    b.rule(removeAccessorDeclaration).is(b.optional(attributes), "remove", block);
    b.rule(indexerDeclaration).is(
        b.optional(attributes), b.zeroOrMore(indexerModifier),
        indexerDeclarator, LCURLYBRACE, accessorDeclarations, RCURLYBRACE);
    b.rule(indexerModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            STATIC,
            VIRTUAL,
            SEALED,
            OVERRIDE,
            ABSTRACT,
            EXTERN,
            UNSAFE));
    b.rule(indexerDeclarator).is(
        type,
        b.zeroOrMore(
            b.firstOf(
                qualifiedAliasMember,
                b.sequence(IDENTIFIER, b.optional(typeArgumentList))),
            DOT),
        THIS, LBRACKET, formalParameterList, RBRACKET);
    b.rule(operatorDeclaration).is(b.optional(attributes), b.oneOrMore(operatorModifier), operatorDeclarator, operatorBody);
    b.rule(operatorModifier).is(
        b.firstOf(
            PUBLIC,
            STATIC,
            EXTERN,
            UNSAFE));
    b.rule(operatorDeclarator).is(
        b.firstOf(
            unaryOperatorDeclarator,
            binaryOperatorDeclarator,
            conversionOperatorDeclarator));
    b.rule(unaryOperatorDeclarator).is(type, OPERATOR, overloadableUnaryOperator, LPARENTHESIS, type, IDENTIFIER, RPARENTHESIS);
    b.rule(overloadableUnaryOperator).is(
        b.firstOf(
            PLUS,
            MINUS,
            EXCLAMATION,
            TILDE,
            INC_OP,
            DEC_OP,
            TRUE,
            FALSE));
    b.rule(binaryOperatorDeclarator).is(type, OPERATOR, overloadableBinaryOperator, LPARENTHESIS, type, IDENTIFIER, COMMA, type, IDENTIFIER, RPARENTHESIS);
    b.rule(overloadableBinaryOperator).is(
        b.firstOf(
            PLUS,
            MINUS,
            STAR,
            SLASH,
            MODULO,
            AND,
            OR,
            XOR,
            LEFT_OP,
            rightShift,
            EQ_OP,
            NE_OP,
            SUPERIOR,
            INFERIOR,
            GE_OP,
            LE_OP));
    b.rule(conversionOperatorDeclarator).is(
        b.firstOf(
            IMPLICIT,
            EXPLICIT),
        OPERATOR, type, LPARENTHESIS, type, IDENTIFIER, RPARENTHESIS);
    b.rule(operatorBody).is(
        b.firstOf(
            block,
            SEMICOLON));
    b.rule(constructorDeclaration).is(b.optional(attributes), b.zeroOrMore(constructorModifier), constructorDeclarator, constructorBody);
    b.rule(constructorModifier).is(
        b.firstOf(
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            EXTERN,
            UNSAFE));
    b.rule(constructorDeclarator).is(IDENTIFIER, LPARENTHESIS, b.optional(formalParameterList), RPARENTHESIS, b.optional(constructorInitializer));
    b.rule(constructorInitializer).is(
        COLON,
        b.firstOf(
            BASE,
            THIS),
        LPARENTHESIS, b.optional(argumentList), RPARENTHESIS);
    b.rule(constructorBody).is(
        b.firstOf(
            block,
            SEMICOLON));
    b.rule(staticConstructorDeclaration).is(
        b.optional(attributes),
        staticConstructorModifiers, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, staticConstructorBody);
    b.rule(staticConstructorModifiers).is(
        b.firstOf(
            b.sequence(b.optional(EXTERN), STATIC, b.nextNot(b.next(EXTERN))),
            b.sequence(STATIC, b.optional(EXTERN))));
    b.rule(staticConstructorBody).is(
        b.firstOf(
            block,
            SEMICOLON));
    b.rule(destructorDeclaration).is(
        b.optional(attributes), b.optional(EXTERN),
        TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, destructorBody);
    b.rule(destructorBody).is(
        b.firstOf(
            block,
            SEMICOLON));
  }

  private static void structs(LexerfulGrammarBuilder b) {
    b.rule(structDeclaration).is(
        b.optional(attributes), b.zeroOrMore(structModifier), b.optional(PARTIAL),
        STRUCT, IDENTIFIER,
        b.optional(typeParameterList), b.optional(structInterfaces), b.optional(typeParameterConstraintsClauses),
        structBody,
        b.optional(SEMICOLON));
    b.rule(structModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    b.rule(structInterfaces).is(COLON, interfaceTypeList);
    b.rule(structBody).is(LCURLYBRACE, b.zeroOrMore(structMemberDeclaration), RCURLYBRACE);
    b.rule(structMemberDeclaration).is(
        b.firstOf(
            constantDeclaration,
            fieldDeclaration,
            methodDeclaration,
            propertyDeclaration,
            eventDeclaration,
            indexerDeclaration,
            operatorDeclaration,
            constructorDeclaration,
            staticConstructorDeclaration,
            typeDeclaration,
            unsafe(fixedSizeBufferDeclaration)));
  }

  private static void arrays(LexerfulGrammarBuilder b) {
    b.rule(arrayInitializer).is(
        LCURLYBRACE,
        b.optional(
            b.firstOf(
                b.sequence(variableInitializerList, COMMA),
                variableInitializerList)),
        RCURLYBRACE);
    b.rule(variableInitializerList).is(variableInitializer, b.zeroOrMore(COMMA, variableInitializer));
  }

  private static void interfaces(LexerfulGrammarBuilder b) {
    b.rule(interfaceDeclaration).is(
        b.optional(attributes), b.zeroOrMore(interfaceModifier), b.optional(PARTIAL),
        INTERFACE, IDENTIFIER,
        b.optional(variantTypeParameterList), b.optional(interfaceBase), b.optional(typeParameterConstraintsClauses),
        interfaceBody,
        b.optional(SEMICOLON));
    b.rule(interfaceModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    b.rule(variantTypeParameterList).is(INFERIOR, variantTypeParameter, b.zeroOrMore(COMMA, variantTypeParameter), SUPERIOR);
    b.rule(variantTypeParameter).is(b.optional(attributes), b.optional(varianceAnnotation), typeParameter);
    b.rule(varianceAnnotation).is(
        b.firstOf(
            IN,
            OUT));
    b.rule(interfaceBase).is(COLON, interfaceTypeList);
    b.rule(interfaceBody).is(LCURLYBRACE, b.zeroOrMore(interfaceMemberDeclaration), RCURLYBRACE);
    b.rule(interfaceMemberDeclaration).is(
        b.firstOf(
            interfaceMethodDeclaration,
            interfacePropertyDeclaration,
            interfaceEventDeclaration,
            interfaceIndexerDeclaration));
    b.rule(interfaceMethodDeclaration).is(
        b.optional(attributes), b.optional(NEW),
        returnType, IDENTIFIER,
        b.optional(typeParameterList),
        LPARENTHESIS, b.optional(formalParameterList), RPARENTHESIS, b.optional(typeParameterConstraintsClauses),
        SEMICOLON);
    b.rule(interfacePropertyDeclaration).is(
        b.optional(attributes), b.optional(NEW),
        type, IDENTIFIER, LCURLYBRACE, interfaceAccessors, RCURLYBRACE);
    b.rule(interfaceAccessors).is(
        b.optional(attributes),
        b.firstOf(
            b.sequence(GET, SEMICOLON, b.optional(attributes), SET),
            b.sequence(SET, SEMICOLON, b.optional(attributes), GET),
            GET,
            SET),
        SEMICOLON);
    b.rule(interfaceEventDeclaration).is(
        b.optional(attributes), b.optional(NEW),
        EVENT, type, IDENTIFIER, SEMICOLON);
    b.rule(interfaceIndexerDeclaration).is(
        b.optional(attributes), b.optional(NEW),
        type, THIS, LBRACKET, formalParameterList, RBRACKET, LCURLYBRACE, interfaceAccessors, RCURLYBRACE);
  }

  private static void enums(LexerfulGrammarBuilder b) {
    b.rule(enumDeclaration).is(b.optional(attributes), b.zeroOrMore(enumModifier), ENUM, IDENTIFIER, b.optional(enumBase), enumBody, b.optional(SEMICOLON));
    b.rule(enumBase).is(COLON, integralType);
    b.rule(enumBody).is(
        LCURLYBRACE,
        b.optional(
            b.firstOf(
                b.sequence(enumMemberDeclarations, COMMA),
                enumMemberDeclarations)),
        RCURLYBRACE);
    b.rule(enumModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE));
    b.rule(enumMemberDeclarations).is(enumMemberDeclaration, b.zeroOrMore(COMMA, enumMemberDeclaration));
    b.rule(enumMemberDeclaration).is(b.optional(attributes), IDENTIFIER, b.optional(EQUAL, expression));
  }

  private static void delegates(LexerfulGrammarBuilder b) {
    b.rule(delegateDeclaration).is(
        b.optional(attributes), b.zeroOrMore(delegateModifier),
        DELEGATE, returnType, IDENTIFIER,
        b.optional(variantTypeParameterList),
        LPARENTHESIS, b.optional(formalParameterList), RPARENTHESIS,
        b.optional(typeParameterConstraintsClauses),
        SEMICOLON);
    b.rule(delegateModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
  }

  private static void attributes(LexerfulGrammarBuilder b) {
    b.rule(globalAttributes).is(b.oneOrMore(globalAttributeSection));
    b.rule(globalAttributeSection).is(LBRACKET, globalAttributeTargetSpecifier, attributeList, b.optional(COMMA), RBRACKET);
    b.rule(globalAttributeTargetSpecifier).is(globalAttributeTarget, COLON);
    b.rule(globalAttributeTarget).is(
        b.firstOf(
            "assembly",
            "module"));
    b.rule(attributes).is(b.oneOrMore(attributeSection));
    b.rule(attributeSection).is(LBRACKET, b.optional(attributeTargetSpecifier), attributeList, b.optional(COMMA), RBRACKET);
    b.rule(attributeTargetSpecifier).is(attributeTarget, COLON);
    b.rule(attributeTarget).is(
        b.firstOf(
            "field",
            "event",
            "method",
            "param",
            "property",
            RETURN,
            "type"));
    b.rule(attributeList).is(attribute, b.zeroOrMore(COMMA, attribute));
    b.rule(attribute).is(attributeName, b.optional(attributeArguments));
    b.rule(attributeName).is(typeName);
    b.rule(attributeArguments).is(
        LPARENTHESIS,
        b.optional(
            b.firstOf(
                namedArgument,
                positionalArgument),
            b.zeroOrMore(
                COMMA,
                b.firstOf(
                    namedArgument,
                    positionalArgument))),
        RPARENTHESIS);
    b.rule(positionalArgument).is(b.optional(argumentName), attributeArgumentExpression);
    b.rule(namedArgument).is(IDENTIFIER, EQUAL, attributeArgumentExpression);
    b.rule(attributeArgumentExpression).is(expression);
  }

  private static void generics(LexerfulGrammarBuilder b) {
    b.rule(typeParameterList).is(INFERIOR, typeParameters, SUPERIOR);
    b.rule(typeParameters).is(b.optional(attributes), typeParameter, b.zeroOrMore(COMMA, b.optional(attributes), typeParameter));
    b.rule(typeParameter).is(IDENTIFIER);
    b.rule(typeArgumentList).is(INFERIOR, typeArgument, b.zeroOrMore(COMMA, typeArgument), SUPERIOR);
    b.rule(typeArgument).is(type);
    b.rule(typeParameterConstraintsClauses).is(b.oneOrMore(typeParameterConstraintsClause));
    b.rule(typeParameterConstraintsClause).is("where", typeParameter, COLON, typeParameterConstraints);
    b.rule(typeParameterConstraints).is(
        b.firstOf(
            b.sequence(primaryConstraint, COMMA, secondaryConstraints, COMMA, constructorConstraint),
            b.sequence(
                primaryConstraint, COMMA,
                b.firstOf(
                    secondaryConstraints,
                    constructorConstraint)),
            b.sequence(secondaryConstraints, COMMA, constructorConstraint),
            primaryConstraint,
            secondaryConstraints,
            constructorConstraint));
    b.rule(primaryConstraint).is(
        b.firstOf(
            classType,
            CLASS,
            STRUCT));
    b.rule(secondaryConstraints).is(
        b.firstOf(
            interfaceType,
            typeParameter),
        b.zeroOrMore(
            COMMA,
            b.firstOf(
                interfaceType,
                typeParameter)));
    b.rule(constructorConstraint).is(NEW, LPARENTHESIS, RPARENTHESIS);
  }

  /**
   * Syntactic sugar to highlight constructs, which were moved from {@link unsafe}
   * to get rid of call to {@link com.sonar.sslr.api.Rule#or} (removed in SSLR 1.13).
   */
  private static Object unsafe(Object matcher) {
    return matcher;
  }

  private static void unsafe(LexerfulGrammarBuilder b) {
    // FIXME override!
    b.rule(destructorDeclaration).override(
        b.optional(attributes),
        b.zeroOrMore(
            b.firstOf(
                EXTERN,
                UNSAFE)),
        TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, destructorBody);
    // FIXME override!
    b.rule(staticConstructorModifiers).override(
        b.zeroOrMore(
            b.firstOf(
                EXTERN,
                UNSAFE)),
        STATIC,
        b.zeroOrMore(
            b.firstOf(
                EXTERN,
                UNSAFE)));
    // FIXME override!
    b.rule(embeddedStatement).override(
        b.firstOf(
            block,
            SEMICOLON,
            expressionStatement,
            selectionStatement,
            iterationStatement,
            jumpStatement,
            tryStatement,
            checkedStatement,
            uncheckedStatement,
            lockStatement,
            usingStatement,
            yieldStatement,
            unsafeStatement,
            fixedStatement));
    b.rule(unsafeStatement).is(UNSAFE, block);
    b.rule(pointerIndirectionExpression).is(STAR, unaryExpression);
    b.rule(pointerElementAccess).is(primaryNoArrayCreationExpression, LBRACKET, expression, RBRACKET);
    b.rule(addressOfExpression).is(AND, unaryExpression);
    b.rule(sizeOfExpression).is(SIZEOF, LPARENTHESIS, type, RPARENTHESIS);
    b.rule(fixedStatement).is(
        FIXED,
        LPARENTHESIS, pointerType, fixedPointerDeclarator, b.zeroOrMore(COMMA, fixedPointerDeclarator), RPARENTHESIS,
        embeddedStatement);
    b.rule(fixedPointerDeclarator).is(IDENTIFIER, EQUAL, fixedPointerInitializer);
    b.rule(fixedPointerInitializer).is(
        b.firstOf(
            b.sequence(AND, variableReference),
            stackallocInitializer,
            expression));
    b.rule(fixedSizeBufferDeclaration).is(
        b.optional(attributes), b.zeroOrMore(fixedSizeBufferModifier),
        FIXED, type, b.oneOrMore(fixedSizeBufferDeclarator), SEMICOLON);
    b.rule(fixedSizeBufferModifier).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    b.rule(fixedSizeBufferDeclarator).is(IDENTIFIER, LBRACKET, expression, RBRACKET);
    b.rule(stackallocInitializer).is(STACKALLOC, type, LBRACKET, expression, RBRACKET);
  }

}
