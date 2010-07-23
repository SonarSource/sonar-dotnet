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
import java.util.Iterator;
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
  private final List<AbstractParsingStrategy> parsingStrategies;
  private AbstractParsingStrategy strategy;
  
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
    parsingStrategies = new ArrayList<AbstractParsingStrategy>();
    parsingStrategies.add(new PartCover23ParsingStrategy());
    parsingStrategies.add(new PartCover22ParsingStrategy());
    parsingStrategies.add(new PartCover4ParsingStrategy());
    parsingStrategies.add(new NCover3ParsingStrategy());
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
    List<Element> methodElements = extractElements(file, strategy.getMethodPath());
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
    NodeList nodes = methodElement.getElementsByTagName(strategy.getPointElement());
    List<Element> elements = convertToList(nodes);
    // First we retrieve the file
    FileCoverage fileCoverage = null;
    // First pass for to retrieve the file
    for (Element pointElement : elements)
    {
      if (pointElement.hasAttribute(strategy.getFileIdPointAttribute()))
      {
        String fileId = pointElement.getAttribute(strategy.getFileIdPointAttribute());
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
    String assemblyName = strategy.findAssemblyName(methodElement);
  
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
      if (!pointElement.hasAttribute(strategy.getStartLinePointAttribute()))
      {
        // We skip the elements with no line
        continue;
      }
      int countVisits = getIntAttribute(pointElement, strategy.getCountVisitsPointAttribute());
      int startLine = getIntAttribute(pointElement,  strategy.getStartLinePointAttribute());
      int endLine = getIntAttribute(pointElement, strategy.getEndLinePointAttribute());
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
    List<Element> elements = extractElements(file, "/*");
    Element root = elements.get(0);
    Iterator<AbstractParsingStrategy> strategyIterator = parsingStrategies.iterator();
    while (strategyIterator.hasNext() && strategy==null) {
	    AbstractParsingStrategy strategy = (AbstractParsingStrategy) strategyIterator.next();
	    if (strategy.isCompatible(root)) {
	    	this.strategy = strategy;
	    }
    }
    if (strategy==null) {
    	log.warn("XML coverage format unknown, using default strategy");
    	this.strategy = parsingStrategies.get(0);
    }
    
  }
  /**
   * Extracts the files from the file
   * 
   * @param the url of the file
   */

  private void extractFiles(URL file)
  {
    List<Element> elements = extractElements(file, strategy.getFilePath());
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
      	log.debug("bad url", e);
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
