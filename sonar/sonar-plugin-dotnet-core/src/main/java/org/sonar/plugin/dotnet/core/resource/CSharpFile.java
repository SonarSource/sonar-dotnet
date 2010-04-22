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
 * Created on Sep 1, 2009
 */
package org.sonar.plugin.dotnet.core.resource;

import java.io.File;

import org.apache.commons.lang.StringUtils;
import org.jfree.util.Log;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.utils.WildcardPattern;
import org.sonar.plugin.dotnet.core.CSharp;

/**
 * A Resource corresponding to a CSharp file
 * 
 * @author Jose CHILLAN Sep 1, 2009
 */
public class CSharpFile extends AbstractCSharpResource<CSharpFolder>
{
  private String       fileName;
  private CSharpFolder folder;

  /**
   * Gets a file resource from a project
   * 
   * @param file
   * @return
   */
  public static CSharpFile from(Project project, File file, boolean unitTest)
  {
    File directory = file.getParentFile();
    CSharpFolder folder = CSharpFolder.fromDirectory(project, directory);
    if (folder == null)
    {
      Log.warn("Could not find the folder for directory : " + directory + " in project " + project);
    }
    return new CSharpFile(folder, file, unitTest);
  }

  /**
   * Constructs a @link{CSharpFile} with the associated file, and a flag indicating if the file is a unit test.
   * 
   * @throws InvalidResourceException in the resource doesn't belong to the assembly
   */
  private CSharpFile(CSharpFolder folder, File file, boolean unitTest)
  {
    super(Resource.SCOPE_ENTITY, (unitTest ? Resource.QUALIFIER_UNIT_TEST_CLASS: Resource.QUALIFIER_CLASS));
    this.folder = folder;
    this.fileName = file.getName();
    CLRAssembly assembly = folder.getParent();
    String key = CSharp.createKey(assembly, file);
    if (key == null)
    {
      throw new InvalidResourceException("The file " + file + " is not included in the assembly " + assembly.getName());
    }
    setKey(key);
    setName(fileName);
  }

  /**
   * @return
   */
  @Override
  public String getLongName()
  {
    return "File " + fileName;
  }
  /**
   * @return
   */
  @Override
  public CSharpFolder getParent()
  {
    return folder;
  }

  /**
   * @param antPattern
   * @return
   */
  @Override
  public boolean matchFilePattern(String antPattern)
  {
    String patternWithoutFileSuffix = StringUtils.substringBeforeLast(antPattern, ".");
    WildcardPattern matcher = WildcardPattern.create(patternWithoutFileSuffix, ".");
    return matcher.match(getKey());

  }

  /**
   * Returns the fileName.
   * 
   * @return The fileName to return.
   */
  public String getFileName()
  {
    return this.fileName;
  }

  /**
   * @return
   */
  @Override
  public String toString()
  {
    return "CSharpFile(" + folder.getName() + "/" + fileName + ")";
  }

}
