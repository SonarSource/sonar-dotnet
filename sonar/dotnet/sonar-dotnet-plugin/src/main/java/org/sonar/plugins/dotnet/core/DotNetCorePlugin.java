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
package org.sonar.plugins.dotnet.core;

import org.sonar.api.Extension;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.PropertyType;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.DotNetConstants;
import org.sonar.plugins.dotnet.api.DotNetResourceBridges;
import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.utils.ResourceHelper;

import java.util.ArrayList;
import java.util.List;

/**
 * .NET Core plugin class.
 */
@Properties({
  @Property(key = DotNetConstants.DOTNET_2_0_SDK_DIR_KEY, defaultValue = DotNetConstants.DOTNET_2_0_SDK_DIR_DEFVALUE,
    name = ".NET 2.0 SDK directory", description = "Absolute path of the .NET SDK 2.0 directory.", global = true, project = false),
  @Property(key = DotNetConstants.DOTNET_3_5_SDK_DIR_KEY, defaultValue = DotNetConstants.DOTNET_3_5_SDK_DIR_DEFVALUE,
    name = ".NET 3.5 SDK directory", description = "Absolute path of the .NET SDK 3.5 directory.", global = true, project = false),
  @Property(key = DotNetConstants.DOTNET_4_0_SDK_DIR_KEY, defaultValue = DotNetConstants.DOTNET_4_0_SDK_DIR_DEFVALUE,
    name = ".NET 4.0 SDK directory", description = "Absolute path of the .NET SDK 4.0 directory.", global = true, project = false),
  // Install directory of .NET 4.5 is the same as for 4.0 - this is why default value points to DOTNET_4_0_SDK_DIR_DEFVALUE
  @Property(key = DotNetConstants.DOTNET_4_5_SDK_DIR_KEY, defaultValue = DotNetConstants.DOTNET_4_0_SDK_DIR_DEFVALUE,
    name = ".NET 4.5 SDK directory", description = "Absolute path of the .NET SDK 4.5 directory.", global = true, project = false),
  @Property(key = DotNetConstants.DOTNET_VERSION_KEY, defaultValue = DotNetConstants.DOTNET_VERSION_DEFVALUE, name = ".NET version",
    description = "Default version of the .NET framework that must be used.", global = true, project = true,
    type = PropertyType.SINGLE_SELECT_LIST, options = {"2.0", "3.5", "4.0", "4.5"}),
  @Property(key = DotNetConstants.SILVERLIGHT_3_MSCORLIB_LOCATION_KEY,
    defaultValue = DotNetConstants.SILVERLIGHT_3_MSCORLIB_LOCATION_DEFVALUE, name = "Silverlight 3 assembly directory",
    description = "Location of the core assembly for Silverlight 3 framework.", global = true, project = false),
  @Property(key = DotNetConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_KEY,
    defaultValue = DotNetConstants.SILVERLIGHT_4_MSCORLIB_LOCATION_DEFVALUE, name = "Silverlight 4 assembly directory",
    description = "Location of the core assembly for Silverlight 4 framework.", global = true, project = false),
  @Property(key = DotNetConstants.SILVERLIGHT_5_MSCORLIB_LOCATION_KEY,
    defaultValue = DotNetConstants.SILVERLIGHT_5_MSCORLIB_LOCATION_DEFVALUE, name = "Silverlight 5 assembly directory",
    description = "Location of the core assembly for Silverlight 4 framework.", global = true, project = false),
  @Property(key = DotNetConstants.SILVERLIGHT_VERSION_KEY, defaultValue = DotNetConstants.SILVERLIGHT_VERSION_DEFVALUE,
    name = "Silverlight version", description = "Default version of the Silverlight framework that must be used.", global = true,
    project = true, type = PropertyType.SINGLE_SELECT_LIST, options = {"3", "4", "5"}),
  @Property(key = DotNetConstants.TEST_PROJECT_PATTERN_KEY, defaultValue = DotNetConstants.TEST_PROJECT_PATTERN_DEFVALUE,
    name = "Test project names", description = "Pattern that check project names to identify test projects.", global = true,
    project = true),
  @Property(
    key = DotNetConstants.SOLUTION_FILE_KEY,
    defaultValue = DotNetConstants.SOLUTION_FILE_DEFVALUE,
    name = "Solution to analyse",
    description = "Relative path to the \".sln\" file that represents the solution to analyse. If none provided, a \".sln\" file will be searched at the root of the project.",
    global = false, project = true),
  @Property(key = DotNetConstants.EXCLUDE_GENERATED_CODE_KEY, defaultValue = DotNetConstants.EXCLUDE_GENERATED_CODE_DEFVALUE + "",
    name = "Exclude generated code",
    description = "Set to false to include generated code like 'Reference.cs' files or '*.designer.cs' files.", global = true,
    project = true, type = PropertyType.BOOLEAN),
  @Property(key = DotNetConstants.BUILD_CONFIGURATION_KEY, defaultValue = DotNetConstants.BUILD_CONFIGURATIONS_DEFVALUE,
    name = "Build configuration", description = "Build configurations used to build the solution.", global = true, project = true),
  @Property(key = DotNetConstants.BUILD_PLATFORM_KEY, defaultValue = DotNetConstants.BUILD_PLATFORM_DEFVALUE,
    name = "Build platform", description = "Build platform used to build the solution.", global = true, project = true),
  @Property(key = DotNetConstants.KEY_GENERATION_STRATEGY_KEY, defaultValue = "",
    name = "Resource key generation strategy", description = "Strategy to generate sonar resource keys. Default value is standard. If you encounter " +
      "any 'NonUniqueResultException' errors you can set this property to 'safe'", global = true, project = true)
})
public class DotNetCorePlugin extends SonarPlugin {

  /**
   * {@inheritDoc}
   */
  public List<Class<? extends Extension>> getExtensions() {
    List<Class<? extends Extension>> extensions = new ArrayList<Class<? extends Extension>>();

    // Environment objects for .NET projects
    extensions.add(DotNetConfiguration.class);
    extensions.add(VisualStudioProjectBuilder.class);
    extensions.add(MicrosoftWindowsEnvironment.class);
    extensions.add(DotNetResourceBridges.class);

    // Utility class shared amongst all the .NET plugin ecosystem through API
    extensions.add(ResourceHelper.class);

    return extensions;
  }
}
