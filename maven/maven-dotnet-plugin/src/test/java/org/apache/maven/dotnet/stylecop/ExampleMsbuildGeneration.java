/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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
 * Created on Jan 14, 2010
 *
 */
package org.apache.maven.dotnet.stylecop;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;
import org.apache.maven.dotnet.commons.XmlUtils;
import org.apache.maven.dotnet.msbuild.xml.CreateItem;
import org.apache.maven.dotnet.msbuild.xml.ItemGroup;
import org.apache.maven.dotnet.msbuild.xml.ItemOutput;
import org.apache.maven.dotnet.msbuild.xml.Project;
import org.apache.maven.dotnet.msbuild.xml.PropertyGroup;
import org.apache.maven.dotnet.msbuild.xml.StyleCopTask;
import org.apache.maven.dotnet.msbuild.xml.Target;
import org.apache.maven.dotnet.msbuild.xml.UsingTask;
import org.codehaus.plexus.util.StringOutputStream;

/**
 * An example of MSBuild project generation.
 * @author Jose CHILLAN Jan 14, 2010
 */
public class ExampleMsbuildGeneration
{
  private final static Logger log = LogManager.getLogger(ExampleMsbuildGeneration.class);
  
  /**
   * Test the generation of stylecop msbuild files. 
   */
  
  public static void main(String[] args)
  {
    StyleCopGenerator generator = new StyleCopGenerator();
    generator.setOutput(new File("C:/Work/CodeQuality/Temp/Example/target/style-cop-results.xml"));
    generator.setProjectRoot(new File("C:/Work/CodeQuality/Temp/Example"));
    generator.setSettings(new File("C:/Work/CodeQuality/Configuration/StyleCop/default-rules.StyleCop"));
    generator.setVisualSolution(new File("C:/Work/CodeQuality/Temp/Example/Example.sln"));
    generator.setStyleCopRoot(new File("C:/Program Files/Microsoft StyleCop 4.3.2.1"));
    List<File> projects = new ArrayList<File>();
    projects.add(new File("C:/Work/CodeQuality/Temp/Example/Example.Core/Example.Core.csproj"));
    projects.add(new File("C:/Work/CodeQuality/Temp/Example/Example.Application/Example.Application.csproj"));
    generator.setVisualProjects(projects);
    StringOutputStream outputStream = new StringOutputStream();
    generator.generate(outputStream);
    System.out.println("Result:\n" + outputStream );
    try
    {
      FileOutputStream outputFile = new FileOutputStream(new File("C:/Work/CodeQuality/Temp/Example/style-build.xml"));
      generator.generate(outputFile);
      outputFile.close();
      
    }
    catch (IOException e)
    {
      log.debug("Generation error", e);
    }
    
  }
  
  /**
   * Tests the MS Project generation
   */
  public static void generateSampleProject()
  {
    Project project= new Project();
    
    // Properties used 
    PropertyGroup propGroup = new PropertyGroup();
    propGroup.setProjectRoot("C:/temp/myProject");
    propGroup.setStyleCopRoot("C:\\Program Files\\Microsoft StyleCop 4.3.2.1");
    
    // StyleCop  task definition
    UsingTask usingTask = new UsingTask();
    usingTask.setAssemblyFile("$(StyleCopRoot)\\Microsoft.StyleCop.dll");
    usingTask.setTaskName("StyleCopTask");
    
    // StyleCop execution target 
    Target target = new Target();
    target.setName("StyleCop");
    StyleCopTask task = new StyleCopTask();
    task.setFullPath("C:/temp/");
    task.setOutputFile("C:/temp/result.xml");
    task.setSettingsFile("C:/temp/rules.StyleCop");
    task.setSourceFiles("@(SourceAnalysisFiles)");

    // Builds the creation item
    CreateItem createItem = new CreateItem();
    createItem.setInclude("%(Project.RootDir)%(Project.Directory)**\\*.cs");
    ItemOutput output = new ItemOutput();
    output.setTaskParameter("Include");
    output.setItemName("SourceAnalysisFiles");
    createItem.setOutput(output);

    // 
    ItemGroup group = new ItemGroup();
    group.addProject("C:\\Temp\\mySolution\\MyProject.csproj");
    	
    // Populates the task    
    target.setStyleCopTask(task);
    
    // Finishes the project
    project.setUsingTask(usingTask);
    project.setPropertyGroup(propGroup);
    project.setDefaultTargets("StyleCop");
    project.setToolsVersion("3.5");
    project.addItem(group);
    project.addTarget(target);
    StringOutputStream outputStream = new StringOutputStream();
    XmlUtils.marshall(project, outputStream);
    System.out.println("Result:\n" + outputStream );
    
  }
}
