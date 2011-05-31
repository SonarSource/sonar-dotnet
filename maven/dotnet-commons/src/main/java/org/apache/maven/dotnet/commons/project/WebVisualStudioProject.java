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

package org.apache.maven.dotnet.commons.project;

import java.io.File;
import java.util.HashSet;
import java.util.Set;

import org.apache.commons.lang.StringUtils;

/**
 * Represent a web/asp folder/project
 * Visual Studio handle in a weird way asp projects since there
 * is no csproj files. The description of the project is embedded 
 * in the sln solution file.
 * 
 * @author Alexandre Victoor
 *
 */
public class WebVisualStudioProject extends VisualStudioProject {
  
  public WebVisualStudioProject() {
    setType(ArtifactType.WEB);
  }

  /**
   * @return the generated web dlls
   *         
   */
  public Set<File> getGeneratedAssemblies(String buildConfigurations) {
    Set<File> result = new HashSet<File>();
    
    // we need to exclude all the dll files
    // that correspond to references
    Set<String> exclusions = new HashSet<String>();
    Set<File> references = getReferences();
    for (File file : references) {
      exclusions.add(file.getName());
    }

    File[] files = getWebPrecompilationDirectory(buildConfigurations).listFiles();

    for (File file : files) {
      String name = file.getName();
      if (StringUtils.endsWith(name, "dll") && !exclusions.contains(name)) {
        result.add(file);
      }
    }
    return result;
  }

  /**
   * @param buildConfigurations Visual Studio build configurations 
   *                            used to generate the project
   * @return the directory where asp.net pages are precompiled. null for a non
   *         web project
   */
  public File getWebPrecompilationDirectory(String buildConfigurations) {
    return new File(getArtifactDirectory(buildConfigurations), "bin");
  }
  
  /**
   * @return the generated web dll
   *         names otherwise.
   */
  public Set<String> getWebAssemblyNames() {
    Set<File> assemblies = getGeneratedAssemblies(null);
    Set<String> assemblyNames = new HashSet<String>();

    for (File assembly : assemblies) {
      assemblyNames.add(StringUtils.substringBeforeLast(assembly.getName(),
          ".dll"));
    }

    return assemblyNames;
  }
  
  /**
   * @return  the dll files that correspond to VS references
   */
  public Set<File> getReferences() {
    File binDirectory = new File(directory, "Bin");
    Set<File> result = new HashSet<File>();
    if (binDirectory.exists()) {
      File[] files = binDirectory.listFiles();
      for (File file : files) {
        String name = file.getName();
        if (StringUtils.endsWith(name, "dll")) {
          result.add(file);
        }
      } 
    } 
    return result;
  }
  
}
