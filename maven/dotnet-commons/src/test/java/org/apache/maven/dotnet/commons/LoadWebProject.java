/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on Feb 23, 2010
 */
package org.apache.maven.dotnet.commons;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStreamReader;
import java.io.LineNumberReader;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.codehaus.plexus.util.FileUtils;
import org.junit.Ignore;

/**
 * A LoadWebProject.
 * 
 * @author Jose CHILLAN Feb 23, 2010
 */
public class LoadWebProject
{

  public static void main(String[] args) throws Exception
  {
    loadWeb();
  }

  public static void loadProjects() throws Exception
  {
    File solutionFile = new File("C:/Work/IWF/DotNetProjects/AllProjects.sln");
     File baseDirectory = solutionFile.getParentFile();
    LineNumberReader reader = new LineNumberReader(new InputStreamReader(new FileInputStream(solutionFile)));
    // This pattern extracts the projects from a Visual Studio solution
    String pattern = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*)\"\\s*,\\s*\"([^\"]*?\\.csproj)\"";
    Pattern projectPattern = Pattern.compile(pattern);
    String line;

    // List<VisualStudioProject> result = new ArrayList<VisualStudioProject>();
    while (((line = reader.readLine()) != null))
    {
      // Looks for project files
      Matcher matcher = projectPattern.matcher(line);
      if (matcher.find())
      {
        String projectName = matcher.group(1);
        String projectPath = matcher.group(2);
        System.out.println("Project : " + projectName + ", file=" + projectPath);
        File projectFile = new File(baseDirectory, projectPath);
        if (!projectFile.exists())
        {
          throw new FileNotFoundException("Could not find the project file: " + projectFile);
        }

      }
    }
  }

  public static void loadWeb() throws Exception
  {
    //File solutionFile = new File("C:/Work/IWF/DotNetProjects/AllProjects.sln");
    File solutionFile = new File("C:/Work/CMS/src/SG.GED.CAR.Execution.sln");

    File baseDirectory = solutionFile.getParentFile();
//    LineNumberReader reader = new LineNumberReader(new InputStreamReader(new FileInputStream(solutionFile)));
    // This pattern extracts the projects from a Visual Studio solution
    String pattern = "(Project.*?^EndProject$)";
    // String pattern =
    // "^\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*)\"\\s*,\\s*\"([^\"]*?)\",\\p{Space}*(\"[^\"]*\"\\p{Space}*$\\p{Space}^)";
    // String pattern = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*)\"\\s*,\\s*\"([^\"]*?)\", \"[^\"]\" *ProjectSection";
    Pattern projectPattern = Pattern.compile(pattern, Pattern.MULTILINE + Pattern.DOTALL);
    String line;

    // List<VisualStudioProject> result = new ArrayList<VisualStudioProject>();
//    StringBuffer buffer = new StringBuffer();
//    while (((line = reader.readLine()) != null))
//    {
//      buffer.append(line);
//      buffer.append("\n");
//    }
    String allSolution = FileUtils.fileRead(solutionFile);
    

    Matcher globalMatcher = projectPattern.matcher(allSolution);
    int idx = 0;
    List<String> projectDeclarations = new ArrayList<String>();

    // Retrieves all the projects
    while (globalMatcher.find())
    {
      String projectDefinition = globalMatcher.group(1);
      projectDeclarations.add(projectDefinition);
    }

    String regularProjectExp = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*)\"\\s*,\\s*\"([^\"]*?\\.csproj)\"";
    String webProjectExp = "\\s*Project\\([^\\)]*\\)\\s*=\\s*\"([^\"]*).*?ProjectSection\\(WebsiteProperties\\).*?"
                           + "Debug\\.AspNetCompiler\\.PhysicalPath\\s*=\\s*\"([^\"]*)";
    Pattern regularPattern = Pattern.compile(regularProjectExp);
    Pattern webPattern = Pattern.compile(webProjectExp, Pattern.MULTILINE + Pattern.DOTALL);

    for (String projectDecl : projectDeclarations)
    {
      Matcher regularMatcher = regularPattern.matcher(projectDecl);
      if (regularMatcher.find())
      {
        String projectName = regularMatcher.group(1);
        String projectPath = regularMatcher.group(2);
        File projectFile = new File(baseDirectory, projectPath);
        System.out.println("Regular project : Name=" + projectName + ", file=" + projectFile);
      }
      else
      {
        // Searches the web project
        Matcher webMatcher = webPattern.matcher(projectDecl);

        if (webMatcher.find())
        {
          String projectName = webMatcher.group(1);
          String projectPath = webMatcher.group(2);
          File projectRoot = new File(baseDirectory, projectPath);
          String targetPath = VisualStudioUtils.extractSolutionProperty("Debug.AspNetCompiler.TargetPath", projectDecl);
          System.out.println("Web project : Name=" + projectName + ", root=" + projectRoot + ", targetPath=" + targetPath);
        }
      }
    }

    // // Looks for project files
    // while (allSolution.length() > 0)
    // {
    // Matcher matcher = projectPattern.matcher(allSolution);
    //      
    // if (matcher.find())
    // {
    // String projectName = matcher.group(1);
    // String projectPath = matcher.group(1);
    // String rest = matcher.group(1);
    // File projectFile = new File(baseDirectory, projectPath);
    // System.out.println("Project : " + projectName + ", file=" + projectFile);
    // System.out.println("Rest : " + rest);
    // System.out.println();
    // if (!projectFile.exists())
    // {
    // System.out.println("Non existing project : " + projectFile);
    // //throw new FileNotFoundException("Could not find the project file: " + projectFile);
    // }
    //
    // }
    //      
    // if (allSolution.indexOf("\n") > 0)
    // {
    // allSolution = allSolution.substring(allSolution.indexOf("\n") + 1);
    // }
    // }
  }
}
