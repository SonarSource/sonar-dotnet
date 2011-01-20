/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import static com.sonar.csharp.api.CSharpKeyword.ABSTRACT;
import static com.sonar.csharp.api.CSharpKeyword.AS;
import static com.sonar.csharp.api.CSharpKeyword.BASE;
import static com.sonar.csharp.api.CSharpKeyword.BOOL;
import static com.sonar.csharp.api.CSharpKeyword.BREAK;
import static com.sonar.csharp.api.CSharpKeyword.BYTE;
import static com.sonar.csharp.api.CSharpKeyword.CASE;
import static com.sonar.csharp.api.CSharpKeyword.CATCH;
import static com.sonar.csharp.api.CSharpKeyword.CHAR;
import static com.sonar.csharp.api.CSharpKeyword.CHECKED;
import static com.sonar.csharp.api.CSharpKeyword.CLASS;
import static com.sonar.csharp.api.CSharpKeyword.CONST;
import static com.sonar.csharp.api.CSharpKeyword.CONTINUE;
import static com.sonar.csharp.api.CSharpKeyword.DECIMAL;
import static com.sonar.csharp.api.CSharpKeyword.DEFAULT;
import static com.sonar.csharp.api.CSharpKeyword.DELEGATE;
import static com.sonar.csharp.api.CSharpKeyword.DO;
import static com.sonar.csharp.api.CSharpKeyword.DOUBLE;
import static com.sonar.csharp.api.CSharpKeyword.ELSE;
import static com.sonar.csharp.api.CSharpKeyword.ENUM;
import static com.sonar.csharp.api.CSharpKeyword.EVENT;
import static com.sonar.csharp.api.CSharpKeyword.EXPLICIT;
import static com.sonar.csharp.api.CSharpKeyword.EXTERN;
import static com.sonar.csharp.api.CSharpKeyword.FALSE;
import static com.sonar.csharp.api.CSharpKeyword.FINALLY;
import static com.sonar.csharp.api.CSharpKeyword.FLOAT;
import static com.sonar.csharp.api.CSharpKeyword.FOR;
import static com.sonar.csharp.api.CSharpKeyword.FOREACH;
import static com.sonar.csharp.api.CSharpKeyword.GOTO;
import static com.sonar.csharp.api.CSharpKeyword.IF;
import static com.sonar.csharp.api.CSharpKeyword.IMPLICIT;
import static com.sonar.csharp.api.CSharpKeyword.IN;
import static com.sonar.csharp.api.CSharpKeyword.INT;
import static com.sonar.csharp.api.CSharpKeyword.INTERFACE;
import static com.sonar.csharp.api.CSharpKeyword.INTERNAL;
import static com.sonar.csharp.api.CSharpKeyword.IS;
import static com.sonar.csharp.api.CSharpKeyword.LOCK;
import static com.sonar.csharp.api.CSharpKeyword.LONG;
import static com.sonar.csharp.api.CSharpKeyword.NAMESPACE;
import static com.sonar.csharp.api.CSharpKeyword.NEW;
import static com.sonar.csharp.api.CSharpKeyword.NULL;
import static com.sonar.csharp.api.CSharpKeyword.OBJECT;
import static com.sonar.csharp.api.CSharpKeyword.OPERATOR;
import static com.sonar.csharp.api.CSharpKeyword.OUT;
import static com.sonar.csharp.api.CSharpKeyword.OVERRIDE;
import static com.sonar.csharp.api.CSharpKeyword.PARAMS;
import static com.sonar.csharp.api.CSharpKeyword.PRIVATE;
import static com.sonar.csharp.api.CSharpKeyword.PROTECTED;
import static com.sonar.csharp.api.CSharpKeyword.PUBLIC;
import static com.sonar.csharp.api.CSharpKeyword.READONLY;
import static com.sonar.csharp.api.CSharpKeyword.REF;
import static com.sonar.csharp.api.CSharpKeyword.RETURN;
import static com.sonar.csharp.api.CSharpKeyword.SBYTE;
import static com.sonar.csharp.api.CSharpKeyword.SEALED;
import static com.sonar.csharp.api.CSharpKeyword.SHORT;
import static com.sonar.csharp.api.CSharpKeyword.STATIC;
import static com.sonar.csharp.api.CSharpKeyword.STRING;
import static com.sonar.csharp.api.CSharpKeyword.STRUCT;
import static com.sonar.csharp.api.CSharpKeyword.SWITCH;
import static com.sonar.csharp.api.CSharpKeyword.THIS;
import static com.sonar.csharp.api.CSharpKeyword.THROW;
import static com.sonar.csharp.api.CSharpKeyword.TRUE;
import static com.sonar.csharp.api.CSharpKeyword.TRY;
import static com.sonar.csharp.api.CSharpKeyword.TYPEOF;
import static com.sonar.csharp.api.CSharpKeyword.UINT;
import static com.sonar.csharp.api.CSharpKeyword.ULONG;
import static com.sonar.csharp.api.CSharpKeyword.UNCHECKED;
import static com.sonar.csharp.api.CSharpKeyword.USHORT;
import static com.sonar.csharp.api.CSharpKeyword.USING;
import static com.sonar.csharp.api.CSharpKeyword.VIRTUAL;
import static com.sonar.csharp.api.CSharpKeyword.VOID;
import static com.sonar.csharp.api.CSharpKeyword.VOLATILE;
import static com.sonar.csharp.api.CSharpKeyword.WHILE;
import static com.sonar.csharp.api.CSharpPunctuator.ADD_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.AND;
import static com.sonar.csharp.api.CSharpPunctuator.AND_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.AND_OP;
import static com.sonar.csharp.api.CSharpPunctuator.COLON;
import static com.sonar.csharp.api.CSharpPunctuator.COMMA;
import static com.sonar.csharp.api.CSharpPunctuator.DEC_OP;
import static com.sonar.csharp.api.CSharpPunctuator.DIV_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.DOT;
import static com.sonar.csharp.api.CSharpPunctuator.DOUBLE_COLON;
import static com.sonar.csharp.api.CSharpPunctuator.DOUBLE_QUESTION;
import static com.sonar.csharp.api.CSharpPunctuator.EQUAL;
import static com.sonar.csharp.api.CSharpPunctuator.EQ_OP;
import static com.sonar.csharp.api.CSharpPunctuator.EXCLAMATION;
import static com.sonar.csharp.api.CSharpPunctuator.GE_OP;
import static com.sonar.csharp.api.CSharpPunctuator.INC_OP;
import static com.sonar.csharp.api.CSharpPunctuator.INFERIOR;
import static com.sonar.csharp.api.CSharpPunctuator.LBRACKET;
import static com.sonar.csharp.api.CSharpPunctuator.LCURLYBRACE;
import static com.sonar.csharp.api.CSharpPunctuator.LEFT_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.LEFT_OP;
import static com.sonar.csharp.api.CSharpPunctuator.LE_OP;
import static com.sonar.csharp.api.CSharpPunctuator.LPARENTHESIS;
import static com.sonar.csharp.api.CSharpPunctuator.MINUS;
import static com.sonar.csharp.api.CSharpPunctuator.MODULO;
import static com.sonar.csharp.api.CSharpPunctuator.MOD_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.MUL_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.NE_OP;
import static com.sonar.csharp.api.CSharpPunctuator.OR;
import static com.sonar.csharp.api.CSharpPunctuator.OR_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.OR_OP;
import static com.sonar.csharp.api.CSharpPunctuator.PLUS;
import static com.sonar.csharp.api.CSharpPunctuator.QUESTION;
import static com.sonar.csharp.api.CSharpPunctuator.RBRACKET;
import static com.sonar.csharp.api.CSharpPunctuator.RCURLYBRACE;
import static com.sonar.csharp.api.CSharpPunctuator.RIGHT_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.RIGHT_OP;
import static com.sonar.csharp.api.CSharpPunctuator.RPARENTHESIS;
import static com.sonar.csharp.api.CSharpPunctuator.SEMICOLON;
import static com.sonar.csharp.api.CSharpPunctuator.SLASH;
import static com.sonar.csharp.api.CSharpPunctuator.STAR;
import static com.sonar.csharp.api.CSharpPunctuator.SUB_ASSIGN;
import static com.sonar.csharp.api.CSharpPunctuator.SUPERIOR;
import static com.sonar.csharp.api.CSharpPunctuator.TILDE;
import static com.sonar.csharp.api.CSharpPunctuator.XOR;
import static com.sonar.csharp.api.CSharpPunctuator.XOR_ASSIGN;
import static com.sonar.csharp.api.CSharpTokenType.CHARACTER_LITERAL;
import static com.sonar.csharp.api.CSharpTokenType.INTEGER_DEC_LITERAL;
import static com.sonar.csharp.api.CSharpTokenType.INTEGER_HEX_LITERAL;
import static com.sonar.csharp.api.CSharpTokenType.REAL_LITERAL;
import static com.sonar.csharp.api.CSharpTokenType.STRING_LITERAL;
import static com.sonar.sslr.api.GenericTokenType.EOF;
import static com.sonar.sslr.api.GenericTokenType.IDENTIFIER;
import static com.sonar.sslr.impl.matcher.Matchers.and;
import static com.sonar.sslr.impl.matcher.Matchers.isOneOfThem;
import static com.sonar.sslr.impl.matcher.Matchers.next;
import static com.sonar.sslr.impl.matcher.Matchers.not;
import static com.sonar.sslr.impl.matcher.Matchers.o2n;
import static com.sonar.sslr.impl.matcher.Matchers.one2n;
import static com.sonar.sslr.impl.matcher.Matchers.opt;
import static com.sonar.sslr.impl.matcher.Matchers.or;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.sslr.api.GrammarDecorator;
import com.sonar.sslr.impl.GrammarRuleLifeCycleManager;

