/*
 * Copyright (C) 2010 SonarSource SA
 * All rights reserved
 * mailto:contact AT sonarsource DOT com
 */
package com.sonar.csharp.plugin.cpd;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import java.io.File;
import java.io.FileNotFoundException;
import java.nio.charset.Charset;

import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.Tokens;

import org.apache.commons.io.FileUtils;
import org.junit.Test;

public class CSharpCpdTokenizerTest {

  private CSharpCPDTokenizer tokenizer = new CSharpCPDTokenizer(true, Charset.defaultCharset());

  @Test
  public void testTokenize() throws FileNotFoundException {
    SourceCode source = new SourceCode(
        new SourceCode.FileCodeLoader(readFile("/cpd/simpleFile.cs"), Charset.defaultCharset().displayName()));
    Tokens tokens = new Tokens();
    tokenizer.tokenize(source, tokens);

    assertThat(tokens.size(), is(18));
  }

  @Test
  public void testExclusionOfComments() throws FileNotFoundException {
    SourceCode source = new SourceCode(new SourceCode.FileCodeLoader(readFile("/cpd/only-comments.cs"), Charset.defaultCharset()
        .displayName()));
    Tokens tokens = new Tokens();
    tokenizer.tokenize(source, tokens);

    assertThat(tokens.size(), is(1));
  }

  private File readFile(String path) throws FileNotFoundException {
    return FileUtils.toFile(getClass().getResource(path));
  }
}
