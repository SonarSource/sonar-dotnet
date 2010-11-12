/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.lexer;

import static com.sonar.sslr.api.GenericTokenType.EOL;
import static com.sonar.sslr.api.GenericTokenType.LITERAL;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.HEXA_DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.OCTAL_DIGIT;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.anyButNot;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.g;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.o2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.one2n;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.opt;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.or;
import static com.sonar.sslr.impl.channel.RegexpChannelBuilder.regexp;
import static com.sonar.csharp.api.CSharpTokenType.*;

import java.util.ArrayList;
import java.util.List;

import org.sonar.channel.Channel;
import org.sonar.channel.ChannelDispatcher;

import com.sonar.csharp.api.CSharpKeyword;
import com.sonar.csharp.api.CSharpPunctuator;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.impl.Lexer;
import com.sonar.sslr.impl.channel.BlackHoleChannel;
import com.sonar.sslr.impl.channel.IdentifierAndKeywordChannel;
import com.sonar.sslr.impl.channel.InlineCommentChannel;
import com.sonar.sslr.impl.channel.MultilineCommentChannel;
import com.sonar.sslr.impl.channel.PunctuatorChannel;

public class CSharpLexer extends Lexer {

//  private static final String EXP = g("[Ee]" + opt("[+-]") + one2n(DIGIT));
//  private static final String BINARY_EXP = g("[Pp]" + opt("[+-]") + one2n(DIGIT));
//  private static final String FLOAT_SUFFIX = or("f", "F", "l", "L");
//  private static final String INT_SUFFIX = g(or("(u|U)?(ll|LL|l|L)", "(ll|LL|l|L|)(u|U)", "(u|U)"));


  @Override
  protected ChannelDispatcher<LexerOutput> getChannelDispatcher() {
    List<Channel> channels = new ArrayList<Channel>();
    channels.add(new InlineCommentChannel("//"));
    channels.add(new MultilineCommentChannel("/*", "*/"));

//    channels.add(regexp(LITERAL, opt("L"), "\"", o2n(or("\\\\.", anyButNot("\""))), "\""));
//    channels.add(regexp(CHARACTER_CONSTANT, opt("L"), "'", one2n(or("\\\\.", anyButNot("'", "\\n"))), "'"));
//
//    channels.add(regexp(DECIMAL_FLOATING_CONSTANT, one2n(DIGIT), "\\.", opt(g(one2n(DIGIT))), opt(EXP), opt(FLOAT_SUFFIX)));
//    channels.add(regexp(DECIMAL_FLOATING_CONSTANT, "\\.", g(one2n(DIGIT)), opt(EXP), opt(FLOAT_SUFFIX)));
//    channels.add(regexp(DECIMAL_FLOATING_CONSTANT, one2n(DIGIT), EXP, opt(FLOAT_SUFFIX)));
//    channels.add(regexp(HEXADECIMAL_FLOATING_CONSTANT, "0[xX]", one2n(HEXA_DIGIT), "\\.", o2n(HEXA_DIGIT), opt(BINARY_EXP),
//        opt(FLOAT_SUFFIX)));
//    channels.add(regexp(HEXADECIMAL_FLOATING_CONSTANT, "0[xX]", one2n(HEXA_DIGIT), BINARY_EXP, opt(FLOAT_SUFFIX)));
//
//    channels.add(regexp(HEXADECIMAL_CONSTANT, "0[xX]", one2n(HEXA_DIGIT), opt(INT_SUFFIX)));
//    channels.add(regexp(DECIMAL_CONSTANT, "[1-9]", o2n(DIGIT), opt(INT_SUFFIX)));
//    channels.add(regexp(OCTAL_CONSTANT, "0", o2n(OCTAL_DIGIT), opt(INT_SUFFIX)));

    channels.add(new IdentifierAndKeywordChannel("[a-zA-Z_][a-zA-Z_0-9]*", true, CSharpKeyword.values()));
    channels.add(new PunctuatorChannel(CSharpPunctuator.values()));
    channels.add(regexp(EOL, "\\r?\\n"));
    channels.add(new BlackHoleChannel("[ \\t]"));
    return new ChannelDispatcher<LexerOutput>(channels);
  }
}
