/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser;

import com.sonar.csharp.squid.api.CSharpGrammar;

import static com.sonar.csharp.squid.api.CSharpKeyword.*;
import static com.sonar.csharp.squid.api.CSharpPunctuator.*;
import static com.sonar.csharp.squid.api.CSharpTokenType.*;
import static com.sonar.sslr.api.GenericTokenType.*;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Advanced.*;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Predicate.*;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Standard.*;

/**
 * Definition of each element of the C# grammar.
 */
public class CSharpGrammarImpl extends CSharpGrammar {

  private static final String SET = "set";
  private static final String GET = "get";
  private static final String PARTIAL = "partial";

  public CSharpGrammarImpl() {
    // We follow the C# language specification 4.0
    literal.is(or(TRUE, FALSE, INTEGER_DEC_LITERAL, INTEGER_HEX_LITERAL, REAL_LITERAL, CHARACTER_LITERAL, STRING_LITERAL, NULL));
    rightShift.is(SUPERIOR, SUPERIOR);
    rightShiftAssignment.is(SUPERIOR, GE_OP);

    // A.2.1 Basic concepts
    basicConcepts();

    // A.2.2 Types
    types();

    // A.2.3 Variables
    variables();

    // A.2.4 Expressions
    expressions();

    // A.2.5 Statements
    statements();

    // A.2.6 Classes
    classes();

    // A.2.7 Struct
    structs();

    // A.2.8 Arrays
    arrays();

    // A.2.9 Interfaces
    interfaces();

    // A.2.10 Enums
    enums();

    // A.2.11 Delegates
    delegates();

    // A.2.12 Attributes
    attributes();

    // A.2.13 Generics
    generics();

    // A.3 Unsafe code
    unsafe();
  }

  private void basicConcepts() {
    compilationUnit.is(o2n(externAliasDirective), o2n(usingDirective), opt(globalAttributes), o2n(namespaceMemberDeclaration),
        EOF);
    namespaceName.is(namespaceOrTypeName);
    typeName.is(namespaceOrTypeName);
    namespaceOrTypeName.is(
        or(
            qualifiedAliasMember,
            and(IDENTIFIER, opt(typeArgumentList))
        ),
        o2n(
            DOT, IDENTIFIER, opt(typeArgumentList)
        ));
  }

  private void types() {
    simpleType.is(or(numericType, BOOL));
    numericType.is(or(integralType, floatingPointType, DECIMAL));
    integralType.is(or(SBYTE, BYTE, SHORT, USHORT, INT, UINT, LONG, ULONG, CHAR));
    floatingPointType.is(or(FLOAT, DOUBLE));

    rankSpecifier.is(LBRACKET, o2n(COMMA), RBRACKET);
    rankSpecifiers.is(one2n(rankSpecifier));

    typePrimary.is(
        or(
            simpleType,
            "dynamic",
            OBJECT,
            STRING,
            typeName
        )).skip();
    nullableType.is(typePrimary, QUESTION, not(and(expression, COLON)));
    pointerType.is( // Moved from unsafe code to remove the left recursions
        or(
            nullableType,
            typePrimary,
            VOID
        ),
        STAR
        );
    arrayType.is(
        or(
            pointerType,
            nullableType,
            typePrimary
        ),
        rankSpecifiers
        );
    type.is(
        or(
            arrayType,
            pointerType,
            nullableType,
            typePrimary
        ));

    nonNullableValueType.is(not(nullableType), type);
    nonArrayType.is(not(arrayType), type);

    classType.is(or("dynamic", OBJECT, STRING, typeName));
    interfaceType.is(typeName);
    enumType.is(typeName);
    delegateType.is(typeName);
  }

  private void variables() {
    variableReference.is(expression);
  }

