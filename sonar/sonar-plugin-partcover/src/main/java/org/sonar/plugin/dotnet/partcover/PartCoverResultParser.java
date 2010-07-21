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
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.partcover;

import java.io.File;
import java.io.IOException;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import javax.xml.xpath.XPathFactory;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.core.SonarPluginException;
import org.sonar.plugin.dotnet.partcover.model.ClassCoverage;
import org.sonar.plugin.dotnet.partcover.model.CoveragePoint;
import org.sonar.plugin.dotnet.partcover.model.FileCoverage;
import org.sonar.plugin.dotnet.partcover.model.ProjectCoverage;
import org.w3c.dom.Element;
import org.w3c.dom.NodeList;

/**
 * A parser for the PartCover XML result file. It supports both version 2.2 and 2.3 of
 * part cover reports that have some differences with elements first letter's case.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class PartCoverResultParser extends AbstractXmlParser
{
  /**
   * Generates the logger.
   */
  private final static Logger log = LoggerFactory.getLogger(PartCoverResultParser.class);
  private final Map<Integer, FileCoverage>   sourceFiles;
  private final Map<String, ProjectCoverage> projects;
  private final List<ClassCoverage>          classes;
  private ReportXPath                        pathDefinitions = PART_COVER_2_3_XPATH;

  /**
   * Internal definition of pathes.
   * 
   * @author Jose CHILLAN Apr 6, 2010
   */
  private static class ReportXPath
  {
    private final String filePath;
    private final String methodPath;

    /**
     * Constructs a @link{ReportXPath}.
     * 
     * @param filePath
     * @param methodPath
     */
    public ReportXPath(String filePath, String methodPath)
    {
      super();
      this.filePath = filePath;
      this.methodPath = methodPath;
    }

    /**
     * Returns the filePath.
     * 
     * @return The filePath to return.
     */
    public String getFilePath()
    {
      return this.filePath;
    }

    /**
     * Returns the methodPath.
     * 
     * @return The methodPath to return.
     */
    public String getMethodPath()
    {
      return this.methodPath;
    }

  }

  /**
   * The XPaths to user with PartCover 2.2 and below.
   */
  public final static ReportXPath PART_COVER_2_2_XPATH = new ReportXPath("//file", "//type/method");

  /**
   * The XPaths to user with PartCover 2.3.
   */
  public final static ReportXPath PART_COVER_2_3_XPATH = new ReportXPath("//File", "//Type/Method");

  /**
   * Constructs a @link{PartCoverResultParser}.
   */
  public PartCoverResultParser()
  {
    sourceFiles = new HashMap<Integer, FileCoverage>();
    classes = new ArrayList<ClassCoverage>();
    projects = new HashMap<String, ProjectCoverage>();
    factory = XPathFactory.newInstance();
    xpath = factory.newXPath();
  }

  /**
   * Parses a file given its URL.
   * 
   * @param url
   */
  public void parse(URL url)
  {
    try
    {
      // First define shte version
      defineVersion(url);
      
      // First, all the indexed files are extracted
      extractFiles(url);

      // Then we process the coverage details
      processDetail(url);
      // We summarize the files
      for (FileCoverage fileCoverage : sourceFiles.values())
      {
        fileCoverage.summarize();
      }
      for (ProjectCoverage project : projects.values())
      {
        project.summarize();
      }
    }
    catch (Exception e)
    {
      throw new SonarPluginException("Could not parse the result file", e);
    }
  }

  /**
   * Processes the details of the coverage.
   * 
   * @param file the coverage file report
   */
  private void processDetail(URL file)
  {
    // We take the coverage in each method
    List<Element> methodElements = extractElements(file, pathDefinitions.getMethodPath());
    for (Element methodElement : methodElements)
    {
      processMethod(methodElement);
    }
  }

  /**
   * Processes a method
   * 
   * @param methodElement
   */
  private void processMethod(Element methodElement)
  {
    NodeList nodes = methodElement.getElementsByTagName("pt");
    List<Element> elements = convertToList(nodes);
    // First we retrieve the file
    FileCoverage fileCoverage = null;
    // First pass for to retrieve the file
    for (Element pointElement : elements)
    {
      if (pointElement.hasAttribute("fid"))
      {
        String fileId = pointElement.getAttribute("fid");
        Integer id = Integer.valueOf(fileId);
        fileCoverage = this.sourceFiles.get(id);
      }
      if (fileCoverage != null)
      {
        // The file is found : we skip the remaining
        break;
      }
    }

    if (fileCoverage == null)
    {
      // No file associated (this should never occur for a consistent result)
      return;
    }

    // We extract the assembly
    Element typeElement = (Element) methodElement.getParentNode();
    String assemblyName = typeElement.getAttribute("asm");
    ProjectCoverage project = projects.get(assemblyName);
    if (project == null)
    {
      project = new ProjectCoverage();
      project.setAssemblyName(assemblyName);
      projects.put(assemblyName, project);
    }

    // We define the assembly name on the file, and add it in the project
    if (StringUtils.isBlank(fileCoverage.getAssemblyName()))
    {
      fileCoverage.setAssemblyName(assemblyName);
      project.addFile(fileCoverage);
    }

    // Second pass to populate the file
    for (Element pointElement : elements)
    {
      if (!pointElement.hasAttribute("sl"))
      {
        // We skip the elements with no line
        continue;
      }
      int countVisits = getIntAttribute(pointElement, "visit");
      int startLine = getIntAttribute(pointElement, "sl");
      int endLine = getIntAttribute(pointElement, "el");
      if (endLine == 0)
      {
        endLine = startLine;
      }
      CoveragePoint point = new CoveragePoint();
      point.setCountVisits(countVisits);
      point.setStartLine(startLine);
      point.setEndLine(endLine);

      // We add the coverage to the file
      if (fileCoverage != null)
      {
        fileCoverage.addPoint(point);
      }
    }
  }

  /**
   * This method is necessary due to a silly modification of the schema between partcover 2.2 and 2.3, for which
   * elements start now with an uppercase letter.
   * @param file
   */
  private void defineVersion(URL file)
  {
    List<Element> elements = extractElements(file, "//PartCoverReport");
    if (elements.size() == 0)
    {
      log.warn("Could not extract the PartCover version : using version 2.2 as default");
      return;
    }
    Element element = elements.get(0);
    String version = element.getAttribute("ver");
    // Evaluates the part cover version
    if (version.startsWith("2.2"))
    {
      log.debug("Using PartCover 2.2 report format");
      pathDefinitions = PART_COVER_2_2_XPATH;
    }
    else
    {
      log.debug("Using PartCover 2.3 report format");
    }
  }
  /**
   * Extracts the files from the file
   * 
   * @param the url of the file
   */

  private void extractFiles(URL file)
  {
    List<Element> elements = extractElements(file, pathDefinitions.getFilePath());
    for (Element fileElement : elements)
    {
      String idStr = fileElement.getAttribute("id");
      Integer id = Integer.valueOf(idStr);
      String filePath = fileElement.getAttribute("url");
      File sourceFile;
      try
      {
        sourceFile = new File(filePath).getCanonicalFile();
        FileCoverage fileCoverage = new FileCoverage(sourceFile);
        sourceFiles.put(id, fileCoverage);
      }
      catch (IOException e)
      {
        // We just skip the file
      }
    }
  }

  /**
   * Returns the classes.
   * 
   * @return The classes to return.
   */
  public List<ClassCoverage> getClasses()
  {
    return this.classes;
  }

  /**
   * Gets the files coverage.
   * 
   * @return
   */
  public List<FileCoverage> getFiles()
  {
    return new ArrayList<FileCoverage>(sourceFiles.values());
  }

  /**
   * Gets the coverage for the projects.
   * 
   * @return the project coverage
   */
  public List<ProjectCoverage> getProjects()
  {
    return new ArrayList<ProjectCoverage>(projects.values());
  }
}
