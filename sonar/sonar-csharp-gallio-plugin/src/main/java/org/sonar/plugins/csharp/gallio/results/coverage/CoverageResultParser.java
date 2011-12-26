/*
 * Sonar C# Plugin :: Gallio
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
package org.sonar.plugins.csharp.gallio.results.coverage;

import static org.sonar.plugins.csharp.gallio.helper.StaxHelper.findElementName;

import java.io.File;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamException;

import org.apache.commons.lang.StringUtils;
import org.codehaus.staxmate.SMInputFactory;
import org.codehaus.staxmate.in.SMHierarchicCursor;
import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import com.google.common.collect.Lists;

/**
 * Parses a coverage report using Stax
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE January 26, 2011
 */
public class CoverageResultParser implements BatchExtension {

  /**
   * Generates the logger.
   */
  private static final Logger LOG = LoggerFactory.getLogger(CoverageResultParser.class);

  private SensorContext context;
  private VisualStudioSolution solution;
  private final List<CoverageResultParsingStrategy> parsingStrategies;
  private CoverageResultParsingStrategy currentStrategy;

  /**
   * Constructs a @link{CoverageResultStaxParser}.
   */
  public CoverageResultParser(SensorContext context, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    this.context = context;
    this.solution = microsoftWindowsEnvironment.getCurrentSolution();
    parsingStrategies = new ArrayList<CoverageResultParsingStrategy>();
    parsingStrategies.add(new PartCover23ParsingStrategy());
    parsingStrategies.add(new PartCover22ParsingStrategy());
    parsingStrategies.add(new PartCover4ParsingStrategy());
    parsingStrategies.add(new NCover3ParsingStrategy());
    parsingStrategies.add(new OpenCoverParsingStrategy());
  }

  /**
   * Parses a file
   * 
   * @param file
   *          : the file to parse
   * 
   */
  public List<FileCoverage> parse(final Project sonarProject, final File file) {

    final SMHierarchicCursor rootCursor;
    final SMInputCursor root;
    try {
      SMInputFactory inf = new SMInputFactory(XMLInputFactory.newInstance());
      rootCursor = inf.rootElementCursor(file);
      root = rootCursor.advance();
    } catch (XMLStreamException e) {
      throw new SonarException("Could not parse the result file", e);
    }

    LOG.debug("\nrootCursor is at : {}", findElementName(rootCursor));
    // First define the version
    chooseParsingStrategy(root);

    List<FileCoverage> fileCoverages = currentStrategy.parse(context, solution, sonarProject, root);
    
    // We filter the files that belong to the current project
    // and we summarize them
    List<FileCoverage> result = Lists.newArrayList();
    VisualStudioProject currentVsProject = solution.getProjectFromSonarProject(sonarProject);
    for (FileCoverage fileCoverage : fileCoverages) {
      VisualStudioProject vsProject = solution.getProject(fileCoverage.getFile());
      if (vsProject == null) {
        LOG.debug("Coverage report contains a reference to a cs file outside the solution {}", fileCoverage.getFile());
      } else if (currentVsProject.equals(vsProject)) {
        fileCoverage.summarize();
        result.add(fileCoverage);
      }
    }
    
    return result;
  }
  
  /**
   * This method is necessary due to a modification of the schema between partcover 2.2 and 2.3, for which elements start now with an
   * uppercase letter. Format is a little bit different with partcover4, and NCover use a different format too.
   * 
   * @param root
   *          : root cursor
   */
  private void chooseParsingStrategy(SMInputCursor root) {

    Iterator<CoverageResultParsingStrategy> strategyIterator = parsingStrategies.iterator();
    while (strategyIterator.hasNext()) {
      CoverageResultParsingStrategy strategy = strategyIterator.next();
      if (strategy.isCompatible(root)) {
        this.currentStrategy = strategy;
      }
    }
    if (currentStrategy == null) {
      LOG.warn("XML coverage format unknown, using default strategy");
      this.currentStrategy = parsingStrategies.get(0);
    }
  }
}
