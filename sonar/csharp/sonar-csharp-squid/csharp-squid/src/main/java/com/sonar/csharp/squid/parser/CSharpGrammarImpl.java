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

import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.sslr.impl.matcher.GrammarFunctions;

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
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Advanced.bridge;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Predicate.next;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Predicate.not;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Standard.and;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Standard.firstOf;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Standard.o2n;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Standard.one2n;
import static com.sonar.sslr.impl.matcher.GrammarFunctions.Standard.opt;

/**
 * Definition of each element of the C# grammar, based on the C# language specification 4.0
 */
public class CSharpGrammarImpl extends CSharpGrammar {

  private static final String SET = "set";
  private static final String GET = "get";
  private static final String PARTIAL = "partial";

  public CSharpGrammarImpl() {
    literal.is(
        firstOf(
            TRUE,
            FALSE,
            INTEGER_DEC_LITERAL,
            INTEGER_HEX_LITERAL,
            REAL_LITERAL,
            CHARACTER_LITERAL,
            STRING_LITERAL,
            NULL));
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

    GrammarFunctions.enableMemoizationOfMatchesForAllRules(this);
  }

  private void basicConcepts() {
    compilationUnit.is(o2n(externAliasDirective), o2n(usingDirective), opt(globalAttributes), o2n(namespaceMemberDeclaration), EOF);
    namespaceName.is(namespaceOrTypeName);
    typeName.is(namespaceOrTypeName);
    namespaceOrTypeName.is(
        firstOf(
            qualifiedAliasMember,
            and(IDENTIFIER, opt(typeArgumentList))),
        o2n(DOT, IDENTIFIER, opt(typeArgumentList)));
  }

  private void types() {
    simpleType.is(
        firstOf(
            numericType,
            BOOL));
    numericType.is(
        firstOf(
            integralType,
            floatingPointType,
            DECIMAL));
    integralType.is(
        firstOf(
            SBYTE,
            BYTE,
            SHORT,
            USHORT,
            INT,
            UINT,
            LONG,
            ULONG,
            CHAR));
    floatingPointType.is(
        firstOf(
            FLOAT,
            DOUBLE));

    rankSpecifier.is(LBRACKET, o2n(COMMA), RBRACKET);
    rankSpecifiers.is(one2n(rankSpecifier));

    typePrimary.is(
        firstOf(
            simpleType,
            "dynamic",
            OBJECT,
            STRING,
            typeName
        )).skip();
    nullableType.is(typePrimary, QUESTION, not(and(expression, COLON)));
    pointerType.is( // Moved from unsafe code to remove the left recursions
        firstOf(
            nullableType,
            typePrimary,
            VOID),
        STAR);
    arrayType.is(
        firstOf(
            pointerType,
            nullableType,
            typePrimary),
        rankSpecifiers);
    type.is(
        firstOf(
            arrayType,
            pointerType,
            nullableType,
            typePrimary));

    nonNullableValueType.is(not(nullableType), type);
    nonArrayType.is(not(arrayType), type);

    classType.is(
        firstOf(
            "dynamic",
            OBJECT,
            STRING,
            typeName));
    interfaceType.is(typeName);
    enumType.is(typeName);
    delegateType.is(typeName);
  }

  private void variables() {
    variableReference.is(expression);
  }

