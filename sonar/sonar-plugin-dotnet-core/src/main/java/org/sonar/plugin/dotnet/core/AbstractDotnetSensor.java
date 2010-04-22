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
 * Created on Apr 28, 2009
 */
package org.sonar.plugin.dotnet.core;

import java.io.File;
import java.io.FilenameFilter;

import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.maven.DependsUponMavenPlugin;
import org.sonar.api.resources.Project;

/**
 * A base class for the Sonar .Net result sensors.
 * 
 * @author Jose CHILLAN Apr 28, 2009
 */
public abstract class AbstractDotnetSensor
  implements Sensor, DependsUponMavenPlugin
{
  /**
   * Constructs a @link{AbstractDotnetMavenCollector}.
   */
  public AbstractDotnetSensor()
  {
  }

  /**
   * Gets the report directory for a given project.
   * 
   * @param pom the project definition
   * @return the report directory
   */
  public File getReportsDirectory(Project project)
  {
    File result = project.getFileSystem().getBuildDir();
    return result;
  }

  /**
   * Finds a report file from its name
   * 
   * @param pom the pom
   * @param fileName the expected name for the file
   * @return the report, or <code>null</code> if not found
   */
  public File findReport(Project project, final String fileName)
  {
    File dir = getReportsDirectory(project);
    if (dir == null || !dir.isDirectory() || !dir.exists())
    {
      return null;
    }
    // Looks for a nunit-report.xml file
    File[] listFiles = dir.listFiles(new FilenameFilter() {
      public boolean accept(File localDir, String name)
      {
        return name.equals(fileName);
      }
    });
    if (listFiles.length > 0)
    {
      return listFiles[0];
    }
    return null;
  }

  /**
   * Only accepts solution artifact projects.
   * @param project
   * @return
   */
  @Override
  public boolean shouldExecuteOnProject(Project project)
  {
    String packaging = project.getPackaging();
    // We only accept the "sln" packaging
    return "sln".equals(packaging);
  }

}
