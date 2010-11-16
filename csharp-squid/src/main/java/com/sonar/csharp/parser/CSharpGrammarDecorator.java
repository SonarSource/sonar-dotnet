/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.parser;

import static com.sonar.csharp.api.CSharpKeyword.ABSTRACT;
import static com.sonar.csharp.api.CSharpKeyword.BOOL;
import static com.sonar.csharp.api.CSharpKeyword.BYTE;
import static com.sonar.csharp.api.CSharpKeyword.CHAR;
import static com.sonar.csharp.api.CSharpKeyword.CLASS;
import static com.sonar.csharp.api.CSharpKeyword.DECIMAL;
import static com.sonar.csharp.api.CSharpKeyword.DOUBLE;
import static com.sonar.csharp.api.CSharpKeyword.FLOAT;
import static com.sonar.csharp.api.CSharpKeyword.INT;
import static com.sonar.csharp.api.CSharpKeyword.INTERNAL;
import static com.sonar.csharp.api.CSharpKeyword.LONG;
import static com.sonar.csharp.api.CSharpKeyword.NEW;
import static com.sonar.csharp.api.CSharpKeyword.OBJECT;
import static com.sonar.csharp.api.CSharpKeyword.PRIVATE;
import static com.sonar.csharp.api.CSharpKeyword.PROTECTED;
import static com.sonar.csharp.api.CSharpKeyword.PUBLIC;
import static com.sonar.csharp.api.CSharpKeyword.SBYTE;
import static com.sonar.csharp.api.CSharpKeyword.SEALED;
import static com.sonar.csharp.api.CSharpKeyword.SHORT;
import static com.sonar.csharp.api.CSharpKeyword.STATIC;
import static com.sonar.csharp.api.CSharpKeyword.STRING;
import static com.sonar.csharp.api.CSharpKeyword.UINT;
import static com.sonar.csharp.api.CSharpKeyword.ULONG;
import static com.sonar.csharp.api.CSharpKeyword.USHORT;
import static com.sonar.csharp.api.CSharpPunctuator.COMMA;
import static com.sonar.csharp.api.CSharpPunctuator.DOT;
import static com.sonar.csharp.api.CSharpPunctuator.DOUBLE_COLON;
import static com.sonar.csharp.api.CSharpPunctuator.INFERIOR;
import static com.sonar.csharp.api.CSharpPunctuator.LBRACKET;
import static com.sonar.csharp.api.CSharpPunctuator.LCURLYBRACE;
import static com.sonar.csharp.api.CSharpPunctuator.QUESTION;
import static com.sonar.csharp.api.CSharpPunctuator.RBRACKET;
import static com.sonar.csharp.api.CSharpPunctuator.RCURLYBRACE;
import static com.sonar.csharp.api.CSharpPunctuator.SEMICOLON;
import static com.sonar.csharp.api.CSharpPunctuator.SUPERIOR;
import static com.sonar.sslr.api.GenericTokenType.EOF;
import static com.sonar.sslr.api.GenericTokenType.IDENTIFIER;
import static com.sonar.sslr.impl.matcher.Matchers.and;
import static com.sonar.sslr.impl.matcher.Matchers.not;
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

  public void decorate(CSharpGrammar cs) {
    GrammarFieldsInitializer.initializeRuleFields(cs, CSharpGrammar.class);

    // TODO Code de Freddy
    cs.compilationUnit.is(opt(cs.compilationUnitLevel), EOF);
    cs.compilationUnitLevel.is(one2n(or(cs.line, cs.block)));
    cs.block.is(one2n(not(or(SEMICOLON, LCURLYBRACE, EOF))), LCURLYBRACE, opt(cs.compilationUnitLevel), RCURLYBRACE);
    cs.line.is(o2n(not(or(SEMICOLON, LCURLYBRACE, EOF))), SEMICOLON);

    // XXX Beginning of my code
    cs.identifier.is(IDENTIFIER);

    // A.2.1 Basic concepts
    cs.namespaceName.is(cs.namespaceOrTypeName);
    cs.typeName.is(cs.namespaceOrTypeName);
    cs.namespaceOrTypeName.isOr(and(cs.identifier, opt(cs.typeArgumentList)), cs.qualifiedAliasMember,
        and(cs.namespaceOrTypeName, DOT, IDENTIFIER, opt(cs.typeArgumentList)));

    // A.2.2 Types
    // Status :
    // - Declaration: OK
    // - Testing: started
    cs.type.isOr(cs.valueType, cs.referenceType, cs.typeParameter);
    cs.valueType.isOr(cs.structType, cs.enumType);
    cs.structType.isOr(cs.typeName, cs.simpleType, cs.nullableType);
    cs.simpleType.isOr(cs.numericType, BOOL);
    cs.numericType.isOr(cs.integralType, cs.floatingPointType, DECIMAL);
    cs.integralType.isOr(SBYTE, BYTE, SHORT, USHORT, INT, UINT, LONG, ULONG, CHAR);
    cs.floatingPointType.isOr(FLOAT, DOUBLE);
    cs.enumType.is(cs.typeName);
    cs.nullableType.is(cs.nonNullableValueType, QUESTION);
    cs.nonNullableValueType.isOr(cs.enumType, cs.typeName, cs.simpleType);
    cs.referenceType.isOr(cs.classType, cs.interfaceType, cs.arrayType, cs.delegateType);
    cs.classType.isOr(cs.typeName, OBJECT, STRING);
    cs.interfaceType.is(cs.typeName);
    cs.arrayType.is(cs.nonArrayType, one2n(cs.rankSpecifier));
    cs.nonArrayType.isOr(cs.valueType, cs.classType, cs.interfaceType, cs.delegateType, cs.typeParameter);
    cs.rankSpecifier.is(LBRACKET, o2n(COMMA), RBRACKET);
    cs.delegateType.is(cs.typeName);

    // A.2.5 Statements
    cs.qualifiedAliasMember.is(IDENTIFIER, DOUBLE_COLON, IDENTIFIER, opt(cs.typeArgumentList));

    // A.2.6 Classes
    cs.classDeclaration.is(opt(cs.attributes), o2n(cs.classModifier), opt("partial"), CLASS, cs.identifier, opt(cs.typeParameterList),
        opt(cs.classBase), opt(cs.typeParameterConstraintsClauses), cs.classBody, opt(SEMICOLON)); // TODO To Test
    cs.classModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, ABSTRACT, SEALED, STATIC);

    cs.classBody.is(LCURLYBRACE, o2n(cs.classMemberDeclaration), RCURLYBRACE); // TODO To Test

    // A.2.13 Generics
    cs.typeArgumentList.is(INFERIOR, cs.type, opt(COMMA, cs.type), SUPERIOR);

  }
}
