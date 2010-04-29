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
 * Created on Apr 7, 2009
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.GregorianCalendar;
import java.util.List;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;
import javax.xml.bind.PropertyException;
import javax.xml.datatype.DatatypeConfigurationException;
import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;

import org.apache.maven.dotnet.metrics.xml.Command;
import org.apache.maven.dotnet.metrics.xml.Configuration;
import org.apache.maven.dotnet.metrics.xml.Export;
import org.apache.maven.dotnet.metrics.xml.SourceSubdirectoryList;
import org.apache.maven.plugin.logging.Log;


/**
 * Generates the command file for SourceMonitor to be applied to a C# project or solution.
 * 
 * @author Jose CHILLAN Apr 7, 2009
 */
public class SrcMonCommandGenerator
{
  private Log          logger;

  private File         workDirectory;
  /**
   * Path of the source monitor executable.
   */
  private String       sourceMonitorPath;
  private String       projectFile;
  private String       checkPointName;
  private String       sourcePath;
  private List<String> excludedDirectories;
  private List<String> excludedExtensions;
  private String       generatedFile;

  public SrcMonCommandGenerator()
  {
    this.excludedDirectories = new ArrayList<String>();
    this.excludedExtensions = new ArrayList<String>();
  }

  /**
   * Launches the report generation.
   * 
   * @throws Exception
   */
  public void launch() throws Exception
  {
    logInfo("Launching Source Monitor on the project");

    File commandFile = generateCommandFile();

    String[] cmdArray = new String[] { sourceMonitorPath, "/C", commandFile.getAbsolutePath()
    };
    logDebug("Executing command: " + Arrays.toString(cmdArray));
    Process process = Runtime.getRuntime().exec(cmdArray, null, workDirectory);

    // We wait for the execution
    process.waitFor();
  }

  /**
   * @return
   * @throws JAXBException
   * @throws PropertyException
   * @throws FileNotFoundException
   */
  protected File generateCommandFile() throws JAXBException, PropertyException, FileNotFoundException
  {
    Configuration configuration = new Configuration();
    configuration.setLog(true);
    Command command = new Command();
    command.setProjectFile(projectFile);
    command.setProjectLanguage("C#");
    StringBuilder extensions = new StringBuilder("*.cs");
    if ((excludedExtensions != null) && (!excludedDirectories.isEmpty()))
    {
      extensions.append("|");
      boolean isFirst = true;
      for (String exclusion : excludedExtensions)
      {
        if (isFirst)
        {
          isFirst = false;
        }
        else
        {
          extensions.append(",");
        }
        extensions.append(exclusion.trim());
      }
    }
    command.setFileExtensions(extensions.toString());
    command.setSourceDirectory(sourcePath);
    command.setCheckPointName(checkPointName);
    command.setIgnoreHeaderFooters(false);
    command.setIncludeSubdirectories(true);
    try
    {
      DatatypeFactory factory = DatatypeFactory.newInstance();
      XMLGregorianCalendar calendar = factory.newXMLGregorianCalendar(new GregorianCalendar());
      command.setCheckPointDate(calendar.toXMLFormat().substring(0, 19));
    }
    catch (DatatypeConfigurationException e)
    {
    }

    logDebug("Code Metrics report :");
    logDebug(" - OutputType Project File : " + projectFile);
    logDebug(" - Source Directory        : " + sourcePath);
    logDebug(" - Source Monitor path     : " + sourceMonitorPath);
    logDebug(" - Generated report        : " + generatedFile);

    SourceSubdirectoryList sourceSubdirectoryList = new SourceSubdirectoryList();
    sourceSubdirectoryList.setExcludeSubdirectories(true);
    //.replace('/', '\\')
    
    String[] excludedArray = new String[excludedDirectories.size()];
    // We build the list of excluded directory
    for (int idxDirectory = 0; idxDirectory < excludedArray.length; idxDirectory++)
    {
      String directory = excludedDirectories.get(idxDirectory);
      excludedArray[idxDirectory] = directory.replace('/', '\\');
    }
    
    sourceSubdirectoryList.setSourceSubdirectory(new String[]{"obj\\debug", "obj\\release"});
    sourceSubdirectoryList.setSourceSubTree(excludedArray);
    command.setSubdirectoryList(sourceSubdirectoryList);
    Export export = new Export();
    export.setFile(generatedFile);
    export.setType("2");
    command.setExport(export);
    configuration.setCommands(Collections.singletonList(command));
    // Establish a JAXB context
    Class<? extends Object> serializedType = Configuration.class;
    JAXBContext jc = JAXBContext.newInstance(serializedType);

    // Get a marshaller
    Marshaller marshaller = jc.createMarshaller();

    // Enable formatted xml output
    marshaller.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, Boolean.TRUE);

