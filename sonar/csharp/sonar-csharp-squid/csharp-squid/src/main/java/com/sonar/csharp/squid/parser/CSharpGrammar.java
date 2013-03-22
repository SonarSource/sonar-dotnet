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

import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpPunctuator;
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
 * Definition of each element of the C# grammar, based on the C# language specification 5.0
 */
public enum CSharpGrammar implements GrammarRuleKey {

  LITERAL,
  RIGHT_SHIFT,
  RIGHT_SHIFT_ASSIGNMENT,

  // A.2.1 Basic concepts
  COMPILATION_UNIT,
  NAMESPACE_NAME,
  TYPE_NAME,
  NAMESPACE_OR_TYPE_NAME,

  // A.2.2 Types
  SIMPLE_TYPE,
  NUMERIC_TYPE,
  INTEGRAL_TYPE,
  FLOATING_POINT_TYPE,

  RANK_SPECIFIER,
  RANK_SPECIFIERS,

  TYPE_PRIMARY,
  NULLABLE_TYPE,
  POINTER_TYPE,
  ARRAY_TYPE,
  TYPE,

  NON_ARRAY_TYPE,
  NON_NULLABLE_VALUE_TYPE,

  CLASS_TYPE,
  INTERFACE_TYPE,
  ENUM_TYPE,
  DELEGATE_TYPE,

  // A.2.3 Variables
  VARIABLE_REFERENCE,

  // A.2.4 Expressions
  PRIMARY_EXPRESSION_PRIMARY,
  PRIMARY_NO_ARRAY_CREATION_EXPRESSION,
  POST_ELEMENT_ACCESS,
  POST_MEMBER_ACCESS,

  POST_INVOCATION,
  POST_INCREMENT,
  POST_DECREMENT,
  POST_POINTER_MEMBER_ACCESS,

  POSTFIX_EXPRESSION,
  PRIMARY_EXPRESSION,

  ARGUMENT_LIST,
  ARGUMENT,
  ARGUMENT_NAME,
  ARGUMENT_VALUE,
  SIMPLE_NAME,
  PARENTHESIZED_EXPRESSION,
  MEMBER_ACCESS,
  PREDEFINED_TYPE,
  THIS_ACCESS,
  BASE_ACCESS,
  OBJECT_CREATION_EXPRESSION,
  OBJECT_OR_COLLECTION_INITIALIZER,
  OBJECT_INITIALIZER,
  MEMBER_INITIALIZER,
  INITIALIZER_VALUE,
  COLLECTION_INITIALIZER,
  ELEMENT_INITIALIZER,
  EXPRESSION_LIST,
  ARRAY_CREATION_EXPRESSION,
  DELEGATE_CREATION_EXPRESSION,
  ANONYMOUS_OBJECT_CREATION_EXPRESSION,
  ANONYMOUS_OBJECT_INITIALIZER,
  MEMBER_DECLARATOR,
  TYPE_OF_EXPRESSION,
  UNBOUND_TYPE_NAME,
  GENERIC_DIMENSION_SPECIFIER,
  CHECKED_EXPRESSION,
  UNCHECKED_EXPRESSION,
  DEFAULT_VALUE_EXPRESSION,
  UNARY_EXPRESSION,
  AWAIT_EXPRESSION,
  MULTIPLICATIVE_EXPRESSION,
  ADDITIVE_EXPRESSION,
  SHIFT_EXPRESSION,
  RELATIONAL_EXPRESSION,
  EQUALITY_EXPRESSION,
  AND_EXPRESSION,
  EXCLUSIVE_OR_EXPRESSION,
  INCLUSIVE_OR_EXPRESSION,
  CONDITIONAL_AND_EXPRESSION,
  CONDITIONAL_OR_EXPRESSION,
  NULL_COALESCING_EXPRESSION,
  CONDITIONAL_EXPRESSION,
  LAMBDA_EXPRESSION,
  ANONYMOUS_METHOD_EXPRESSION,
  ANONYMOUS_FUNCTION_SIGNATURE,
  EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE,
  EXPLICIT_ANONYMOUS_FUNCTION_PARAMETER,
  ANONYMOUS_FUNCTION_PARAMETER_MODIFIER,
  IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE,
  IMPLICIT_ANONYMOUS_FUNCTION_PARAMETER,
  ANONYMOUS_FUNCTION_BODY,
  QUERY_EXPRESSION,
  FROM_CLAUSE,
  QUERY_BODY,
  QUERY_BODY_CLAUSE,
  LET_CLAUSE,
  WHERE_CLAUSE,
  JOIN_CLAUSE,
  JOIN_INTO_CLAUSE,
  ORDER_BY_CLAUSE,
  ORDERING,
  ORDERING_DIRECTION,
  SELECT_OR_GROUP_CLAUSE,
  SELECT_CLAUSE,
  GROUP_CLAUSE,
  QUERY_CONTINUATION,
  ASSIGNMENT,
  ASSIGNMENT_TARGET,
  EXPRESSION,
  NON_ASSIGNMENT_EXPRESSION,

  // A.2.5 Statement
  STATEMENT,
  EMBEDDED_STATEMENT,
  BLOCK,
  LABELED_STATEMENT,
  DECLARATION_STATEMENT,
  LOCAL_VARIABLE_DECLARATION,
  LOCAL_VARIABLE_DECLARATOR,
  LOCAL_VARIABLE_INITIALIZER,
  LOCAL_CONSTANT_DECLARATION,
  CONSTANT_DECLARATOR,
  EXPRESSION_STATEMENT,
  STATEMENT_EXPRESSION,
  SELECTION_STATEMENT,
  IF_STATEMENT,
  SWITCH_STATEMENT,
  SWITCH_SECTION,
  SWITCH_LABEL,
  ITERATION_STATEMENT,
  WHILE_STATEMENT,
  DO_STATEMENT,
  FOR_STATEMENT,
  FOR_INITIALIZER,
  FOR_CONDITION,
  FOR_ITERATOR,
  STATEMENT_EXPRESSION_LIST,
  FOREACH_STATEMENT,
  JUMP_STATEMENT,
  BREAK_STATEMENT,
  CONTINUE_STATEMENT,
  GOTO_STATEMENT,
  RETURN_STATEMENT,
  THROW_STATEMENT,
  TRY_STATEMENT,
  CATCH_CLAUSES,
  SPECIFIC_CATCH_CLAUSE,
  GENERAL_CATCH_CLAUSE,
  FINALLY_CLAUSE,
  CHECKED_STATEMENT,
  UNCHECKED_STATEMENT,
  LOCK_STATEMENT,
  USING_STATEMENT,
  RESOURCE_ACQUISITION,
  YIELD_STATEMENT,
  NAMESPACE_DECLARATION,
  QUALIFIED_IDENTIFIER,
  NAMESPACE_BODY,
  EXTERN_ALIAS_DIRECTIVE,
  USING_DIRECTIVE,
  USING_ALIAS_DIRECTIVE,
  USING_NAMESPACE_DIRECTIVE,
  NAMESPACE_MEMBER_DECLARATION,
  TYPE_DECLARATION,
  QUALIFIED_ALIAS_MEMBER,

  // A.2.6 Classes
  CLASS_DECLARATION,
  CLASS_MODIFIER,
  CLASS_BASE,
  INTERFACE_TYPE_LIST,
  CLASS_BODY,
  CLASS_MEMBER_DECLARATION,
  CONSTANT_DECLARATION,
  CONSTANT_MODIFIER,
  FIELD_DECLARATION,
  FIELD_MODIFIER,
  VARIABLE_DECLARATOR,
  VARIABLE_INITIALIZER,
  METHOD_DECLARATION,
  METHOD_HEADER,
  METHOD_MODIFIERS,
  METHOD_MODIFIER,
  RETURN_TYPE,
  MEMBER_NAME,
  METHOD_BODY,
  FORMAL_PARAMETER_LIST,
  FIXED_PARAMETERS,
  FIXED_PARAMETER,
  PARAMETER_MODIFIER,
  PARAMETER_ARRAY,
  PROPERTY_DECLARATION,
  PROPERTY_MODIFIER,
  ACCESSOR_DECLARATIONS,
  GET_ACCESSOR_DECLARATION,
  SET_ACCESSOR_DECLARATION,
  ACCESSOR_MODIFIER,
  ACCESSOR_BODY,
  EVENT_DECLARATION,
  EVENT_MODIFIER,
  EVENT_ACCESSOR_DECLARATIONS,
  ADD_ACCESSOR_DECLARATION,
  REMOVE_ACCESSOR_DECLARATION,
  INDEXER_DECLARATION,
  INDEXER_MODIFIER,
  INDEXER_DECLARATOR,
  OPERATOR_DECLARATION,
  OPERATOR_MODIFIER,
  OPERATOR_DECLARATOR,
  UNARY_OPERATOR_DECLARATOR,
  OVERLOADABLE_UNARY_OPERATOR,
  BINARY_OPERATOR_DECLARATOR,
  OVERLOADABLE_BINARY_OPERATOR,
  CONVERSION_OPERATOR_DECLARATOR,
  OPERATOR_BODY,
  CONSTRUCTOR_DECLARATION,
  CONSTRUCTOR_MODIFIER,
  CONSTRUCTOR_DECLARATOR,
  CONSTRUCTOR_INITIALIZER,
  CONSTRUCTOR_BODY,
  STATIC_CONSTRUCTOR_DECLARATION,
  STATIC_CONSTRUCTOR_MODIFIERS,
  STATIC_CONSTRUCTOR_BODY,
  DESTRUCTOR_DECLARATION,
  DESTRUCTOR_BODY,

