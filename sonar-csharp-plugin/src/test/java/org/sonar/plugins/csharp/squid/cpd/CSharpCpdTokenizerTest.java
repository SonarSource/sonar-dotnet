/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp.squid.cpd;

import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.Tokens;
import org.apache.commons.io.FileUtils;
import org.junit.Test;

import java.io.File;
import java.io.FileNotFoundException;
import java.nio.charset.Charset;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

public class CSharpCpdTokenizerTest {

  private final CSharpCPDTokenizer tokenizer = new CSharpCPDTokenizer(Charset.defaultCharset());

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

  @Test
  public void usingDirectiveIgnored() throws FileNotFoundException {
    SourceCode source = new SourceCode(new SourceCode.FileCodeLoader(readFile("/cpd/usingDirective.cs"), Charset.defaultCharset()
      .displayName()));
    Tokens tokens = new Tokens();
    tokenizer.tokenize(source, tokens);

    assertThat(tokens.size(), is(1));
  }

  private File readFile(String path) throws FileNotFoundException {
    return FileUtils.toFile(getClass().getResource(path));
  }
}