/**
 * Definition of each element of the C# grammar.
 */
public class CSharpGrammarDecorator implements GrammarDecorator<CSharpGrammar> {

  /**
   * ${@inheritDoc}
   */
  public void decorate(CSharpGrammar g) {
    GrammarRuleLifeCycleManager.initializeLeftRecursionRuleFields(g, CSharpGrammar.class);

    // We follow the ECMA specification for the C# language, 4th edition of June 2006
    g.literal.isOr(TRUE, FALSE, INTEGER_DEC_LITERAL, INTEGER_HEX_LITERAL, REAL_LITERAL, CHARACTER_LITERAL, STRING_LITERAL, NULL);

    // A.2.1 Basic concepts
    basicConcepts(g);

    // A.2.2 Types
    types(g);

    // A.2.3 Variables
    variables(g);

    // A.2.4 Expressions
    expressions(g);

    // A.2.5 Statements
    statements(g);

    // A.2.6 Classes
    classes(g);

    // A.2.7 Struct
    structs(g);

    // A.2.8 Arrays
    arrays(g);

    // A.2.9 Interfaces
    interfaces(g);

    // A.2.10 Enums
    enums(g);

    // A.2.11 Delegates
    delegates(g);

    // A.2.12 Attributes
    attributes(g);

    // A.2.13 Generics
    generics(g);

  }

