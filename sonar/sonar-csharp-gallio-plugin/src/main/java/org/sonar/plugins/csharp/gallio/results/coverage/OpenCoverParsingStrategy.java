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
import java.util.Collection;
import java.util.List;
import java.util.Map;

import javax.xml.stream.XMLStreamException;

import org.codehaus.staxmate.in.SMInputCursor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.SonarException;
import org.sonar.plugins.csharp.gallio.results.coverage.model.CoveragePoint;
import org.sonar.plugins.csharp.gallio.results.coverage.model.FileCoverage;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ParserResult;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ProjectCoverage;

import com.google.common.collect.Lists;
import com.google.common.collect.Maps;

public class OpenCoverParsingStrategy implements CoverageResultParsingStrategy {

  private static final Logger LOG = LoggerFactory.getLogger(OpenCoverParsingStrategy.class);
  
  private Map<Integer, File> fileRegistry = Maps.newHashMap();
  
  private Map<File, FileCoverage> fileCoverageRegistry = Maps.newHashMap(); 
  
  private Map<String, ProjectCoverage> projectsByAssemblyName = Maps.newHashMap();

  private ProjectCoverage currentProject = null;
  
  public boolean isCompatible(SMInputCursor rootCursor) {
    return "CoverageSession".equals(findElementName(rootCursor));
  }

  public ParserResult parse(SensorContext ctx, Project sonarProject, SMInputCursor cursor) {

    try {
      cursor = cursor.childElementCursor().advance().childElementCursor();
      while (cursor.getNext() != null) {
        SMInputCursor moduleChildrenCursor = cursor.childElementCursor();
        while (moduleChildrenCursor.getNext() != null) {
          if ("ModuleName".equals(moduleChildrenCursor.getLocalName())) {
            String assemblyName = moduleChildrenCursor.collectDescendantText();
            currentProject = projectsByAssemblyName.get(assemblyName);
            if (currentProject==null) {
              currentProject = new ProjectCoverage();
              currentProject.setAssemblyName(assemblyName);
              projectsByAssemblyName.put(assemblyName, currentProject);
            }
          } else if ("Files".equals(moduleChildrenCursor.getQName().getLocalPart())) {
            fileRegistry = Maps.newHashMap();
            SMInputCursor fileCursor = moduleChildrenCursor.childElementCursor();
            while (fileCursor.getNext() != null) { 
              int uid = Integer.valueOf(fileCursor.getAttrValue("uid"));
              File sourceFile = new File(fileCursor.getAttrValue("fullPath"));
              fileRegistry.put(uid, sourceFile);
            }
          } else if ("Classes".equals(moduleChildrenCursor.getQName().getLocalPart())) {
            parseClassesBloc(moduleChildrenCursor);
          }
        }
      }
    } catch (XMLStreamException e) {
      throw new SonarException(e);
    }
    
    List<ProjectCoverage> projects = new ArrayList<ProjectCoverage>(projectsByAssemblyName.values());
    List<FileCoverage> sourceFiles = new ArrayList<FileCoverage>(fileCoverageRegistry.values());
    return new ParserResult(projects, sourceFiles);
  }
  
  private void parseClassesBloc(SMInputCursor cursor) throws XMLStreamException {
    SMInputCursor classCursor 
      = cursor.childElementCursor(); 
    while (classCursor.getNext() != null) {
      SMInputCursor methodCursor 
        = classCursor.childElementCursor("Methods").advance().childElementCursor(); 
      while (methodCursor.getNext() != null) {
        parseMethodBloc(methodCursor);
      }
    }
  }
  
  private void parseMethodBloc(SMInputCursor cursor) throws XMLStreamException {
    SMInputCursor methodCursor = cursor.childElementCursor();
    FileCoverage fileCoverage = null;
    while (methodCursor.getNext() != null) {
      if ("FileRef".equals(methodCursor.getLocalName())) {
        int fileId = Integer.valueOf(methodCursor.getAttrValue("uid"));
        File sourceFile = fileRegistry.get(fileId);
        fileCoverage = fileCoverageRegistry.get(sourceFile);
        if (fileCoverage==null) {
          fileCoverage = new FileCoverage(sourceFile);
          fileCoverageRegistry.put(sourceFile, fileCoverage);
        }
        currentProject.addFile(fileCoverage);
      } else if ("SequencePoints".equals(methodCursor.getLocalName())) {
        Collection<CoveragePoint> points = parseSequencePointsBloc(methodCursor);
        if (fileCoverage==null) {
          LOG.info("Coverage point not associated to any source file");
        } else {
          for (CoveragePoint point : points) {
            fileCoverage.addPoint(point);
          }
        }
      }
    }
  }
  
  private Collection<CoveragePoint> parseSequencePointsBloc(SMInputCursor cursor) throws XMLStreamException {
    SMInputCursor pointCursor = cursor.childElementCursor();
    List<CoveragePoint> result = Lists.newArrayList();
    while (pointCursor.getNext() != null) {
      CoveragePoint point = new CoveragePoint();
      point.setCountVisits(Integer.valueOf(pointCursor.getAttrValue("vc")));
      point.setStartLine(Integer.valueOf(pointCursor.getAttrValue("sl")));
      point.setEndLine(Integer.valueOf(pointCursor.getAttrValue("el")));
      result.add(point);
    }
    return result;
  }

}
