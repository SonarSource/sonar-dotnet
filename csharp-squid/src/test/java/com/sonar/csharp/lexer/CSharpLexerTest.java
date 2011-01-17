/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.lexer;

import static com.sonar.sslr.test.lexer.LexerMatchers.hasComment;
import static com.sonar.sslr.test.lexer.LexerMatchers.hasToken;
import static org.hamcrest.Matchers.is;
import static org.hamcrest.Matchers.not;
import static org.junit.Assert.assertThat;

import java.io.FileNotFoundException;
import java.nio.charset.Charset;

import org.apache.commons.io.FileUtils;
import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.CSharpPunctuator;
import com.sonar.csharp.api.CSharpTokenType;
import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.LexerOutput;

public class CSharpLexerTest {

  private CSharpLexer lexer;

  @Before
  public void init() {
    lexer = new CSharpLexer(Charset.forName("UTF-8"));
  }

  @Test
  public void lexInlineComment() {
    assertThat(lexer.lex("// This is a comment"), hasComment("// This is a comment"));
    assertThat(lexer.lex("// This is a comment // !"), hasComment("// This is a comment // !"));
    assertThat(lexer.lex("int a = 2;  // This is a comment \nint b = 3;"), hasComment("// This is a comment "));
  }

  @Test
  public void lexMultipleLineComment() {
    assertThat(lexer.lex("/* This is a multiple\n line \n comment */"), hasComment("/* This is a multiple\n line \n comment */"));
  }

