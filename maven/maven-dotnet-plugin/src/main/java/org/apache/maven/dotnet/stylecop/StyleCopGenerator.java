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
 * Created on Jan 19, 2010
 */
package org.apache.maven.dotnet.stylecop;

import java.io.File;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.Marshaller;
import javax.xml.namespace.NamespaceContext;
import javax.xml.stream.XMLOutputFactory;
import javax.xml.stream.XMLStreamWriter;

import org.apache.commons.lang.StringUtils;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;
import org.apache.maven.dotnet.msbuild.xml.CreateItem;
import org.apache.maven.dotnet.msbuild.xml.ItemGroup;
import org.apache.maven.dotnet.msbuild.xml.ItemOutput;
import org.apache.maven.dotnet.msbuild.xml.Project;
import org.apache.maven.dotnet.msbuild.xml.PropertyGroup;
import org.apache.maven.dotnet.msbuild.xml.StyleCopTask;
import org.apache.maven.dotnet.msbuild.xml.Target;
import org.apache.maven.dotnet.msbuild.xml.UsingTask;

/**
 * Generates a MSBuild configuration file to run stylecop.
 * 
 * @author Jose CHILLAN Jan 19, 2010
 */
public class StyleCopGenerator
{
  /**
   * STYLE_COP_NAMESPACE
   */
  private static final String STYLE_COP_NAMESPACE = "http://schemas.microsoft.com/developer/msbuild/2003";

  private final static Logger log                 = LogManager.getLogger(StyleCopGenerator.class);

  private File                projectRoot;
  private File                styleCopRoot;
  private File                visualSolution;
  private List<File>          visualProjects;
  private File                output;
  private File                settings;

  /**
   * Constructs a @link{StyleCopGenerator}.
   * 
   * @param styleCopRoot
   * @param settings
   * @param projectRoot
   * @param visualSolution
   * @param visualProjects
   * @param output
   */
  public StyleCopGenerator(File styleCopRoot, File settings, File projectRoot, File visualSolution, List<File> visualProjects, File output)
  {
    super();
    this.styleCopRoot = styleCopRoot;
    this.settings = settings;
    this.projectRoot = projectRoot;
    this.visualSolution = visualSolution;
    this.visualProjects = visualProjects;
    this.output = output;
  }

  /**
   * Constructs a @link{StyleCopGenerator}.
   */
  public StyleCopGenerator()
  {
    this.visualProjects = new ArrayList<File>();
  }

  /**
   * Generates the msbuild configuration for a project.
   * 
   * @param stream
   */
  public void generate(OutputStream stream)
  {
    Project project = new Project();

    // Properties used
    PropertyGroup propGroup = new PropertyGroup();
    propGroup.setProjectRoot(toWindowsPath(projectRoot));
    propGroup.setStyleCopRoot(toWindowsPath(styleCopRoot));

    // StyleCop task definition
    UsingTask usingTask = new UsingTask();
    usingTask.setAssemblyFile("$(StyleCopRoot)\\Microsoft.StyleCop.dll");
    usingTask.setTaskName("StyleCopTask");

    // StyleCop execution target
    Target target = new Target();
    target.setName("CheckStyle");
    StyleCopTask task = new StyleCopTask();
    task.setFullPath(toWindowsPath(visualSolution));
    task.setOutputFile(toWindowsPath(output));
    task.setSettingsFile(toWindowsPath(settings));
    task.setSourceFiles("@(SourceAnalysisFiles)");

    // Builds the creation item
    CreateItem createItem = new CreateItem();
    createItem.setInclude("%(Project.RootDir)%(Project.Directory)**\\*.cs");
    ItemOutput itemOutput = new ItemOutput();
    itemOutput.setTaskParameter("Include");
    itemOutput.setItemName("SourceAnalysisFiles");
    createItem.setOutput(itemOutput);

    // 
    ItemGroup group = new ItemGroup();
    // Adds all the projects files
    for (File visualProject : visualProjects)
    {
      group.addProject(toWindowsPath(visualProject));
    }

    // Populates the task
    target.setItem(createItem);
    target.setStyleCopTask(task);

    // Finishes the project
    project.setUsingTask(usingTask);
    project.setPropertyGroup(propGroup);
    project.setDefaultTargets("CheckStyle");
    project.setToolsVersion("3.5");
    project.addItem(group);
    project.addTarget(target);

    XMLOutputFactory xof = XMLOutputFactory.newInstance();
    StringWriter writer = new StringWriter();
    XMLStreamWriter xtw = null;
    try
    {
      // Gets control of the generated namespaces
      xtw = xof.createXMLStreamWriter(writer);
      xtw.setNamespaceContext(new NamespaceContext() {

        @Override
        public Iterator getPrefixes(String arg0)
        {
          return null;
        }

        @Override
        public String getPrefix(String arg0)
        {
          if (STYLE_COP_NAMESPACE.equals(arg0))
          {
            return "stylecop";
          }
          return null;
        }

        @Override
        public String getNamespaceURI(String arg0)
        {
          return null;
        }
      });
      // Establish a jaxb context
      JAXBContext jc = JAXBContext.newInstance(Project.class);

      // Get a marshaller
      Marshaller m = jc.createMarshaller();

      // Enable formatted xml output
      m.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.TRUE);
      // Marshal to system output: java to xml
      m.marshal(project, xtw);
    }
    catch (Exception e)
    {
      log.debug("Generation error", e);
    }
    String xmlContent = writer.toString();
    
