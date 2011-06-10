/*
 * .NET tools :: StyleCop Runner
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
package org.sonar.dotnet.tools.stylecop;

import static org.junit.Assert.assertTrue;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;
import java.io.StringWriter;

import org.junit.Before;
import org.junit.Test;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class MsBuildFileGeneratorTest {

  private MsBuildFileGenerator generator;
  private File outputFolder;
  private VisualStudioProject project;

  @Before
  public void init() {
    VisualStudioSolution solution = mock(VisualStudioSolution.class);
    File solutionDir = mock(File.class);
    when(solutionDir.getAbsolutePath()).thenReturn("SOLUTIONDIR");
    File solutionFile = mock(File.class);
    when(solutionFile.getAbsolutePath()).thenReturn("SOLUTIONFILE.sln");
    when(solution.getSolutionDir()).thenReturn(solutionDir);
    when(solution.getSolutionFile()).thenReturn(solutionFile);

    project = mock(VisualStudioProject.class);
    File projectFile = mock(File.class);
    when(projectFile.getAbsolutePath()).thenReturn("PROJECT.csproj");
    when(project.getProjectFile()).thenReturn(projectFile);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(project));

    File styleCopDir = mock(File.class);
    when(styleCopDir.getAbsolutePath()).thenReturn("StyleCopDir");

    outputFolder = new File("target/MsBuildFileGenerator");
    generator = new MsBuildFileGenerator(solution, TestUtils.getResource("/Runner/Command/SimpleRules.StyleCop"), outputFolder, styleCopDir);
  }

  @Test
  public void testGenerateFile() {
    if ( !outputFolder.exists()) {
      outputFolder.mkdirs();
    }
    generator.generateFile(outputFolder, null);
    File generatedFile = new File(outputFolder, MsBuildFileGenerator.MSBUILD_FILE);
    assertTrue(generatedFile.exists());
    generatedFile.delete();
  }

  @Test
  public void testGenerateContent() throws Exception {
    File reportFile = mock(File.class);
    when(reportFile.getAbsolutePath()).thenReturn("REPORTFILE");
    File styleCopRuleFile = mock(File.class);
    when(styleCopRuleFile.getAbsolutePath()).thenReturn("STYLECOPRULEFILE");

    StringWriter writer = new StringWriter();
    generator.generateContent(writer, styleCopRuleFile, reportFile, Lists.newArrayList(project));
    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/Runner/MSBuild/stylecop-msbuild_for-tests.xml"), writer.toString());
  }

  @Test
  public void testGenerateContentForWebProject() throws Exception {
    when(project.getProjectFile()).thenReturn(null);
    File projectDir = mock(File.class);
    when(projectDir.getAbsolutePath()).thenReturn("PROJECT-PATH");
    when(project.getDirectory()).thenReturn(projectDir);

    File reportFile = mock(File.class);
    when(reportFile.getAbsolutePath()).thenReturn("REPORTFILE");
    File styleCopRuleFile = mock(File.class);
    when(styleCopRuleFile.getAbsolutePath()).thenReturn("STYLECOPRULEFILE");

    StringWriter writer = new StringWriter();
    generator.generateContent(writer, styleCopRuleFile, reportFile, Lists.newArrayList(project));
    TestUtils.assertSimilarXml(TestUtils.getResourceContent("/Runner/MSBuild/stylecop-msbuild_for-tests-web-project.xml"),
        writer.toString());
  }
}
