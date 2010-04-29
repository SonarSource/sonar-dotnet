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
 * Created on Sep 3, 2009
 */
package org.sonar.plugin.dotnet.core.resource;

import java.io.File;

import javax.print.attribute.standard.MediaSize.NA;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.CSharp;

/**
 * A folder from an assembly that contains some source code.
 * 
 * @author Jose CHILLAN Sep 3, 2009
 */
public class CSharpFolder extends AbstractCSharpResource<CLRAssembly>
{
  private CLRAssembly assembly;

  /**
   * Creates a CSharp folder from a directory.
   * 
   * @param project the project containing the directory
   * @param directory the directory to see as a resource
   * @return the new folder, or <code>null</code> if a problem occured
   */
  public static CSharpFolder fromDirectory(Project project, File directory)
  {
    CLRAssembly assembly = CLRAssembly.forFile(project, directory);
    if (assembly != null)
    {
      return new CSharpFolder(assembly, directory);
    }
    return null;
  }

  /**
   * Constructs a @link{CSharpFolder} with the containing assembly and location
   * 
   * @param assembly the assembly that contains the folder
   * @param file the folder location
   */
  public CSharpFolder(CLRAssembly assembly, File directory)
  {
    super(SCOPE_SPACE, QUALIFIER_DIRECTORY);
    this.assembly = assembly;
    VisualStudioProject visualProject = assembly.getVisualProject();
    String folder = visualProject.getRelativePath(directory);
    String assemblyName = assembly.getAssemblyName();
    String key = CSharp.createKey(assemblyName, folder, null);
    setKey(key);
    String name;
    // Defines the folder name
    if (StringUtils.isBlank(folder))
    {
      name = assemblyName + "/";
    }
    else
    {
      name = assemblyName+ "/" + folder + "/";
    }
    setName("Folder " + name);
  }

  /**
   * @param antPattern
   * @return
   */
  @Override
  public boolean matchFilePattern(String antPattern)
  {
    return false;
  }

  /**
   * @return
   */
  @Override
  public String getLongName()
  {
    return "Folder " + getName();
  }
  /**
   * @return
   */
  @Override
  public CLRAssembly getParent()
  {
    return assembly;
  }
}
