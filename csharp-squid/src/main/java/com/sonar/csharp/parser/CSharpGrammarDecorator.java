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
import static com.sonar.csharp.api.CSharpKeyword.EXTERN;
import static com.sonar.csharp.api.CSharpKeyword.FINALLY;
import static com.sonar.csharp.api.CSharpKeyword.FLOAT;
import static com.sonar.csharp.api.CSharpKeyword.FOR;
import static com.sonar.csharp.api.CSharpKeyword.FOREACH;
import static com.sonar.csharp.api.CSharpKeyword.GOTO;
import static com.sonar.csharp.api.CSharpKeyword.IF;
import static com.sonar.csharp.api.CSharpKeyword.IN;
import static com.sonar.csharp.api.CSharpKeyword.INT;
import static com.sonar.csharp.api.CSharpKeyword.INTERNAL;
import static com.sonar.csharp.api.CSharpKeyword.IS;
import static com.sonar.csharp.api.CSharpKeyword.LOCK;
import static com.sonar.csharp.api.CSharpKeyword.LONG;
import static com.sonar.csharp.api.CSharpKeyword.NAMESPACE;
import static com.sonar.csharp.api.CSharpKeyword.NEW;
import static com.sonar.csharp.api.CSharpKeyword.NULL;
import static com.sonar.csharp.api.CSharpKeyword.OBJECT;
import static com.sonar.csharp.api.CSharpKeyword.OUT;
import static com.sonar.csharp.api.CSharpKeyword.PRIVATE;
import static com.sonar.csharp.api.CSharpKeyword.PROTECTED;
import static com.sonar.csharp.api.CSharpKeyword.PUBLIC;
import static com.sonar.csharp.api.CSharpKeyword.REF;
import static com.sonar.csharp.api.CSharpKeyword.RETURN;
import static com.sonar.csharp.api.CSharpKeyword.SBYTE;
import static com.sonar.csharp.api.CSharpKeyword.SEALED;
import static com.sonar.csharp.api.CSharpKeyword.SHORT;
import static com.sonar.csharp.api.CSharpKeyword.STATIC;
import static com.sonar.csharp.api.CSharpKeyword.STRING;
import static com.sonar.csharp.api.CSharpKeyword.SWITCH;
import static com.sonar.csharp.api.CSharpKeyword.THIS;
import static com.sonar.csharp.api.CSharpKeyword.THROW;
import static com.sonar.csharp.api.CSharpKeyword.TRY;
import static com.sonar.csharp.api.CSharpKeyword.TYPEOF;
import static com.sonar.csharp.api.CSharpKeyword.UINT;
import static com.sonar.csharp.api.CSharpKeyword.ULONG;
import static com.sonar.csharp.api.CSharpKeyword.UNCHECKED;
import static com.sonar.csharp.api.CSharpKeyword.USHORT;
import static com.sonar.csharp.api.CSharpKeyword.USING;
import static com.sonar.csharp.api.CSharpKeyword.VOID;
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
import static com.sonar.sslr.api.GenericTokenType.IDENTIFIER;
import static com.sonar.sslr.impl.matcher.Matchers.and;
import static com.sonar.sslr.impl.matcher.Matchers.bridge;
import static com.sonar.sslr.impl.matcher.Matchers.o2n;
import static com.sonar.sslr.impl.matcher.Matchers.one2n;
import static com.sonar.sslr.impl.matcher.Matchers.opt;
import static com.sonar.sslr.impl.matcher.Matchers.or;

import com.sonar.sslr.api.GrammarDecorator;
import com.sonar.sslr.impl.GrammarFieldsInitializer;

/**
 * Definition of each element of the C# grammar.
 */
public class CSharpGrammarDecorator implements GrammarDecorator<CSharpGrammar> {

  /**
   * ${@inheritDoc}
   */
  public void decorate(CSharpGrammar g) {
    GrammarFieldsInitializer.initializeRuleFields(g, CSharpGrammar.class);

    // We follow the ECMA specification for the C# language, 4th edition of June 2006
    g.literal.isOr(BOOL, INTEGER_DEC_LITERAL, INTEGER_HEX_LITERAL, REAL_LITERAL, CHARACTER_LITERAL, STRING_LITERAL, NULL);
    g.identifier.is(IDENTIFIER);

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

    // A.2.13 Generics
    generics(g);

  }