  // A.2.7 Struct
  STRUCT_DECLARATION,
  STRUCT_MODIFIER,
  STRUCT_INTERFACES,
  STRUCT_BODY,
  STRUCT_MEMBER_DECLARATION,

  // A.2.8 Arrays
  ARRAY_INITIALIZER,
  VARIABLE_INITIALIZER_LIST,

  // A.2.9 Interfaces
  INTERFACE_DECLARATION,
  INTERFACE_MODIFIER,
  VARIANT_TYPE_PARAMETER_LIST,
  VARIANT_TYPE_PARAMETER,
  VARIANCE_ANNOTATION,
  INTERFACE_BASE,
  INTERFACE_BODY,
  INTERFACE_MEMBER_DECLARATION,
  INTERFACE_METHOD_DECLARATION,
  INTERFACE_PROPERTY_DECLARATION,
  INTERFACE_ACCESSORS,
  INTERFACE_EVENT_DECLARATION,
  INTERFACE_INDEXER_DECLARATION,

  // A.2.10 Enums
  ENUM_DECLARATION,
  ENUM_BASE,
  ENUM_BODY,
  ENUM_MODIFIER,
  ENUM_MEMBER_DECLARATIONS,
  ENUM_MEMBER_DECLARATION,

  // A.2.11 Delegates
  DELEGATE_DECLARATION,
  DELEGATE_MODIFIER,

  // A.2.12 Attributes
  GLOBAL_ATTRIBUTES,
  GLOBAL_ATTRIBUTE_SECTION,
  GLOBAL_ATTRIBUTE_TARGET_SPECIFIER,
  GLOBAL_ATTRIBUTE_TARGET,
  ATTRIBUTES,
  ATTRIBUTE_SECTION,
  ATTRIBUTE_TARGET_SPECIFIER,
  ATTRIBUTE_TARGET,
  ATTRIBUTE_LIST,
  ATTRIBUTE,
  ATTRIBUTE_NAME,
  ATTRIBUTE_ARGUMENTS,
  POSITIONAL_ARGUMENT,
  NAMED_ARGUMENT,
  ATTRIBUTE_ARGUMENT_EXPRESSION,

  // A.2.13 Generics
  TYPE_PARAMETER_LIST,
  TYPE_PARAMETERS,
  TYPE_PARAMETER,
  TYPE_ARGUMENT_LIST,
  TYPE_ARGUMENT,
  TYPE_PARAMETER_CONSTRAINTS_CLAUSES,
  TYPE_PARAMETER_CONSTRAINTS_CLAUSE,
  TYPE_PARAMETER_CONSTRAINTS,
  PRIMARY_CONSTRAINT,
  SECONDARY_CONSTRAINTS,
  CONSTRUCTOR_CONSTRAINT,

  // A.3 Unsafe code
  UNSAFE_STATEMENT,
  POINTER_INDIRECTION_EXPRESSION,
  POINTER_ELEMENT_ACCESS,
  ADDRESS_OF_EXPRESSION,
  SIZE_OF_EXPRESSION,
  FIXED_STATEMENT,
  FIXED_POINTER_DECLARATOR,
  FIXED_POINTER_INITIALIZER,
  FIXED_SIZE_BUFFER_DECLARATION,
  FIXED_SIZE_BUFFER_MODIFIER,
  FIXED_SIZE_BUFFER_DECLARATOR,
  STACKALLOC_INITIALIZER,

  // Contextual keywords
  ASYNC,
  SET,
  GET,
  PARTIAL;

  public static LexerfulGrammarBuilder create() {
    LexerfulGrammarBuilder b = LexerfulGrammarBuilder.create();

    b.rule(LITERAL).is(
        b.firstOf(
            TRUE,
            FALSE,
            INTEGER_DEC_LITERAL,
            INTEGER_HEX_LITERAL,
            REAL_LITERAL,
            CHARACTER_LITERAL,
            STRING_LITERAL,
            NULL));
    b.rule(RIGHT_SHIFT).is(SUPERIOR, SUPERIOR);
    b.rule(RIGHT_SHIFT_ASSIGNMENT).is(SUPERIOR, GE_OP);

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

    // Contextual keywords
    contextualKeywords(b);

    b.setRootRule(COMPILATION_UNIT);

    return b;
  }

