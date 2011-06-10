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

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.Writer;
import java.util.List;

import org.apache.commons.io.IOUtils;
import org.apache.commons.lang.StringEscapeUtils;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioProject;
import org.sonar.plugins.csharp.api.visualstudio.VisualStudioSolution;

import com.google.common.collect.Lists;

/**
 * Class that generates MSBuild
 */
public class MsBuildFileGenerator {

  protected static final String MSBUILD_FILE = "stylecop-msbuild.xml";
  private VisualStudioSolution solution;
  private File styleCopRuleFile;
  private File reportFile;
  private File styleCopFolder;

  public MsBuildFileGenerator(VisualStudioSolution solution, File styleCopRuleFile, File reportFile, File styleCopFolder) {
    this.solution = solution;
    this.styleCopRuleFile = styleCopRuleFile;
    this.reportFile = reportFile;
    this.styleCopFolder = styleCopFolder;
  }

  /**
   * Generates the MSBuild file in the given output folder
   * 
   * @param outputFolder
   *          the output folder
   * @param vsProject
   *          the VS project that should be analysed. May be NULL, in which case all the projects of the solution will be analysed.
   */
  public File generateFile(File outputFolder, VisualStudioProject vsProject) {
    File msBuildFile = new File(outputFolder, MSBUILD_FILE);
    List<VisualStudioProject> vsProjects = solution.getProjects();
    if (vsProject != null) {
      vsProjects = Lists.newArrayList(vsProject);
    }

    FileWriter writer = null;
    try {
      writer = new FileWriter(msBuildFile);
      generateContent(writer, styleCopRuleFile, reportFile, vsProjects);
      writer.flush();
    } catch (IOException e) {
      throw new SonarException("Error while generating the MSBuild file needed to launch StyleCop: " + msBuildFile.getAbsolutePath(), e);
    } finally {
      IOUtils.closeQuietly(writer);
    }

    return msBuildFile;
  }

  protected void generateContent(Writer writer, File styleCopRuleFile, File reportFile, List<VisualStudioProject> vsProjects)
      throws IOException {
    writer.append("<?xml version=\"1.0\" ?>\n");
    writer
        .append("<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" DefaultTargets=\"StyleCopLaunch\" ToolsVersion=\"3.5\">\n");
    writer.append("    <PropertyGroup>\n");
    writer.append("        <ProjectRoot>");
    StringEscapeUtils.escapeXml(writer, solution.getSolutionDir().getAbsolutePath());
    writer.append("</ProjectRoot>\n");
    writer.append("        <StyleCopRoot>");
    StringEscapeUtils.escapeXml(writer, styleCopFolder.getAbsolutePath());
    writer.append("</StyleCopRoot>\n");
    writer.append("    </PropertyGroup>\n");
    writer.append("    <UsingTask TaskName=\"StyleCopTask\" AssemblyFile=\"$(StyleCopRoot)\\Microsoft.StyleCop.dll\"></UsingTask>\n");
    writer.append("    <ItemGroup>\n");
    generateProjectList(writer, vsProjects);
    writer.append("    </ItemGroup>\n");
    writer.append("    <Target Name=\"StyleCopLaunch\">\n");
    writer.append("        <CreateItem Include=\"%(Project.RootDir)%(Project.Directory)**\\*.cs\">\n");
    writer.append("            <Output ItemName=\"SourceAnalysisFiles\" TaskParameter=\"Include\"></Output>\n");
    writer.append("        </CreateItem>\n");
    writer.append("        <StyleCopTask MaxViolationCount=\"-1\" OverrideSettingsFile=\"");
    StringEscapeUtils.escapeXml(writer, styleCopRuleFile.getAbsolutePath());
    writer.append("\"\n            OutputFile=\"");
    StringEscapeUtils.escapeXml(writer, reportFile.getAbsolutePath());
    writer.append("\"\n            TreatErrorsAsWarnings=\"true\" ForceFullAnalysis=\"true\"\n");
    writer.append("            SourceFiles=\"@(SourceAnalysisFiles);@(CSFile)\"\n");
    writer.append("            ProjectFullPath=\"");
    StringEscapeUtils.escapeXml(writer, solution.getSolutionFile().getAbsolutePath());
    writer.append("\"></StyleCopTask>\n");
    writer.append("    </Target>\n");
    writer.append("</Project>");
  }

  private void generateProjectList(Writer writer, List<VisualStudioProject> vsProjects) throws IOException {
    for (VisualStudioProject project : vsProjects) {
      if (project.getProjectFile() == null) {
        // this is a Web project without ".csproj" file, we need to add a wildcard pattern
        writer.append("        <CSFile Include=\"");
        StringEscapeUtils.escapeXml(writer, project.getDirectory().getAbsolutePath() + "\\**\\*.cs");
        writer.append("\"></CSFile>\n");
      } else {
        writer.append("        <Project Include=\"");
        StringEscapeUtils.escapeXml(writer, project.getProjectFile().getAbsolutePath());
        writer.append("\"></Project>\n");
      }
    }
  }

}