  private void generics(CSharpGrammar g) {
    g.typeArgumentList.is(INFERIOR, g.type, opt(COMMA, g.type), SUPERIOR);
  }

  private void classes(CSharpGrammar g) {
    g.classDeclaration.is(opt(g.attributes), o2n(g.classModifier), opt("partial"), CLASS, g.identifier, opt(g.typeParameterList),
        opt(g.classBase), opt(g.typeParameterConstraintsClauses), g.classBody, opt(SEMICOLON));
    g.classModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, ABSTRACT, SEALED, STATIC);
    g.classBody.is(LCURLYBRACE, o2n(g.classMemberDeclaration), RCURLYBRACE);
    g.parameterModifier.isOr(REF, OUT);
  }

  private void statements(CSharpGrammar g) {
    g.statement.isOr(g.labeledStatement, g.declarationStatement, g.embeddedStatement);
    g.embeddedStatement.isOr(g.block, SEMICOLON, g.expressionStatement, g.selectionStatement, g.iterationStatement, g.jumpStatement,
        g.tryStatement, g.checkedStatement, g.uncheckedStatement, g.lockStatement, g.usingStatement, g.yieldStatement);
    g.block.is(bridge(LCURLYBRACE, RCURLYBRACE)); // TODO : uncomment following definition to activate block rule
    // g.block.is(LCURLYBRACE, o2n(g.statement), RCURLYBRACE);
    g.labeledStatement.is(g.identifier, COLON, g.statement);
    g.declarationStatement.is(or(g.localVariableDeclaration, g.localConstantDeclaration), SEMICOLON);
    g.localVariableDeclaration.is(g.type, g.localVariableDeclarator, o2n(COMMA, g.localVariableDeclarator));
    g.localVariableDeclarator.is(IDENTIFIER, opt(EQUAL, g.localVariableInitializer));
    g.localVariableInitializer.isOr(g.expression, g.arrayInitializer);
    g.localConstantDeclaration.is(CONST, g.type, g.constantDeclarator, o2n(COMMA, g.constantDeclarator));
    g.constantDeclarator.is(IDENTIFIER, EQUAL, g.constantExpression);
    g.expressionStatement.is(g.statementExpression, SEMICOLON);
    g.statementExpression.isOr(g.invocationExpression, g.objectCreationExpression, g.assignment, g.postIncrementExpression,
        g.postDecrementExpression, g.preIncrementExpression, g.preDecrementExpression);
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
    g.forInitializer.isOr(g.localConstantDeclaration, g.statementExpressionList);
    g.forCondition.is(g.booleanExpression);
    g.forIterator.is(g.statementExpressionList);
    g.statementExpressionList.is(g.statementExpression, o2n(COMMA, g.statementExpression));
    g.foreachStatement.is(FOREACH, LPARENTHESIS, g.type, IDENTIFIER, IN, g.expression, RPARENTHESIS, g.embeddedStatement);
    g.jumpStatement.is(g.breakStatement, g.continueStatement, g.gotoStatement, g.returnStatement, g.throwStatement);
    g.breakStatement.is(BREAK, SEMICOLON);
    g.continueStatement.is(CONTINUE, SEMICOLON);
    g.gotoStatement.is(GOTO, or(IDENTIFIER, and(CASE, g.constantExpression), DEFAULT), SEMICOLON);
    g.returnStatement.is(RETURN, opt(g.expression), SEMICOLON);
    g.throwStatement.is(THROW, opt(g.expression), SEMICOLON);
    g.tryStatement.is(TRY, g.block, or(g.catchClauses, and(opt(g.catchClauses), g.finallyClause)));
    g.catchClauses.isOr(one2n(g.specificCatchClause), and(o2n(g.specificCatchClause), g.generalCatchClause));
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

  private void expressions(CSharpGrammar g) {
    g.argumentList.is(g.argument, o2n(COMMA, g.argument));
    g.argument.isOr(g.expression, and(REF, g.variableReference), and(OUT, g.variableReference));
    g.primaryExpression.isOr(g.arrayCreationExpression, g.primaryNoArrayCreationExpression);
    g.primaryNoArrayCreationExpression.isOr(g.literal, g.simpleName, g.parenthesizedExpression, g.memberAccess, g.invocationExpression,
        g.elementAccess, g.thisAccess, g.baseAccess, g.postIncrementExpression, g.postDecrementExpression, g.objectCreationExpression,
        g.delegateCreationExpression, g.typeOfExpression, g.checkedExpression, g.uncheckedExpression, g.defaultValueExpression,
        g.anonymousMethodExpression);
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
    g.unboundTypeName.isOr(and(IDENTIFIER, opt(DOUBLE_COLON, IDENTIFIER), opt(g.genericDimensionSpecifier))); // /////
    g.unboundTypeName.is(
        one2n(IDENTIFIER, opt(DOUBLE_COLON, IDENTIFIER), opt(g.genericDimensionSpecifier),
            opt(DOT, IDENTIFIER, opt(g.genericDimensionSpecifier))), opt(DOT, IDENTIFIER, opt(g.genericDimensionSpecifier)));
    g.genericDimensionSpecifier.is(INFERIOR, o2n(COMMA), SUPERIOR);
    g.checkedExpression.is(CHECKED, LPARENTHESIS, g.expression, RPARENTHESIS);
    g.uncheckedExpression.is(UNCHECKED, LPARENTHESIS, g.expression, RPARENTHESIS);
    g.defaultValueExpression.is(DEFAULT, LPARENTHESIS, g.type, RPARENTHESIS);
    g.anonymousMethodExpression.is(DELEGATE, opt(g.anonymousMethodSignature), g.block);
    g.anonymousMethodSignature.is(LPARENTHESIS, opt(g.anonymousMethodParameter, o2n(COMMA, g.anonymousMethodParameter)), RPARENTHESIS);
    g.anonymousMethodParameter.is(opt(g.parameterModifier), g.type, g.identifier);
    g.unaryExpression.isOr(g.primaryExpression, and(or(PLUS, MINUS, EXCLAMATION, TILDE), g.unaryExpression), g.preIncrementExpression,
        g.preDecrementExpression, g.castExpression);
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
    g.expression.isOr(g.conditionalExpression, g.assignment);
    g.constantExpression.is(g.expression);
    g.booleanExpression.is(g.expression);
  }

  private void variables(CSharpGrammar g) {
    g.variableReference.is(g.expression);
  }

  private void types(CSharpGrammar g) {
    g.type.isOr(g.valueType, g.referenceType, g.typeParameter);
    g.valueType.isOr(g.structType, g.enumType);
    g.structType.isOr(g.typeName, g.simpleType, g.nullableType);
    g.simpleType.isOr(g.numericType, BOOL);
    g.numericType.isOr(g.integralType, g.floatingPointType, DECIMAL);
    g.integralType.isOr(SBYTE, BYTE, SHORT, USHORT, INT, UINT, LONG, ULONG, CHAR);
    g.floatingPointType.isOr(FLOAT, DOUBLE);
    g.enumType.is(g.typeName);
    g.nullableType.is(g.nonNullableValueType, QUESTION);
    g.nonNullableValueType.isOr(g.enumType, g.typeName, g.simpleType);
    g.referenceType.isOr(g.classType, g.interfaceType, g.arrayType, g.delegateType);
    g.classType.isOr(g.typeName, OBJECT, STRING);
    g.interfaceType.is(g.typeName);
    g.arrayType.is(g.nonArrayType, one2n(g.rankSpecifier));
    g.nonArrayType.isOr(g.valueType, g.classType, g.interfaceType, g.delegateType, g.typeParameter);
    g.rankSpecifier.is(LBRACKET, o2n(COMMA), RBRACKET);
    g.delegateType.is(g.typeName);
  }

  private void basicConcepts(CSharpGrammar g) {
    g.compilationUnit.is(o2n(g.externAliasDirective), o2n(g.usingDirective), opt(g.globalAttributes), o2n(g.namespaceMemberDeclaration));
    g.namespaceName.is(g.namespaceOrTypeName);
    g.typeName.is(g.namespaceOrTypeName);
    g.namespaceOrTypeName.is(
        one2n(or(g.qualifiedAliasMember, and(IDENTIFIER, opt(g.typeArgumentList))), opt(DOT, IDENTIFIER, opt(g.typeArgumentList))),
        opt(DOT, IDENTIFIER, opt(g.typeArgumentList)));
  }
}
