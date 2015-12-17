/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonar.plugins.csharp;

import com.google.common.base.Charsets;
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
    when(fs.encoding()).thenReturn(Charsets.UTF_8);
    CSharpCPDMapping cpd = new CSharpCPDMapping(csharp, fs);

    assertThat(cpd.getLanguage()).isSameAs(csharp);

    SourceCode source = mock(SourceCode.class);
    when(source.getFileName()).thenReturn(new File("src/test/resources/CSharpCPDMappingTest/File.cs").getAbsolutePath());

    Tokens cpdTokens = new Tokens();
    assertThat(cpdTokens.getTokens()).isEmpty();

    cpd.getTokenizer().tokenize(source, cpdTokens);

    assertThat(cpdTokens.getTokens()).hasSize(13);

    assertThat(cpdTokens.getTokens().get(0).getValue()).isEqualTo("namespace");
    assertThat(cpdTokens.getTokens().get(0).getBeginLine()).isEqualTo(5);

    assertThat(cpdTokens.getTokens().get(1).getValue()).isEqualTo("Foo");
    assertThat(cpdTokens.getTokens().get(1).getBeginLine()).isEqualTo(5);

    assertThat(cpdTokens.getTokens().get(2).getValue()).isEqualTo("{");
    assertThat(cpdTokens.getTokens().get(2).getBeginLine()).isEqualTo(6);

    assertThat(cpdTokens.getTokens().get(3).getValue()).isEqualTo("using");
    assertThat(cpdTokens.getTokens().get(3).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(4).getValue()).isEqualTo("(");
    assertThat(cpdTokens.getTokens().get(4).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(5).getValue()).isEqualTo("foo");
    assertThat(cpdTokens.getTokens().get(5).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(6).getValue()).isEqualTo("=");
    assertThat(cpdTokens.getTokens().get(6).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(7).getValue()).isEqualTo("LITERAL");
    assertThat(cpdTokens.getTokens().get(7).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(8).getValue()).isEqualTo(")");
    assertThat(cpdTokens.getTokens().get(8).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(9).getValue()).isEqualTo("{");
    assertThat(cpdTokens.getTokens().get(9).getBeginLine()).isEqualTo(10);
    assertThat(cpdTokens.getTokens().get(10).getValue()).isEqualTo("}");
    assertThat(cpdTokens.getTokens().get(10).getBeginLine()).isEqualTo(10);

    assertThat(cpdTokens.getTokens().get(11).getValue()).isEqualTo("}");
    assertThat(cpdTokens.getTokens().get(11).getBeginLine()).isEqualTo(11);

    assertThat(cpdTokens.getTokens().get(12)).isSameAs(TokenEntry.EOF);
  }

}