  private static void basicConcepts(LexerfulGrammarBuilder b) {
    b.rule(COMPILATION_UNIT)
        .is(b.zeroOrMore(EXTERN_ALIAS_DIRECTIVE), b.zeroOrMore(USING_DIRECTIVE), b.optional(GLOBAL_ATTRIBUTES), b.zeroOrMore(NAMESPACE_MEMBER_DECLARATION), EOF);
    b.rule(NAMESPACE_NAME).is(NAMESPACE_OR_TYPE_NAME);
    b.rule(TYPE_NAME).is(NAMESPACE_OR_TYPE_NAME);
    b.rule(NAMESPACE_OR_TYPE_NAME).is(
        b.firstOf(
            QUALIFIED_ALIAS_MEMBER,
            b.sequence(IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST))),
        b.zeroOrMore(DOT, IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST)));
  }

  private static void types(LexerfulGrammarBuilder b) {
    b.rule(SIMPLE_TYPE).is(
        b.firstOf(
            NUMERIC_TYPE,
            BOOL));
    b.rule(NUMERIC_TYPE).is(
        b.firstOf(
            INTEGRAL_TYPE,
            FLOATING_POINT_TYPE,
            DECIMAL));
    b.rule(INTEGRAL_TYPE).is(
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
    b.rule(FLOATING_POINT_TYPE).is(
        b.firstOf(
            FLOAT,
            DOUBLE));

    b.rule(RANK_SPECIFIER).is(LBRACKET, b.zeroOrMore(COMMA), RBRACKET);
    b.rule(RANK_SPECIFIERS).is(b.oneOrMore(RANK_SPECIFIER));

    b.rule(TYPE_PRIMARY).is(
        b.firstOf(
            SIMPLE_TYPE,
            "dynamic",
            OBJECT,
            STRING,
            TYPE_NAME)).skip();
    b.rule(NULLABLE_TYPE).is(
        TYPE_PRIMARY, QUESTION,
        b.firstOf(
            b.next(b.bridge(CSharpPunctuator.LPARENTHESIS, CSharpPunctuator.RPARENTHESIS), CSharpPunctuator.COLON),
            b.nextNot(EXPRESSION, COLON)));
    b.rule(POINTER_TYPE).is(
        b.firstOf(
            NULLABLE_TYPE,
            TYPE_PRIMARY,
            VOID),
        STAR);
    b.rule(ARRAY_TYPE).is(
        b.firstOf(
            POINTER_TYPE,
            NULLABLE_TYPE,
            TYPE_PRIMARY),
        RANK_SPECIFIERS);
    b.rule(TYPE).is(
        b.firstOf(
            ARRAY_TYPE,
            POINTER_TYPE,
            NULLABLE_TYPE,
            TYPE_PRIMARY));

    b.rule(NON_NULLABLE_VALUE_TYPE).is(b.nextNot(NULLABLE_TYPE), TYPE);
    b.rule(NON_ARRAY_TYPE).is(b.nextNot(ARRAY_TYPE), TYPE);

    b.rule(CLASS_TYPE).is(
        b.firstOf(
            "dynamic",
            OBJECT,
            STRING,
            TYPE_NAME));
    b.rule(INTERFACE_TYPE).is(TYPE_NAME);
    b.rule(ENUM_TYPE).is(TYPE_NAME);
    b.rule(DELEGATE_TYPE).is(TYPE_NAME);
  }

  private static void variables(LexerfulGrammarBuilder b) {
    b.rule(VARIABLE_REFERENCE).is(EXPRESSION);
  }

  private static void expressions(LexerfulGrammarBuilder b) {
    b.rule(PRIMARY_EXPRESSION_PRIMARY).is(
        b.firstOf(
            ARRAY_CREATION_EXPRESSION,
            PRIMARY_NO_ARRAY_CREATION_EXPRESSION)).skip();
    b.rule(PRIMARY_NO_ARRAY_CREATION_EXPRESSION).is(
        b.firstOf(
            PARENTHESIZED_EXPRESSION,
            MEMBER_ACCESS,
            THIS_ACCESS,
            BASE_ACCESS,
            OBJECT_CREATION_EXPRESSION,
            DELEGATE_CREATION_EXPRESSION,
            ANONYMOUS_OBJECT_CREATION_EXPRESSION,
            TYPE_OF_EXPRESSION,
            CHECKED_EXPRESSION,
            UNCHECKED_EXPRESSION,
            DEFAULT_VALUE_EXPRESSION,
            ANONYMOUS_METHOD_EXPRESSION,
            LITERAL,
            SIMPLE_NAME,
            unsafe(SIZE_OF_EXPRESSION))).skip();

    b.rule(POST_MEMBER_ACCESS).is(DOT, IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST));
    b.rule(POST_ELEMENT_ACCESS).is(LBRACKET, ARGUMENT_LIST, RBRACKET);
    b.rule(POST_POINTER_MEMBER_ACCESS).is(PTR_OP, IDENTIFIER);
    b.rule(POST_INCREMENT).is(INC_OP);
    b.rule(POST_DECREMENT).is(DEC_OP);
    b.rule(POST_INVOCATION).is(LPARENTHESIS, b.optional(ARGUMENT_LIST), RPARENTHESIS);

    b.rule(POSTFIX_EXPRESSION).is(
        PRIMARY_EXPRESSION_PRIMARY,
        b.zeroOrMore(
            b.firstOf(
                POST_MEMBER_ACCESS,
                POST_ELEMENT_ACCESS,
                POST_POINTER_MEMBER_ACCESS,
                POST_INCREMENT,
                POST_DECREMENT,
                POST_INVOCATION))).skipIfOneChild();
    b.rule(PRIMARY_EXPRESSION).is(POSTFIX_EXPRESSION);

    b.rule(ARGUMENT_LIST).is(ARGUMENT, b.zeroOrMore(COMMA, ARGUMENT));
    b.rule(ARGUMENT).is(b.optional(ARGUMENT_NAME), ARGUMENT_VALUE);
    b.rule(ARGUMENT_NAME).is(IDENTIFIER, COLON);
    b.rule(ARGUMENT_VALUE).is(
        b.firstOf(
            EXPRESSION,
            b.sequence(REF, VARIABLE_REFERENCE),
            b.sequence(OUT, VARIABLE_REFERENCE)));
    b.rule(SIMPLE_NAME).is(IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST));
    b.rule(PARENTHESIZED_EXPRESSION).is(LPARENTHESIS, EXPRESSION, RPARENTHESIS);
    b.rule(MEMBER_ACCESS).is(
        b.firstOf(
            b.sequence(QUALIFIED_ALIAS_MEMBER, DOT, IDENTIFIER),
            b.sequence(PREDEFINED_TYPE, DOT, IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST))));
    b.rule(PREDEFINED_TYPE).is(
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
    b.rule(THIS_ACCESS).is(THIS);
    b.rule(BASE_ACCESS).is(
        BASE,
        b.firstOf(
            b.sequence(DOT, IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST)),
            b.sequence(LBRACKET, ARGUMENT_LIST, RBRACKET)));
    b.rule(OBJECT_CREATION_EXPRESSION).is(
        NEW, TYPE,
        b.firstOf(
            b.sequence(LPARENTHESIS, b.optional(ARGUMENT_LIST), RPARENTHESIS, b.optional(OBJECT_OR_COLLECTION_INITIALIZER)),
            OBJECT_OR_COLLECTION_INITIALIZER));
    b.rule(OBJECT_OR_COLLECTION_INITIALIZER).is(
        b.firstOf(
            OBJECT_INITIALIZER,
            COLLECTION_INITIALIZER));
    b.rule(OBJECT_INITIALIZER).is(LCURLYBRACE, b.optional(MEMBER_INITIALIZER), b.zeroOrMore(COMMA, MEMBER_INITIALIZER), b.optional(COMMA), RCURLYBRACE);
    b.rule(MEMBER_INITIALIZER).is(IDENTIFIER, EQUAL, INITIALIZER_VALUE);
    b.rule(INITIALIZER_VALUE).is(
        b.firstOf(
            EXPRESSION,
            OBJECT_OR_COLLECTION_INITIALIZER));
    b.rule(COLLECTION_INITIALIZER).is(LCURLYBRACE, ELEMENT_INITIALIZER, b.zeroOrMore(COMMA, ELEMENT_INITIALIZER), b.optional(COMMA), RCURLYBRACE);
    b.rule(ELEMENT_INITIALIZER).is(
        b.firstOf(
            NON_ASSIGNMENT_EXPRESSION,
            b.sequence(LCURLYBRACE, EXPRESSION_LIST, RCURLYBRACE)));
    b.rule(EXPRESSION_LIST).is(EXPRESSION, b.zeroOrMore(COMMA, EXPRESSION));
    b.rule(ARRAY_CREATION_EXPRESSION).is(
        b.firstOf(
            b.sequence(NEW, NON_ARRAY_TYPE, LBRACKET, EXPRESSION_LIST, RBRACKET, b.zeroOrMore(RANK_SPECIFIER), b.optional(ARRAY_INITIALIZER)),
            b.sequence(NEW, ARRAY_TYPE, ARRAY_INITIALIZER), b.sequence(NEW, RANK_SPECIFIER, ARRAY_INITIALIZER)));
    b.rule(DELEGATE_CREATION_EXPRESSION).is(NEW, DELEGATE_TYPE, LPARENTHESIS, EXPRESSION, RPARENTHESIS);
    b.rule(ANONYMOUS_OBJECT_CREATION_EXPRESSION).is(NEW, ANONYMOUS_OBJECT_INITIALIZER);
    b.rule(ANONYMOUS_OBJECT_INITIALIZER).is(LCURLYBRACE, b.optional(MEMBER_DECLARATOR), b.zeroOrMore(COMMA, MEMBER_DECLARATOR), b.optional(COMMA), RCURLYBRACE);
    b.rule(MEMBER_DECLARATOR).is(b.optional(IDENTIFIER, EQUAL), EXPRESSION);
    b.rule(TYPE_OF_EXPRESSION).is(TYPEOF, b.bridge(LPARENTHESIS, RPARENTHESIS));
    b.rule(UNBOUND_TYPE_NAME).is(
        b.oneOrMore(
            IDENTIFIER, b.optional(DOUBLE_COLON, IDENTIFIER), b.optional(GENERIC_DIMENSION_SPECIFIER),
            b.optional(DOT, IDENTIFIER, b.optional(GENERIC_DIMENSION_SPECIFIER))), b.optional(DOT, IDENTIFIER, b.optional(GENERIC_DIMENSION_SPECIFIER)));
    b.rule(GENERIC_DIMENSION_SPECIFIER).is(INFERIOR, b.zeroOrMore(COMMA), SUPERIOR);
    b.rule(CHECKED_EXPRESSION).is(CHECKED, LPARENTHESIS, EXPRESSION, RPARENTHESIS);
    b.rule(UNCHECKED_EXPRESSION).is(UNCHECKED, LPARENTHESIS, EXPRESSION, RPARENTHESIS);
    b.rule(DEFAULT_VALUE_EXPRESSION).is(DEFAULT, LPARENTHESIS, TYPE, RPARENTHESIS);

    b.rule(UNARY_EXPRESSION).is(
        b.firstOf(
            AWAIT_EXPRESSION,
            b.sequence(LPARENTHESIS, TYPE, RPARENTHESIS, UNARY_EXPRESSION),
            PRIMARY_EXPRESSION,
            b.sequence(
                b.firstOf(
                    MINUS,
                    EXCLAMATION,
                    INC_OP,
                    DEC_OP,
                    TILDE,
                    PLUS),
                UNARY_EXPRESSION),
            unsafe(
            b.firstOf(
                POINTER_INDIRECTION_EXPRESSION,
                ADDRESS_OF_EXPRESSION)))).skipIfOneChild();

    b.rule(AWAIT_EXPRESSION).is("await", UNARY_EXPRESSION);

    b.rule(MULTIPLICATIVE_EXPRESSION).is(
        UNARY_EXPRESSION,
        b.zeroOrMore(
            b.firstOf(
                STAR,
                SLASH,
                MODULO),
            UNARY_EXPRESSION)).skipIfOneChild();
    b.rule(ADDITIVE_EXPRESSION).is(
        MULTIPLICATIVE_EXPRESSION,
        b.zeroOrMore(
            b.firstOf(
                PLUS,
                MINUS),
            MULTIPLICATIVE_EXPRESSION)).skipIfOneChild();
    b.rule(SHIFT_EXPRESSION).is(
        ADDITIVE_EXPRESSION,
        b.zeroOrMore(
            b.firstOf(
                LEFT_OP,
                RIGHT_SHIFT),
            ADDITIVE_EXPRESSION)).skipIfOneChild();
    b.rule(RELATIONAL_EXPRESSION).is(
        SHIFT_EXPRESSION,
        b.zeroOrMore(
            b.firstOf(
                b.sequence(
                    b.firstOf(
                        INFERIOR,
                        SUPERIOR,
                        LE_OP,
                        GE_OP),
                    SHIFT_EXPRESSION),
                b.sequence(
                    b.firstOf(
                        IS,
                        AS),
                    TYPE)))).skipIfOneChild();
    b.rule(EQUALITY_EXPRESSION).is(
        RELATIONAL_EXPRESSION,
        b.zeroOrMore(
            b.firstOf(
                EQ_OP,
                NE_OP),
            RELATIONAL_EXPRESSION)).skipIfOneChild();
    b.rule(AND_EXPRESSION).is(EQUALITY_EXPRESSION, b.zeroOrMore(AND, EQUALITY_EXPRESSION)).skipIfOneChild();
    b.rule(EXCLUSIVE_OR_EXPRESSION).is(AND_EXPRESSION, b.zeroOrMore(XOR, AND_EXPRESSION)).skipIfOneChild();
    b.rule(INCLUSIVE_OR_EXPRESSION).is(EXCLUSIVE_OR_EXPRESSION, b.zeroOrMore(OR, EXCLUSIVE_OR_EXPRESSION)).skipIfOneChild();
    b.rule(CONDITIONAL_AND_EXPRESSION).is(INCLUSIVE_OR_EXPRESSION, b.zeroOrMore(AND_OP, INCLUSIVE_OR_EXPRESSION)).skipIfOneChild();
    b.rule(CONDITIONAL_OR_EXPRESSION).is(CONDITIONAL_AND_EXPRESSION, b.zeroOrMore(OR_OP, CONDITIONAL_AND_EXPRESSION)).skipIfOneChild();
    b.rule(NULL_COALESCING_EXPRESSION).is(CONDITIONAL_OR_EXPRESSION, b.optional(DOUBLE_QUESTION, NULL_COALESCING_EXPRESSION)).skipIfOneChild();
    b.rule(CONDITIONAL_EXPRESSION).is(NULL_COALESCING_EXPRESSION, b.optional(QUESTION, EXPRESSION, COLON, EXPRESSION)).skipIfOneChild();
    b.rule(LAMBDA_EXPRESSION).is(b.optional(ASYNC), ANONYMOUS_FUNCTION_SIGNATURE, LAMBDA, ANONYMOUS_FUNCTION_BODY);
    b.rule(ANONYMOUS_METHOD_EXPRESSION).is(b.optional(ASYNC), DELEGATE, b.optional(EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE), BLOCK);
    b.rule(ANONYMOUS_FUNCTION_SIGNATURE).is(
        b.firstOf(
            EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE,
            IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE));
    b.rule(EXPLICIT_ANONYMOUS_FUNCTION_SIGNATURE).is(LPARENTHESIS, b.optional(EXPLICIT_ANONYMOUS_FUNCTION_PARAMETER, b.zeroOrMore(COMMA, EXPLICIT_ANONYMOUS_FUNCTION_PARAMETER)),
        RPARENTHESIS);
    b.rule(EXPLICIT_ANONYMOUS_FUNCTION_PARAMETER).is(b.optional(ANONYMOUS_FUNCTION_PARAMETER_MODIFIER), TYPE, IDENTIFIER);
    b.rule(ANONYMOUS_FUNCTION_PARAMETER_MODIFIER).is(
        b.firstOf(
            CSharpKeyword.REF,
            CSharpKeyword.OUT));
    b.rule(IMPLICIT_ANONYMOUS_FUNCTION_SIGNATURE).is(
        b.firstOf(
            IMPLICIT_ANONYMOUS_FUNCTION_PARAMETER,
            b.sequence(LPARENTHESIS, b.optional(IMPLICIT_ANONYMOUS_FUNCTION_PARAMETER, b.zeroOrMore(COMMA, IMPLICIT_ANONYMOUS_FUNCTION_PARAMETER)), RPARENTHESIS)));
    b.rule(IMPLICIT_ANONYMOUS_FUNCTION_PARAMETER).is(IDENTIFIER);
    b.rule(ANONYMOUS_FUNCTION_BODY).is(
        b.firstOf(
            EXPRESSION,
            BLOCK));
    b.rule(QUERY_EXPRESSION).is(FROM_CLAUSE, QUERY_BODY);
    b.rule(FROM_CLAUSE).is(
        "from",
        b.firstOf(
            b.sequence(TYPE, IDENTIFIER),
            IDENTIFIER),
        IN, EXPRESSION);
    b.rule(QUERY_BODY).is(b.zeroOrMore(QUERY_BODY_CLAUSE), SELECT_OR_GROUP_CLAUSE, b.optional(QUERY_CONTINUATION));
    b.rule(QUERY_BODY_CLAUSE).is(
        b.firstOf(
            FROM_CLAUSE,
            LET_CLAUSE,
            WHERE_CLAUSE,
            JOIN_INTO_CLAUSE,
            JOIN_CLAUSE,
            ORDER_BY_CLAUSE));
    b.rule(LET_CLAUSE).is("let", IDENTIFIER, EQUAL, EXPRESSION);
    b.rule(WHERE_CLAUSE).is("where", EXPRESSION);
    b.rule(JOIN_CLAUSE).is(
        "join",
        b.firstOf(
            b.sequence(TYPE, IDENTIFIER),
            IDENTIFIER),
        IN, EXPRESSION, "on", EXPRESSION, "equals", EXPRESSION);
    b.rule(JOIN_INTO_CLAUSE).is(
        "join",
        b.firstOf(
            b.sequence(TYPE, IDENTIFIER),
            IDENTIFIER),
        IN, EXPRESSION, "on", EXPRESSION, "equals", EXPRESSION,
        "into", IDENTIFIER);
    b.rule(ORDER_BY_CLAUSE).is("orderby", ORDERING, b.zeroOrMore(COMMA, ORDERING));
    b.rule(ORDERING).is(EXPRESSION, b.optional(ORDERING_DIRECTION));
    b.rule(ORDERING_DIRECTION).is(
        b.firstOf(
            "ascending",
            "descending"));
    b.rule(SELECT_OR_GROUP_CLAUSE).is(
        b.firstOf(
            SELECT_CLAUSE,
            GROUP_CLAUSE));
    b.rule(SELECT_CLAUSE).is("select", EXPRESSION);
    b.rule(GROUP_CLAUSE).is("group", EXPRESSION, "by", EXPRESSION);
    b.rule(QUERY_CONTINUATION).is("into", IDENTIFIER, QUERY_BODY);
    b.rule(ASSIGNMENT).is(
        ASSIGNMENT_TARGET,
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
            RIGHT_SHIFT_ASSIGNMENT),
        EXPRESSION);
    b.rule(ASSIGNMENT_TARGET).is(UNARY_EXPRESSION);
    b.rule(NON_ASSIGNMENT_EXPRESSION).is(
        b.firstOf(
            LAMBDA_EXPRESSION,
            QUERY_EXPRESSION,
            CONDITIONAL_EXPRESSION)).skip();
    b.rule(EXPRESSION).is(
        b.firstOf(
            ASSIGNMENT,
            NON_ASSIGNMENT_EXPRESSION));
  }

  private static void statements(LexerfulGrammarBuilder b) {
    b.rule(STATEMENT).is(
        b.firstOf(
            LABELED_STATEMENT,
            DECLARATION_STATEMENT,
            EMBEDDED_STATEMENT));
    b.rule(EMBEDDED_STATEMENT).is(
        b.firstOf(
            BLOCK,
            SEMICOLON,
            EXPRESSION_STATEMENT,
            SELECTION_STATEMENT,
            ITERATION_STATEMENT,
            JUMP_STATEMENT,
            TRY_STATEMENT,
            CHECKED_STATEMENT,
            UNCHECKED_STATEMENT,
            LOCK_STATEMENT,
            USING_STATEMENT,
            YIELD_STATEMENT));
    b.rule(BLOCK).is(LCURLYBRACE, b.zeroOrMore(STATEMENT), RCURLYBRACE);
    b.rule(LABELED_STATEMENT).is(IDENTIFIER, COLON, STATEMENT);
    b.rule(DECLARATION_STATEMENT).is(
        b.firstOf(
            LOCAL_VARIABLE_DECLARATION,
            LOCAL_CONSTANT_DECLARATION),
        SEMICOLON);
    b.rule(LOCAL_VARIABLE_DECLARATION).is(TYPE, LOCAL_VARIABLE_DECLARATOR, b.zeroOrMore(COMMA, LOCAL_VARIABLE_DECLARATOR));
    b.rule(LOCAL_VARIABLE_DECLARATOR).is(IDENTIFIER, b.optional(EQUAL, LOCAL_VARIABLE_INITIALIZER));
    b.rule(LOCAL_VARIABLE_INITIALIZER).is(
        b.firstOf(
            EXPRESSION,
            ARRAY_INITIALIZER,
            unsafe(STACKALLOC_INITIALIZER)));
    b.rule(LOCAL_CONSTANT_DECLARATION).is(CONST, TYPE, CONSTANT_DECLARATOR, b.zeroOrMore(COMMA, CONSTANT_DECLARATOR));
    b.rule(CONSTANT_DECLARATOR).is(IDENTIFIER, EQUAL, EXPRESSION);
    b.rule(EXPRESSION_STATEMENT).is(STATEMENT_EXPRESSION, SEMICOLON);
    b.rule(STATEMENT_EXPRESSION).is(EXPRESSION);
    b.rule(SELECTION_STATEMENT).is(
        b.firstOf(
            IF_STATEMENT,
            SWITCH_STATEMENT));
    b.rule(IF_STATEMENT).is(IF, LPARENTHESIS, EXPRESSION, RPARENTHESIS, EMBEDDED_STATEMENT, b.optional(ELSE, EMBEDDED_STATEMENT));
    b.rule(SWITCH_STATEMENT).is(SWITCH, LPARENTHESIS, EXPRESSION, RPARENTHESIS, LCURLYBRACE, b.zeroOrMore(SWITCH_SECTION), RCURLYBRACE);
    b.rule(SWITCH_SECTION).is(b.oneOrMore(SWITCH_LABEL), b.oneOrMore(STATEMENT));
    b.rule(SWITCH_LABEL).is(
        b.firstOf(
            b.sequence(CASE, EXPRESSION, COLON),
            b.sequence(DEFAULT, COLON)));
    b.rule(ITERATION_STATEMENT).is(
        b.firstOf(
            WHILE_STATEMENT,
            DO_STATEMENT,
            FOR_STATEMENT,
            FOREACH_STATEMENT));
    b.rule(WHILE_STATEMENT).is(WHILE, LPARENTHESIS, EXPRESSION, RPARENTHESIS, EMBEDDED_STATEMENT);
    b.rule(DO_STATEMENT).is(DO, EMBEDDED_STATEMENT, WHILE, LPARENTHESIS, EXPRESSION, RPARENTHESIS, SEMICOLON);
    b.rule(FOR_STATEMENT)
        .is(FOR, LPARENTHESIS, b.optional(FOR_INITIALIZER), SEMICOLON, b.optional(FOR_CONDITION), SEMICOLON, b.optional(FOR_ITERATOR), RPARENTHESIS, EMBEDDED_STATEMENT);
    b.rule(FOR_INITIALIZER).is(
        b.firstOf(
            LOCAL_VARIABLE_DECLARATION,
            STATEMENT_EXPRESSION_LIST));
    b.rule(FOR_CONDITION).is(EXPRESSION);
    b.rule(FOR_ITERATOR).is(STATEMENT_EXPRESSION_LIST);
    b.rule(STATEMENT_EXPRESSION_LIST).is(EXPRESSION, b.zeroOrMore(COMMA, EXPRESSION));
    b.rule(FOREACH_STATEMENT).is(FOREACH, LPARENTHESIS, TYPE, IDENTIFIER, IN, EXPRESSION, RPARENTHESIS, EMBEDDED_STATEMENT);
    b.rule(JUMP_STATEMENT).is(
        b.firstOf(
            BREAK_STATEMENT,
            CONTINUE_STATEMENT,
            GOTO_STATEMENT,
            RETURN_STATEMENT,
            THROW_STATEMENT));
    b.rule(BREAK_STATEMENT).is(BREAK, SEMICOLON);
    b.rule(CONTINUE_STATEMENT).is(CONTINUE, SEMICOLON);
    b.rule(GOTO_STATEMENT).is(
        GOTO,
        b.firstOf(
            IDENTIFIER,
            b.sequence(CASE, EXPRESSION),
            DEFAULT),
        SEMICOLON);
    b.rule(RETURN_STATEMENT).is(RETURN, b.optional(EXPRESSION), SEMICOLON);
    b.rule(THROW_STATEMENT).is(THROW, b.optional(EXPRESSION), SEMICOLON);
    b.rule(TRY_STATEMENT).is(
        TRY, BLOCK,
        b.firstOf(
            b.sequence(b.optional(CATCH_CLAUSES), FINALLY_CLAUSE),
            CATCH_CLAUSES));
    b.rule(CATCH_CLAUSES).is(
        b.firstOf(
            b.sequence(b.zeroOrMore(SPECIFIC_CATCH_CLAUSE), GENERAL_CATCH_CLAUSE),
            b.oneOrMore(SPECIFIC_CATCH_CLAUSE)));
    b.rule(SPECIFIC_CATCH_CLAUSE).is(CATCH, LPARENTHESIS, CLASS_TYPE, b.optional(IDENTIFIER), RPARENTHESIS, BLOCK);
    b.rule(GENERAL_CATCH_CLAUSE).is(CATCH, BLOCK);
    b.rule(FINALLY_CLAUSE).is(FINALLY, BLOCK);
    b.rule(CHECKED_STATEMENT).is(CHECKED, BLOCK);
    b.rule(UNCHECKED_STATEMENT).is(UNCHECKED, BLOCK);
    b.rule(LOCK_STATEMENT).is(LOCK, LPARENTHESIS, EXPRESSION, RPARENTHESIS, EMBEDDED_STATEMENT);
    b.rule(USING_STATEMENT).is(USING, LPARENTHESIS, RESOURCE_ACQUISITION, RPARENTHESIS, EMBEDDED_STATEMENT);
    b.rule(RESOURCE_ACQUISITION).is(
        b.firstOf(
            LOCAL_VARIABLE_DECLARATION,
            EXPRESSION));
    b.rule(YIELD_STATEMENT).is(
        "yield",
        b.firstOf(
            b.sequence(RETURN, EXPRESSION),
            BREAK),
        SEMICOLON);
    b.rule(NAMESPACE_DECLARATION).is(NAMESPACE, QUALIFIED_IDENTIFIER, NAMESPACE_BODY, b.optional(SEMICOLON));
    b.rule(QUALIFIED_IDENTIFIER).is(IDENTIFIER, b.zeroOrMore(DOT, IDENTIFIER));
    b.rule(NAMESPACE_BODY).is(LCURLYBRACE, b.zeroOrMore(EXTERN_ALIAS_DIRECTIVE), b.zeroOrMore(USING_DIRECTIVE), b.zeroOrMore(NAMESPACE_MEMBER_DECLARATION), RCURLYBRACE);
    b.rule(EXTERN_ALIAS_DIRECTIVE).is(EXTERN, "alias", IDENTIFIER, SEMICOLON);
    b.rule(USING_DIRECTIVE).is(
        b.firstOf(
            USING_ALIAS_DIRECTIVE,
            USING_NAMESPACE_DIRECTIVE));
    b.rule(USING_ALIAS_DIRECTIVE).is(USING, IDENTIFIER, EQUAL, NAMESPACE_OR_TYPE_NAME, SEMICOLON);
    b.rule(USING_NAMESPACE_DIRECTIVE).is(USING, NAMESPACE_NAME, SEMICOLON);
    b.rule(NAMESPACE_MEMBER_DECLARATION).is(
        b.firstOf(
            NAMESPACE_DECLARATION,
            TYPE_DECLARATION));
    b.rule(TYPE_DECLARATION).is(
        b.firstOf(
            CLASS_DECLARATION,
            STRUCT_DECLARATION,
            INTERFACE_DECLARATION,
            ENUM_DECLARATION,
            DELEGATE_DECLARATION));
    b.rule(QUALIFIED_ALIAS_MEMBER).is(IDENTIFIER, DOUBLE_COLON, IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST));
  }

  private static void classes(LexerfulGrammarBuilder b) {
    b.rule(CLASS_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(CLASS_MODIFIER), b.optional(PARTIAL),
        CLASS, IDENTIFIER, b.optional(TYPE_PARAMETER_LIST), b.optional(CLASS_BASE), b.optional(TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
        CLASS_BODY,
        b.optional(SEMICOLON));
    b.rule(CLASS_MODIFIER).is(
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
    b.rule(CLASS_BASE).is(
        COLON,
        b.firstOf(
            b.sequence(CLASS_TYPE, COMMA, INTERFACE_TYPE_LIST),
            CLASS_TYPE,
            INTERFACE_TYPE_LIST));
    b.rule(INTERFACE_TYPE_LIST).is(INTERFACE_TYPE, b.zeroOrMore(COMMA, INTERFACE_TYPE));
    b.rule(CLASS_BODY).is(LCURLYBRACE, b.zeroOrMore(CLASS_MEMBER_DECLARATION), RCURLYBRACE);
    b.rule(CLASS_MEMBER_DECLARATION).is(
        b.firstOf(
            CONSTANT_DECLARATION,
            FIELD_DECLARATION,
            METHOD_DECLARATION,
            PROPERTY_DECLARATION,
            EVENT_DECLARATION,
            INDEXER_DECLARATION,
            OPERATOR_DECLARATION,
            CONSTRUCTOR_DECLARATION,
            DESTRUCTOR_DECLARATION,
            STATIC_CONSTRUCTOR_DECLARATION,
            TYPE_DECLARATION));
    b.rule(CONSTANT_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(CONSTANT_MODIFIER),
        CONST, TYPE,
        CONSTANT_DECLARATOR, b.zeroOrMore(COMMA, CONSTANT_DECLARATOR),
        SEMICOLON);
    b.rule(CONSTANT_MODIFIER).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE));
    b.rule(FIELD_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(FIELD_MODIFIER),
        TYPE,
        VARIABLE_DECLARATOR, b.zeroOrMore(COMMA, VARIABLE_DECLARATOR),
        SEMICOLON);
    b.rule(FIELD_MODIFIER).is(
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
    b.rule(VARIABLE_DECLARATOR).is(IDENTIFIER, b.optional(EQUAL, VARIABLE_INITIALIZER));
    b.rule(VARIABLE_INITIALIZER).is(
        b.firstOf(
            EXPRESSION,
            ARRAY_INITIALIZER));
    b.rule(METHOD_DECLARATION).is(METHOD_HEADER, METHOD_BODY);
    b.rule(METHOD_HEADER).is(
        b.optional(ATTRIBUTES), b.optional(METHOD_MODIFIERS), b.optional(PARTIAL),
        RETURN_TYPE, MEMBER_NAME,
        b.optional(TYPE_PARAMETER_LIST),
        LPARENTHESIS, b.optional(FORMAL_PARAMETER_LIST), RPARENTHESIS,
        b.optional(TYPE_PARAMETER_CONSTRAINTS_CLAUSES)).skip();
    b.rule(METHOD_MODIFIERS).is(
        b.firstOf(
            b.sequence(
                b.next(ASYNC), METHOD_MODIFIER,
                b.zeroOrMore(b.nextNot(ASYNC), METHOD_MODIFIER)),
            b.sequence(
                b.oneOrMore(b.nextNot(ASYNC), METHOD_MODIFIER),
                b.optional(
                    b.next(ASYNC), METHOD_MODIFIER,
                    b.zeroOrMore(b.nextNot(ASYNC), METHOD_MODIFIER))))).skip();
    b.rule(METHOD_MODIFIER).is(
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
            UNSAFE,
            ASYNC));
    b.rule(RETURN_TYPE).is(
        b.firstOf(
            TYPE,
            VOID));
    b.rule(MEMBER_NAME).is(
        b.zeroOrMore(
            b.firstOf(
                QUALIFIED_ALIAS_MEMBER,
                b.sequence(
                    b.firstOf(
                        THIS,
                        IDENTIFIER),
                    b.optional(TYPE_ARGUMENT_LIST))),
            DOT),
        b.firstOf(
            THIS,
            IDENTIFIER),
        b.optional(TYPE_ARGUMENT_LIST));
    b.rule(METHOD_BODY).is(
        b.firstOf(
            BLOCK,
            SEMICOLON));
    b.rule(FORMAL_PARAMETER_LIST).is(
        b.firstOf(
            b.sequence(FIXED_PARAMETERS, b.optional(COMMA, PARAMETER_ARRAY)),
            PARAMETER_ARRAY));
    b.rule(FIXED_PARAMETERS).is(FIXED_PARAMETER, b.zeroOrMore(COMMA, FIXED_PARAMETER));
    b.rule(FIXED_PARAMETER).is(b.optional(ATTRIBUTES), b.optional(PARAMETER_MODIFIER), TYPE, IDENTIFIER, b.optional(EQUAL, EXPRESSION));
    b.rule(PARAMETER_MODIFIER).is(
        b.firstOf(
            REF,
            OUT,
            THIS));
    b.rule(PARAMETER_ARRAY).is(b.optional(ATTRIBUTES), PARAMS, ARRAY_TYPE, IDENTIFIER);
    b.rule(PROPERTY_DECLARATION).is(b.optional(ATTRIBUTES), b.zeroOrMore(PROPERTY_MODIFIER), TYPE, MEMBER_NAME, LCURLYBRACE, ACCESSOR_DECLARATIONS, RCURLYBRACE);
    b.rule(PROPERTY_MODIFIER).is(
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
    b.rule(ACCESSOR_DECLARATIONS).is(
        b.firstOf(
            b.sequence(GET_ACCESSOR_DECLARATION, b.optional(SET_ACCESSOR_DECLARATION)),
            b.sequence(SET_ACCESSOR_DECLARATION, b.optional(GET_ACCESSOR_DECLARATION))));
    b.rule(GET_ACCESSOR_DECLARATION).is(b.optional(ATTRIBUTES), b.zeroOrMore(ACCESSOR_MODIFIER), GET, ACCESSOR_BODY);
    b.rule(SET_ACCESSOR_DECLARATION).is(b.optional(ATTRIBUTES), b.zeroOrMore(ACCESSOR_MODIFIER), SET, ACCESSOR_BODY);
    b.rule(ACCESSOR_MODIFIER).is(
        b.firstOf(
            b.sequence(PROTECTED, INTERNAL),
            b.sequence(INTERNAL, PROTECTED),
            PROTECTED,
            INTERNAL,
            PRIVATE));
    b.rule(ACCESSOR_BODY).is(
        b.firstOf(
            BLOCK,
            SEMICOLON));
    b.rule(EVENT_DECLARATION).is(
        b.optional(ATTRIBUTES),
        b.zeroOrMore(EVENT_MODIFIER),
        EVENT, TYPE,
        b.firstOf(
            b.sequence(VARIABLE_DECLARATOR, b.zeroOrMore(COMMA, VARIABLE_DECLARATOR), SEMICOLON),
            b.sequence(MEMBER_NAME, LCURLYBRACE, EVENT_ACCESSOR_DECLARATIONS, RCURLYBRACE)));
    b.rule(EVENT_MODIFIER).is(
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
    b.rule(EVENT_ACCESSOR_DECLARATIONS).is(
        b.firstOf(
            b.sequence(ADD_ACCESSOR_DECLARATION, REMOVE_ACCESSOR_DECLARATION),
            b.sequence(REMOVE_ACCESSOR_DECLARATION, ADD_ACCESSOR_DECLARATION)));
    b.rule(ADD_ACCESSOR_DECLARATION).is(b.optional(ATTRIBUTES), "add", BLOCK);
    b.rule(REMOVE_ACCESSOR_DECLARATION).is(b.optional(ATTRIBUTES), "remove", BLOCK);
    b.rule(INDEXER_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(INDEXER_MODIFIER),
        INDEXER_DECLARATOR, LCURLYBRACE, ACCESSOR_DECLARATIONS, RCURLYBRACE);
    b.rule(INDEXER_MODIFIER).is(
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
    b.rule(INDEXER_DECLARATOR).is(
        TYPE,
        b.zeroOrMore(
            b.firstOf(
                QUALIFIED_ALIAS_MEMBER,
                b.sequence(IDENTIFIER, b.optional(TYPE_ARGUMENT_LIST))),
            DOT),
        THIS, LBRACKET, FORMAL_PARAMETER_LIST, RBRACKET);
    b.rule(OPERATOR_DECLARATION).is(b.optional(ATTRIBUTES), b.oneOrMore(OPERATOR_MODIFIER), OPERATOR_DECLARATOR, OPERATOR_BODY);
    b.rule(OPERATOR_MODIFIER).is(
        b.firstOf(
            PUBLIC,
            STATIC,
            EXTERN,
            UNSAFE));
    b.rule(OPERATOR_DECLARATOR).is(
        b.firstOf(
            UNARY_OPERATOR_DECLARATOR,
            BINARY_OPERATOR_DECLARATOR,
            CONVERSION_OPERATOR_DECLARATOR));
    b.rule(UNARY_OPERATOR_DECLARATOR).is(TYPE, OPERATOR, OVERLOADABLE_UNARY_OPERATOR, LPARENTHESIS, TYPE, IDENTIFIER, RPARENTHESIS);
    b.rule(OVERLOADABLE_UNARY_OPERATOR).is(
        b.firstOf(
            PLUS,
            MINUS,
            EXCLAMATION,
            TILDE,
            INC_OP,
            DEC_OP,
            TRUE,
            FALSE));
    b.rule(BINARY_OPERATOR_DECLARATOR).is(TYPE, OPERATOR, OVERLOADABLE_BINARY_OPERATOR, LPARENTHESIS, TYPE, IDENTIFIER, COMMA, TYPE, IDENTIFIER, RPARENTHESIS);
    b.rule(OVERLOADABLE_BINARY_OPERATOR).is(
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
            RIGHT_SHIFT,
            EQ_OP,
            NE_OP,
            SUPERIOR,
            INFERIOR,
            GE_OP,
            LE_OP));
    b.rule(CONVERSION_OPERATOR_DECLARATOR).is(
        b.firstOf(
            IMPLICIT,
            EXPLICIT),
        OPERATOR, TYPE, LPARENTHESIS, TYPE, IDENTIFIER, RPARENTHESIS);
    b.rule(OPERATOR_BODY).is(
        b.firstOf(
            BLOCK,
            SEMICOLON));
    b.rule(CONSTRUCTOR_DECLARATION).is(b.optional(ATTRIBUTES), b.zeroOrMore(CONSTRUCTOR_MODIFIER), CONSTRUCTOR_DECLARATOR, CONSTRUCTOR_BODY);
    b.rule(CONSTRUCTOR_MODIFIER).is(
        b.firstOf(
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            EXTERN,
            UNSAFE));
    b.rule(CONSTRUCTOR_DECLARATOR).is(IDENTIFIER, LPARENTHESIS, b.optional(FORMAL_PARAMETER_LIST), RPARENTHESIS, b.optional(CONSTRUCTOR_INITIALIZER));
    b.rule(CONSTRUCTOR_INITIALIZER).is(
        COLON,
        b.firstOf(
            BASE,
            THIS),
        LPARENTHESIS, b.optional(ARGUMENT_LIST), RPARENTHESIS);
    b.rule(CONSTRUCTOR_BODY).is(
        b.firstOf(
            BLOCK,
            SEMICOLON));
    b.rule(STATIC_CONSTRUCTOR_DECLARATION).is(
        b.optional(ATTRIBUTES),
        STATIC_CONSTRUCTOR_MODIFIERS, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, STATIC_CONSTRUCTOR_BODY);
    b.rule(STATIC_CONSTRUCTOR_MODIFIERS).is(
        b.firstOf(
            b.sequence(b.optional(EXTERN), STATIC, b.nextNot(b.next(EXTERN))),
            b.sequence(STATIC, b.optional(EXTERN))));
    b.rule(STATIC_CONSTRUCTOR_BODY).is(
        b.firstOf(
            BLOCK,
            SEMICOLON));
    b.rule(DESTRUCTOR_DECLARATION).is(
        b.optional(ATTRIBUTES), b.optional(EXTERN),
        TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, DESTRUCTOR_BODY);
    b.rule(DESTRUCTOR_BODY).is(
        b.firstOf(
            BLOCK,
            SEMICOLON));
  }

  private static void structs(LexerfulGrammarBuilder b) {
    b.rule(STRUCT_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(STRUCT_MODIFIER), b.optional(PARTIAL),
        STRUCT, IDENTIFIER,
        b.optional(TYPE_PARAMETER_LIST), b.optional(STRUCT_INTERFACES), b.optional(TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
        STRUCT_BODY,
        b.optional(SEMICOLON));
    b.rule(STRUCT_MODIFIER).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    b.rule(STRUCT_INTERFACES).is(COLON, INTERFACE_TYPE_LIST);
    b.rule(STRUCT_BODY).is(LCURLYBRACE, b.zeroOrMore(STRUCT_MEMBER_DECLARATION), RCURLYBRACE);
    b.rule(STRUCT_MEMBER_DECLARATION).is(
        b.firstOf(
            CONSTANT_DECLARATION,
            FIELD_DECLARATION,
            METHOD_DECLARATION,
            PROPERTY_DECLARATION,
            EVENT_DECLARATION,
            INDEXER_DECLARATION,
            OPERATOR_DECLARATION,
            CONSTRUCTOR_DECLARATION,
            STATIC_CONSTRUCTOR_DECLARATION,
            TYPE_DECLARATION,
            unsafe(FIXED_SIZE_BUFFER_DECLARATION)));
  }

  private static void arrays(LexerfulGrammarBuilder b) {
    b.rule(ARRAY_INITIALIZER).is(
        LCURLYBRACE,
        b.optional(
            b.firstOf(
                b.sequence(VARIABLE_INITIALIZER_LIST, COMMA),
                VARIABLE_INITIALIZER_LIST)),
        RCURLYBRACE);
    b.rule(VARIABLE_INITIALIZER_LIST).is(VARIABLE_INITIALIZER, b.zeroOrMore(COMMA, VARIABLE_INITIALIZER));
  }

  private static void interfaces(LexerfulGrammarBuilder b) {
    b.rule(INTERFACE_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(INTERFACE_MODIFIER), b.optional(PARTIAL),
        INTERFACE, IDENTIFIER,
        b.optional(VARIANT_TYPE_PARAMETER_LIST), b.optional(INTERFACE_BASE), b.optional(TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
        INTERFACE_BODY,
        b.optional(SEMICOLON));
    b.rule(INTERFACE_MODIFIER).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    b.rule(VARIANT_TYPE_PARAMETER_LIST).is(INFERIOR, VARIANT_TYPE_PARAMETER, b.zeroOrMore(COMMA, VARIANT_TYPE_PARAMETER), SUPERIOR);
    b.rule(VARIANT_TYPE_PARAMETER).is(b.optional(ATTRIBUTES), b.optional(VARIANCE_ANNOTATION), TYPE_PARAMETER);
    b.rule(VARIANCE_ANNOTATION).is(
        b.firstOf(
            IN,
            OUT));
    b.rule(INTERFACE_BASE).is(COLON, INTERFACE_TYPE_LIST);
    b.rule(INTERFACE_BODY).is(LCURLYBRACE, b.zeroOrMore(INTERFACE_MEMBER_DECLARATION), RCURLYBRACE);
    b.rule(INTERFACE_MEMBER_DECLARATION).is(
        b.firstOf(
            INTERFACE_METHOD_DECLARATION,
            INTERFACE_PROPERTY_DECLARATION,
            INTERFACE_EVENT_DECLARATION,
            INTERFACE_INDEXER_DECLARATION));
    b.rule(INTERFACE_METHOD_DECLARATION).is(
        b.optional(ATTRIBUTES), b.optional(NEW),
        RETURN_TYPE, IDENTIFIER,
        b.optional(TYPE_PARAMETER_LIST),
        LPARENTHESIS, b.optional(FORMAL_PARAMETER_LIST), RPARENTHESIS, b.optional(TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
        SEMICOLON);
    b.rule(INTERFACE_PROPERTY_DECLARATION).is(
        b.optional(ATTRIBUTES), b.optional(NEW),
        TYPE, IDENTIFIER, LCURLYBRACE, INTERFACE_ACCESSORS, RCURLYBRACE);
    b.rule(INTERFACE_ACCESSORS).is(
        b.optional(ATTRIBUTES),
        b.firstOf(
            b.sequence(GET, SEMICOLON, b.optional(ATTRIBUTES), SET),
            b.sequence(SET, SEMICOLON, b.optional(ATTRIBUTES), GET),
            GET,
            SET),
        SEMICOLON);
    b.rule(INTERFACE_EVENT_DECLARATION).is(
        b.optional(ATTRIBUTES), b.optional(NEW),
        EVENT, TYPE, IDENTIFIER, SEMICOLON);
    b.rule(INTERFACE_INDEXER_DECLARATION).is(
        b.optional(ATTRIBUTES), b.optional(NEW),
        TYPE, THIS, LBRACKET, FORMAL_PARAMETER_LIST, RBRACKET, LCURLYBRACE, INTERFACE_ACCESSORS, RCURLYBRACE);
  }

  private static void enums(LexerfulGrammarBuilder b) {
    b.rule(ENUM_DECLARATION).is(b.optional(ATTRIBUTES), b.zeroOrMore(ENUM_MODIFIER), ENUM, IDENTIFIER, b.optional(ENUM_BASE), ENUM_BODY, b.optional(SEMICOLON));
    b.rule(ENUM_BASE).is(COLON, INTEGRAL_TYPE);
    b.rule(ENUM_BODY).is(
        LCURLYBRACE,
        b.optional(
            b.firstOf(
                b.sequence(ENUM_MEMBER_DECLARATIONS, COMMA),
                ENUM_MEMBER_DECLARATIONS)),
        RCURLYBRACE);
    b.rule(ENUM_MODIFIER).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE));
    b.rule(ENUM_MEMBER_DECLARATIONS).is(ENUM_MEMBER_DECLARATION, b.zeroOrMore(COMMA, ENUM_MEMBER_DECLARATION));
    b.rule(ENUM_MEMBER_DECLARATION).is(b.optional(ATTRIBUTES), IDENTIFIER, b.optional(EQUAL, EXPRESSION));
  }

  private static void delegates(LexerfulGrammarBuilder b) {
    b.rule(DELEGATE_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(DELEGATE_MODIFIER),
        DELEGATE, RETURN_TYPE, IDENTIFIER,
        b.optional(VARIANT_TYPE_PARAMETER_LIST),
        LPARENTHESIS, b.optional(FORMAL_PARAMETER_LIST), RPARENTHESIS,
        b.optional(TYPE_PARAMETER_CONSTRAINTS_CLAUSES),
        SEMICOLON);
    b.rule(DELEGATE_MODIFIER).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
  }

  private static void attributes(LexerfulGrammarBuilder b) {
    b.rule(GLOBAL_ATTRIBUTES).is(b.oneOrMore(GLOBAL_ATTRIBUTE_SECTION));
    b.rule(GLOBAL_ATTRIBUTE_SECTION).is(LBRACKET, GLOBAL_ATTRIBUTE_TARGET_SPECIFIER, ATTRIBUTE_LIST, b.optional(COMMA), RBRACKET);
    b.rule(GLOBAL_ATTRIBUTE_TARGET_SPECIFIER).is(GLOBAL_ATTRIBUTE_TARGET, COLON);
    b.rule(GLOBAL_ATTRIBUTE_TARGET).is(
        b.firstOf(
            "assembly",
            "module"));
    b.rule(ATTRIBUTES).is(b.oneOrMore(ATTRIBUTE_SECTION));
    b.rule(ATTRIBUTE_SECTION).is(LBRACKET, b.optional(ATTRIBUTE_TARGET_SPECIFIER), ATTRIBUTE_LIST, b.optional(COMMA), RBRACKET);
    b.rule(ATTRIBUTE_TARGET_SPECIFIER).is(ATTRIBUTE_TARGET, COLON);
    b.rule(ATTRIBUTE_TARGET).is(
        b.firstOf(
            "field",
            "event",
            "method",
            "param",
            "property",
            RETURN,
            "type"));
    b.rule(ATTRIBUTE_LIST).is(ATTRIBUTE, b.zeroOrMore(COMMA, ATTRIBUTE));
    b.rule(ATTRIBUTE).is(ATTRIBUTE_NAME, b.optional(ATTRIBUTE_ARGUMENTS));
    b.rule(ATTRIBUTE_NAME).is(TYPE_NAME);
    b.rule(ATTRIBUTE_ARGUMENTS).is(
        LPARENTHESIS,
        b.optional(
            b.firstOf(
                NAMED_ARGUMENT,
                POSITIONAL_ARGUMENT),
            b.zeroOrMore(
                COMMA,
                b.firstOf(
                    NAMED_ARGUMENT,
                    POSITIONAL_ARGUMENT))),
        RPARENTHESIS);
    b.rule(POSITIONAL_ARGUMENT).is(b.optional(ARGUMENT_NAME), ATTRIBUTE_ARGUMENT_EXPRESSION);
    b.rule(NAMED_ARGUMENT).is(IDENTIFIER, EQUAL, ATTRIBUTE_ARGUMENT_EXPRESSION);
    b.rule(ATTRIBUTE_ARGUMENT_EXPRESSION).is(EXPRESSION);
  }

  private static void generics(LexerfulGrammarBuilder b) {
    b.rule(TYPE_PARAMETER_LIST).is(INFERIOR, TYPE_PARAMETERS, SUPERIOR);
    b.rule(TYPE_PARAMETERS).is(b.optional(ATTRIBUTES), TYPE_PARAMETER, b.zeroOrMore(COMMA, b.optional(ATTRIBUTES), TYPE_PARAMETER));
    b.rule(TYPE_PARAMETER).is(IDENTIFIER);
    b.rule(TYPE_ARGUMENT_LIST).is(INFERIOR, TYPE_ARGUMENT, b.zeroOrMore(COMMA, TYPE_ARGUMENT), SUPERIOR);
    b.rule(TYPE_ARGUMENT).is(TYPE);
    b.rule(TYPE_PARAMETER_CONSTRAINTS_CLAUSES).is(b.oneOrMore(TYPE_PARAMETER_CONSTRAINTS_CLAUSE));
    b.rule(TYPE_PARAMETER_CONSTRAINTS_CLAUSE).is("where", TYPE_PARAMETER, COLON, TYPE_PARAMETER_CONSTRAINTS);
    b.rule(TYPE_PARAMETER_CONSTRAINTS).is(
        b.firstOf(
            b.sequence(PRIMARY_CONSTRAINT, COMMA, SECONDARY_CONSTRAINTS, COMMA, CONSTRUCTOR_CONSTRAINT),
            b.sequence(
                PRIMARY_CONSTRAINT, COMMA,
                b.firstOf(
                    SECONDARY_CONSTRAINTS,
                    CONSTRUCTOR_CONSTRAINT)),
            b.sequence(SECONDARY_CONSTRAINTS, COMMA, CONSTRUCTOR_CONSTRAINT),
            PRIMARY_CONSTRAINT,
            SECONDARY_CONSTRAINTS,
            CONSTRUCTOR_CONSTRAINT));
    b.rule(PRIMARY_CONSTRAINT).is(
        b.firstOf(
            CLASS_TYPE,
            CLASS,
            STRUCT));
    b.rule(SECONDARY_CONSTRAINTS).is(
        b.firstOf(
            INTERFACE_TYPE,
            TYPE_PARAMETER),
        b.zeroOrMore(
            COMMA,
            b.firstOf(
                INTERFACE_TYPE,
                TYPE_PARAMETER)));
    b.rule(CONSTRUCTOR_CONSTRAINT).is(NEW, LPARENTHESIS, RPARENTHESIS);
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
    b.rule(DESTRUCTOR_DECLARATION).override(
        b.optional(ATTRIBUTES),
        b.zeroOrMore(
            b.firstOf(
                EXTERN,
                UNSAFE)),
        TILDE, IDENTIFIER, LPARENTHESIS, RPARENTHESIS, DESTRUCTOR_BODY);
    // FIXME override!
    b.rule(STATIC_CONSTRUCTOR_MODIFIERS).override(
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
    b.rule(EMBEDDED_STATEMENT).override(
        b.firstOf(
            BLOCK,
            SEMICOLON,
            EXPRESSION_STATEMENT,
            SELECTION_STATEMENT,
            ITERATION_STATEMENT,
            JUMP_STATEMENT,
            TRY_STATEMENT,
            CHECKED_STATEMENT,
            UNCHECKED_STATEMENT,
            LOCK_STATEMENT,
            USING_STATEMENT,
            YIELD_STATEMENT,
            UNSAFE_STATEMENT,
            FIXED_STATEMENT));
    b.rule(UNSAFE_STATEMENT).is(UNSAFE, BLOCK);
    b.rule(POINTER_INDIRECTION_EXPRESSION).is(STAR, UNARY_EXPRESSION);
    b.rule(POINTER_ELEMENT_ACCESS).is(PRIMARY_NO_ARRAY_CREATION_EXPRESSION, LBRACKET, EXPRESSION, RBRACKET);
    b.rule(ADDRESS_OF_EXPRESSION).is(AND, UNARY_EXPRESSION);
    b.rule(SIZE_OF_EXPRESSION).is(SIZEOF, LPARENTHESIS, TYPE, RPARENTHESIS);
    b.rule(FIXED_STATEMENT).is(
        FIXED,
        LPARENTHESIS, POINTER_TYPE, FIXED_POINTER_DECLARATOR, b.zeroOrMore(COMMA, FIXED_POINTER_DECLARATOR), RPARENTHESIS,
        EMBEDDED_STATEMENT);
    b.rule(FIXED_POINTER_DECLARATOR).is(IDENTIFIER, EQUAL, FIXED_POINTER_INITIALIZER);
    b.rule(FIXED_POINTER_INITIALIZER).is(
        b.firstOf(
            b.sequence(AND, VARIABLE_REFERENCE),
            STACKALLOC_INITIALIZER,
            EXPRESSION));
    b.rule(FIXED_SIZE_BUFFER_DECLARATION).is(
        b.optional(ATTRIBUTES), b.zeroOrMore(FIXED_SIZE_BUFFER_MODIFIER),
        FIXED, TYPE, b.oneOrMore(FIXED_SIZE_BUFFER_DECLARATOR), SEMICOLON);
    b.rule(FIXED_SIZE_BUFFER_MODIFIER).is(
        b.firstOf(
            NEW,
            PUBLIC,
            PROTECTED,
            INTERNAL,
            PRIVATE,
            UNSAFE));
    b.rule(FIXED_SIZE_BUFFER_DECLARATOR).is(IDENTIFIER, LBRACKET, EXPRESSION, RBRACKET);
    b.rule(STACKALLOC_INITIALIZER).is(STACKALLOC, TYPE, LBRACKET, EXPRESSION, RBRACKET);
  }

  private static void contextualKeywords(LexerfulGrammarBuilder b) {
    b.rule(ASYNC).is("async");
    b.rule(SET).is("set");
    b.rule(GET).is("get");
    b.rule(PARTIAL).is("partial");
  }

}
