/*
 * Sonar C# Plugin :: Core
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
package org.sonar.plugins.csharp.api;

import org.sonar.api.BatchExtension;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;


public class ResourceHelper implements BatchExtension {
  
  private final SensorContext sensorContext;
  
 
  public ResourceHelper(SensorContext sensorContext) {
    this.sensorContext = sensorContext;
  }


  public boolean isResourceInProject(Resource<?> resource, Project project) {
    final boolean result;
    
    if (resource instanceof Project) {
      result = resource.getEffectiveKey().equals(project.getEffectiveKey());
    } else {
      Resource<?> parent = sensorContext.getParent(resource);
      if (parent==null) {
        // should not happen
        result = false;
      } else {
        result = isResourceInProject(parent, project);
      }
    }
    return result;
  }
  
}