    // Defines additional properties
    // marshaller.setProperty("com.sun.xml.bind.indentString", "  ");
    // Marshal to system output: java to xml
    if (!workDirectory.exists())
    {
      // We create the directory if necessary
      workDirectory.mkdir();
    }

    File commandFile = new File(workDirectory, "source-monitor-command.xml");
    OutputStream stream = new FileOutputStream(commandFile);
    marshaller.marshal(configuration, stream);
    logDebug("Source Monitor command file generated : " + commandFile);
    return commandFile;
  }

  /**
   * Returns the workDirectory.
   * 
   * @return The workDirectory to return.
   */
  public File getWorkDirectory()
  {
    return this.workDirectory;
  }

  /**
   * Sets the workDirectory.
   * 
   * @param workDirectory The workDirectory to set.
   */
  public void setWorkDirectory(File workDirectory)
  {
    this.workDirectory = workDirectory;
  }

  /**
   * Returns the sourceMonitorPath.
   * 
   * @return The sourceMonitorPath to return.
   */
  public String getSourceMonitorPath()
  {
    return this.sourceMonitorPath;
  }

  /**
   * Sets the sourceMonitorPath.
   * 
   * @param sourceMonitorPath The sourceMonitorPath to set.
   */
  public void setSourceMonitorPath(String sourceMonitorPath)
  {
    this.sourceMonitorPath = sourceMonitorPath;
  }

  /**
   * Returns the checkPointName.
   * 
   * @return The checkPointName to return.
   */
  public String getCheckPointName()
  {
    return this.checkPointName;
  }

  /**
   * Sets the checkPointName.
   * 
   * @param checkPointName The checkPointName to set.
   */
  public void setCheckPointName(String checkPointName)
  {
    this.checkPointName = checkPointName;
  }

  /**
   * Returns the sourcePath.
   * 
   * @return The sourcePath to return.
   */
  public String getSourcePath()
  {
    return this.sourcePath;
  }

  /**
   * Sets the sourcePath.
   * 
   * @param sourcePath The sourcePath to set.
   */
  public void setSourcePath(String sourcePath)
  {
    this.sourcePath = sourcePath;
  }

  /**
   * Returns the excludedDirectories.
   * 
   * @return The excludedDirectories to return.
   */
  public List<String> getExcludedDirectories()
  {
    return this.excludedDirectories;
  }

  /**
   * Sets the excludedDirectories.
   * 
   * @param excludedDirectories The excludedDirectories to set.
   */
  public void setExcludedDirectories(List<String> excludedDirectories)
  {
    this.excludedDirectories = excludedDirectories;
  }

  /**
   * Add a new directory to exclude (and all its subdirectories)
   * @param directory
   */
  public void addExcludedDirectory(String directory)
  {
    excludedDirectories.add(directory);
  }

  /**
   * Adds a new extension to exclude.
   * @param extension
   */
  public void addExcludedExtension(String extension)
  {
    excludedExtensions.add(extension);
  }

  /**
   * Returns the generatedFile.
   * 
   * @return The generatedFile to return.
   */
  public String getGeneratedFile()
  {
    return this.generatedFile;
  }

  /**
   * Sets the generatedFile.
   * 
   * @param generatedFile The generatedFile to set.
   */
  public void setGeneratedFile(String generatedFile)
  {
    this.generatedFile = generatedFile;
  }

  /**
   * Returns the projectFile.
   * 
   * @return The projectFile to return.
   */
  public String getProjectFile()
  {
    return this.projectFile;
  }

  /**
   * Sets the projectFile.
   * 
   * @param projectFile The projectFile to set.
   */
  public void setProjectFile(String projectFile)
  {
    this.projectFile = projectFile;
  }

  /**
   * Sets the logger.
   * 
   * @param logger The logger to set.
   */
  public void setLogger(Log logger)
  {
    this.logger = logger;
  }

  private void logDebug(String message)
  {
    if (logger != null)
    {
      logger.debug(message);
    }
  }

  private void logInfo(String message)
  {
    if (logger != null)
    {
      logger.info(message);
    }
  }

  public List<String> getExcludedExtensions()
  {
    return this.excludedExtensions;
  }

  public void setExcludedExtensions(List<String> excludedExtensions)
  {
    this.excludedExtensions = excludedExtensions;
  }
}
