/*
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

package org.sonar.plugin.dotnet.core.resource;

import java.io.File;
import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.project.VisualUtils;

public enum CSharpFileLocator {
  INSTANCE;
  
  private final static Logger log = LoggerFactory.getLogger(CSharpFileLocator.class);
  
  private Set<File> knownCsFiles = Collections.EMPTY_SET;

  public void registerProject(Project project) {
	log.debug("Init C# file locator");  
	knownCsFiles = new HashSet<File>(VisualUtils.getCsFiles(project));
  }
  
  public CSharpFile locate(Project project, File file, boolean unitTest) {
	if (knownCsFiles.isEmpty()) {
	  registerProject(project);
	}
	final CSharpFile result;
	if (knownCsFiles.contains(file)) {
	  result = CSharpFile.from(project, file, unitTest);
	} else {
		log.debug("file {} ignored (i.e. link file or file not referenced by any project)", file);
	  result = null;
	}
	
	return result;
  }
  
  
	
	
}
