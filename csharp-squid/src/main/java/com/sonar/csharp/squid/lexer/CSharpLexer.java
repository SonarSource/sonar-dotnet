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
package com.sonar.csharp.squid.lexer;

import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpKeyword;
import com.sonar.csharp.squid.api.CSharpPunctuator;
import com.sonar.csharp.squid.api.CSharpTokenType;
import com.sonar.csharp.squid.lexer.preprocessors.StandardPreprocessorLinePreprocessor;
import com.sonar.sslr.api.Preprocessor;
import com.sonar.sslr.impl.Lexer;
import com.sonar.sslr.impl.channel.BlackHoleChannel;
import com.sonar.sslr.impl.channel.BomCharacterChannel;
import com.sonar.sslr.impl.channel.IdentifierAndKeywordChannel;
import com.sonar.sslr.impl.channel.PunctuatorChannel;
import com.sonar.sslr.impl.channel.UnknownCharacterChannel;

import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.ANY_CHAR;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.HEXA_DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.anyButNot;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.commentRegexp;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.g;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.o2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.one2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.opt;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.or;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.regexp;

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

  public static Lexer create(CSharpConfiguration conf, Preprocessor... preprocessors) {
    Lexer.Builder builder = Lexer.builder()
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
        .withChannel(new BomCharacterChannel())
        .withChannel(new UnknownCharacterChannel());

    if (preprocessors.length > 0) {
      for (Preprocessor preprocessor : preprocessors) {
        builder.withPreprocessor(preprocessor);
      }
    } else {
      builder.withPreprocessor(new StandardPreprocessorLinePreprocessor());
    }

    return builder.build();
  }

}
