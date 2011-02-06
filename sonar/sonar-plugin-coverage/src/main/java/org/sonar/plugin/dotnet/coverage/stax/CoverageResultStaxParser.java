/*
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

package org.sonar.plugin.dotnet.coverage.stax;

import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.descendantElements;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findAttributeIntValue;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findElementName;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.findXMLEvent;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.isAStartElement;
import static org.sonar.plugin.dotnet.coverage.stax.StaxHelper.nextPosition;

import java.io.File;
import java.io.IOException;
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
import org.sonar.api.utils.WildcardPattern;
import org.sonar.plugin.dotnet.core.SonarPluginException;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;
import org.sonar.plugin.dotnet.coverage.model.CoveragePoint;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ParserResult;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;

import com.google.common.base.Predicate;
import com.google.common.base.Predicates;
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

  private Map<Integer, FileCoverage> sourceFilesById;
  private final Map<String, ProjectCoverage> projectsByAssemblyName;
  private final List<AbstractParsingStrategy> parsingStrategies;
  private AbstractParsingStrategy currentStrategy;

  /**
   * Constructs a @link{CoverageResultStaxParser}.
   */
  public CoverageResultStaxParser() {
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
                return CSharpFileLocator.INSTANCE.locate(sonarProject, input.getFile(), false)!=null;
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
      fileCoverage = parseMethod(methodElements, assemblyName);
    }
    if(fileCoverage != null){
      ProjectCoverage project = new ProjectCoverage();
      project.setAssemblyName(assemblyName);
      project.addFile(fileCoverage);
      projectsByAssemblyName.put(assemblyName, project);
    }
  }

  /**
   * Parse a method, retrieving all its points
   * 
   * @param method : method to parse
   * @param assemblyName : corresponding assembly name
   */
  private FileCoverage parseMethod(SMInputCursor method, String assemblyName) {

    FileCoverage fileCoverage = null;
    if(isAStartElement(method)){
      String lineCount = findAttributeValue(method, "linecount");
      String temporaryFileId = findAttributeValue(method, "fid");
      boolean areUncoveredLines = false;
      if( temporaryFileId != null ){
        areUncoveredLines = true;
      }
      SMInputCursor pointTag = descendantElements(method);
      List<CoveragePoint> points = new ArrayList<CoveragePoint>();
      int fid = 0;
      int pointCounter = 0;
      while(nextPosition(pointTag) != null){
        pointCounter++;
        if(isAStartElement(pointTag) && (findAttributeValue(pointTag, currentStrategy
            .getFileIdPointAttribute()) != null)){

          int startLine = findAttributeIntValue(pointTag, currentStrategy
              .getStartLinePointAttribute());
          int endLine = findAttributeIntValue(pointTag, currentStrategy
              .getEndLinePointAttribute());

          CoveragePoint point = new CoveragePoint();
          point.setCountVisits(findAttributeIntValue(pointTag, currentStrategy
              .getCountVisitsPointAttribute()));
          point.setStartLine(startLine);
          point.setEndLine(endLine);
          fid = findAttributeIntValue(pointTag, currentStrategy
              .getFileIdPointAttribute());

          points.add(point);
        }
      }
      if( pointCounter == 0 && areUncoveredLines){
        fileCoverage = sourceFilesById.get(Integer.valueOf(temporaryFileId));
        currentStrategy.handleMethodWithoutPoints(lineCount, fileCoverage);
      }
      fileCoverage = sourceFilesById.get(Integer.valueOf(fid));
      fillFileCoverage(assemblyName, fileCoverage, points);
    }
    return fileCoverage;
  }

  private void fillFileCoverage(String assemblyName, FileCoverage fileCoverage,
      List<CoveragePoint> points) {

    if(fileCoverage != null){
      for(CoveragePoint point : points){
        fileCoverage.addPoint(point);
      }
      fileCoverage.setAssemblyName(assemblyName);
      log.debug(" {} points have been added", points.size());
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