  private void expressions() {
    primaryExpressionPrimary.is(or(arrayCreationExpression, primaryNoArrayCreationExpression)).skip();
    primaryNoArrayCreationExpression.is(
        or(
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
            simpleName
        )).skip();

    postMemberAccess.is(DOT, IDENTIFIER, opt(typeArgumentList));
    postElementAccess.is(LBRACKET, argumentList, RBRACKET);
    postPointerMemberAccess.is(PTR_OP, IDENTIFIER);
    postIncrement.is(INC_OP);
    postDecrement.is(DEC_OP);
    postInvocation.is(LPARENTHESIS, opt(argumentList), RPARENTHESIS);

    postfixExpression.is(
        primaryExpressionPrimary,
        o2n(
        or(
            postMemberAccess,
            postElementAccess,
            postPointerMemberAccess,
            postIncrement,
            postDecrement,
            postInvocation
        )
        )).skipIfOneChild();
    primaryExpression.is(postfixExpression);

    argumentList.is(argument, o2n(COMMA, argument));
    argument.is(opt(argumentName), argumentValue);
    argumentName.is(IDENTIFIER, COLON);
    argumentValue.is(or(expression, and(REF, variableReference), and(OUT, variableReference)));
    simpleName.is(IDENTIFIER, opt(typeArgumentList));
    parenthesizedExpression.is(LPARENTHESIS, expression, RPARENTHESIS);
    memberAccess.is(or(and(qualifiedAliasMember, DOT, IDENTIFIER),
        and(predefinedType, DOT, IDENTIFIER, opt(typeArgumentList))));
    predefinedType.is(or(BOOL, BYTE, CHAR, DECIMAL, DOUBLE, FLOAT, INT, LONG, OBJECT, SBYTE, SHORT, STRING, UINT, ULONG, USHORT));
    thisAccess.is(THIS);
    // NOTE: baseAccess does not exactly stick to the specification: "opt(typeArgumentList)" has been added here, whereas it is not
    // present in the "base-access" rule in the specification of C# 4.0
    baseAccess.is(BASE, or(and(DOT, IDENTIFIER, opt(typeArgumentList)), and(LBRACKET, argumentList, RBRACKET)));
    objectCreationExpression.is(NEW, type,
        or(and(LPARENTHESIS, opt(argumentList), RPARENTHESIS, opt(objectOrCollectionInitializer)), objectOrCollectionInitializer));
    objectOrCollectionInitializer.is(or(objectInitializer, collectionInitializer));
    objectInitializer.is(LCURLYBRACE, opt(memberInitializer), o2n(COMMA, memberInitializer), opt(COMMA), RCURLYBRACE);
    memberInitializer.is(IDENTIFIER, EQUAL, initializerValue);
    initializerValue.is(or(expression, objectOrCollectionInitializer));
    collectionInitializer.is(LCURLYBRACE, elementInitializer, o2n(COMMA, elementInitializer), opt(COMMA), RCURLYBRACE);
    elementInitializer.is(or(nonAssignmentExpression, and(LCURLYBRACE, expressionList, RCURLYBRACE)));
    expressionList.is(expression, o2n(COMMA, expression));
    arrayCreationExpression.is(or(
        and(NEW, nonArrayType, LBRACKET, expressionList, RBRACKET, o2n(rankSpecifier), opt(arrayInitializer)),
        and(NEW, arrayType, arrayInitializer), and(NEW, rankSpecifier, arrayInitializer)));
    delegateCreationExpression.is(NEW, delegateType, LPARENTHESIS, expression, RPARENTHESIS);
    anonymousObjectCreationExpression.is(NEW, anonymousObjectInitializer);
    anonymousObjectInitializer.is(LCURLYBRACE, opt(memberDeclarator), o2n(COMMA, memberDeclarator), opt(COMMA), RCURLYBRACE);
    // NOTE Rule memberDeclarator is relaxed to accept any expression
    memberDeclarator.is(opt(IDENTIFIER, EQUAL), expression);
    // NOTE : typeOfExpression does not exactly stick to the specification, but the bridge makes its easier to parse for now.
    typeOfExpression.is(TYPEOF, bridge(LPARENTHESIS, RPARENTHESIS));
    unboundTypeName.is(
        one2n(IDENTIFIER, opt(DOUBLE_COLON, IDENTIFIER), opt(genericDimensionSpecifier),
            opt(DOT, IDENTIFIER, opt(genericDimensionSpecifier))), opt(DOT, IDENTIFIER, opt(genericDimensionSpecifier)));
    genericDimensionSpecifier.is(INFERIOR, o2n(COMMA), SUPERIOR);
    checkedExpression.is(CHECKED, LPARENTHESIS, expression, RPARENTHESIS);
    uncheckedExpression.is(UNCHECKED, LPARENTHESIS, expression, RPARENTHESIS);
    defaultValueExpression.is(DEFAULT, LPARENTHESIS, type, RPARENTHESIS);

    unaryExpression.is(
        or(
            and(LPARENTHESIS, type, RPARENTHESIS, unaryExpression),
            primaryExpression,
            and(or(MINUS, EXCLAMATION, INC_OP, DEC_OP, TILDE, PLUS), unaryExpression)
        )).skipIfOneChild();

    multiplicativeExpression.is(unaryExpression, o2n(or(STAR, SLASH, MODULO), unaryExpression)).skipIfOneChild();
    additiveExpression.is(multiplicativeExpression, o2n(or(PLUS, MINUS), multiplicativeExpression)).skipIfOneChild();
    shiftExpression.is(additiveExpression, o2n(or(LEFT_OP, rightShift), additiveExpression)).skipIfOneChild();
    relationalExpression.is(
        shiftExpression,
        o2n(
        or(
            and(or(INFERIOR, SUPERIOR, LE_OP, GE_OP), shiftExpression),
            and(or(IS, AS), type))
        )).skipIfOneChild();
    equalityExpression.is(relationalExpression, o2n(or(EQ_OP, NE_OP), relationalExpression)).skipIfOneChild();
    andExpression.is(equalityExpression, o2n(AND, equalityExpression)).skipIfOneChild();
    exclusiveOrExpression.is(andExpression, o2n(XOR, andExpression)).skipIfOneChild();
    inclusiveOrExpression.is(exclusiveOrExpression, o2n(OR, exclusiveOrExpression)).skipIfOneChild();
    conditionalAndExpression.is(inclusiveOrExpression, o2n(AND_OP, inclusiveOrExpression)).skipIfOneChild();
    conditionalOrExpression.is(conditionalAndExpression, o2n(OR_OP, conditionalAndExpression)).skipIfOneChild();
    nullCoalescingExpression.is(conditionalOrExpression, opt(DOUBLE_QUESTION, nullCoalescingExpression)).skipIfOneChild();
    conditionalExpression.is(nullCoalescingExpression, opt(QUESTION, expression, COLON, expression)).skipIfOneChild();
    lambdaExpression.is(anonymousFunctionSignature, LAMBDA, anonymousFunctionBody);
    anonymousMethodExpression.is(DELEGATE, opt(explicitAnonymousFunctionSignature), block);
    anonymousFunctionSignature.is(or(explicitAnonymousFunctionSignature, implicitAnonymousFunctionSignature));
    explicitAnonymousFunctionSignature.is(LPARENTHESIS,
        opt(explicitAnonymousFunctionParameter, o2n(COMMA, explicitAnonymousFunctionParameter)), RPARENTHESIS);
    explicitAnonymousFunctionParameter.is(opt(anonymousFunctionParameterModifier), type, IDENTIFIER);
    anonymousFunctionParameterModifier.is(or("ref", "out"));
    implicitAnonymousFunctionSignature.is(or(implicitAnonymousFunctionParameter,
        and(LPARENTHESIS, opt(implicitAnonymousFunctionParameter, o2n(COMMA, implicitAnonymousFunctionParameter)), RPARENTHESIS)));
    implicitAnonymousFunctionParameter.is(IDENTIFIER);
    anonymousFunctionBody.is(or(expression, block));
    queryExpression.is(fromClause, queryBody);
    fromClause.is("from", or(and(type, IDENTIFIER), IDENTIFIER), IN, expression);
    queryBody.is(o2n(queryBodyClause), selectOrGroupClause, opt(queryContinuation));
    queryBodyClause.is(or(fromClause, letClause, whereClause, joinIntoClause, joinClause, orderByClause));
    letClause.is("let", IDENTIFIER, EQUAL, expression);
    whereClause.is("where", expression);
    joinClause.is("join", or(and(type, IDENTIFIER), IDENTIFIER), IN, expression, "on", expression, "equals", expression);
    joinIntoClause.is("join", or(and(type, IDENTIFIER), IDENTIFIER), IN, expression, "on", expression, "equals", expression,
        "into", IDENTIFIER);
    orderByClause.is("orderby", ordering, o2n(COMMA, ordering));
    ordering.is(expression, opt(orderingDirection));
    orderingDirection.is(or("ascending", "descending"));
    selectOrGroupClause.is(or(selectClause, groupClause));
    selectClause.is("select", expression);
    groupClause.is("group", expression, "by", expression);
    queryContinuation.is("into", IDENTIFIER, queryBody);
    assignment.is(
        unaryExpression,
        or(EQUAL, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, MOD_ASSIGN, AND_ASSIGN, OR_ASSIGN, XOR_ASSIGN, LEFT_ASSIGN,
            rightShiftAssignment), expression);
    nonAssignmentExpression.is(or(lambdaExpression, queryExpression, conditionalExpression)).skip();
    expression.is(or(assignment, nonAssignmentExpression));
  }