  private void basicConcepts(CSharpGrammar g) {
    g.compilationUnit.is(o2n(g.externAliasDirective), o2n(g.usingDirective), opt(g.globalAttributes), o2n(g.namespaceMemberDeclaration),
        EOF);
    g.namespaceName.is(g.namespaceOrTypeName);
    g.typeName.is(g.namespaceOrTypeName);
    g.namespaceOrTypeName.is(o2n(or(g.qualifiedAliasMember, and(IDENTIFIER, opt(g.typeArgumentList))), DOT), IDENTIFIER,
        opt(g.typeArgumentList));

  }

  private void types(CSharpGrammar g) {
    g.type.isOr(g.referenceType, g.valueType, g.typeParameter);
    g.valueType.isOr(g.structType, g.enumType);
    g.structType.isOr(g.nullableType, g.typeName, g.simpleType);
    g.simpleType.isOr(g.numericType, BOOL);
    g.numericType.isOr(g.integralType, g.floatingPointType, DECIMAL);
    g.integralType.isOr(SBYTE, BYTE, SHORT, USHORT, INT, UINT, LONG, ULONG, CHAR);
    g.floatingPointType.isOr(FLOAT, DOUBLE);
    g.enumType.is(g.typeName);
    g.nullableType.is(g.nonNullableValueType, QUESTION);
    g.nonNullableValueType.isOr(g.enumType, g.typeName, g.simpleType);
    g.referenceType.isOr(g.arrayType, g.classType, g.interfaceType, g.delegateType);
    g.classType.isOr(g.typeName, OBJECT, STRING);
    g.interfaceType.is(g.typeName);
    g.arrayType.is(g.nonArrayType, one2n(g.rankSpecifier));
    g.nonArrayType.isOr(g.valueType, g.classType, g.interfaceType, g.delegateType, g.typeParameter);
    g.rankSpecifier.is(LBRACKET, o2n(COMMA), RBRACKET);
    g.delegateType.is(g.typeName);
  }

  private void variables(CSharpGrammar g) {
    g.variableReference.is(g.expression);
  }

