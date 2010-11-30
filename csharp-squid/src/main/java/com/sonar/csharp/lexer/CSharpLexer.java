/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.lexer;

import static com.sonar.sslr.api.GenericTokenType.COMMENT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.ANY_CHAR;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.HEXA_DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.anyButNot;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.g;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.o2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.one2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.opt;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.or;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.regexp;

import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.List;

import org.sonar.channel.Channel;
import org.sonar.channel.ChannelDispatcher;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.CSharpPunctuator;
import com.sonar.csharp.api.CSharpTokenType;
import com.sonar.csharp.lexer.channel.UnknownCharacterChannel;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.impl.Lexer;
import com.sonar.sslr.impl.channel.BlackHoleChannel;
import com.sonar.sslr.impl.channel.IdentifierAndKeywordChannel;
import com.sonar.sslr.impl.channel.PunctuatorChannel;

/**
 * Lexer for the C# language.
 */
public class CSharpLexer extends Lexer {

  private static final String INT_SUFFIX = "(((U|u)(L|l)?)|((L|l)(u|U)?))";
  private static final String REAL_SUFFIX = "(F|f|D|d|M|m)";
  private static final String EXP = g("[Ee]" + opt("[+-]") + one2n(DIGIT));

  private static final String LETTER_CHAR = g("\\p{Lu}|\\p{Ll}|\\p{Lt}|\\p{Lm}|\\p{Lo}|\\p{Nl}");
  private static final String COMBINING_CHAR = g("\\p{Mn}|\\p{Mc}");
  private static final String DECIMAL_DIGIT_CHAR = g("\\p{Nd}");
  private static final String CONNECTING_CHAR = g("\\p{Pc}");
  private static final String FORMATTING_CHAR = g("\\p{Cf}");

  /**
   * ${@inheritDoc}
   */
  public CSharpLexer() {
    super();
  }

  /**
   * ${@inheritDoc}
   */
  public CSharpLexer(Charset defaultCharset) {
    super(defaultCharset);
  }

  @Override
  protected ChannelDispatcher<LexerOutput> getChannelDispatcher() {
    @SuppressWarnings("rawtypes")
    List<Channel> channels = new ArrayList<Channel>();
    // Comments
    channels.add(regexp(COMMENT, "//", o2n("[^\\n\\r]")));
    channels.add(regexp(COMMENT, "/\\*", ANY_CHAR + "*?", "\\*/"));
    // Literals : Strings
    channels.add(regexp(CSharpTokenType.STRING_LITERAL, "\"", o2n(or("\\\\.", anyButNot("\"", "\\n", "\\r"))), "\""));
    channels.add(regexp(CSharpTokenType.STRING_LITERAL, "@\"", o2n(or("\\\\.", anyButNot("\""))), "\""));
    // Literals : Character
    channels.add(regexp(CSharpTokenType.CHARACTER_LITERAL, "'", one2n(or("\\\\.", anyButNot("'", "\\n", "\\r"))), "'"));
    // Literals : Reals
    channels.add(regexp(CSharpTokenType.REAL_LITERAL, o2n(DIGIT), "\\.", one2n(DIGIT), opt(EXP), opt(REAL_SUFFIX)));
    channels.add(regexp(CSharpTokenType.REAL_LITERAL, one2n(DIGIT), EXP, opt(REAL_SUFFIX)));
    channels.add(regexp(CSharpTokenType.REAL_LITERAL, one2n(DIGIT), REAL_SUFFIX));
    // Literals : Integers
    channels.add(regexp(CSharpTokenType.INTEGER_HEX_LITERAL, "0[xX]", one2n(HEXA_DIGIT), opt(INT_SUFFIX)));
    channels.add(regexp(CSharpTokenType.INTEGER_DEC_LITERAL, one2n(DIGIT), opt(INT_SUFFIX)));
    // Identifiers, keywords, punctuators and operators
    channels.add(new IdentifierAndKeywordChannel(g(opt("@"), or(LETTER_CHAR, "_"),
        o2n(or(LETTER_CHAR, DECIMAL_DIGIT_CHAR, CONNECTING_CHAR, COMBINING_CHAR, FORMATTING_CHAR))), true, CSharpKeyword.values()));
    channels.add(new PunctuatorChannel(CSharpPunctuator.values()));
    // Preprocessor directives
    //channels.add(regexp(CSharpTokenType.PREPROCESSOR, "#[^\\r\\n]*"));
    channels.add(new BlackHoleChannel("#[^\\r\\n]*"));
    // Others
    channels.add(new BlackHoleChannel("[ \\t\\r\\n]"));
    // TODO : remove at the end
    channels.add(new UnknownCharacterChannel());
    return new ChannelDispatcher<LexerOutput>(channels);
  }

}
