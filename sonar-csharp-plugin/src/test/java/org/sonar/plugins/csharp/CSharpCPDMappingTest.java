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
package org.sonar.plugins.csharp;

import net.sourceforge.pmd.cpd.SourceCode;
import net.sourceforge.pmd.cpd.TokenEntry;
import net.sourceforge.pmd.cpd.Tokens;
import org.junit.Test;
import org.sonar.api.batch.fs.FileSystem;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

public class CSharpCPDMappingTest {

  @Test
  public void test() throws Exception {
    CSharp csharp = mock(CSharp.class);
    FileSystem fs = mock(FileSystem.class);
    when(fs.workDir()).thenReturn(new File("src/test/resources/CSharpCPDMappingTest"));
    CSharpCPDMapping cpd = new CSharpCPDMapping(csharp, fs);

    assertThat(cpd.getLanguage()).isSameAs(csharp);

    SourceCode source = mock(SourceCode.class);
    when(source.getFileName()).thenReturn("C:\\File2.cs");

    Tokens cpdTokens = new Tokens();
    assertThat(cpdTokens.getTokens()).isEmpty();

    cpd.getTokenizer().tokenize(source, cpdTokens);

    assertThat(cpdTokens.getTokens()).hasSize(3);

    assertThat(cpdTokens.getTokens().get(0).getValue()).isEqualTo("bar1");
    assertThat(cpdTokens.getTokens().get(0).getBeginLine()).isEqualTo(3);

    assertThat(cpdTokens.getTokens().get(1).getValue()).isEqualTo("bar2");
    assertThat(cpdTokens.getTokens().get(1).getBeginLine()).isEqualTo(4);

    assertThat(cpdTokens.getTokens().get(2)).isSameAs(TokenEntry.EOF);
  }

}
