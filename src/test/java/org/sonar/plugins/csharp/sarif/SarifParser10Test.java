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
package org.sonar.plugins.csharp.sarif;

import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.inOrder;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.verifyNoMoreInteractions;

import java.io.File;
import java.io.IOException;

import org.apache.commons.io.FileUtils;
import org.apache.commons.lang.SystemUtils;
import org.junit.Test;
import org.mockito.InOrder;
import org.mockito.Mockito;

public class SarifParser10Test {
  private String getContents(String fileName) throws IOException {
    return FileUtils.readFileToString(new File("src/test/resources/SarifParserTest/" + fileName));
  }

  // VS 2015 Update 3
  @Test
  public void sarif_version_1_0() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(getContents("v1_0.json")).parse(callback);

    if (SystemUtils.IS_OS_WINDOWS) {
      InOrder inOrder = inOrder(callback);
      inOrder.verify(callback).onIssue("S1234", "C:\\Foo.cs", "One issue per line", 1);
      inOrder.verify(callback).onIssue("S1234", "C:\\Bar.cs", "One issue per line", 2);
      verify(callback, Mockito.times(2)).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());
    }
  }
  
  @Test
  public void sarif_version_1_0_no_location() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(getContents("v1_0_no_location.json")).parse(callback);

    if (SystemUtils.IS_OS_WINDOWS) {
      InOrder inOrder = inOrder(callback);
      inOrder.verify(callback).onProjectIssue("S1234", "One issue per line");
      verify(callback, Mockito.times(1)).onProjectIssue(anyString(), anyString());
      verifyNoMoreInteractions(callback);
    }
  }

  @Test
  public void sarif_version_1_0_more_rules() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(getContents("v1_0_another.json")).parse(callback);

    if (SystemUtils.IS_OS_WINDOWS) {
      InOrder inOrder = inOrder(callback);
      inOrder.verify(callback).onIssue("S1186", "C:\\Program.cs",
        "Add a nested comment explaining why this method is empty, throw a \"NotSupportedException\" or complete the implementation.", 26);
      inOrder.verify(callback).onIssue("S1172", "C:\\Program.cs", "Remove this unused method parameter \"args\".", 26);
      inOrder.verify(callback).onIssue("S1118", "C:\\Program.cs", "Add a \"protected\" constructor or the \"static\" keyword to the class declaration.", 9);

      verify(callback, Mockito.times(3)).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());
    }
  }

  @Test
  public void sarif_path_escaping() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(getContents("v1_0_escaping.json")).parse(callback);

    if (SystemUtils.IS_OS_WINDOWS) {
      InOrder inOrder = inOrder(callback);
      inOrder.verify(callback).onIssue("S107", "C:\\git\\Temp Folder SomeRandom!@#$%^&()\\csharp\\ConsoleApplication1\\Program.cs",
        "Method has 3 parameters, which is greater than the 2 authorized.", 52);
      verify(callback, Mockito.times(1)).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());
    }
  }

  @Test
  public void dont_fail_on_empty_report() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser10(getContents("v1_0_empty.json")).parse(callback);
    verify(callback, Mockito.never()).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());
  }

}
