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

/*
 * Created on Apr 23, 2009
 *
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;

/**
 * A base class for unit test mojo for .Net.
 * 
 * @author Jose CHILLAN Mar 25, 2010
 */
public abstract class AbstractUnitTestMojo extends AbstractDotNetMojo {

  /**
   * @param solution
   * @return
   * @throws MojoFailureException
   */
  protected List<File> extractTestAssemblies(VisualStudioSolution solution)
      throws MojoFailureException {

    List<VisualStudioProject> projects = solution.getTestProjects();
    List<File> testAssemblies = new ArrayList<File>();

    for (VisualStudioProject visualProject : projects) {
      File generatedAssembly = visualProject.getArtifact(buildConfigurations);
      if (generatedAssembly.exists()) {
        testAssemblies.add(generatedAssembly);
      } else {
        getLog().warn("Skipping missing test assembly " + generatedAssembly);
      }
    }

    return testAssemblies;
  }

  @Override
  protected boolean checkExecutionAllowed() throws MojoExecutionException {
    Log log = getLog();
    String skipTest = System.getProperty("maven.test.skip");

    if ("TRUE".equalsIgnoreCase(skipTest)) {
      log.info("Skipping Test Execution");
      return false;
    }
    return super.checkExecutionAllowed();
  }

}