    // Due to a bug of missing feature in JAXB, I could not generate an XML file having the default XML namespace
    // of stylecop (it defines the objects in a namespace that is not the default). 
    // The problem is that MSBuild is non fully XML compliant and requires the stylecop namespace as being explicitely
    // the default one for the file. This is achived by hand-made replacement in hte generated file, hoping for something
    // more robust later.
    String temp = StringUtils.replace(xmlContent, "xmlns=\"\"", "xmlns=\"" + STYLE_COP_NAMESPACE + "\"");
    String result = StringUtils.replace(temp, "stylecop:Project", "Project");
    PrintWriter outputWriter = new PrintWriter(stream);
    outputWriter.print(result);
    outputWriter.flush();
  }

  /**
   * @return
   */
  private String toWindowsPath(File file)
  {
    return file.toString();
  }

  /**
   * Returns the projectRoot.
   * 
   * @return The projectRoot to return.
   */
  public File getProjectRoot()
  {
    return this.projectRoot;
  }

  /**
   * Sets the projectRoot.
   * 
   * @param projectRoot The projectRoot to set.
   */
  public void setProjectRoot(File projectRoot)
  {
    this.projectRoot = projectRoot;
  }

  /**
   * Returns the styleCopRoot.
   * 
   * @return The styleCopRoot to return.
   */
  public File getStyleCopRoot()
  {
    return this.styleCopRoot;
  }

  /**
   * Sets the styleCopRoot.
   * 
   * @param styleCopRoot The styleCopRoot to set.
   */
  public void setStyleCopRoot(File styleCopRoot)
  {
    this.styleCopRoot = styleCopRoot;
  }

  /**
   * Returns the visualSolution.
   * 
   * @return The visualSolution to return.
   */
  public File getVisualSolution()
  {
    return this.visualSolution;
  }

  /**
   * Sets the visualSolution.
   * 
   * @param visualSolution The visualSolution to set.
   */
  public void setVisualSolution(File visualSolution)
  {
    this.visualSolution = visualSolution;
  }

  /**
   * Returns the visualProjects.
   * 
   * @return The visualProjects to return.
   */
  public List<File> getVisualProjects()
  {
    return this.visualProjects;
  }

  /**
   * Adds a visual studio project file.
   * @param visualProject
   */
  public void addVisualProject(File visualProject)
  {
    this.visualProjects.add(visualProject);
  }
  
  /**
   * Sets the visualProjects.
   * 
   * @param visualProjects The visualProjects to set.
   */
  public void setVisualProjects(List<File> visualProjects)
  {
    this.visualProjects = visualProjects;
  }

  /**
   * Returns the output.
   * 
   * @return The output to return.
   */
  public File getOutput()
  {
    return this.output;
  }

  /**
   * Sets the output.
   * 
   * @param output The output to set.
   */
  public void setOutput(File output)
  {
    this.output = output;
  }

  /**
   * Returns the settings.
   * 
   * @return The settings to return.
   */
  public File getSettings()
  {
    return this.settings;
  }

  /**
   * Sets the settings.
   * 
   * @param settings The settings to set.
   */
  public void setSettings(File settings)
  {
    this.settings = settings;
  }

}
