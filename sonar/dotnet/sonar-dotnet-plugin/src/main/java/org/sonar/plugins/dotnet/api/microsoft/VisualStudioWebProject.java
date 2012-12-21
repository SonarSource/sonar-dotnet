/*
 * Sonar .NET Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
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
package org.sonar.plugins.dotnet.api.microsoft;

import org.apache.commons.lang.StringUtils;

import java.io.File;
import java.util.HashSet;
import java.util.Set;

/**
 * Represent a web/asp folder/project Visual Studio handle in a weird way asp projects since there is no csproj files. The description of
 * the project is embedded in the sln solution file.
 *
 * @author Fabrice BELLINGARD
 * @author Alexandre Victoor
 */
public class VisualStudioWebProject extends VisualStudioProject {

  /** The default value of reference directory. Should be relative to solution base directory */
  private static final String DEFAULT_REFERENCE__DIR_VALUE = "Bin";

  /** The value of reference directory. Should be relative to solution base directory */
  private String referenceDir = DEFAULT_REFERENCE__DIR_VALUE;

  public VisualStudioWebProject() {
    setType(ArtifactType.WEB);
  }

  /**
   * @return the generated web dlls
   *
   */
  @Override
  public Set<File> getGeneratedAssemblies(String buildConfigurations, String buildPlatform) {
    Set<File> result = new HashSet<File>();

    // we need to exclude all the dll files
    // that correspond to references
    Set<String> exclusions = new HashSet<String>();
    Set<File> references = getReferences();
    for (File file : references) {
      exclusions.add(file.getName());
    }

    File precompilationDirectory = getWebPrecompilationDirectory(buildConfigurations, buildPlatform);
    if (precompilationDirectory != null && precompilationDirectory.isDirectory()) {
      File[] files = precompilationDirectory.listFiles();
      for (File file : files) {
        String name = file.getName();
        if (StringUtils.endsWith(name, "dll") && !exclusions.contains(name)) {
          result.add(file);
        }
      }
    }

    return result;
  }

  /**
   * @param buildConfigurations
   *          Visual Studio build configurations used to generate the project
   * @return the directory where asp.net pages are precompiled. null for a non web project
   */
  public File getWebPrecompilationDirectory(String buildConfigurations, String buildPlatform) {
    return new File(getArtifactDirectory(buildConfigurations, buildPlatform), "bin");
  }

  /**
   * @return the generated web dll names otherwise.
   */
  public Set<String> getWebAssemblyNames() {
    Set<File> assemblies = getGeneratedAssemblies(null, null);
    Set<String> assemblyNames = new HashSet<String>();

    for (File assembly : assemblies) {
      assemblyNames.add(StringUtils.substringBeforeLast(assembly.getName(), ".dll"));
    }

    return assemblyNames;
  }

  /**
   * @return the dll files that correspond to VS references
   */
  public Set<File> getReferences() {
    File binDirectory = new File(getDirectory(), referenceDir);
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

  /**
   * Set reference directory.
   * @param referenceDir the relative(to solution base directory) reference directory.
   */
  public void setReferenceDirectory(String referenceDir) {
    this.referenceDir = referenceDir;
  }

}
