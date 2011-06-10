/*
 * .NET tools :: Commons
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
/*
 * Created on Apr 16, 2009
 *
 */
package org.sonar.plugins.csharp.api.visualstudio;

/**
 * Possible types of .Net Artifact.
 * 
 * @author Jose CHILLAN Apr 16, 2009
 */
public enum ArtifactType {
  /**
   * Artifact that corresponds to a .dll.
   */
  LIBRARY,
  /**
   * Artifact that corresponds to a .exe.
   */
  EXECUTABLE,

  /**
   * Artifact that corresponds to a web project.
   */
  WEB
}
