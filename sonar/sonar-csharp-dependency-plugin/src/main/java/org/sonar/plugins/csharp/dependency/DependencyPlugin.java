/*
 * Sonar C# Plugin :: Dependency
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

package org.sonar.plugins.csharp.dependency;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.csharp.dependency.results.DependencyResultParser;

@Properties({
  @Property(key = DependencyConstants.INSTALL_DIR_KEY, 
      name = "DependencyParser install directory", description = "Absolute path of the DependencyParser installation folder.", global = true,
      project = false),
  @Property(key = DependencyConstants.TIMEOUT_MINUTES_KEY, defaultValue = DependencyConstants.TIMEOUT_MINUTES_DEFVALUE + "",
      name = "DependencyParser program timeout", description = "Maximum number of minutes before the DependencyParser program will be stopped.",
      global = true, project = true),
  @Property(key = DependencyConstants.MODE, defaultValue = "", name = "DependencyParser activation mode",
      description = "Possible values : empty (means active), 'skip' and 'reuseReport'.", global = false, project = false),
  @Property(key = DependencyConstants.REPORTS_PATH_KEY, defaultValue = "", name = "Name of the DependencyParser report files",
      description = "Name of the DependencyParser report file used when reuse report mode is activated. "
          + "This can be an absolute path, or a path relative to each project base directory.", global = false, project = false) })
public class DependencyPlugin extends SonarPlugin {
  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> list = new ArrayList<Class<? extends Extension>>();
    list.add(DependencySensor.class);
    
    list.add(DependencyResultParser.class);
    list.add(CSharpDsmDecorator.class);
    
    return list;
  }
}
