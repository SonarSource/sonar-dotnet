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
package org.sonar.plugins.csharp.gallio.results.coverage.model;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Result returned by the parser containing source files and projects coverage
 * 
 * @author Maxime SCHNEIDER-DUFEUTRELLE January 26, 2011
 */
public class ParserResult {

  private static final Logger LOG = LoggerFactory.getLogger(ParserResult.class);
  
  private final List<FileCoverage> sourceFiles;
  private final List<ProjectCoverage> projects;

  public ParserResult(List<ProjectCoverage> projects, List<FileCoverage> sourceFiles) {
    this.projects = projects;
    this.sourceFiles = sourceFiles;
    
    // We summarize the files
    for (FileCoverage file : sourceFiles) {
      file.summarize();
    }
    for (ProjectCoverage project : projects) {
      LOG.debug("Summarize project: {}", project);
      project.summarize();
    }
  }

  public List<FileCoverage> getSourceFiles() {
    return sourceFiles;
  }

  public List<ProjectCoverage> getProjects() {
    return projects;
  }

}