  private void expressions() {
    primaryExpressionPrimary.is(
        firstOf(
            arrayCreationExpression,
            primaryNoArrayCreationExpression)).skip();
    primaryNoArrayCreationExpression.is(
        firstOf(
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
            // NOTE : unsafe.pointerElementAccess deactivated here because it shadows the "elementAccess" in the main grammar...
            // Need to look into that later.
            unsafe(sizeOfExpression))).skip();

    postMemberAccess.is(DOT, IDENTIFIER, opt(typeArgumentList));
    postElementAccess.is(LBRACKET, argumentList, RBRACKET);
    postPointerMemberAccess.is(PTR_OP, IDENTIFIER);
    postIncrement.is(INC_OP);
    postDecrement.is(DEC_OP);
    postInvocation.is(LPARENTHESIS, opt(argumentList), RPARENTHESIS);

    postfixExpression.is(
        primaryExpressionPrimary,
        o2n(
        firstOf(
            postMemberAccess,
            postElementAccess,
            postPointerMemberAccess,
            postIncrement,
            postDecrement,
            postInvocation))).skipIfOneChild();
    primaryExpression.is(postfixExpression);

    argumentList.is(argument, o2n(COMMA, argument));
    argument.is(opt(argumentName), argumentValue);
    argumentName.is(IDENTIFIER, COLON);
    argumentValue.is(
        firstOf(
            expression,
            and(REF, variableReference),
            and(OUT, variableReference)));
    simpleName.is(IDENTIFIER, opt(typeArgumentList));
    parenthesizedExpression.is(LPARENTHESIS, expression, RPARENTHESIS);
    memberAccess.is(
        firstOf(
            and(qualifiedAliasMember, DOT, IDENTIFIER),
            and(predefinedType, DOT, IDENTIFIER, opt(typeArgumentList))));
    predefinedType.is(
        firstOf(
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
    thisAccess.is(THIS);
    // NOTE: baseAccess does not exactly stick to the specification: "opt(typeArgumentList)" has been added here, whereas it is not
    // present in the "base-access" rule in the specification of C# 4.0
    baseAccess.is(
        BASE,
        firstOf(
            and(DOT, IDENTIFIER, opt(typeArgumentList)),
            and(LBRACKET, argumentList, RBRACKET)));
    objectCreationExpression.is(
        NEW, type,
        firstOf(
            and(LPARENTHESIS, opt(argumentList), RPARENTHESIS, opt(objectOrCollectionInitializer)),
            objectOrCollectionInitializer));
    objectOrCollectionInitializer.is(
        firstOf(
            objectInitializer,
            collectionInitializer));
    objectInitializer.is(LCURLYBRACE, opt(memberInitializer), o2n(COMMA, memberInitializer), opt(COMMA), RCURLYBRACE);
    memberInitializer.is(IDENTIFIER, EQUAL, initializerValue);
    initializerValue.is(
        firstOf(
            expression,
            objectOrCollectionInitializer));
    collectionInitializer.is(LCURLYBRACE, elementInitializer, o2n(COMMA, elementInitializer), opt(COMMA), RCURLYBRACE);
    elementInitializer.is(
        firstOf(
            nonAssignmentExpression,
            and(LCURLYBRACE, expressionList, RCURLYBRACE)));
    expressionList.is(expression, o2n(COMMA, expression));
    arrayCreationExpression.is(
        firstOf(
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
        one2n(
            IDENTIFIER, opt(DOUBLE_COLON, IDENTIFIER), opt(genericDimensionSpecifier),
            opt(DOT, IDENTIFIER, opt(genericDimensionSpecifier))), opt(DOT, IDENTIFIER, opt(genericDimensionSpecifier)));
    genericDimensionSpecifier.is(INFERIOR, o2n(COMMA), SUPERIOR);
    checkedExpression.is(CHECKED, LPARENTHESIS, expression, RPARENTHESIS);
    uncheckedExpression.is(UNCHECKED, LPARENTHESIS, expression, RPARENTHESIS);
    defaultValueExpression.is(DEFAULT, LPARENTHESIS, type, RPARENTHESIS);

    unaryExpression.is(
        firstOf(
            and(LPARENTHESIS, type, RPARENTHESIS, unaryExpression),
            primaryExpression,
            and(
                firstOf(
                    MINUS,
                    EXCLAMATION,
                    INC_OP,
                    DEC_OP,
                    TILDE,
                    PLUS),
                unaryExpression),
            unsafe(
            firstOf(
                pointerIndirectionExpression,
                addressOfExpression)))).skipIfOneChild();

    multiplicativeExpression.is(
        unaryExpression,
        o2n(
            firstOf(
                STAR,
                SLASH,
                MODULO),
            unaryExpression)).skipIfOneChild();
    additiveExpression.is(
        multiplicativeExpression,
        o2n(
            firstOf(
                PLUS,
                MINUS),
            multiplicativeExpression)).skipIfOneChild();
    shiftExpression.is(
        additiveExpression,
        o2n(
            firstOf(
                LEFT_OP,
                rightShift),
            additiveExpression)).skipIfOneChild();
    relationalExpression.is(
        shiftExpression,
        o2n(
        firstOf(
            and(
                firstOf(
                    INFERIOR,
                    SUPERIOR,
                    LE_OP,
                    GE_OP),
                shiftExpression),
            and(
                firstOf(
                    IS,
                    AS),
                type)))).skipIfOneChild();
    equalityExpression.is(
        relationalExpression,
        o2n(
            firstOf(
                EQ_OP,
                NE_OP),
            relationalExpression)).skipIfOneChild();
    andExpression.is(equalityExpression, o2n(AND, equalityExpression)).skipIfOneChild();
    exclusiveOrExpression.is(andExpression, o2n(XOR, andExpression)).skipIfOneChild();
    inclusiveOrExpression.is(exclusiveOrExpression, o2n(OR, exclusiveOrExpression)).skipIfOneChild();
    conditionalAndExpression.is(inclusiveOrExpression, o2n(AND_OP, inclusiveOrExpression)).skipIfOneChild();
    conditionalOrExpression.is(conditionalAndExpression, o2n(OR_OP, conditionalAndExpression)).skipIfOneChild();
    nullCoalescingExpression.is(conditionalOrExpression, opt(DOUBLE_QUESTION, nullCoalescingExpression)).skipIfOneChild();
    conditionalExpression.is(nullCoalescingExpression, opt(QUESTION, expression, COLON, expression)).skipIfOneChild();
    lambdaExpression.is(anonymousFunctionSignature, LAMBDA, anonymousFunctionBody);
    anonymousMethodExpression.is(DELEGATE, opt(explicitAnonymousFunctionSignature), block);
    anonymousFunctionSignature.is(
        firstOf(
            explicitAnonymousFunctionSignature,
            implicitAnonymousFunctionSignature));
    explicitAnonymousFunctionSignature.is(LPARENTHESIS, opt(explicitAnonymousFunctionParameter, o2n(COMMA, explicitAnonymousFunctionParameter)), RPARENTHESIS);
    explicitAnonymousFunctionParameter.is(opt(anonymousFunctionParameterModifier), type, IDENTIFIER);
    anonymousFunctionParameterModifier.is(
        firstOf(
            "ref",
            "out"));
    implicitAnonymousFunctionSignature.is(
        firstOf(
            implicitAnonymousFunctionParameter,
            and(LPARENTHESIS, opt(implicitAnonymousFunctionParameter, o2n(COMMA, implicitAnonymousFunctionParameter)), RPARENTHESIS)));
    implicitAnonymousFunctionParameter.is(IDENTIFIER);
    anonymousFunctionBody.is(
        firstOf(
            expression,
            block));
    queryExpression.is(fromClause, queryBody);
    fromClause.is(
        "from",
        firstOf(
            and(type, IDENTIFIER),
            IDENTIFIER),
        IN, expression);
    queryBody.is(o2n(queryBodyClause), selectOrGroupClause, opt(queryContinuation));
    queryBodyClause.is(
        firstOf(
            fromClause,
            letClause,
            whereClause,
            joinIntoClause,
            joinClause,
            orderByClause));
    letClause.is("let", IDENTIFIER, EQUAL, expression);
    whereClause.is("where", expression);
    joinClause.is(
        "join",
        firstOf(
            and(type, IDENTIFIER),
            IDENTIFIER),
        IN, expression, "on", expression, "equals", expression);
    joinIntoClause.is(
        "join",
        firstOf(
            and(type, IDENTIFIER),
            IDENTIFIER),
        IN, expression, "on", expression, "equals", expression,
        "into", IDENTIFIER);
    orderByClause.is("orderby", ordering, o2n(COMMA, ordering));
    ordering.is(expression, opt(orderingDirection));
    orderingDirection.is(
        firstOf(
            "ascending",
            "descending"));
    selectOrGroupClause.is(
        firstOf(
            selectClause,
            groupClause));
    selectClause.is("select", expression);
    groupClause.is("group", expression, "by", expression);
    queryContinuation.is("into", IDENTIFIER, queryBody);
    assignment.is(
        unaryExpression,
        firstOf(
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
    nonAssignmentExpression.is(
        firstOf(
            lambdaExpression,
            queryExpression,
            conditionalExpression)).skip();
    expression.is(
        firstOf(
            assignment,
            nonAssignmentExpression));
  }

  private void statements() {
    statement.is(
        firstOf(
            labeledStatement,
            declarationStatement,
            embeddedStatement));
    embeddedStatement.is(
        firstOf(
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
    block.is(LCURLYBRACE, o2n(statement), RCURLYBRACE);
    labeledStatement.is(IDENTIFIER, COLON, statement);
    declarationStatement.is(
        firstOf(
            localVariableDeclaration,
            localConstantDeclaration),
        SEMICOLON);
    localVariableDeclaration.is(type, localVariableDeclarator, o2n(COMMA, localVariableDeclarator));
    localVariableDeclarator.is(IDENTIFIER, opt(EQUAL, localVariableInitializer));
    localVariableInitializer.is(
        firstOf(
            expression,
            arrayInitializer,
            unsafe(stackallocInitializer)));
    localConstantDeclaration.is(CONST, type, constantDeclarator, o2n(COMMA, constantDeclarator));
    constantDeclarator.is(IDENTIFIER, EQUAL, expression);
    expressionStatement.is(expression, SEMICOLON);
    selectionStatement.is(
        firstOf(
            ifStatement,
            switchStatement));
    ifStatement.is(IF, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement, opt(ELSE, embeddedStatement));
    switchStatement.is(SWITCH, LPARENTHESIS, expression, RPARENTHESIS, LCURLYBRACE, o2n(switchSection), RCURLYBRACE);
    switchSection.is(one2n(switchLabel), one2n(statement));
    switchLabel.is(
        firstOf(
            and(CASE, expression, COLON),
            and(DEFAULT, COLON)));
    iterationStatement.is(
        firstOf(
            whileStatement,
            doStatement,
            forStatement,
            foreachStatement));
    whileStatement.is(WHILE, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement);
    doStatement.is(DO, embeddedStatement, WHILE, LPARENTHESIS, expression, RPARENTHESIS, SEMICOLON);
    forStatement.is(FOR, LPARENTHESIS, opt(forInitializer), SEMICOLON, opt(forCondition), SEMICOLON, opt(forIterator), RPARENTHESIS, embeddedStatement);
    forInitializer.is(
        firstOf(
            localVariableDeclaration,
            statementExpressionList));
    forCondition.is(expression);
    forIterator.is(statementExpressionList);
    statementExpressionList.is(expression, o2n(COMMA, expression));
    foreachStatement.is(FOREACH, LPARENTHESIS, type, IDENTIFIER, IN, expression, RPARENTHESIS, embeddedStatement);
    jumpStatement.is(
        firstOf(
            breakStatement,
            continueStatement,
            gotoStatement,
            returnStatement,
            throwStatement));
    breakStatement.is(BREAK, SEMICOLON);
    continueStatement.is(CONTINUE, SEMICOLON);
    gotoStatement.is(
        GOTO,
        firstOf(
            IDENTIFIER,
            and(CASE, expression),
            DEFAULT),
        SEMICOLON);
    returnStatement.is(RETURN, opt(expression), SEMICOLON);
    throwStatement.is(THROW, opt(expression), SEMICOLON);
    tryStatement.is(
        TRY, block,
        firstOf(
            and(opt(catchClauses), finallyClause),
            catchClauses));
    catchClauses.is(
        firstOf(
            and(o2n(specificCatchClause), generalCatchClause),
            one2n(specificCatchClause)));
    specificCatchClause.is(CATCH, LPARENTHESIS, classType, opt(IDENTIFIER), RPARENTHESIS, block);
    generalCatchClause.is(CATCH, block);
    finallyClause.is(FINALLY, block);
    checkedStatement.is(CHECKED, block);
    uncheckedStatement.is(UNCHECKED, block);
    lockStatement.is(LOCK, LPARENTHESIS, expression, RPARENTHESIS, embeddedStatement);
    usingStatement.is(USING, LPARENTHESIS, resourceAcquisition, RPARENTHESIS, embeddedStatement);
    resourceAcquisition.is(
        firstOf(
            localVariableDeclaration,
            expression));
    yieldStatement.is(
        "yield",
        firstOf(
            and(RETURN, expression),
            BREAK),
        SEMICOLON);
    namespaceDeclaration.is(NAMESPACE, qualifiedIdentifier, namespaceBody, opt(SEMICOLON));
    qualifiedIdentifier.is(IDENTIFIER, o2n(DOT, IDENTIFIER));
    namespaceBody.is(LCURLYBRACE, o2n(externAliasDirective), o2n(usingDirective), o2n(namespaceMemberDeclaration), RCURLYBRACE);
    externAliasDirective.is(EXTERN, "alias", IDENTIFIER, SEMICOLON);
    usingDirective.is(
        firstOf(
            usingAliasDirective,
            usingNamespaceDirective));
    usingAliasDirective.is(USING, IDENTIFIER, EQUAL, namespaceOrTypeName, SEMICOLON);
    usingNamespaceDirective.is(USING, namespaceName, SEMICOLON);
    namespaceMemberDeclaration.is(
        firstOf(
            namespaceDeclaration,
            typeDeclaration));
    typeDeclaration.is(
        firstOf(
            classDeclaration,
            structDeclaration,
            interfaceDeclaration,
            enumDeclaration,
            delegateDeclaration));
    qualifiedAliasMember.is(IDENTIFIER, DOUBLE_COLON, IDENTIFIER, opt(typeArgumentList));
  }

  private void classes() {
    classDeclaration.is(
        opt(attributes), o2n(classModifier), opt(PARTIAL),
        CLASS, IDENTIFIER, opt(typeParameterList), opt(classBase), opt(typeParameterConstraintsClauses),
        classBody,
        opt(SEMICOLON));
    classModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            ABSTRACT,
            SEALED,
            STATIC,
            UNSAFE));
    classBase.is(
        COLON,
        firstOf(
            and(classType, COMMA, interfaceTypeList),
            classType,
            interfaceTypeList));
    interfaceTypeList.is(interfaceType, o2n(COMMA, interfaceType));
    classBody.is(LCURLYBRACE, o2n(classMemberDeclaration), RCURLYBRACE);
    classMemberDeclaration.is(
        firstOf(
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
    constantDeclaration.is(
        opt(attributes), o2n(constantModifier),
        CONST, type,
        constantDeclarator, o2n(COMMA, constantDeclarator),
        SEMICOLON);
    constantModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE));
    fieldDeclaration.is(
        opt(attributes), o2n(fieldModifier),
        type,
        variableDeclarator, o2n(COMMA, variableDeclarator),
        SEMICOLON);
    fieldModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            STATIC,
            READONLY,
            VOLATILE,
            UNSAFE));
    variableDeclarator.is(IDENTIFIER, opt(EQUAL, variableInitializer));
    variableInitializer.is(
        firstOf(
            expression,
            arrayInitializer));
    methodDeclaration.is(methodHeader, methodBody);
    methodHeader.is(
        opt(attributes), o2n(methodModifier), opt(PARTIAL),
        returnType, memberName,
        opt(typeParameterList),
        LPARENTHESIS, opt(formalParameterList), RPARENTHESIS,
        opt(typeParameterConstraintsClauses)).skip();
    methodModifier.is(
        firstOf(
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
    returnType.is(
        firstOf(
            type,
            VOID));
    memberName.is(
        o2n(
            firstOf(
                qualifiedAliasMember,
                and(
                    firstOf(
                        THIS,
                        IDENTIFIER),
                    opt(typeArgumentList))),
            DOT),
        firstOf(
            THIS,
            IDENTIFIER),
        opt(typeArgumentList));
    methodBody.is(
        firstOf(
            block,
            SEMICOLON));
    formalParameterList.is(
        firstOf(
            and(fixedParameters, opt(COMMA, parameterArray)),
            parameterArray));
    fixedParameters.is(fixedParameter, o2n(COMMA, fixedParameter));
    fixedParameter.is(opt(attributes), opt(parameterModifier), type, IDENTIFIER, opt(EQUAL, expression));
    parameterModifier.is(
        firstOf(
            REF,
            OUT,
            THIS));
    parameterArray.is(opt(attributes), PARAMS, arrayType, IDENTIFIER);
    propertyDeclaration.is(opt(attributes), o2n(propertyModifier), type, memberName, LCURLYBRACE, accessorDeclarations, RCURLYBRACE);
    propertyModifier.is(
        firstOf(
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
    accessorDeclarations.is(
        firstOf(
            and(getAccessorDeclaration, opt(setAccessorDeclaration)),
            and(setAccessorDeclaration, opt(getAccessorDeclaration))));
    getAccessorDeclaration.is(opt(attributes), o2n(accessorModifier), GET, accessorBody);
    setAccessorDeclaration.is(opt(attributes), o2n(accessorModifier), SET, accessorBody);
    accessorModifier.is(
        firstOf(
            and(PROTECTED, INTERNAL),
            and(INTERNAL, PROTECTED),
            PROTECTED,
            INTERNAL,
            PRIVATE));
    accessorBody.is(
        firstOf(
            block,
            SEMICOLON));
    eventDeclaration.is(
        opt(attributes),
        o2n(eventModifier),
        EVENT, type,
        firstOf(
            and(variableDeclarator, o2n(COMMA, variableDeclarator), SEMICOLON),
            and(memberName, LCURLYBRACE, eventAccessorDeclarations, RCURLYBRACE)));
    eventModifier.is(
        firstOf(
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
    eventAccessorDeclarations.is(
        firstOf(
            and(addAccessorDeclaration, removeAccessorDeclaration),
            and(removeAccessorDeclaration, addAccessorDeclaration)));
    addAccessorDeclaration.is(opt(attributes), "add", block);
    removeAccessorDeclaration.is(opt(attributes), "remove", block);
    indexerDeclaration.is(
        opt(attributes), o2n(indexerModifier),
        indexerDeclarator, LCURLYBRACE, accessorDeclarations, RCURLYBRACE);
    indexerModifier.is(
        firstOf(
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
    indexerDeclarator.is(
        type,
        o2n(
            firstOf(
                qualifiedAliasMember,
                and(IDENTIFIER, opt(typeArgumentList))),
            DOT),
        THIS, LBRACKET, formalParameterList, RBRACKET);
    operatorDeclaration.is(opt(attributes), one2n(operatorModifier), operatorDeclarator, operatorBody);
    operatorModifier.is(
        firstOf(
            PUBLIC,
            STATIC,
            EXTERN,
            UNSAFE));
    operatorDeclarator.is(
        firstOf(
            unaryOperatorDeclarator,
            binaryOperatorDeclarator,
            conversionOperatorDeclarator));
    unaryOperatorDeclarator.is(type, OPERATOR, overloadableUnaryOperator, LPARENTHESIS, type, IDENTIFIER, RPARENTHESIS);
    overloadableUnaryOperator.is(
        firstOf(
            PLUS,
            MINUS,
            EXCLAMATION,
            TILDE,
            INC_OP,
            DEC_OP,
            TRUE,
            FALSE));
    binaryOperatorDeclarator.is(type, OPERATOR, overloadableBinaryOperator, LPARENTHESIS, type, IDENTIFIER, COMMA, type, IDENTIFIER, RPARENTHESIS);
    overloadableBinaryOperator.is(
        firstOf(
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
    conversionOperatorDeclarator.is(
        firstOf(
            IMPLICIT,
            EXPLICIT),
        OPERATOR, type, LPARENTHESIS, type, IDENTIFIER, RPARENTHESIS);
    operatorBody.is(
        firstOf(
            block,
            SEMICOLON));
    constructorDeclaration.is(opt(attributes), o2n(constructorModifier), constructorDeclarator, constructorBody);
    constructorModifier.is(
        firstOf(
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            EXTERN,
            UNSAFE));
    constructorDeclarator.is(IDENTIFIER, LPARENTHESIS, opt(formalParameterList), RPARENTHESIS, opt(constructorInitializer));
    constructorInitializer.is(
        COLON,
        firstOf(
            BASE,
            THIS),
        LPARENTHESIS, opt(argumentList), RPARENTHESIS);
    constructorBody.is(
        firstOf(
            block,
            SEMICOLON));
    staticConstructorDeclaration.is(
        opt(attributes),
        staticConstructorModifiers, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, staticConstructorBody);
    staticConstructorModifiers.is(
        firstOf(
            and(opt(EXTERN), STATIC, not(next(EXTERN))),
            and(STATIC, opt(EXTERN))));
    staticConstructorBody.is(
        firstOf(
            block,
            SEMICOLON));
    destructorDeclaration.is(
        opt(attributes), opt(EXTERN),
        TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, destructorBody);
    destructorBody.is(
        firstOf(
            block,
            SEMICOLON));
  }

  private void structs() {
    structDeclaration.is(
        opt(attributes), o2n(structModifier), opt(PARTIAL),
        STRUCT, IDENTIFIER,
        opt(typeParameterList), opt(structInterfaces), opt(typeParameterConstraintsClauses),
        structBody,
        opt(SEMICOLON));
    structModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    structInterfaces.is(COLON, interfaceTypeList);
    structBody.is(LCURLYBRACE, o2n(structMemberDeclaration), RCURLYBRACE);
    structMemberDeclaration.is(
        firstOf(
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

  private void arrays() {
    arrayInitializer.is(
        LCURLYBRACE,
        opt(
        firstOf(
            and(variableInitializerList, COMMA),
            variableInitializerList)),
        RCURLYBRACE);
    variableInitializerList.is(variableInitializer, o2n(COMMA, variableInitializer));
  }

  private void interfaces() {
    interfaceDeclaration.is(
        opt(attributes), o2n(interfaceModifier), opt(PARTIAL),
        INTERFACE, IDENTIFIER,
        opt(variantTypeParameterList), opt(interfaceBase), opt(typeParameterConstraintsClauses),
        interfaceBody,
        opt(SEMICOLON));
    interfaceModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    variantTypeParameterList.is(INFERIOR, variantTypeParameter, o2n(COMMA, variantTypeParameter), SUPERIOR);
    variantTypeParameter.is(opt(attributes), opt(varianceAnnotation), typeParameter);
    varianceAnnotation.is(
        firstOf(
            IN,
            OUT));
    interfaceBase.is(COLON, interfaceTypeList);
    interfaceBody.is(LCURLYBRACE, o2n(interfaceMemberDeclaration), RCURLYBRACE);
    interfaceMemberDeclaration.is(
        firstOf(
            interfaceMethodDeclaration,
            interfacePropertyDeclaration,
            interfaceEventDeclaration,
            interfaceIndexerDeclaration));
    interfaceMethodDeclaration.is(
        opt(attributes), opt(NEW),
        returnType, IDENTIFIER,
        opt(typeParameterList),
        LPARENTHESIS, opt(formalParameterList), RPARENTHESIS, opt(typeParameterConstraintsClauses),
        SEMICOLON);
    interfacePropertyDeclaration.is(
        opt(attributes), opt(NEW),
        type, IDENTIFIER, LCURLYBRACE, interfaceAccessors, RCURLYBRACE);
    interfaceAccessors.is(
        opt(attributes),
        firstOf(
            and(GET, SEMICOLON, opt(attributes), SET),
            and(SET, SEMICOLON, opt(attributes), GET),
            GET,
            SET),
        SEMICOLON);
    interfaceEventDeclaration.is(
        opt(attributes), opt(NEW),
        EVENT, type, IDENTIFIER, SEMICOLON);
    interfaceIndexerDeclaration.is(
        opt(attributes), opt(NEW),
        type, THIS, LBRACKET, formalParameterList, RBRACKET, LCURLYBRACE, interfaceAccessors, RCURLYBRACE);
  }

  private void enums() {
    enumDeclaration.is(opt(attributes), o2n(enumModifier), ENUM, IDENTIFIER, opt(enumBase), enumBody, opt(SEMICOLON));
    enumBase.is(COLON, integralType);
    enumBody.is(
        LCURLYBRACE,
        opt(
        firstOf(
            and(enumMemberDeclarations, COMMA),
            enumMemberDeclarations)),
        RCURLYBRACE);
    enumModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE));
    enumMemberDeclarations.is(enumMemberDeclaration, o2n(COMMA, enumMemberDeclaration));
    enumMemberDeclaration.is(opt(attributes), IDENTIFIER, opt(EQUAL, expression));
  }

  private void delegates() {
    delegateDeclaration.is(
        opt(attributes), o2n(delegateModifier),
        DELEGATE, returnType, IDENTIFIER,
        opt(variantTypeParameterList),
        LPARENTHESIS, opt(formalParameterList), RPARENTHESIS,
        opt(typeParameterConstraintsClauses),
        SEMICOLON);
    delegateModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
  }

  private void attributes() {
    globalAttributes.is(one2n(globalAttributeSection));
    globalAttributeSection.is(LBRACKET, globalAttributeTargetSpecifier, attributeList, opt(COMMA), RBRACKET);
    globalAttributeTargetSpecifier.is(globalAttributeTarget, COLON);
    globalAttributeTarget.is(
        firstOf(
            "assembly",
            "module"));
    attributes.is(one2n(attributeSection));
    attributeSection.is(LBRACKET, opt(attributeTargetSpecifier), attributeList, opt(COMMA), RBRACKET);
    attributeTargetSpecifier.is(attributeTarget, COLON);
    attributeTarget.is(
        firstOf(
            "field",
            "event",
            "method",
            "param",
            "property",
            RETURN,
            "type"));
    attributeList.is(attribute, o2n(COMMA, attribute));
    attribute.is(attributeName, opt(attributeArguments));
    attributeName.is(typeName);
    attributeArguments.is(
        LPARENTHESIS,
        opt(
            firstOf(
                namedArgument,
                positionalArgument),
            o2n(
                COMMA,
                firstOf(
                    namedArgument,
                    positionalArgument))),
        RPARENTHESIS);
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
    typeParameterConstraints.is(
        firstOf(
            and(primaryConstraint, COMMA, secondaryConstraints, COMMA, constructorConstraint),
            and(
                primaryConstraint, COMMA,
                firstOf(
                    secondaryConstraints,
                    constructorConstraint)),
            and(secondaryConstraints, COMMA, constructorConstraint),
            primaryConstraint,
            secondaryConstraints,
            constructorConstraint));
    primaryConstraint.is(
        firstOf(
            classType,
            CLASS,
            STRUCT));
    secondaryConstraints.is(
        firstOf(
            interfaceType,
            typeParameter),
        o2n(
            COMMA,
            firstOf(
                interfaceType,
                typeParameter)));
    constructorConstraint.is(NEW, LPARENTHESIS, RPARENTHESIS);
  }

  /**
   * Syntactic sugar to highlight constructs, which were moved from {@link unsafe}
   * to get rid of call to {@link com.sonar.sslr.api.Rule#or} (removed in SSLR 1.13).
   */
  private Object unsafe(Object matcher) {
    return matcher;
  }

  private void unsafe() {
    destructorDeclaration.override(
        opt(attributes),
        o2n(
        firstOf(
            EXTERN,
            UNSAFE)),
        TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, destructorBody);
    // FIXME override!
    staticConstructorModifiers.override(
        o2n(
        firstOf(
            EXTERN,
            UNSAFE)),
        STATIC,
        o2n(
        firstOf(
            EXTERN,
            UNSAFE)));
    // FIXME override!
    embeddedStatement.override(
        firstOf(
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
    unsafeStatement.is(UNSAFE, block);
    pointerIndirectionExpression.is(STAR, unaryExpression);
    pointerElementAccess.is(primaryNoArrayCreationExpression, LBRACKET, expression, RBRACKET);
    addressOfExpression.is(AND, unaryExpression);
    sizeOfExpression.is(SIZEOF, LPARENTHESIS, type, RPARENTHESIS);
    fixedStatement.is(
        FIXED,
        LPARENTHESIS, pointerType, fixedPointerDeclarator, o2n(COMMA, fixedPointerDeclarator), RPARENTHESIS,
        embeddedStatement);
    fixedPointerDeclarator.is(IDENTIFIER, EQUAL, fixedPointerInitializer);
    fixedPointerInitializer.is(
        firstOf(
            and(AND, variableReference),
            stackallocInitializer,
            expression));
    fixedSizeBufferDeclaration.is(
        opt(attributes), o2n(fixedSizeBufferModifier),
        FIXED, type, one2n(fixedSizeBufferDeclarator), SEMICOLON);
    fixedSizeBufferModifier.is(
        firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    fixedSizeBufferDeclarator.is(IDENTIFIER, LBRACKET, expression, RBRACKET);
    stackallocInitializer.is(STACKALLOC, type, LBRACKET, expression, RBRACKET);
  }

}
