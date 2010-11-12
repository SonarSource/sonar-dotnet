/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */

package com.sonar.csharp.lexer;

import static com.sonar.sslr.test.lexer.LexerMatchers.hasComment;
import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import java.io.FileNotFoundException;

import org.apache.commons.io.FileUtils;
import org.junit.Before;
import org.junit.Test;

import com.sonar.sslr.api.GenericTokenType;
import com.sonar.sslr.api.LexerOutput;
import com.sonar.sslr.api.Token;
import com.sonar.sslr.api.TokenType;


public class CSharpLexerTest {

  private CSharpLexer lexer;
  
  @Before
  public void init() {
    lexer = new CSharpLexer();
  }
  


  @Test
  public void testLexNaturalSourceCode() throws FileNotFoundException {
    LexerOutput output = lexer.lex(FileUtils.toFile(getClass().getResource("/csharpExample.cs")));



  }
  
}
