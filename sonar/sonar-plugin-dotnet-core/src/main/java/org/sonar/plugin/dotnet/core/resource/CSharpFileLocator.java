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

package org.sonar.plugin.dotnet.core.resource;

import java.io.File;
import java.util.Collections;
import java.util.Map;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.BatchExtension;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.project.VisualUtils;

/**
 * Singleton helper used to locate
 * source files.
 * @author Alexandre Victoor
 *
 */
public class CSharpFileLocator implements BatchExtension {
  
  private final static Logger log = LoggerFactory.getLogger(CSharpFileLocator.class);

  private Map<File, VisualStudioProject> csFilesProjectMap = Collections.EMPTY_MAP;

  public void registerProject(Project project) {
    log.debug("Init C# file locator");
    csFilesProjectMap = VisualUtils.buildCsFileProjectMap(project);
  }

  public CSharpFile locate(Project project, File file, boolean unitTest) {
    if (csFilesProjectMap.isEmpty()) {
      registerProject(project);
    }
    File absoluteFile = file.getAbsoluteFile();
    final CSharpFile result;
    if (csFilesProjectMap.containsKey(absoluteFile)) {
      VisualStudioProject visualProject = csFilesProjectMap.get(absoluteFile);
      result = CSharpFile.from(visualProject, absoluteFile, unitTest);
    } else {
      log.debug(
          "file {} ignored (i.e. link file, file not referenced by any project, or project/file excluded)",
          absoluteFile);
      result = null;
    }

    return result;
  }
  
  private Resource<?> getResource(Project project, File file) {
    CSharpFile fileResource;
    if (file.exists()) {
      try {
        fileResource = locate(project, file, false);
      } catch (InvalidResourceException ex) {
        log.warn("resource error", ex);
        fileResource = null;
      }
    } else {
      log.error("Unable to get resource for path {}", file);
      fileResource = null;
    }

    return fileResource;
  }
  
  public Resource<?> getResource(Project project, String filePath) {
    if (log.isDebugEnabled()) {
      log.debug("Getting resource for path {}", filePath);
    }
    File file = new File(filePath);
    return getResource(project, file);
  }
  
  public Resource<?> getResource(Project project, String path, String fileName) {
    if (log.isDebugEnabled()) {
      log.debug("Getting resource for path {} {}", path, fileName);
    }
    File file = new File(path, fileName);
    return getResource(project, file);
  }

}
