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
 * Created on May 19, 2009
 */
package org.sonar.plugin.dotnet.srcmon.model;

import java.util.ArrayList;
import java.util.List;

/**
 * A base class for entities having metrics.
 * 
 * @author Jose CHILLAN May 19, 2009
 */
public class AbstractMeterable extends SourceMetric {
  private List<FileMetrics> files;
  private Distribution filesComplexity;
  private Distribution methodsComplexity;

  /**
   * Constructs a @link{NamespaceMetrics}.
   */
  public AbstractMeterable() {
    files = new ArrayList<FileMetrics>();
    this.filesComplexity = new Distribution(
        DistributionClassification.CLASS_COMPLEXITY);
    this.methodsComplexity = new Distribution(
        DistributionClassification.METHOD_COMPLEXITY);
  }

  /**
   * Adds a metrics for a file.
   * 
   * @param file
   *          the file metrics
   */
  public void addFile(FileMetrics file) {
    files.add(file);
    countLines += file.getCountLines();
    countBlankLines += file.getCountBlankLines();
    countStatements += file.getCountStatements();
    commentLines += file.getCommentLines();
    documentationLines += file.getDocumentationLines();
    countClasses += file.getCountClasses();
    countMethods += file.getCountMethods();
    countCalls += file.getCountCalls();
    countMethodStatements += file.getCountMethodStatements();
    complexity += file.getComplexity();
    countAccessors += file.getCountAccessors();

    // Updates the complexity distribution
    int fileComplexity = file.getComplexity();
    filesComplexity.addEntry(fileComplexity);
    List<MethodMetric> methods = file.getMethods();
    for (MethodMetric methodMetric : methods) {
      int methodComplexity = methodMetric.getComplexity();
      methodsComplexity.addEntry(methodComplexity);
    }
  }

  /**
   * Returns the files.
   * 
   * @return The files to return.
   */
  public List<FileMetrics> getFiles() {
    return this.files;
  }

  /**
   * Gets the sonar representation of the class complexity distribution
   * 
   * @return
   */
  public String getClassComplexityDistribution() {
    return filesComplexity.toSonarRepresentation();
  }

  /**
   * Gets the sonar representation of the class complexity distribution
   * 
   * @return
   */
  public String getMethodComplexityDistribution() {
    return methodsComplexity.toSonarRepresentation();
  }

}
