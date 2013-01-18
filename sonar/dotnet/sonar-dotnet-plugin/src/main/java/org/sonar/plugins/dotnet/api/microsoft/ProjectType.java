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
/*
 * Created on Apr 16, 2009
 *
 */
package org.sonar.plugins.dotnet.api.microsoft;

/**
 * Possible types of Visual Studio Project.
 * @author stefvic
 */
public enum ProjectType {

  /**
   * Base Visual Studio project type.
   */
  PROJECT,

  /**
   * Unit Test Visual Studio project type.
   */
  UNIT_TEST_PROJECT,

  /**
   * Integration Test Visual Studio project type.
   */
  IT_TEST_PROJECT,

  /**
   * Silverlight Visual Studio project type.
   */
  SILVERLIGHT_PROJECT,

  /**
   * Web Visual Studio project type.
   */
  WEB_PROJECT
}
