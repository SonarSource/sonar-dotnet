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

package org.sonar.plugin.dotnet.core;

import java.util.List;

import org.apache.maven.dotnet.commons.project.BinaryReference;
import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.SonarIndex;
import org.sonar.api.design.Dependency;
import org.sonar.api.resources.Library;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.project.VisualUtils;

public class BinaryDependenciesSensor implements Sensor {

  private final static Logger log = LoggerFactory.getLogger(BinaryDependenciesSensor.class);
  
  private final SonarIndex index;
  
  public BinaryDependenciesSensor(SonarIndex index) {
    this.index = index;
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    
    try {
      VisualStudioSolution solution = VisualUtils.getSolution(project);
      List<VisualStudioProject> projects = solution.getProjects();
      for (VisualStudioProject vsProject : projects) {
        // TODO find a way to get dependencies associated to assemblies
        // CLRAssembly assembly = new CLRAssembly(vsProject);
        // Resource<?> savedAssembly = context.getResource(assembly);
        List<BinaryReference> binaryReferences = vsProject
            .getBinaryReferences();
        for (BinaryReference binaryReference : binaryReferences) {
          Resource<?> lib = 
            new Library(binaryReference.getAssemblyName(), binaryReference.getVersion());

          Resource<?> savedLib = context.getResource(lib);
          if (savedLib == null) {
            context.saveResource(lib);
            savedLib = context.getResource(lib);
          } 
          Dependency dependency = new Dependency(index.getProject(), savedLib);
          dependency.setUsage(binaryReference.getScope());
          dependency.setWeight(1);
          context.saveDependency(dependency);
        }
      }

    } catch (DotNetProjectException e) {
      log.error("Error during binary dependency analysis", e);
    }

  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    String packaging = project.getPackaging();
    return VisualStudioUtils.SOLUTION_PACKAGINGS.contains(packaging);
  }

}
