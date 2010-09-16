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
 *
 */
package org.sonar.plugin.dotnet.core.project;

import java.io.File;
import java.io.FilenameFilter;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import net.sourceforge.pmd.cpd.CsLanguage;

import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.SonarException;

/**
 * Utility classes for Visual Studio projects associated to Maven projects.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public final class VisualUtils {
  
  private final static Logger log = LoggerFactory.getLogger(VisualUtils.class);
  
  // maybe way too complicated since one thread and one solution only during the analysis
  private final static Map<MavenProject, VisualStudioSolution> solutionCache 
    = Collections.synchronizedMap(new HashMap<MavenProject, VisualStudioSolution>());
  
  /**
   * Extracts a visual studio solution if the project is a valid solution.
   * 
   * @param project
   *          the maven project from which a solution will be extracted
   * @return a visual studio solution
   * @throws DotNetProjectException
   *           if the project is not a valid .Net project
   */
  public static VisualStudioSolution getSolution(Project project)
      throws DotNetProjectException {
    MavenProject mavenProject = project.getPom();
    final VisualStudioSolution solution;
    if (solutionCache.containsKey(mavenProject)) {
      solution = solutionCache.get(mavenProject);
    } else {
      solution = VisualStudioUtils.getVisualSolution(mavenProject, (String) null);
      solutionCache.put(mavenProject, solution);
    }
    return solution;
  }
  
  /**
   * Search all cs files (with corresponding VS project) included in the VS solution corresponding to the 
   * sonar project object argument.
   * @param project
   * @return
   * @throws DotNetProjectException
   */
  public static Map<File, VisualStudioProject> buildCsFileProjectMap(Project project) {

    VisualStudioSolution solution;
    try {
      solution = VisualUtils.getSolution(project);
    } catch (DotNetProjectException e) {
      throw new SonarException(e);
    }
    List<VisualStudioProject> projects = solution.getProjects();
    FilenameFilter filter = new CsLanguage().getFileFilter();

    Map<File, VisualStudioProject> csFiles = new HashMap<File, VisualStudioProject>();

    for (VisualStudioProject visualStudioProject : projects) {
      if (visualStudioProject.isTest()) {
        log.debug("skipping test project " + visualStudioProject.getName());
      } else {
        Collection<SourceFile> sources = visualStudioProject.getSourceFiles();
        for (SourceFile sourceFile : sources) {
          if (filter.accept(sourceFile.getFile().getParentFile(),
              sourceFile.getName())) {
            csFiles.put(sourceFile.getFile(), visualStudioProject);
          }
        }
      }
    }
    return csFiles;
  }
  
  public static List<File> buildCsFileList(Project project) {
    return new ArrayList<File>(buildCsFileProjectMap(project).keySet());
  }
  
}