  private void expressions(CSharpGrammar g) {
    g.argumentList.is(g.argument, o2n(COMMA, g.argument));
    g.argument.isOr(g.expression, and(REF, g.variableReference), and(OUT, g.variableReference));
    g.primaryExpression.isOr(g.arrayCreationExpression, g.primaryNoArrayCreationExpression);
    g.primaryNoArrayCreationExpression.isOr(g.literal, g.simpleName, g.parenthesizedExpression, g.elementAccess, g.memberAccess,
        g.invocationExpression, g.thisAccess, g.baseAccess, g.postIncrementExpression, g.postDecrementExpression,
        g.objectCreationExpression, g.delegateCreationExpression, g.typeOfExpression, g.checkedExpression, g.uncheckedExpression,
        g.defaultValueExpression, g.anonymousMethodExpression);
    g.simpleName.is(IDENTIFIER, opt(g.typeArgumentList));
    g.parenthesizedExpression.is(LPARENTHESIS, g.expression, RPARENTHESIS);
    g.memberAccess.is(or(g.primaryExpression, g.predefinedType, g.qualifiedAliasMember), DOT, IDENTIFIER, opt(g.typeArgumentList));
    g.predefinedType.isOr(BOOL, BYTE, CHAR, DECIMAL, DOUBLE, FLOAT, INT, LONG, OBJECT, SBYTE, SHORT, STRING, UINT, ULONG, USHORT);
    g.invocationExpression.is(g.primaryExpression, LPARENTHESIS, opt(g.argumentList), RPARENTHESIS);
    g.elementAccess.is(g.primaryNoArrayCreationExpression, LBRACKET, g.expressionList, RBRACKET);
    g.expressionList.is(g.expression, o2n(COMMA, g.expression));
    g.thisAccess.is(THIS);
    g.baseAccess.is(BASE, or(and(DOT, IDENTIFIER, opt(g.typeArgumentList)), and(LBRACKET, g.expressionList, RBRACKET)));
    g.postIncrementExpression.is(g.primaryExpression, INC_OP);
    g.postDecrementExpression.is(g.primaryExpression, DEC_OP);
    g.objectCreationExpression.is(NEW, g.type, LPARENTHESIS, opt(g.argumentList), RPARENTHESIS);
    g.arrayCreationExpression.isOr(
        and(NEW, g.nonArrayType, LBRACKET, g.expressionList, RBRACKET, o2n(g.rankSpecifier), opt(g.arrayInitializer)),
        and(NEW, g.arrayType, g.arrayInitializer));
    g.delegateCreationExpression.is(NEW, g.delegateType, LPARENTHESIS, g.expression, RPARENTHESIS);
    g.typeOfExpression.is(TYPEOF, LPARENTHESIS, or(g.type, g.unboundTypeName, VOID), RPARENTHESIS);
    g.unboundTypeName.is(
        one2n(IDENTIFIER, opt(DOUBLE_COLON, IDENTIFIER), opt(g.genericDimensionSpecifier),
            opt(DOT, IDENTIFIER, opt(g.genericDimensionSpecifier))), opt(DOT, IDENTIFIER, opt(g.genericDimensionSpecifier)));
    g.genericDimensionSpecifier.is(INFERIOR, o2n(COMMA), SUPERIOR);
    g.checkedExpression.is(CHECKED, LPARENTHESIS, g.expression, RPARENTHESIS);
    g.uncheckedExpression.is(UNCHECKED, LPARENTHESIS, g.expression, RPARENTHESIS);
    g.defaultValueExpression.is(DEFAULT, LPARENTHESIS, g.type, RPARENTHESIS);
    g.anonymousMethodExpression.is(DELEGATE, opt(g.anonymousMethodSignature), g.block);
    g.anonymousMethodSignature.is(LPARENTHESIS, opt(g.anonymousMethodParameter, o2n(COMMA, g.anonymousMethodParameter)), RPARENTHESIS);
    g.anonymousMethodParameter.is(opt(g.parameterModifier), g.type, IDENTIFIER);
    g.unaryExpression.isOr(g.castExpression, g.primaryExpression, and(or(PLUS, MINUS, EXCLAMATION, TILDE), g.unaryExpression),
        g.preIncrementExpression, g.preDecrementExpression);
    g.preIncrementExpression.is(INC_OP, g.unaryExpression);
    g.preDecrementExpression.is(DEC_OP, g.unaryExpression);
    g.castExpression.is(LPARENTHESIS, g.type, RPARENTHESIS, g.unaryExpression);
    g.multiplicativeExpression.is(g.unaryExpression, o2n(or(STAR, SLASH, MODULO), g.unaryExpression));
    g.additiveExpression.is(g.multiplicativeExpression, o2n(or(PLUS, MINUS), g.multiplicativeExpression));
    g.shiftExpression.is(g.additiveExpression, o2n(or(LEFT_OP, RIGHT_OP), g.additiveExpression));
    g.relationalExpression.is(g.shiftExpression,
        o2n(or(and(or(INFERIOR, SUPERIOR, LE_OP, GE_OP), g.shiftExpression), and(or(IS, AS), g.type))));
    g.equalityExpression.is(g.relationalExpression, o2n(or(EQ_OP, NE_OP), g.relationalExpression));
    g.andExpression.is(g.equalityExpression, o2n(AND, g.equalityExpression));
    g.exclusiveOrExpression.is(g.andExpression, o2n(XOR, g.andExpression));
    g.inclusiveOrExpression.is(g.exclusiveOrExpression, o2n(OR, g.exclusiveOrExpression));
    g.conditionalAndExpression.is(g.inclusiveOrExpression, o2n(AND_OP, g.inclusiveOrExpression));
    g.conditionalOrExpression.is(g.conditionalAndExpression, o2n(OR_OP, g.conditionalAndExpression));
    g.nullCoalescingExpression.is(g.conditionalOrExpression, opt(DOUBLE_QUESTION, g.nullCoalescingExpression));
    g.conditionalExpression.is(g.nullCoalescingExpression, opt(QUESTION, g.expression, COLON, g.expression));
    g.assignment
        .is(g.unaryExpression,
            or(EQUAL, ADD_ASSIGN, SUB_ASSIGN, MUL_ASSIGN, DIV_ASSIGN, MOD_ASSIGN, AND_ASSIGN, OR_ASSIGN, XOR_ASSIGN, LEFT_ASSIGN,
                RIGHT_ASSIGN), g.expression);
    g.expression.isOr(g.assignment, g.conditionalExpression);
    g.constantExpression.is(g.expression);
    g.booleanExpression.is(g.expression);
  }

