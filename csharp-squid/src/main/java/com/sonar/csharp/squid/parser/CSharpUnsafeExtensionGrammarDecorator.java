/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.parser;

import static com.sonar.csharp.api.CSharpKeyword.EXTERN;
import static com.sonar.csharp.api.CSharpKeyword.FIXED;
import static com.sonar.csharp.api.CSharpKeyword.INTERNAL;
import static com.sonar.csharp.api.CSharpKeyword.NEW;
import static com.sonar.csharp.api.CSharpKeyword.PRIVATE;
import static com.sonar.csharp.api.CSharpKeyword.PROTECTED;
import static com.sonar.csharp.api.CSharpKeyword.PUBLIC;
import static com.sonar.csharp.api.CSharpKeyword.SIZEOF;
import static com.sonar.csharp.api.CSharpKeyword.STACKALLOC;
import static com.sonar.csharp.api.CSharpKeyword.STATIC;
import static com.sonar.csharp.api.CSharpKeyword.UNSAFE;
import static com.sonar.csharp.api.CSharpKeyword.VOID;
import static com.sonar.csharp.api.CSharpPunctuator.AND;
import static com.sonar.csharp.api.CSharpPunctuator.COMMA;
import static com.sonar.csharp.api.CSharpPunctuator.EQUAL;
import static com.sonar.csharp.api.CSharpPunctuator.LBRACKET;
import static com.sonar.csharp.api.CSharpPunctuator.LPARENTHESIS;
import static com.sonar.csharp.api.CSharpPunctuator.PTR_OP;
import static com.sonar.csharp.api.CSharpPunctuator.RBRACKET;
import static com.sonar.csharp.api.CSharpPunctuator.RPARENTHESIS;
import static com.sonar.csharp.api.CSharpPunctuator.SEMICOLON;
import static com.sonar.csharp.api.CSharpPunctuator.STAR;
import static com.sonar.csharp.api.CSharpPunctuator.TILDE;
import static com.sonar.sslr.api.GenericTokenType.IDENTIFIER;
import static com.sonar.sslr.impl.matcher.Matchers.and;
import static com.sonar.sslr.impl.matcher.Matchers.o2n;
import static com.sonar.sslr.impl.matcher.Matchers.one2n;
import static com.sonar.sslr.impl.matcher.Matchers.opt;
import static com.sonar.sslr.impl.matcher.Matchers.or;

import com.sonar.csharp.api.CSharpGrammar;
import com.sonar.csharp.api.CSharpUnsafeExtensionGrammar;
import com.sonar.sslr.api.GrammarDecorator;
import com.sonar.sslr.impl.GrammarRuleLifeCycleManager;

/**
 * Definition of each element of the C# grammar.
 */
public class CSharpUnsafeExtensionGrammarDecorator implements GrammarDecorator<CSharpGrammar> {

  /**
   * ${@inheritDoc}
   */
  public void decorate(CSharpGrammar g) {
    g.unsafe = new CSharpUnsafeExtensionGrammar();
    GrammarRuleLifeCycleManager.initializeLeftRecursionRuleFields(g.unsafe, CSharpUnsafeExtensionGrammar.class);

    g.classModifier.or(UNSAFE);
    g.structModifier.or(UNSAFE);
    g.interfaceModifier.or(UNSAFE);
    g.delegateModifier.or(UNSAFE);
    g.fieldModifier.or(UNSAFE);
    g.methodModifier.or(UNSAFE);
    g.propertyModifier.or(UNSAFE);
    g.eventModifier.or(UNSAFE);
    g.indexerModifier.or(UNSAFE);
    g.operatorModifier.or(UNSAFE);
    g.constructorModifier.or(UNSAFE);

    g.unsafe.destructorDeclaration.is(opt(g.attributes), o2n(or(EXTERN, UNSAFE)), TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS,
        g.destructorBody);
    g.staticConstructorModifiers.override(o2n(or(EXTERN, UNSAFE)), STATIC, o2n(or(EXTERN, UNSAFE)));
    g.embeddedStatement.or(or(g.unsafe.unsafeStatement, g.unsafe.fixedStatement));
    g.unsafe.unsafeStatement.is(UNSAFE, g.block);
    g.type.orBefore(g.unsafe.pointerType);
    g.unsafe.pointerType.is(or(g.type, VOID), STAR);
    // NOTE : g.unsafe.pointerElementAccess deactivated here because it shadows the "g.elementAccess" in the main grammar...
    // Need to look into that later.
    g.primaryNoArrayCreationExpression.or(or(g.unsafe.pointerMemberAccess, /* g.unsafe.pointerElementAccess, */g.unsafe.sizeOfExpression));
    g.unaryExpression.or(or(g.unsafe.pointerIndirectionExpression, g.unsafe.addressOfExpression));
    g.unsafe.pointerIndirectionExpression.is(STAR, g.unaryExpression);
    g.unsafe.pointerMemberAccess.is(g.primaryExpression, PTR_OP, IDENTIFIER);
    g.unsafe.pointerElementAccess.is(g.primaryNoArrayCreationExpression, LBRACKET, g.expression, RBRACKET);
    g.unsafe.addressOfExpression.is(AND, g.unaryExpression);
    g.unsafe.sizeOfExpression.is(SIZEOF, LPARENTHESIS, g.type, RPARENTHESIS);
    g.unsafe.fixedStatement.is(FIXED, LPARENTHESIS, g.unsafe.pointerType, g.unsafe.fixedPointerDeclarator,
        o2n(COMMA, g.unsafe.fixedPointerDeclarator), RPARENTHESIS, g.embeddedStatement);
    g.unsafe.fixedPointerDeclarator.is(IDENTIFIER, EQUAL, g.unsafe.fixedPointerInitializer);
    // NOTE : g.unsafe.stackallocInitializer should not be here according to the specifications, but it seems it can in reality
    g.unsafe.fixedPointerInitializer.isOr(and(AND, g.variableReference), g.unsafe.stackallocInitializer, g.expression);
    g.structMemberDeclaration.or(g.unsafe.fixedSizeBufferDeclaration);
    g.unsafe.fixedSizeBufferDeclaration.is(opt(g.attributes), o2n(g.unsafe.fixedSizeBufferModifier), FIXED, g.type,
        one2n(g.unsafe.fixedSizeBufferDeclarator), SEMICOLON);
    g.unsafe.fixedSizeBufferModifier.isOr(NEW, PUBLIC, PROTECTED, INTERNAL, PRIVATE, UNSAFE);
    g.unsafe.fixedSizeBufferDeclarator.is(IDENTIFIER, LBRACKET, g.constantExpression, RBRACKET);
    g.localVariableInitializer.or(g.unsafe.stackallocInitializer);
    g.unsafe.stackallocInitializer.is(STACKALLOC, g.type, LBRACKET, g.expression, RBRACKET);

  }

}
