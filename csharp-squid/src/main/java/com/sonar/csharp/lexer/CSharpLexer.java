/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.lexer;

import static com.sonar.sslr.api.GenericTokenType.EOL;
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
import com.sonar.csharp.api.CSharpPreprocessingKeyword;
import com.sonar.csharp.api.CSharpPunctuator;
import com.sonar.csharp.api.CSharpTokenType;
import com.sonar.csharp.lexer.channel.UnknownCharacterChannel;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.impl.Lexer;
import com.sonar.sslr.impl.channel.BlackHoleChannel;
import com.sonar.sslr.impl.channel.IdentifierAndKeywordChannel;
import com.sonar.sslr.impl.channel.InlineCommentChannel;
import com.sonar.sslr.impl.channel.MultilineCommentChannel;
import com.sonar.sslr.impl.channel.PunctuatorChannel;

/**
 * Lexer for the C# language.
 */
public class CSharpLexer extends Lexer {

  private static final String INT_SUFFIX = "(((U|u)(L|l)?)|((L|l)(u|U)?))";
  private static final String REAL_SUFFIX = "(F|f|D|d|M|m)";
  private static final String EXP = g("[Ee]" + opt("[+-]") + one2n(DIGIT));
  
  private static final String ESCAPE_SEQ_SIMPLE = "(\\\\[0abfnrtv'\"\\])";
  private static final String ESCAPE_SEQ_HEXA = g("\\\\x", HEXA_DIGIT, opt(HEXA_DIGIT), opt(HEXA_DIGIT), opt(HEXA_DIGIT));
  private static final String ESCAPE_SEQ_UNICODE = g("\\\\[uU]", HEXA_DIGIT, "3", opt(g(HEXA_DIGIT, "3")));

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
    List<Channel> channels = new ArrayList<Channel>();
    channels.add(new InlineCommentChannel("//"));
    channels.add(new MultilineCommentChannel("/*", "*/"));
    // Literals : Strings
    channels.add(regexp(CSharpTokenType.STRING_LITERAL, "\"[^\"\\\\\\r\\n]*\""));
    channels.add(regexp(CSharpTokenType.STRING_LITERAL, "@\"[^\"]*\""));
    // Literals : Character
    channels.add(regexp(CSharpTokenType.CHARACTER_LITERAL, "'[^'\\\\\\r\\n]*'"));
//    channels.add(regexp(CSharpTokenType.CHARACTER_LITERAL, "'", ESCAPE_SEQ_SIMPLE,"'"));
//    channels.add(regexp(CSharpTokenType.CHARACTER_LITERAL, "'", ESCAPE_SEQ_HEXA,"'"));
//    channels.add(regexp(CSharpTokenType.CHARACTER_LITERAL, "'", ESCAPE_SEQ_UNICODE,"'"));
    // Literals : Reals
    channels.add(regexp(CSharpTokenType.REAL_LITERAL, o2n(DIGIT), "\\.", one2n(DIGIT), opt(EXP), opt(REAL_SUFFIX)));
    channels.add(regexp(CSharpTokenType.REAL_LITERAL, one2n(DIGIT), EXP, opt(REAL_SUFFIX)));
    channels.add(regexp(CSharpTokenType.REAL_LITERAL, one2n(DIGIT), REAL_SUFFIX));
    // Literals : Integers
    channels.add(regexp(CSharpTokenType.INTEGER_HEX_LITERAL, "0[xX]", one2n(HEXA_DIGIT), opt(INT_SUFFIX)));
    channels.add(regexp(CSharpTokenType.INTEGER_DEC_LITERAL, one2n(DIGIT), opt(INT_SUFFIX))); // TODO: see with Freddy why it's better to have: "[1-9]", o2n(DIGIT)
    // Identifiers, keywords, punctuators and operators
    channels.add(new IdentifierAndKeywordChannel("@?[a-zA-Z_][a-zA-Z0-9]*", true, CSharpKeyword.values())); // combining/connecting/formatting character ???
    channels.add(new PunctuatorChannel(CSharpPunctuator.values()));
    // Preprocessor directives
    channels.add(regexp(CSharpTokenType.PREPROCESSOR, "#[^\\r\\n]*"));
    // Others
    channels.add(regexp(EOL, "\\r?\\n"));
    channels.add(new BlackHoleChannel("[ \\t]"));
    // TODO : remove at the end
    channels.add(new UnknownCharacterChannel());
    return new ChannelDispatcher<LexerOutput>(channels);
  }

  // private static final String EXP = g("[Ee]" + opt("[+-]") + one2n(DIGIT));
  // private static final String BINARY_EXP = g("[Pp]" + opt("[+-]") + one2n(DIGIT));
  // private static final String FLOAT_SUFFIX = or("f", "F", "l", "L");
  // private static final String INT_SUFFIX = g(or("(u|U)?(ll|LL|l|L)", "(ll|LL|l|L|)(u|U)", "(u|U)"));

  // channels.add(regexp(LITERAL, opt("L"), "\"", o2n(or("\\\\.", anyButNot("\""))), "\""));
  // channels.add(regexp(CHARACTER_CONSTANT, opt("L"), "'", one2n(or("\\\\.", anyButNot("'", "\\n"))), "'"));
  //
  // channels.add(regexp(DECIMAL_FLOATING_CONSTANT, one2n(DIGIT), "\\.", opt(g(one2n(DIGIT))), opt(EXP), opt(FLOAT_SUFFIX)));
  // channels.add(regexp(DECIMAL_FLOATING_CONSTANT, "\\.", g(one2n(DIGIT)), opt(EXP), opt(FLOAT_SUFFIX)));
  // channels.add(regexp(DECIMAL_FLOATING_CONSTANT, one2n(DIGIT), EXP, opt(FLOAT_SUFFIX)));
  // channels.add(regexp(HEXADECIMAL_FLOATING_CONSTANT, "0[xX]", one2n(HEXA_DIGIT), "\\.", o2n(HEXA_DIGIT), opt(BINARY_EXP),
  // opt(FLOAT_SUFFIX)));
  // channels.add(regexp(HEXADECIMAL_FLOATING_CONSTANT, "0[xX]", one2n(HEXA_DIGIT), BINARY_EXP, opt(FLOAT_SUFFIX)));
  //
  // channels.add(regexp(HEXADECIMAL_CONSTANT, "0[xX]", one2n(HEXA_DIGIT), opt(INT_SUFFIX)));
  // channels.add(regexp(DECIMAL_CONSTANT, "[1-9]", o2n(DIGIT), opt(INT_SUFFIX)));
  // channels.add(regexp(OCTAL_CONSTANT, "0", o2n(OCTAL_DIGIT), opt(INT_SUFFIX)));

}