  private void statements(CSharpGrammar g) {
    g.statement.isOr(g.labeledStatement, g.declarationStatement, g.embeddedStatement);
    g.embeddedStatement.isOr(g.block, SEMICOLON, g.expressionStatement, g.selectionStatement, g.iterationStatement, g.jumpStatement,
        g.tryStatement, g.checkedStatement, g.uncheckedStatement, g.lockStatement, g.usingStatement, g.yieldStatement);
    g.block.is(LCURLYBRACE, o2n(g.statement), RCURLYBRACE);
    g.labeledStatement.is(IDENTIFIER, COLON, g.statement);
    g.declarationStatement.is(or(g.localVariableDeclaration, g.localConstantDeclaration), SEMICOLON);
    g.localVariableDeclaration.is(g.type, g.localVariableDeclarator, o2n(COMMA, g.localVariableDeclarator));
    g.localVariableDeclarator.is(IDENTIFIER, opt(EQUAL, g.localVariableInitializer));
    g.localVariableInitializer.isOr(g.expression, g.arrayInitializer);
    g.localConstantDeclaration.is(CONST, g.type, g.constantDeclarator, o2n(COMMA, g.constantDeclarator));
    g.constantDeclarator.is(IDENTIFIER, EQUAL, g.constantExpression);
    g.expressionStatement.is(g.statementExpression, SEMICOLON);
    g.statementExpression.isOr(g.postIncrementExpression, g.postDecrementExpression, g.preIncrementExpression, g.preDecrementExpression,
        g.assignment, g.invocationExpression, g.objectCreationExpression);
    g.selectionStatement.isOr(g.ifStatement, g.switchStatement);
    g.ifStatement.is(IF, LPARENTHESIS, g.booleanExpression, RPARENTHESIS, g.embeddedStatement, opt(ELSE, g.embeddedStatement));
    g.switchStatement.is(SWITCH, LPARENTHESIS, g.expression, RPARENTHESIS, LCURLYBRACE, o2n(g.switchSection), RCURLYBRACE);
    g.switchSection.is(one2n(g.switchLabel), one2n(g.statement));
    g.switchLabel.isOr(and(CASE, g.constantExpression, COLON), and(DEFAULT, COLON));
    g.iterationStatement.isOr(g.whileStatement, g.doStatement, g.forStatement, g.foreachStatement);
    g.whileStatement.is(WHILE, LPARENTHESIS, g.booleanExpression, RPARENTHESIS, g.embeddedStatement);
    g.doStatement.is(DO, g.embeddedStatement, WHILE, LPARENTHESIS, g.booleanExpression, RPARENTHESIS, SEMICOLON);
    g.forStatement.is(FOR, LPARENTHESIS, opt(g.forInitializer), SEMICOLON, opt(g.forCondition), SEMICOLON, opt(g.forIterator),
        RPARENTHESIS, g.embeddedStatement);
    g.forInitializer.isOr(g.localVariableDeclaration, g.statementExpressionList);
    g.forCondition.is(g.booleanExpression);
    g.forIterator.is(g.statementExpressionList);
    g.statementExpressionList.is(g.statementExpression, o2n(COMMA, g.statementExpression));
    g.foreachStatement.is(FOREACH, LPARENTHESIS, g.type, IDENTIFIER, IN, g.expression, RPARENTHESIS, g.embeddedStatement);
    g.jumpStatement.isOr(g.breakStatement, g.continueStatement, g.gotoStatement, g.returnStatement, g.throwStatement);
    g.breakStatement.is(BREAK, SEMICOLON);
    g.continueStatement.is(CONTINUE, SEMICOLON);
    g.gotoStatement.is(GOTO, or(IDENTIFIER, and(CASE, g.constantExpression), DEFAULT), SEMICOLON);
    g.returnStatement.is(RETURN, opt(g.expression), SEMICOLON);
    g.throwStatement.is(THROW, opt(g.expression), SEMICOLON);
    g.tryStatement.is(TRY, g.block, or(and(opt(g.catchClauses), g.finallyClause), g.catchClauses));
    g.catchClauses.isOr(and(o2n(g.specificCatchClause), g.generalCatchClause), one2n(g.specificCatchClause));
    g.specificCatchClause.is(CATCH, LPARENTHESIS, g.classType, opt(IDENTIFIER), RPARENTHESIS, g.block);
    g.generalCatchClause.is(CATCH, g.block);
    g.finallyClause.is(FINALLY, g.block);
    g.checkedStatement.is(CHECKED, g.block);
    g.uncheckedStatement.is(UNCHECKED, g.block);
    g.lockStatement.is(LOCK, LPARENTHESIS, g.expression, RPARENTHESIS, g.embeddedStatement);
    g.usingStatement.is(USING, LPARENTHESIS, g.resourceAcquisition, RPARENTHESIS, g.embeddedStatement);
    g.resourceAcquisition.isOr(g.localVariableDeclaration, g.expression);
    g.yieldStatement.is("yield", or(and(RETURN, g.expression), BREAK), SEMICOLON);
    g.namespaceDeclaration.is(NAMESPACE, g.qualifiedIdentifier, g.namespaceBody, opt(SEMICOLON));
    g.qualifiedIdentifier.is(IDENTIFIER, o2n(DOT, IDENTIFIER));
    g.namespaceBody.is(LCURLYBRACE, o2n(g.externAliasDirective), o2n(g.usingDirective), o2n(g.namespaceMemberDeclaration), RCURLYBRACE);
    g.externAliasDirective.is(EXTERN, "alias", IDENTIFIER, SEMICOLON);
    g.usingDirective.isOr(g.usingAliasDirective, g.usingNamespaceDirective);
    g.usingAliasDirective.is(USING, IDENTIFIER, EQUAL, g.namespaceOrTypeName, SEMICOLON);
    g.usingNamespaceDirective.is(USING, g.namespaceName, SEMICOLON);
    g.namespaceMemberDeclaration.isOr(g.namespaceDeclaration, g.typeDeclaration);
    g.typeDeclaration.isOr(g.classDeclaration, g.structDeclaration, g.interfaceDeclaration, g.enumDeclaration, g.delegateDeclaration);
    g.qualifiedAliasMember.is(IDENTIFIER, DOUBLE_COLON, IDENTIFIER, opt(g.typeArgumentList));
  }

