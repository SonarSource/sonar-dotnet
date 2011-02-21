/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.sonar.plugin.dotnet.coverage.stax;

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.descendantElements;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findElementName;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findXMLEvent;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.nextPosition;

import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMFilterFactory;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.SonarPluginException;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ParserResult;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;

import com.google.common.base.Predicate;
import com.google.common.collect.Maps;

/**
 * Parses a coverage report using Stax
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE January 26, 2011
 */
public class CoverageResultStaxParser implements PointParserCallback, BatchExtension{
  /**
   * Generates the logger.
   */
  private final static Logger log = LoggerFactory.getLogger(CoverageResultStaxParser.class);

  private final CSharpFileLocator fileLocator;
  
  private Map<Integer, FileCoverage> sourceFilesById;
  private final Map<String, ProjectCoverage> projectsByAssemblyName;
  private final List<AbstractParsingStrategy> parsingStrategies;
  private AbstractParsingStrategy currentStrategy;
  

  /**
   * Constructs a @link{CoverageResultStaxParser}.
   */
  public CoverageResultStaxParser(CSharpFileLocator fileLocator) {
    this.fileLocator = fileLocator;
    sourceFilesById = new HashMap<Integer, FileCoverage>();
    projectsByAssemblyName = new HashMap<String, ProjectCoverage>();
    parsingStrategies = new ArrayList<AbstractParsingStrategy>();
    parsingStrategies.add(new PartCover23ParsingStrategy());
    parsingStrategies.add(new PartCover22ParsingStrategy());
    parsingStrategies.add(new PartCover4ParsingStrategy());
    parsingStrategies.add(new NCover3ParsingStrategy());
  }

  /**
   * Parses a file
   * 
   * @param file : the file to parse
   * 
   */
  public ParserResult parse(final Project sonarProject, final File file) {

    try {
      SMInputFactory inf = new SMInputFactory(XMLInputFactory.newInstance());
      SMHierarchicCursor rootCursor = inf.rootElementCursor(file);
      SMInputCursor root = rootCursor.advance();

      log.debug("\nrootCursor is at : {}", findElementName(rootCursor));
      // First define the version
      chooseParsingStrategy(root);

      SMInputCursor rootChildCursor = descendantElements(root);

      // Then all the indexed files are extracted
      sourceFilesById = currentStrategy.findFiles(rootChildCursor);

      // filter files according to the exclusion patterns
      sourceFilesById = Maps.filterValues(sourceFilesById, 
          new Predicate<FileCoverage>(){
        @Override
        public boolean apply(FileCoverage input) {
          return fileLocator.locate(sonarProject, input.getFile(), false)!=null;
        }
      }
      );


      // We finally process the coverage details
      fillProjects(rootChildCursor);

      // We summarize the files
      for (FileCoverage fileCoverage : sourceFilesById.values()) {
        fileCoverage.summarize();
      }
      for (ProjectCoverage project : projectsByAssemblyName.values()) {
        project.summarize();
      }
    }catch (XMLStreamException e) {
      throw new SonarPluginException("Could not parse the result file", e);
    }
    List<ProjectCoverage> projects = new ArrayList<ProjectCoverage>(projectsByAssemblyName.values());
    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>(sourceFilesById.values());
    return new ParserResult(projects, sourceFiles);
  }

  /**
   * Processes the details of the coverage
   * 
   * @param rootChildCursor
   *          cursor positioned to get the method elements
   */
  private void fillProjects(SMInputCursor rootChildCursor) {

    // Because of a different structure in PartCover 4, we need to get the assemblies first
    // if the report is from PartCover 4
    currentStrategy.saveAssemblyNamesById(rootChildCursor);

    //Sets the cursor to the tags "Type" for PartCover and "Module" for NCover
    rootChildCursor.setFilter(SMFilterFactory.getElementOnlyFilter(currentStrategy.getModuleTag()));
    do{
      if(findXMLEvent(rootChildCursor) != null){

        currentStrategy.saveId(findAttributeValue(rootChildCursor,
            currentStrategy.getAssemblyReference()));

        String assemblyName = currentStrategy.findAssemblyName(rootChildCursor);
        log.debug("AssemblyName : {}", assemblyName);

        currentStrategy.findPoints(assemblyName, rootChildCursor, this);
      }
    } while (nextPosition(rootChildCursor) != null);
  }

  @Override
  public void createProjects(String assemblyName, SMInputCursor classElements) {
    FileCoverage fileCoverage = null;
    SMInputCursor methodElements = descendantElements(classElements);
    while(nextPosition(methodElements) != null){
      fileCoverage = currentStrategy.parseMethod(methodElements, assemblyName, sourceFilesById);
      if(fileCoverage != null){
        final ProjectCoverage project;
        if( projectsByAssemblyName.containsKey(assemblyName) ){
          project = projectsByAssemblyName.get(assemblyName);
        }
        else{
          project = new ProjectCoverage();
          project.setAssemblyName(assemblyName);
          projectsByAssemblyName.put(assemblyName, project);
        }
        project.addFile(fileCoverage);
      }
    }
  }

  /**
   * This method is necessary due to a modification of the schema between
   * partcover 2.2 and 2.3, for which elements start now with an uppercase
   * letter. Format is a little bit different with partcover4, and NCover use a
   * different format too.
   * 
   * @param root : root cursor
   */
  private void chooseParsingStrategy(SMInputCursor root) {

    Iterator<AbstractParsingStrategy> strategyIterator = parsingStrategies
    .iterator();
    while ( strategyIterator.hasNext() ) {
      AbstractParsingStrategy strategy = (AbstractParsingStrategy) strategyIterator
      .next();
      if (strategy.isCompatible(root)) {
        this.currentStrategy = strategy;
      }
    }
    if (currentStrategy == null) {
      log.warn("XML coverage format unknown, using default strategy");
      this.currentStrategy = parsingStrategies.get(0);
    }
  }

}
