/*
 * Sonar .NET Plugin :: Tests
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
package org.sonar.plugins.csharp.tests;

import org.sonar.api.resources.Project;

import java.io.File;

/**
 * Layer of abstraction to improve the testability of the sensor.
 */
public class FileProvider {

  private final Project project;

  public FileProvider(Project project) {
    this.project = project;
  }

  public org.sonar.api.resources.File fromPath(String path) {
    return org.sonar.api.resources.File.fromIOFile(new File(path), project);
  }

}
