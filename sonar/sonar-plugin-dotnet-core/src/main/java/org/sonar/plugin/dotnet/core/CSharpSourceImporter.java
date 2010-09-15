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
 */
package org.sonar.plugin.dotnet.core;

import java.io.File;
import java.util.Collection;
import java.util.List;

import org.apache.commons.io.FileUtils;
import org.apache.maven.dotnet.commons.GeneratedCodeFilter;
import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.AbstractSourceImporter;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.project.VisualUtils;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CSharpFileLocator;

import static org.sonar.plugin.dotnet.core.Constant.*;

/**
 * Collects the CSharp source files for Sonar.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class CSharpSourceImporter extends AbstractSourceImporter {

  private final static Logger log = LoggerFactory
      .getLogger(CSharpSourceImporter.class);

  /**
   * Constructs the collector.
   */
  public CSharpSourceImporter() {
    super(CSharp.INSTANCE);
  }

  /**
   * @param project
   * @param context
   */
  @Override
  public void analyse(Project project, SensorContext context) {
    VisualStudioSolution solution;
    try {
      solution = VisualUtils.getSolution(project);
    } catch (DotNetProjectException e1) {
      return;
    }
    List<VisualStudioProject> projects = solution.getProjects();

    // We load the content of all the projects
    for (VisualStudioProject visualStudioProject : projects) {
      parseVisualProject(visualStudioProject, context, project);
    }
  }

  /**
   * Retrieves and stores the files contained in a visual studio project.
   * 
   * @param visualStudioProject
   * @param context
   * @param project
   */
  private void parseVisualProject(VisualStudioProject visualStudioProject,
      SensorContext context, Project project) {
    boolean unitTest = visualStudioProject.isTest();
    boolean excludeGeneratedCode = project.getConfiguration().getBoolean(
        SONAR_EXCLUDE_GEN_CODE_KEY, true);
    Collection<SourceFile> sourceFiles = visualStudioProject.getSourceFiles();
    for (SourceFile sourceFile : sourceFiles) {
      try {
        File sourcePath = sourceFile.getFile();
        if (excludeGeneratedCode
            && GeneratedCodeFilter.INSTANCE.isGenerated(sourcePath.getName())) {
          // we will not include the generated code
          // in the sonar database
          log.info("Ignoring generated cs file " + sourcePath);
          continue;
        }
        CSharpFile resource = CSharpFileLocator.INSTANCE.locate(project, sourcePath, unitTest);
        if (resource == null) {
        	continue;
        }
        
        // Windows may sometime generate endian-recognition characters that are
        // not
        // supported by the Sonar GUI, so we remove them
        String content = FileUtils.readFileToString(sourcePath, "UTF-8");
        if (content.startsWith("\uFEFF") || content.startsWith("\uFFFE")) {
          content = content.substring(1);
        }
        context.saveSource(resource, content);
      } catch (Exception e) {
        log.debug("Could not import file " + sourceFile, e);
      }
    }
  }
}