  @Test
  public void lexIdentifierOrKeyword() {
    assertThat(lexer.lex("abstract"), hasToken(CSharpKeyword.ABSTRACT));
    assertThat(lexer.lex("_myId"), hasToken("_myId", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("myId"), hasToken("myId", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("MyId"), hasToken("MyId", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("myId10"), hasToken("myId10", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("_10Id"), hasToken("_10Id", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("@myId"), hasToken("@myId", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("a"), hasToken("a", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("A"), hasToken("A", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("@abstract"), hasToken("@abstract", GenericTokenType.IDENTIFIER));
  }

  @Test
  @Ignore
  public void lexIdentifierWithUnicodeChars() {
    assertThat(lexer.lex("éléphant"), hasToken("éléphant", GenericTokenType.IDENTIFIER));
    assertThat(lexer.lex("A‿"), hasToken("A‿", GenericTokenType.IDENTIFIER)); // Char of class Pc: U+203F UNDERTIE
    assertThat(lexer.lex("A﹏"), hasToken("A﹏", GenericTokenType.IDENTIFIER)); // Char of class Pc: U+FE4F WAVY LOW LINE
    assertThat(lexer.lex("A؂"), hasToken("A؂", GenericTokenType.IDENTIFIER)); // Char of class Cf: U+0602 ARABIC FOOTNOTE MARKER
                                                                              // (there's a hidden char here, this is no empty string)
    assertThat(lexer.lex("A⃕"), hasToken("A⃕", GenericTokenType.IDENTIFIER)); // Char of class Mn: U+20D5 COMBINING CLOCKWISE ARROW ABOVE
    assertThat(lexer.lex("Aொ"), hasToken("Aொ", GenericTokenType.IDENTIFIER)); // Char of class Mc: U+0BCA TAMIL VOWEL SIGN O
  }

  @Test
  public void lexNoIdentifier() {
    assertThat(lexer.lex("abstract"), not(hasToken(GenericTokenType.IDENTIFIER)));
    assertThat(lexer.lex("10myId"), not(hasToken("10myId", GenericTokenType.IDENTIFIER)));
    assertThat(lexer.lex("#myId"), not(hasToken("#myId", GenericTokenType.IDENTIFIER)));
    assertThat(lexer.lex("-myId"), not(hasToken("-myId", GenericTokenType.IDENTIFIER)));
    assertThat(lexer.lex("$myId"), not(hasToken("$myId", GenericTokenType.IDENTIFIER)));
  }

  @Test
  public void lexOperatorsAndPonctuators() {
    LexerOutput output = lexer.lex("int a = 2;");
    assertThat(output, hasToken("=", CSharpPunctuator.EQUAL));
    assertThat(output, hasToken(";", CSharpPunctuator.SEMICOLON));
  }

  @Test
  public void lexStringLiteral() {
    // regular string literal
    assertThat(lexer.lex("String s =\"\";"), hasToken("\"\"", CSharpTokenType.STRING_LITERAL));
    assertThat(lexer.lex("String path =\"\\temp\";"), hasToken("\"\\temp\"", CSharpTokenType.STRING_LITERAL));
    assertThat(lexer.lex("String s =\"Foo and bar\";"), hasToken("\"Foo and bar\"", CSharpTokenType.STRING_LITERAL));
    assertThat(lexer.lex("String s =\"A string with an escape quote \\\" !\";"),
        hasToken("\"A string with an escape quote \\\" !\"", CSharpTokenType.STRING_LITERAL));
    assertThat(lexer.lex("String s =\"This is an hexadecimal escape sequence: \\1 !\";"),
        hasToken("\"This is an hexadecimal escape sequence: \\1 !\"", CSharpTokenType.STRING_LITERAL));
    assertThat(lexer.lex("String s =\"Foo\" and bar\";"), not(hasToken("\"Foo\" and bar\"", CSharpTokenType.STRING_LITERAL)));
    assertThat(lexer.lex("String s =\"Foo\n and bar\";"), not(hasToken(CSharpTokenType.STRING_LITERAL)));
    // verbatim string literal
    assertThat(lexer.lex("@\"Foo \n and \n bar\""), hasToken("@\"Foo \n and \n bar\"", CSharpTokenType.STRING_LITERAL));
    assertThat(lexer.lex("@\"Software\\nunit.org\\Nunit-Test\\\""),
        hasToken("@\"Software\\nunit.org\\Nunit-Test\\\"", CSharpTokenType.STRING_LITERAL));
  }

  @Test
  public void lexIntegerLiteral() {
    // decimal literals
    assertThat(lexer.lex("123"), hasToken("123", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123U"), hasToken("123U", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123u"), hasToken("123u", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123L"), hasToken("123L", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123l"), hasToken("123l", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123UL"), hasToken("123UL", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123Ul"), hasToken("123Ul", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123uL"), hasToken("123uL", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123ul"), hasToken("123ul", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123LU"), hasToken("123LU", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123Lu"), hasToken("123Lu", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123lU"), hasToken("123lU", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123lu"), hasToken("123lu", CSharpTokenType.INTEGER_DEC_LITERAL));
    assertThat(lexer.lex("123Xu"), not(hasToken("123Xu", CSharpTokenType.INTEGER_DEC_LITERAL)));
    // Hexadecimal literals
    assertThat(lexer.lex("0x1A2B3C"), hasToken("0x1A2B3C", CSharpTokenType.INTEGER_HEX_LITERAL));
    assertThat(lexer.lex("0x1A2B3CuL"), hasToken("0x1A2B3CuL", CSharpTokenType.INTEGER_HEX_LITERAL));
  }

  @Test
  public void lexRealLiteral() {
    assertThat(lexer.lex("12.34"), hasToken("12.34", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34e1"), hasToken("12.34e1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34e+1"), hasToken("12.34e+1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34e-1"), hasToken("12.34e-1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34E1"), hasToken("12.34E1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34E+1"), hasToken("12.34E+1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34E-1"), hasToken("12.34E-1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34F"), hasToken("12.34F", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34f"), hasToken("12.34f", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34D"), hasToken("12.34D", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34d"), hasToken("12.34d", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34M"), hasToken("12.34M", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34m"), hasToken("12.34m", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("12.34e+1F"), hasToken("12.34e+1F", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex(".1234e+1F"), hasToken(".1234e+1F", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("1234e+1"), hasToken("1234e+1", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("1234e+1F"), hasToken("1234e+1F", CSharpTokenType.REAL_LITERAL));
    assertThat(lexer.lex("1234F"), hasToken("1234F", CSharpTokenType.REAL_LITERAL));
  }

  @Test
  public void lexCharacterLiteral() {
    assertThat(lexer.lex("char c ='a';"), hasToken("'a'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\n';"), hasToken("'\\n'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\'';"), hasToken("'\\''", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\\\';"), hasToken("'\\\\'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\xABC';"), hasToken("'\\xABC'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\xABC1';"), hasToken("'\\xABC1'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\xABC12';"), hasToken("'\\xABC12'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\xABC123';"), hasToken("'\\xABC123'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\uA1Z';"), hasToken("'\\uA1Z'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\\UA1Z2E3';"), hasToken("'\\UA1Z2E3'", CSharpTokenType.CHARACTER_LITERAL));
    assertThat(lexer.lex("char c ='\na';"), not(hasToken(CSharpTokenType.CHARACTER_LITERAL)));
  }

  @Test
  public void lexPreprocessingDirective() {
    assertThat(lexer.lex("#region Constants").getPreprocessingTokens().get(0).getValue(), is("#region Constants"));
    assertThat(lexer.lex(" #  region Constants\nint a = '1';").getPreprocessingTokens().get(0).getValue(), is("#  region Constants"));
  }

  @Test
  public void testLexCSharpSourceCode() throws FileNotFoundException {
    LexerOutput output = lexer.lex(FileUtils.toFile(getClass().getResource("/lexer/NUnitFramework.cs")));
    assertThat(output, hasToken("System", GenericTokenType.IDENTIFIER));
    assertThat(output, hasComment("// Copyright 2007, Charlie Poole"));
  }

}