  private void statements() {
    statement.is(or(labeledStatement, declarationStatement, embeddedStatement));
    embeddedStatement.is(or(block, SEMICOLON, expressionStatement, selectionStatement, iterationStatement, jumpStatement,
        tryStatement, checkedStatement, uncheckedStatement, lockStatement, usingStatement, yieldStatement));
    block.is(LCURLYBRACE, o2n(statement), RCURLYBRACE);
    labeledStatement.is(IDENTIFIER, COLON, statement);
    declarationStatement.is(or(localVariableDeclaration, localConstantDeclaration), SEMICOLON);
    localVariableDeclaration.is(type, localVariableDeclarator, o2n(COMMA, localVariableDeclarator));
    localVariableDeclarator.is(IDENTIFIER, opt(EQUAL, localVariableInitializer));
    localVariableInitializer.is(or(expression, arrayInitializer));
    localConstantDeclaration.is(CONST, type, constantDeclarator, o2n(COMMA, constantDeclarator));
    constantDeclarator.is(IDENTIFIER, EQUAL, expression);
    // NOTE Rule expressionStatement is relaxed to accept any expression
    expressionStatement.is(expression, SEMICOLON);
    selectionStatement.is(or(ifStatement, switchStatement));
    ifStatement.is(IF, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement, opt(ELSE, embeddedStatement));
    switchStatement.is(SWITCH, LPARENTHESIS, expression, RPARENTHESIS, LCURLYBRACE, o2n(switchSection), RCURLYBRACE);
    switchSection.is(one2n(switchLabel), one2n(statement));
    switchLabel.is(or(and(CASE, expression, COLON), and(DEFAULT, COLON)));
    iterationStatement.is(or(whileStatement, doStatement, forStatement, foreachStatement));
    whileStatement.is(WHILE, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement);
    doStatement.is(DO, embeddedStatement, WHILE, LPARENTHESIS, expression, RPARENTHESIS, SEMICOLON);
    forStatement.is(FOR, LPARENTHESIS, opt(forInitializer), SEMICOLON, opt(forCondition), SEMICOLON, opt(forIterator),
        RPARENTHESIS, embeddedStatement);
    forInitializer.is(or(localVariableDeclaration, statementExpressionList));
    forCondition.is(expression);
    forIterator.is(statementExpressionList);
    statementExpressionList.is(expression, o2n(COMMA, expression));
    foreachStatement.is(FOREACH, LPARENTHESIS, type, IDENTIFIER, IN, expression, RPARENTHESIS, embeddedStatement);
    jumpStatement.is(or(breakStatement, continueStatement, gotoStatement, returnStatement, throwStatement));
    breakStatement.is(BREAK, SEMICOLON);
    continueStatement.is(CONTINUE, SEMICOLON);
    gotoStatement.is(GOTO, or(IDENTIFIER, and(CASE, expression), DEFAULT), SEMICOLON);
    returnStatement.is(RETURN, opt(expression), SEMICOLON);
    throwStatement.is(THROW, opt(expression), SEMICOLON);
    tryStatement.is(TRY, block, or(and(opt(catchClauses), finallyClause), catchClauses));
    catchClauses.is(or(and(o2n(specificCatchClause), generalCatchClause), one2n(specificCatchClause)));
    specificCatchClause.is(CATCH, LPARENTHESIS, classType, opt(IDENTIFIER), RPARENTHESIS, block);
    generalCatchClause.is(CATCH, block);
    finallyClause.is(FINALLY, block);
    checkedStatement.is(CHECKED, block);
    uncheckedStatement.is(UNCHECKED, block);
    lockStatement.is(LOCK, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement);
    usingStatement.is(USING, LPARENTHESIS, resourceAcquisition, RPARENTHESIS, embeddedStatement);
    resourceAcquisition.is(or(localVariableDeclaration, expression));
    yieldStatement.is("yield", or(and(RETURN, expression), BREAK), SEMICOLON);
    namespaceDeclaration.is(NAMESPACE, qualifiedIdentifier, namespaceBody, opt(SEMICOLON));
    qualifiedIdentifier.is(IDENTIFIER, o2n(DOT, IDENTIFIER));
    namespaceBody.is(LCURLYBRACE, o2n(externAliasDirective), o2n(usingDirective), o2n(namespaceMemberDeclaration), RCURLYBRACE);
    externAliasDirective.is(EXTERN, "alias", IDENTIFIER, SEMICOLON);
    usingDirective.is(or(usingAliasDirective, usingNamespaceDirective));
    usingAliasDirective.is(USING, IDENTIFIER, EQUAL, namespaceOrTypeName, SEMICOLON);
    usingNamespaceDirective.is(USING, namespaceName, SEMICOLON);
    namespaceMemberDeclaration.is(or(namespaceDeclaration, typeDeclaration));
    typeDeclaration.is(or(classDeclaration, structDeclaration, interfaceDeclaration, enumDeclaration, delegateDeclaration));
    qualifiedAliasMember.is(IDENTIFIER, DOUBLE_COLON, IDENTIFIER, opt(typeArgumentList));
  }

