/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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

import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.sonar.api.resources.Project;

/**
 * Utility classes for Visual Studio projects associated to Maven projects.
 * @author Jose CHILLAN May 14, 2009
 */
public final class VisualUtils
{
  /**
   * Extracts a visual studio solution if the project is a valid solution.
   * @param project the maven project from which a solution will be extracted
   * @return a visual studio solution
   * @throws DotNetProjectException if the project is not a valid .Net project
   */
  public static VisualStudioSolution getSolution(Project project) throws DotNetProjectException
  {
    MavenProject mavenProject = project.getPom();
    return VisualStudioUtils.getVisualSolution(mavenProject, (String) null);
  }
}
