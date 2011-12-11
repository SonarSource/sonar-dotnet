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

package org.sonar.plugins.csharp.core;

import java.util.ArrayList;
import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.csharp.api.CSharp;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.ResourceHelper;

/**
 * C# Core plugin class.
 */
@Properties({
    @Property(key = CSharpConstants.DOTNET_2_0_SDK_DIR_KEY, defaultValue = CSharpConstants.DOTNET_2_0_SDK_DIR_DEFVALUE,
        name = ".NET 2.0 SDK directory", description = "Absolute path of the .NET SDK 2.0 directory.", global = true, project = false),
    @Property(key = CSharpConstants.DOTNET_3_5_SDK_DIR_KEY, defaultValue = CSharpConstants.DOTNET_3_5_SDK_DIR_DEFVALUE,
        name = ".NET 3.5 SDK directory", description = "Absolute path of the .NET SDK 3.5 directory.", global = true, project = false),
    @Property(key = CSharpConstants.DOTNET_4_0_SDK_DIR_KEY, defaultValue = CSharpConstants.DOTNET_4_0_SDK_DIR_DEFVALUE,
        name = ".NET 4.0 SDK directory", description = "Absolute path of the .NET SDK 4.0 directory.", global = true, project = false),
    @Property(key = CSharpConstants.DOTNET_VERSION_KEY, defaultValue = CSharpConstants.DOTNET_VERSION_DEFVALUE, name = ".NET version",
        description = "Default version of the .NET framework that must be used.", global = true, project = true),
    @Property(key = CSharpConstants.SILVERLIGHT_3_MSCORLIB_LOCATION_KEY,
        defaultValue = CSharpConstants.SILVERLIGHT_3_MSCORLIB_LOCATION_DEFVALUE, name = "Silverlight 3 assembly directory",
        description = "Location of the core assembly for Silverlight 3 framework.", global = true, project = false),
    @Property(key = CSharpConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_KEY,
        defaultValue = CSharpConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_DEFVALUE, name = "Silverlight 4 assembly directory",
        description = "Location of the core assembly for Silverlight 4 framework.", global = true, project = false),
    @Property(key = CSharpConstants.SILVERLIGHT_VERSION_KEY, defaultValue = CSharpConstants.SILVERLIGHT_VERSION_DEFVALUE,
        name = "Silverlight version", description = "Default version of the Silverlight framework that must be used.", global = true,
        project = true),
    @Property(key = CSharpConstants.TEST_PROJET_PATTERN_KEY, defaultValue = CSharpConstants.TEST_PROJET_PATTERN_DEFVALUE,
        name = "Test project names", description = "Pattern that check project names to identify test projects.", global = true,
        project = true),
    @Property(
        key = CSharpConstants.SOLUTION_FILE_KEY,
        defaultValue = CSharpConstants.SOLUTION_FILE_DEFVALUE,
        name = "Solution to analyse",
        description = "Relative path to the \".sln\" file that represents the solution to analyse. If none provided, a \".sln\" file will be searched at the root of the project.",
        global = false, project = true),
    @Property(key = CSharpConstants.EXCLUDE_GENERATED_CODE_KEY, defaultValue = CSharpConstants.EXCLUDE_GENERATED_CODE_DEFVALUE + "",
        name = "Exclude generated code",
        description = "Set to false to include generated code like 'Reference.cs' files or '*.designer.cs' files.", global = true,
        project = true),
    @Property(key = CSharpConstants.BUILD_CONFIGURATIONS_KEY, defaultValue = CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE,
        name = "Build configurations", description = "Comma-seperated list of build configurations to use.", global = true, project = true) })
public class CSharpCorePlugin extends SonarPlugin {

  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> extensions = new ArrayList<Class<? extends Extension>>();
    extensions.add(CSharp.class);
    extensions.add(CSharpConfiguration.class);

    // Project Builder for .NET projects
    extensions.add(VisualStudioProjectBuilder.class);
    extensions.add(MicrosoftWindowsEnvironment.class);
    extensions.add(CSharpProjectInitializer.class);

    // Utility class shared amongst all the C# plugin ecosystem through API
    extensions.add(CSharpResourcesBridge.class);
    extensions.add(ResourceHelper.class);

    // Sensors
    extensions.add(CSharpSourceImporter.class);

    return extensions;
  }
}