  private void classes() {
    classDeclaration.is(opt(attributes), o2n(classModifier), opt(PARTIAL), CLASS, IDENTIFIER, opt(typeParameterList),
        opt(classBase), opt(typeParameterConstraintsClauses), classBody, opt(SEMICOLON));
    classModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, ABSTRACT, SEALED, STATIC));
    classBase.is(COLON, or(and(classType, COMMA, interfaceTypeList), classType, interfaceTypeList));
    interfaceTypeList.is(interfaceType, o2n(COMMA, interfaceType));
    classBody.is(LCURLYBRACE, o2n(classMemberDeclaration), RCURLYBRACE);
    classMemberDeclaration.is(or(constantDeclaration, fieldDeclaration, methodDeclaration, propertyDeclaration,
        eventDeclaration, indexerDeclaration, operatorDeclaration, constructorDeclaration, destructorDeclaration,
        staticConstructorDeclaration, typeDeclaration));
    constantDeclaration.is(opt(attributes), o2n(constantModifier), CONST, type, constantDeclarator,
        o2n(COMMA, constantDeclarator), SEMICOLON);
    constantModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE));
    fieldDeclaration.is(opt(attributes), o2n(fieldModifier), type, variableDeclarator, o2n(COMMA, variableDeclarator),
        SEMICOLON);
    fieldModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, READONLY, VOLATILE));
    variableDeclarator.is(IDENTIFIER, opt(EQUAL, variableInitializer));
    variableInitializer.is(or(expression, arrayInitializer));
    methodDeclaration.is(methodHeader, methodBody);
    methodHeader.is(opt(attributes), o2n(methodModifier), opt(PARTIAL), returnType, memberName, opt(typeParameterList),
        LPARENTHESIS, opt(formalParameterList), RPARENTHESIS, opt(typeParameterConstraintsClauses)).skip();
    methodModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN));
    returnType.is(or(type, VOID));
    // NOTE: memberName does not exactly stick to the specification (see page 462 of ECMA specification)
    memberName.is(o2n(or(qualifiedAliasMember, and(or(THIS, IDENTIFIER), opt(typeArgumentList))), DOT), or(THIS, IDENTIFIER),
        opt(typeArgumentList));
    methodBody.is(or(block, SEMICOLON));
    formalParameterList.is(or(and(fixedParameters, opt(COMMA, parameterArray)), parameterArray));
    fixedParameters.is(fixedParameter, o2n(COMMA, fixedParameter));
    fixedParameter.is(opt(attributes), opt(parameterModifier), type, IDENTIFIER, opt(EQUAL, expression));
    parameterModifier.is(or(REF, OUT, THIS));
    parameterArray.is(opt(attributes), PARAMS, arrayType, IDENTIFIER);
    propertyDeclaration.is(opt(attributes), o2n(propertyModifier), type, memberName, LCURLYBRACE, accessorDeclarations,
        RCURLYBRACE);
    propertyModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN));
    accessorDeclarations.is(or(and(getAccessorDeclaration, opt(setAccessorDeclaration)),
        and(setAccessorDeclaration, opt(getAccessorDeclaration))));
    getAccessorDeclaration.is(opt(attributes), o2n(accessorModifier), GET, accessorBody);
    setAccessorDeclaration.is(opt(attributes), o2n(accessorModifier), SET, accessorBody);
    accessorModifier.is(or(and(PROTECTED, INTERNAL), and(INTERNAL, PROTECTED), PROTECTED, INTERNAL, PRIVATE));
    accessorBody.is(or(block, SEMICOLON));
    eventDeclaration.is(
        opt(attributes),
        o2n(eventModifier),
        EVENT,
        type,
        or(and(variableDeclarator, o2n(COMMA, variableDeclarator), SEMICOLON),
            and(memberName, LCURLYBRACE, eventAccessorDeclarations, RCURLYBRACE)));
    eventModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN));
    eventAccessorDeclarations.is(or(and(addAccessorDeclaration, removeAccessorDeclaration),
        and(removeAccessorDeclaration, addAccessorDeclaration)));
    addAccessorDeclaration.is(opt(attributes), "add", block);
    removeAccessorDeclaration.is(opt(attributes), "remove", block);
    indexerDeclaration.is(opt(attributes), o2n(indexerModifier), indexerDeclarator, LCURLYBRACE, accessorDeclarations,
        RCURLYBRACE);
    indexerModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN));
    // NOTE: indexerDeclarator does not exactly stick to the specification. Normally it would be:
    indexerDeclarator.is(type, o2n(or(qualifiedAliasMember, and(IDENTIFIER, opt(typeArgumentList))), DOT), THIS, LBRACKET,
        formalParameterList, RBRACKET);
    operatorDeclaration.is(opt(attributes), one2n(operatorModifier), operatorDeclarator, operatorBody);
    operatorModifier.is(or(PUBLIC, STATIC, EXTERN));
    operatorDeclarator.is(or(unaryOperatorDeclarator, binaryOperatorDeclarator, conversionOperatorDeclarator));
    unaryOperatorDeclarator.is(type, OPERATOR, overloadableUnaryOperator, LPARENTHESIS, type, IDENTIFIER, RPARENTHESIS);
    overloadableUnaryOperator.is(or(PLUS, MINUS, EXCLAMATION, TILDE, INC_OP, DEC_OP, TRUE, FALSE));
    binaryOperatorDeclarator.is(type, OPERATOR, overloadableBinaryOperator, LPARENTHESIS, type, IDENTIFIER, COMMA, type,
        IDENTIFIER, RPARENTHESIS);
    overloadableBinaryOperator.is(or(PLUS, MINUS, STAR, SLASH, MODULO, AND, OR, XOR, LEFT_OP, rightShift, EQ_OP, NE_OP, SUPERIOR,
        INFERIOR, GE_OP, LE_OP));
    conversionOperatorDeclarator.is(or(IMPLICIT, EXPLICIT), OPERATOR, type, LPARENTHESIS, type, IDENTIFIER, RPARENTHESIS);
    operatorBody.is(or(block, SEMICOLON));
    constructorDeclaration.is(opt(attributes), o2n(constructorModifier), constructorDeclarator, constructorBody);
    constructorModifier.is(or(PUBLIC, PROTECTED, INTERNAL, PRIVATE, EXTERN));
    constructorDeclarator.is(IDENTIFIER, LPARENTHESIS, opt(formalParameterList), RPARENTHESIS, opt(constructorInitializer));
    constructorInitializer.is(COLON, or(BASE, THIS), LPARENTHESIS, opt(argumentList), RPARENTHESIS);
    constructorBody.is(or(block, SEMICOLON));
    staticConstructorDeclaration.is(opt(attributes), staticConstructorModifiers, IDENTIFIER, LPARENTHESIS, RPARENTHESIS,
        staticConstructorBody);
    staticConstructorModifiers.is(or(and(opt(EXTERN), STATIC, not(next(EXTERN))), and(STATIC, opt(EXTERN))));
    staticConstructorBody.is(or(block, SEMICOLON));
    destructorDeclaration.is(opt(attributes), opt(EXTERN), TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, destructorBody);
    destructorBody.is(or(block, SEMICOLON));
  }

  private void structs() {
    structDeclaration.is(opt(attributes), o2n(structModifier), opt(PARTIAL), STRUCT, IDENTIFIER, opt(typeParameterList),
        opt(structInterfaces), opt(typeParameterConstraintsClauses), structBody, opt(SEMICOLON));
    structModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE));
    structInterfaces.is(COLON, interfaceTypeList);
    structBody.is(LCURLYBRACE, o2n(structMemberDeclaration), RCURLYBRACE);
    structMemberDeclaration.is(or(constantDeclaration, fieldDeclaration, methodDeclaration, propertyDeclaration,
        eventDeclaration, indexerDeclaration, operatorDeclaration, constructorDeclaration, staticConstructorDeclaration,
        typeDeclaration));
  }

  private void arrays() {
    arrayInitializer.is(LCURLYBRACE, or(and(variableInitializerList, COMMA), opt(variableInitializerList)), RCURLYBRACE);
    variableInitializerList.is(variableInitializer, o2n(COMMA, variableInitializer));
  }

  private void interfaces() {
    interfaceDeclaration.is(opt(attributes), o2n(interfaceModifier), opt(PARTIAL), INTERFACE, IDENTIFIER,
        opt(variantTypeParameterList), opt(interfaceBase), opt(typeParameterConstraintsClauses), interfaceBody, opt(SEMICOLON));
    interfaceModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE));
    variantTypeParameterList.is(INFERIOR, variantTypeParameter, o2n(COMMA, variantTypeParameter), SUPERIOR);
    variantTypeParameter.is(opt(attributes), opt(varianceAnnotation), typeParameter);
    varianceAnnotation.is(or(IN, OUT));
    interfaceBase.is(COLON, interfaceTypeList);
    interfaceBody.is(LCURLYBRACE, o2n(interfaceMemberDeclaration), RCURLYBRACE);
    interfaceMemberDeclaration.is(or(interfaceMethodDeclaration, interfacePropertyDeclaration, interfaceEventDeclaration,
        interfaceIndexerDeclaration));
    interfaceMethodDeclaration.is(opt(attributes), opt(NEW), returnType, IDENTIFIER, opt(typeParameterList), LPARENTHESIS,
        opt(formalParameterList), RPARENTHESIS, opt(typeParameterConstraintsClauses), SEMICOLON);
    interfacePropertyDeclaration.is(opt(attributes), opt(NEW), type, IDENTIFIER, LCURLYBRACE, interfaceAccessors, RCURLYBRACE);
    interfaceAccessors.is(opt(attributes),
        or(and(GET, SEMICOLON, opt(attributes), SET), and(SET, SEMICOLON, opt(attributes), GET), GET, SET), SEMICOLON);
    interfaceEventDeclaration.is(opt(attributes), opt(NEW), EVENT, type, IDENTIFIER, SEMICOLON);
    interfaceIndexerDeclaration.is(opt(attributes), opt(NEW), type, THIS, LBRACKET, formalParameterList, RBRACKET, LCURLYBRACE,
        interfaceAccessors, RCURLYBRACE);
  }

  private void enums() {
    enumDeclaration.is(opt(attributes), o2n(enumModifier), ENUM, IDENTIFIER, opt(enumBase), enumBody, opt(SEMICOLON));
    enumBase.is(COLON, integralType);
    enumBody.is(LCURLYBRACE, or(and(enumMemberDeclarations, COMMA), opt(enumMemberDeclarations)), RCURLYBRACE);
    enumModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE));
    enumMemberDeclarations.is(enumMemberDeclaration, o2n(COMMA, enumMemberDeclaration));
    enumMemberDeclaration.is(opt(attributes), IDENTIFIER, opt(EQUAL, expression));
  }

  private void delegates() {
    delegateDeclaration.is(opt(attributes), o2n(delegateModifier), DELEGATE, returnType, IDENTIFIER,
        opt(variantTypeParameterList), LPARENTHESIS, opt(formalParameterList), RPARENTHESIS, opt(typeParameterConstraintsClauses),
        SEMICOLON);
    delegateModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE));
  }

  private void attributes() {
    globalAttributes.is(one2n(globalAttributeSection));
    globalAttributeSection.is(LBRACKET, globalAttributeTargetSpecifier, attributeList, opt(COMMA), RBRACKET);
    globalAttributeTargetSpecifier.is(globalAttributeTarget, COLON);
    globalAttributeTarget.is(or("assembly", "module"));
    attributes.is(one2n(attributeSection));
    attributeSection.is(LBRACKET, opt(attributeTargetSpecifier), attributeList, opt(COMMA), RBRACKET);
    attributeTargetSpecifier.is(attributeTarget, COLON);
    attributeTarget.is(or("field", "event", "method", "param", "property", RETURN, "type"));
    attributeList.is(attribute, o2n(COMMA, attribute));
    attribute.is(attributeName, opt(attributeArguments));
    attributeName.is(typeName);
    // NOTE: attributeArguments does not exactly stick to the specification, as normally a positionalArgument can not appear after a
    // namedArgument (see page 469 of ECMA specification)
    attributeArguments.is(LPARENTHESIS,
        opt(or(namedArgument, positionalArgument), o2n(COMMA, or(namedArgument, positionalArgument))), RPARENTHESIS);
    positionalArgument.is(opt(argumentName), attributeArgumentExpression);
    namedArgument.is(IDENTIFIER, EQUAL, attributeArgumentExpression);
    attributeArgumentExpression.is(expression);
  }

  private void generics() {
    typeParameterList.is(INFERIOR, typeParameters, SUPERIOR);
    typeParameters.is(opt(attributes), typeParameter, o2n(COMMA, opt(attributes), typeParameter));
    typeParameter.is(IDENTIFIER);
    typeArgumentList.is(INFERIOR, typeArgument, o2n(COMMA, typeArgument), SUPERIOR);
    typeArgument.is(type);
    typeParameterConstraintsClauses.is(one2n(typeParameterConstraintsClause));
    typeParameterConstraintsClause.is("where", typeParameter, COLON, typeParameterConstraints);
    typeParameterConstraints.is(or(and(primaryConstraint, COMMA, secondaryConstraints, COMMA, constructorConstraint),
        and(primaryConstraint, COMMA, or(secondaryConstraints, constructorConstraint)),
        and(secondaryConstraints, COMMA, constructorConstraint), primaryConstraint, secondaryConstraints, constructorConstraint));
    primaryConstraint.is(or(classType, CLASS, STRUCT));
    secondaryConstraints.is(or(interfaceType, typeParameter), o2n(COMMA, or(interfaceType, typeParameter)));
    constructorConstraint.is(NEW, LPARENTHESIS, RPARENTHESIS);
  }

  private void unsafe() {
    classModifier.or(UNSAFE);
    structModifier.or(UNSAFE);
    interfaceModifier.or(UNSAFE);
    delegateModifier.or(UNSAFE);
    fieldModifier.or(UNSAFE);
    methodModifier.or(UNSAFE);
    propertyModifier.or(UNSAFE);
    eventModifier.or(UNSAFE);
    indexerModifier.or(UNSAFE);
    operatorModifier.or(UNSAFE);
    constructorModifier.or(UNSAFE);

    destructorDeclaration.or(opt(attributes), o2n(or(EXTERN, UNSAFE)), TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, destructorBody);
    staticConstructorModifiers.override(o2n(or(EXTERN, UNSAFE)), STATIC, o2n(or(EXTERN, UNSAFE)));
    embeddedStatement.or(or(unsafeStatement, fixedStatement));
    unsafeStatement.is(UNSAFE, block);

    // pointerType was moved to the types part in order to remove the left recursions
    // pointerMemberAccess was moved to the expressions part (as postPointerMemberAccess) in order to remove the left recursions

    // NOTE : unsafe.pointerElementAccess deactivated here because it shadows the "elementAccess" in the main grammar...
    // Need to look into that later.
    primaryNoArrayCreationExpression.or(or(/* unsafe.pointerElementAccess, */sizeOfExpression));
    unaryExpression.or(or(pointerIndirectionExpression, addressOfExpression));
    pointerIndirectionExpression.is(STAR, unaryExpression);
    pointerElementAccess.is(primaryNoArrayCreationExpression, LBRACKET, expression, RBRACKET);
    addressOfExpression.is(AND, unaryExpression);
    sizeOfExpression.is(SIZEOF, LPARENTHESIS, type, RPARENTHESIS);
    fixedStatement.is(FIXED, LPARENTHESIS, pointerType, fixedPointerDeclarator,
        o2n(COMMA, fixedPointerDeclarator), RPARENTHESIS, embeddedStatement);
    fixedPointerDeclarator.is(IDENTIFIER, EQUAL, fixedPointerInitializer);
    // NOTE : stackallocInitializer should not be here according to the specifications, but it seems it can in reality
    fixedPointerInitializer.is(or(and(AND, variableReference), stackallocInitializer, expression));
    structMemberDeclaration.or(fixedSizeBufferDeclaration);
    fixedSizeBufferDeclaration.is(opt(attributes), o2n(fixedSizeBufferModifier), FIXED, type,
        one2n(fixedSizeBufferDeclarator), SEMICOLON);
    fixedSizeBufferModifier.is(or(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, UNSAFE));
    fixedSizeBufferDeclarator.is(IDENTIFIER, LBRACKET, expression, RBRACKET);
    localVariableInitializer.or(stackallocInitializer);
    stackallocInitializer.is(STACKALLOC, type, LBRACKET, expression, RBRACKET);
  }

}