  private void classes(CSharpGrammar g) {
    g.classDeclaration.is(opt(g.attributes), o2n(g.classModifier), opt("partial"), CLASS, IDENTIFIER, opt(g.typeParameterList),
        opt(g.classBase), opt(g.typeParameterConstraintsClauses), g.classBody, opt(SEMICOLON));
    g.classModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, ABSTRACT, SEALED, STATIC);
    g.classBase.is(COLON, or(and(g.classType, COMMA, g.interfaceTypeList), g.classType, g.interfaceTypeList));
    g.interfaceTypeList.is(g.interfaceType, o2n(COMMA, g.interfaceType));
    g.classBody.is(LCURLYBRACE, o2n(g.classMemberDeclaration), RCURLYBRACE);
    g.classMemberDeclaration.isOr(g.constantDeclaration, g.fieldDeclaration, g.methodDeclaration, g.propertyDeclaration,
        g.eventDeclaration, g.indexerDeclaration, g.operatorDeclaration, g.constructorDeclaration, g.finalizerDeclaration,
        g.staticConstructorDeclaration, g.typeDeclaration);
    g.constantDeclaration.is(opt(g.attributes), o2n(g.constantModifier), CONST, g.type, g.constantDeclarator,
        o2n(COMMA, g.constantDeclarator), SEMICOLON);
    g.constantModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE);
    g.fieldDeclaration.is(opt(g.attributes), o2n(g.fieldModifier), g.type, g.variableDeclarator, o2n(COMMA, g.variableDeclarator),
        SEMICOLON);
    g.fieldModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, READONLY, VOLATILE);
    g.variableDeclarator.is(IDENTIFIER, opt(EQUAL, g.variableInitializer));
    g.variableInitializer.isOr(g.expression, g.arrayInitializer);
    g.methodDeclaration.is(g.methodHeader, g.methodBody);
    g.methodHeader.is(opt(g.attributes), o2n(g.methodModifier), g.returnType, g.memberName, opt(g.typeParameterList), LPARENTHESIS,
        opt(g.formalParameterList), RPARENTHESIS, opt(g.typeParameterConstraintsClauses));
    g.methodModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN);
    g.returnType.isOr(g.type, VOID);
    // NOTE: g.memberName does not exactly stick to the specification (see page 462 of ECMA specification)
    // Normally it would be: g.memberName.isOr(and(g.interfaceType, DOT, IDENTIFIER), IDENTIFIER);
    g.memberName.is(g.namespaceOrTypeName);
    g.methodBody.isOr(g.block, SEMICOLON);
    g.formalParameterList.isOr(and(g.fixedParameters, opt(COMMA, g.parameterArray)), g.parameterArray);
    g.fixedParameters.is(g.fixedParameter, o2n(COMMA, g.fixedParameter));
    g.fixedParameter.is(opt(g.attributes), opt(g.parameterModifier), g.type, IDENTIFIER);
    g.parameterModifier.isOr(REF, OUT);
    g.parameterArray.is(opt(g.attributes), PARAMS, g.arrayType, IDENTIFIER);
    g.propertyDeclaration.is(opt(g.attributes), o2n(g.propertyModifier), g.type, g.memberName, LCURLYBRACE, g.accessorDeclarations,
        RCURLYBRACE);
    g.propertyModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN);
    g.accessorDeclarations.isOr(and(g.getAccessorDeclaration, opt(g.setAccessorDeclaration)),
        and(g.setAccessorDeclaration, opt(g.getAccessorDeclaration)));
    g.getAccessorDeclaration.is(opt(g.attributes), o2n(g.accessorModifier), "get", g.accessorBody);
    g.setAccessorDeclaration.is(opt(g.attributes), o2n(g.accessorModifier), "set", g.accessorBody);
    g.accessorModifier.isOr(and(PROTECTED, INTERNAL), and(INTERNAL, PROTECTED), PROTECTED, INTERNAL, PRIVATE);
    g.accessorBody.isOr(g.block, SEMICOLON);
    g.eventDeclaration.is(
        opt(g.attributes),
        o2n(g.eventModifier),
        EVENT,
        g.type,
        or(and(g.variableDeclarator, opt(COMMA, g.variableDeclarator), SEMICOLON),
            and(g.memberName, LCURLYBRACE, g.eventAccessorDeclarations, RCURLYBRACE)));
    g.eventModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN);
    g.eventAccessorDeclarations.isOr(and(g.addAccessorDeclaration, g.removeAccessorDeclaration),
        and(g.removeAccessorDeclaration, g.addAccessorDeclaration));
    g.addAccessorDeclaration.is(opt(g.attributes), "add", g.block);
    g.removeAccessorDeclaration.is(opt(g.attributes), "remove", g.block);
    g.indexerDeclaration.is(opt(g.attributes), o2n(g.indexerModifier), g.indexerDeclarator, LCURLYBRACE, g.accessorDeclarations,
        RCURLYBRACE);
    g.indexerModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, STATIC, VIRTUAL, SEALED, OVERRIDE, ABSTRACT, EXTERN);
    g.indexerDeclarator.is(g.type, opt(g.interfaceType, DOT), THIS, LBRACKET, g.formalParameterList, RBRACKET);
    g.operatorDeclaration.is(opt(g.attributes), one2n(g.operatorModifier), g.operatorDeclarator, g.operatorBody);
    g.operatorModifier.isOr(PUBLIC, STATIC, EXTERN);
    g.operatorDeclarator.isOr(g.unaryOperatorDeclarator, g.binaryOperatorDeclarator, g.conversionOperatorDeclarator);
    g.unaryOperatorDeclarator.is(g.type, OPERATOR, g.overloadableUnaryOperator, LPARENTHESIS, g.type, IDENTIFIER, RPARENTHESIS);
    g.overloadableUnaryOperator.isOr(PLUS, MINUS, EXCLAMATION, TILDE, INC_OP, DEC_OP, TRUE, FALSE);
    g.binaryOperatorDeclarator.is(g.type, OPERATOR, g.overloadableBinaryOperator, LPARENTHESIS, g.type, IDENTIFIER, COMMA, g.type,
        IDENTIFIER, RPARENTHESIS);
    g.overloadableBinaryOperator.isOr(PLUS, MINUS, STAR, SLASH, MODULO, AND, OR, XOR, LEFT_OP, RIGHT_OP, EQ_OP, NE_OP, SUPERIOR, INFERIOR,
        GE_OP, LE_OP);
    g.conversionOperatorDeclarator.is(or(IMPLICIT, EXPLICIT), OPERATOR, g.type, LPARENTHESIS, g.type, IDENTIFIER, RPARENTHESIS);
    g.operatorBody.isOr(g.block, SEMICOLON);
    g.constructorDeclaration.is(opt(g.attributes), o2n(g.constructorModifier), g.constructorDeclarator, g.constructorBody);
    g.constructorModifier.isOr(PUBLIC, PROTECTED, INTERNAL, PRIVATE, EXTERN);
    g.constructorDeclarator.is(IDENTIFIER, LPARENTHESIS, opt(g.formalParameterList), RPARENTHESIS, opt(g.constructorInitializer));
    g.constructorInitializer.is(COLON, or(BASE, THIS), LPARENTHESIS, opt(g.argumentList), RPARENTHESIS);
    g.constructorBody.isOr(g.block, SEMICOLON);
    g.staticConstructorDeclaration.is(opt(g.attributes), g.staticConstructorModifiers, IDENTIFIER, LPARENTHESIS, RPARENTHESIS,
        g.staticConstructorBody);
    g.staticConstructorModifiers.isOr(and(opt(EXTERN), STATIC, not(next(EXTERN))), and(STATIC, opt(EXTERN)));
    g.staticConstructorBody.isOr(g.block, SEMICOLON);
    g.finalizerDeclaration.is(opt(g.attributes), opt(EXTERN), TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, g.finalizerBody);
    g.finalizerBody.isOr(g.block, SEMICOLON);
  }

  private void structs(CSharpGrammar g) {
    g.structDeclaration.is(opt(g.attributes), o2n(g.structModifier), opt("partial"), STRUCT, IDENTIFIER, opt(g.typeParameterList),
        opt(g.structInterfaces), opt(g.typeParameterConstraintsClauses), g.structBody, opt(SEMICOLON));
    g.structModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE);
    g.structInterfaces.is(COLON, g.interfaceTypeList);
    g.structBody.is(LCURLYBRACE, o2n(g.structMemberDeclaration), RCURLYBRACE);
    g.structMemberDeclaration.isOr(g.constantDeclaration, g.fieldDeclaration, g.methodDeclaration, g.propertyDeclaration,
        g.eventDeclaration, g.indexerDeclaration, g.operatorDeclaration, g.constructorDeclaration, g.staticConstructorDeclaration,
        g.typeDeclaration);
  }

  private void arrays(CSharpGrammar g) {
    g.arrayInitializer.is(LCURLYBRACE, or(and(g.variableInitializerList, COMMA), opt(g.variableInitializerList)), RCURLYBRACE);
    g.variableInitializerList.is(g.variableInitializer, o2n(COMMA, g.variableInitializer));
  }

  private void interfaces(CSharpGrammar g) {
    g.interfaceDeclaration.is(opt(g.attributes), o2n(g.interfaceModifier), opt("partial"), INTERFACE, IDENTIFIER, opt(g.typeParameterList),
        opt(g.interfaceBase), opt(g.typeParameterConstraintsClauses), g.interfaceBody, opt(SEMICOLON));
    g.interfaceModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE);
    g.interfaceBase.is(COLON, g.interfaceTypeList);
    g.interfaceBody.is(LCURLYBRACE, o2n(g.interfaceMemberDeclaration), RCURLYBRACE);
    g.interfaceMemberDeclaration.isOr(g.interfaceMethodDeclaration, g.interfacePropertyDeclaration, g.interfaceEventDeclaration,
        g.interfaceIndexerDeclaration);
    g.interfaceMethodDeclaration.is(opt(g.attributes), opt(NEW), g.returnType, IDENTIFIER, opt(g.typeParameterList), LPARENTHESIS,
        opt(g.formalParameterList), RPARENTHESIS, opt(g.typeParameterConstraintsClauses), SEMICOLON);
    g.interfacePropertyDeclaration.is(opt(g.attributes), opt(NEW), g.type, IDENTIFIER, LCURLYBRACE, g.interfaceAccessors, RCURLYBRACE);
    g.interfaceAccessors.is(opt(g.attributes),
        or(and("get", SEMICOLON, opt(g.attributes), "set"), and("set", SEMICOLON, opt(g.attributes), "get"), "get", "set"), SEMICOLON);
    g.interfaceEventDeclaration.is(opt(g.attributes), opt(NEW), EVENT, g.type, IDENTIFIER, SEMICOLON);
    g.interfaceIndexerDeclaration.is(opt(g.attributes), opt(NEW), g.type, THIS, LBRACKET, g.formalParameterList, RBRACKET, LCURLYBRACE,
        g.interfaceAccessors, RCURLYBRACE);
  }

  private void enums(CSharpGrammar g) {
    g.enumDeclaration.is(opt(g.attributes), o2n(g.enumModifier), ENUM, IDENTIFIER, opt(g.enumBase), g.enumBody, opt(SEMICOLON));
    g.enumBase.is(COLON, g.integralType);
    g.enumBody.is(LCURLYBRACE, or(and(g.enumMemberDeclarations, COMMA), opt(g.enumMemberDeclarations)), RCURLYBRACE);
    g.enumModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE);
    g.enumMemberDeclarations.is(g.enumMemberDeclaration, o2n(COMMA, g.enumMemberDeclaration));
    g.enumMemberDeclaration.is(opt(g.attributes), IDENTIFIER, opt(EQUAL, g.constantExpression));
  }

  private void delegates(CSharpGrammar g) {
    g.delegateDeclaration.is(opt(g.attributes), o2n(g.delegateModifier), DELEGATE, g.returnType, IDENTIFIER, opt(g.typeParameterList),
        LPARENTHESIS, opt(g.formalParameterList), RPARENTHESIS, opt(g.typeParameterConstraintsClauses), SEMICOLON);
    g.delegateModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE);
  }

  private void attributes(CSharpGrammar g) {
    g.globalAttributes.is(one2n(g.globalAttributeSection));
    g.globalAttributeSection.is(LBRACKET, g.globalAttributeTargetSpecifier, g.attributeList, opt(COMMA), RBRACKET);
    g.globalAttributeTargetSpecifier.is(g.globalAttributeTarget, COLON);
    g.globalAttributeTarget.isOr(IDENTIFIER, isOneOfThem(CSharpKeyword.values()));
    g.attributes.is(one2n(g.attributeSection));
    g.attributeSection.is(LBRACKET, opt(g.attributeTargetSpecifier), g.attributeList, opt(COMMA), RBRACKET);
    g.attributeTargetSpecifier.is(g.attributeTarget, COLON);
    g.attributeTarget.isOr(IDENTIFIER, isOneOfThem(CSharpKeyword.values()));
    g.attributeList.is(g.attribute, o2n(COMMA, g.attribute));
    g.attribute.is(g.attributeName, opt(g.attributeArguments));
    g.attributeName.is(g.typeName);
    // NOTE: g.attributeArguments does not exactly stick to the specification, as normally a positionalArgument can not appear after a
    // namedArgument (see page 469 of ECMA specification)
    g.attributeArguments.is(LPARENTHESIS,
        opt(or(g.namedArgument, g.positionalArgument), o2n(COMMA, or(g.namedArgument, g.positionalArgument))), RPARENTHESIS);
    g.positionalArgument.is(g.attributeArgumentExpression);
    g.namedArgument.is(IDENTIFIER, EQUAL, g.attributeArgumentExpression);
    g.attributeArgumentExpression.is(g.expression);
  }

  private void generics(CSharpGrammar g) {
    g.typeParameterList.is(INFERIOR, g.typeParameters, SUPERIOR);
    g.typeParameters.is(opt(g.attributes), g.typeParameter, o2n(COMMA, opt(g.attributes), g.typeParameter));
    g.typeParameter.is(IDENTIFIER);
    g.typeArgumentList.is(INFERIOR, g.typeArgument, opt(COMMA, g.typeArgument), SUPERIOR);
    g.typeArgument.is(g.type);
    g.typeParameterConstraintsClauses.is(one2n(g.typeParameterConstraintsClause));
    g.typeParameterConstraintsClause.is("where", g.typeParameter, COLON, g.typeParameterConstraints);
    g.typeParameterConstraints.isOr(and(g.primaryConstraint, COMMA, g.secondaryConstraints, COMMA, g.constructorConstraint),
        and(g.primaryConstraint, COMMA, or(g.secondaryConstraints, g.constructorConstraint)),
        and(g.secondaryConstraints, COMMA, g.constructorConstraint), g.primaryConstraint, g.secondaryConstraints, g.constructorConstraint);
    g.primaryConstraint.isOr(g.classType, CLASS, STRUCT);
    g.secondaryConstraints.is(or(g.interfaceType, g.typeParameter), o2n(COMMA, or(g.interfaceType, g.typeParameter)));
    g.constructorConstraint.is(NEW, LPARENTHESIS, RPARENTHESIS);
  }
}
