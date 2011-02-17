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
 * Created on Apr 28, 2009
 */
package com.sonar.csharp.plugin.resource;

import java.io.File;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class Utils {

  private final static Logger log = LoggerFactory.getLogger(Utils.class);

  /**
   * Creates a key for a source file contained in a given assembly. This key is used as a discriminant in the sonar database (for the
   * PROJECTS table column KEE)
   * 
   * @param assembly
   *          the assembly object
   * @param sourcePath
   *          the path of the source file
   * @return the generated key
   */
  public static String createKey(CLRAssembly assembly, File sourcePath) {
    VisualStudioProject visualProject = assembly.getVisualProject();
    SourceFile sourceFile = visualProject.getFile(sourcePath);
    final String key;
    if (sourceFile == null) {
      log.warn("A source file is not included in the project : " + sourcePath);
      key = null;
    } else {
      String assemblyName = assembly.getAssemblyName();
      String folder = sourceFile.getFolder();
      String fileName = sourceFile.getName();
      key = createKey(assemblyName, folder, fileName);
    }
    return key;
  }

  /**
   * Creates a key for a given resource.
   * 
   * @param assemblyName
   * @param namespace
   * @param className
   * @return
   */
  public static String createKey(String assemblyName, String folder, String fileName) {
    StringBuilder builder = new StringBuilder();
    String keyAssemblyName = assemblyName.toLowerCase();
    builder.append(keyAssemblyName);

    // Not that spaces and "/" are not supported by the sonar web application
    // and should be removed
    String canonicalFolder = StringUtils.remove(StringUtils.replace(StringUtils.replace(folder, "\\", "-"), "/", "."), " ");
    String processedFolder = StringUtils.removeEnd(canonicalFolder, ".");
    // We add the folder to the assembly name
    if ((processedFolder != null) || (fileName != null)) {
      // This is not just a namespace
      builder.append(":");
      if (processedFolder != null) {
        if (StringUtils.isNotBlank(processedFolder)) {
          String keyFolderName = processedFolder.toLowerCase();
          builder.append(keyFolderName);
        }
      }
      // We finish by the file name
      if (StringUtils.isNotBlank(fileName)) {
        builder.append("_");
        String keyFileName = fileName.toLowerCase();
        builder.append(keyFileName);
      }
    }
    return builder.toString();
  }
}
