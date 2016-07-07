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

import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;

import org.apache.commons.io.FileUtils;
import org.junit.Test;
import org.mockito.InOrder;
import org.mockito.Mockito;
import org.sonar.plugins.csharp.sarif.SarifParser01And04;
import org.sonar.plugins.csharp.sarif.SarifParserCallback;

import static org.mockito.Mockito.inOrder;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;

public class SarifParser01And04Test {
  private String getContents(String fileName) throws IOException {
    return FileUtils.readFileToString(new File("src/test/resources/SarifParserTest/" + fileName), StandardCharsets.UTF_8);
  }

  @Test
  public void should_not_fail_ony_empty_report() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getContents("v0_1_empty_issues.json")).parse(callback);
    new SarifParser01And04(getContents("v0_1_empty_no_issues.json")).parse(callback);
    new SarifParser01And04(getContents("v0_4_empty_no_results.json")).parse(callback);
    new SarifParser01And04(getContents("v0_4_empty_no_runLogs.json")).parse(callback);
    new SarifParser01And04(getContents("v0_4_empty_results.json")).parse(callback);
    new SarifParser01And04(getContents("v0_4_empty_runLogs.json")).parse(callback);
    verify(callback, Mockito.never()).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());
  }

  // VS 2015 Update 1
  @Test
  public void sarif_version_0_1() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getContents("v0_1.json")).parse(callback);

    InOrder inOrder = inOrder(callback);

    inOrder.verify(callback).onIssue("S1172", "C:\\Foo.cs", "Remove this unused method parameter \"args\".", 43);
    inOrder.verify(callback).onIssue("CA1000", "C:\\Bar.cs", "There is just a full message.", 2);
    verify(callback, Mockito.times(2)).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());

    inOrder.verify(callback).onProjectIssue("AssemblyLevelRule", "This is an assembly level Roslyn issue with no location.");
    inOrder.verify(callback).onProjectIssue("NoAnalysisTargetsLocation", "No analysis targets, report at assembly level.");
    verify(callback, Mockito.times(2)).onProjectIssue(Mockito.anyString(), Mockito.anyString());
  }

  // VS 2015 Update 2
  @Test
  public void sarif_version_0_4() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getContents("v0_4.json")).parse(callback);

    InOrder inOrder = inOrder(callback);

    inOrder.verify(callback).onIssue("S125", "C:\\Foo`1.cs", "Remove this commented out code.", 58);
    verify(callback, Mockito.only()).onIssue(Mockito.anyString(), Mockito.anyString(), Mockito.anyString(), Mockito.anyInt());

    verify(callback, Mockito.never()).onProjectIssue(Mockito.anyString(), Mockito.anyString());
  }

}
