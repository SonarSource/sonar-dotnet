package com.sonar.csharp.lexer.channel;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.channel.Channel;
import org.sonar.channel.CodeReader;

import com.sonar.sslr.api.LexerOutput;

public class UnknownCharacterChannel extends Channel<LexerOutput> {

  private Logger log = LoggerFactory.getLogger(UnknownCharacterChannel.class);

  @Override
  public boolean consume(CodeReader code, LexerOutput lexerOutput) {
    if (code.peek() != -1) {
      log.warn("Unknown: " + (char) code.pop());
      return true;
    }
    return false;
  }

}
