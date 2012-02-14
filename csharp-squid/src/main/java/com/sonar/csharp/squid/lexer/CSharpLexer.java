/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.squid.lexer;

import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.*;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.csharp.squid.lexer.preprocessors.StandardPreprocessorLinePreprocessor;
import com.sonar.sslr.impl.Lexer;
import com.sonar.sslr.impl.channel.BlackHoleChannel;
import com.sonar.sslr.impl.channel.IdentifierAndKeywordChannel;
import com.sonar.sslr.impl.channel.PunctuatorChannel;
import com.sonar.sslr.impl.channel.UnknownCharacterChannel;

/**
 * Lexer for the C# language.
 */
public final class CSharpLexer {

  private static final String INT_SUFFIX = "(((U|u)(L|l)?)|((L|l)(u|U)?))";
  private static final String REAL_SUFFIX = "(F|f|D|d|M|m)";
  private static final String EXP = g("[Ee]" + opt("[+-]") + one2n(DIGIT));

  private static final String LETTER_CHAR = g("\\p{Lu}|\\p{Ll}|\\p{Lt}|\\p{Lm}|\\p{Lo}|\\p{Nl}");
  private static final String COMBINING_CHAR = g("\\p{Mn}|\\p{Mc}");
  private static final String DECIMAL_DIGIT_CHAR = g("\\p{Nd}");
  private static final String CONNECTING_CHAR = g("\\p{Pc}");
  private static final String FORMATTING_CHAR = g("\\p{Cf}");

  private CSharpLexer() {
  }

  public static Lexer create() {
    return create(new CSharpConfiguration());
  }

  public static Lexer create(CSharpConfiguration conf) {
    return Lexer.builder()
        .withCharset(conf.getCharset())

        .withFailIfNoChannelToConsumeOneCharacter(true)

        // Comments
        .withChannel(commentRegexp("//", o2n("[^\\n\\r]")))
        .withChannel(commentRegexp("/\\*", ANY_CHAR + "*?", "\\*/"))
        // Literals : Strings
        .withChannel(regexp(CSharpTokenType.STRING_LITERAL, "\"", o2n(or("\\\\.", anyButNot("\"", "\\n", "\\r"))), "\""))
        .withChannel(regexp(CSharpTokenType.STRING_LITERAL, "@\"", o2n(or("\"\"", anyButNot("\""))), "\""))
        // Literals : Character
        .withChannel(regexp(CSharpTokenType.CHARACTER_LITERAL, "'", one2n(or("\\\\.", anyButNot("'", "\\n", "\\r"))), "'"))
        // Literals : Reals
        .withChannel(regexp(CSharpTokenType.REAL_LITERAL, o2n(DIGIT), "\\.", one2n(DIGIT), opt(EXP), opt(REAL_SUFFIX)))
        .withChannel(regexp(CSharpTokenType.REAL_LITERAL, one2n(DIGIT), EXP, opt(REAL_SUFFIX)))
        .withChannel(regexp(CSharpTokenType.REAL_LITERAL, one2n(DIGIT), REAL_SUFFIX))
        // Literals : Integers
        .withChannel(regexp(CSharpTokenType.INTEGER_HEX_LITERAL, "0[xX]", one2n(HEXA_DIGIT), opt(INT_SUFFIX)))
        .withChannel(regexp(CSharpTokenType.INTEGER_DEC_LITERAL, one2n(DIGIT), opt(INT_SUFFIX)))
        // Identifiers, keywords, punctuators and operators
        .withChannel(new IdentifierAndKeywordChannel(g(opt("@"), or(LETTER_CHAR, "_"),
            o2n(or(LETTER_CHAR, DECIMAL_DIGIT_CHAR, CONNECTING_CHAR, COMBINING_CHAR, FORMATTING_CHAR))), true, CSharpKeyword.values()))
        .withChannel(new PunctuatorChannel(CSharpPunctuator.values()))
        // Preprocessor directives
        .withChannel(regexp(CSharpTokenType.PREPROCESSOR, "#[^\\r\\n]*"))
        // Others
        .withChannel(new BlackHoleChannel("[\\s]"))
        .withChannel(new UnknownCharacterChannel(true))

        .withPreprocessor(new StandardPreprocessorLinePreprocessor())

        .build();
  }

}
