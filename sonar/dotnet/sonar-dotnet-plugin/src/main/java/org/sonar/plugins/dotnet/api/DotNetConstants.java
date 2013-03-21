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
package org.sonar.plugins.dotnet.api;

import com.google.common.collect.Maps;
import org.sonar.plugins.dotnet.api.microsoft.BuildConfiguration;

import java.util.Map;

/**
 * Constants for the .NET platform.
 */
public final class DotNetConstants {

  private DotNetConstants() {
  }

  public static final String CORE_PLUGIN_EXECUTED = "Core executed";

  // ----------- Plugin Configuration Properties ----------- //

  public static final String DOTNET_2_0_SDK_DIR_KEY = "sonar.dotnet.2.0.sdk.directory";
  public static final String MVN_DOTNET_2_0_SDK_DIR_KEY = "dotnet.2.0.sdk.directory";
  public static final String DOTNET_2_0_SDK_DIR_DEFVALUE = "C:/WINDOWS/Microsoft.NET/Framework/v2.0.50727";

  public static final String DOTNET_3_5_SDK_DIR_KEY = "sonar.dotnet.3.5.sdk.directory";
  public static final String MVN_DOTNET_3_5_SDK_DIR_KEY = "dotnet.3.5.sdk.directory";
  public static final String DOTNET_3_5_SDK_DIR_DEFVALUE = "C:/WINDOWS/Microsoft.NET/Framework/v3.5";

  public static final String DOTNET_4_0_SDK_DIR_KEY = "sonar.dotnet.4.0.sdk.directory";
  public static final String MVN_DOTNET_4_0_SDK_DIR_KEY = "dotnet.4.0.sdk.directory";
  public static final String DOTNET_4_0_SDK_DIR_DEFVALUE = "C:/WINDOWS/Microsoft.NET/Framework/v4.0.30319";

  // There's no "DOTNET_4_5_SDK_DIR_DEFVALUE" as this is the same value as "DOTNET_4_0_SDK_DIR_DEFVALUE"
  public static final String DOTNET_4_5_SDK_DIR_KEY = "sonar.dotnet.4.5.sdk.directory";

  public static final String DOTNET_VERSION_KEY = "sonar.dotnet.version";
  public static final String MVN_DOTNET_VERSION_KEY = "dotnet.tool.version";
  public static final String DOTNET_VERSION_DEFVALUE = "4.0";

  public static final String SILVERLIGHT_3_MSCORLIB_LOCATION_KEY = "sonar.silverlight.3.mscorlib.location";
  public static final String MVN_SILVERLIGHT_3_MSCORLIB_LOCATION_KEY = "silverlight.3.mscorlib.location";
  public static final String SILVERLIGHT_3_MSCORLIB_LOCATION_DEFVALUE = "C:/Program Files/Reference Assemblies/Microsoft/Framework/Silverlight/v3.0";

  public static final String SILVERLIGHT_4_MSCORLIB_LOCATION_KEY = "sonar.silverlight.4.mscorlib.location";
  public static final String MVN_SILVERLIGHT_4_MSCORLIB_LOCATION_KEY = "silverlight.4.mscorlib.location";
  public static final String SILVERLIGHT_4_MSCORLIB_LOCATION_DEFVALUE = "C:/Program Files/Reference Assemblies/Microsoft/Framework/Silverlight/v4.0";

  public static final String SILVERLIGHT_5_MSCORLIB_LOCATION_KEY = "sonar.silverlight.5.mscorlib.location";
  public static final String MVN_SILVERLIGHT_5_MSCORLIB_LOCATION_KEY = "silverlight.5.mscorlib.location";
  public static final String SILVERLIGHT_5_MSCORLIB_LOCATION_DEFVALUE = "C:/Program Files/Reference Assemblies/Microsoft/Framework/Silverlight/v5.0";

  public static final String SILVERLIGHT_VERSION_KEY = "sonar.silverlight.version";
  public static final String MVN_SILVERLIGHT_VERSION_KEY = "silverlight.version";
  public static final String SILVERLIGHT_VERSION_DEFVALUE = "4";

  public static final String TEST_PROJECT_PATTERN_KEY = "sonar.dotnet.visualstudio.testProjectPattern";
  public static final String TEST_PROJECT_PATTERN_DEFVALUE = "*.Tests";

  public static final String IT_PROJECT_PATTERN_KEY = "sonar.dotnet.visualstudio.itProjectPattern";
  public static final String IT_PROJECT_PATTERN_DEFVALUE = "";

  public static final String SOLUTION_FILE_KEY = "sonar.dotnet.visualstudio.solution.file";
  public static final String SOLUTION_FILE_DEFVALUE = "";

  public static final String EXCLUDE_GENERATED_CODE_KEY = "sonar.dotnet.excludeGeneratedCode";
  public static final boolean EXCLUDE_GENERATED_CODE_DEFVALUE = true;

  public static final String BUILD_CONFIGURATION_KEY = "sonar.dotnet.buildConfiguration";
  public static final String BUILD_CONFIGURATIONS_DEFVALUE = "Debug";

  public static final String BUILD_PLATFORM_KEY = "sonar.dotnet.buildPlatform";
  public static final String BUILD_PLATFORM_DEFVALUE = BuildConfiguration.DEFAULT_PLATFORM;

  public static final String ASSEMBLIES_TO_SCAN_KEY = "sonar.dotnet.assemblies";

  public static final String TEST_ASSEMBLIES_KEY = "sonar.dotnet.test.assemblies";

  public static final String KEY_GENERATION_STRATEGY_KEY = "sonar.dotnet.key.generation.strategy";

  /**
   * Returns the config key that will allow to retrieve the .NET SDK directory from the plugin configuration.
   * 
   * @param sdkVersion
   *          the wanted .NET version (2.5, 3.0, 4.0 or 4.5)
   * @return the config key
   */
  public static String getDotnetSdkDirKey(String sdkVersion) {
    return dotnetFrameworkLocationKeys.get(sdkVersion);
  }

  /**
   * Returns the config key that will allow to retrieve the Silverlight directory from the plugin configuration.
   * 
   * @param silverlightVersion
   *          the wanted Silverlight version (3, 4 or 5)
   * @return the config key
   */
  public static String getSilverlightDirKey(String silverlightVersion) {
    return silverlightFrameworkLocationKeys.get(silverlightVersion);
  }

  private static Map<String, String> dotnetFrameworkLocationKeys;
  private static Map<String, String> silverlightFrameworkLocationKeys;

  static {
    // === .NET Framework ===
    dotnetFrameworkLocationKeys = Maps.newHashMap();
    dotnetFrameworkLocationKeys.put("2.0", DOTNET_2_0_SDK_DIR_KEY);
    dotnetFrameworkLocationKeys.put("3.5", DOTNET_3_5_SDK_DIR_KEY);
    dotnetFrameworkLocationKeys.put("4.0", DOTNET_4_0_SDK_DIR_KEY);
    // .NET 4.5 is installed in the same folder as 4.0
    dotnetFrameworkLocationKeys.put("4.5", DOTNET_4_0_SDK_DIR_KEY);

    // === Silverlight ===
    silverlightFrameworkLocationKeys = Maps.newHashMap();
    silverlightFrameworkLocationKeys.put("3", SILVERLIGHT_3_MSCORLIB_LOCATION_KEY);
    silverlightFrameworkLocationKeys.put("4", SILVERLIGHT_4_MSCORLIB_LOCATION_KEY);
    silverlightFrameworkLocationKeys.put("5", SILVERLIGHT_5_MSCORLIB_LOCATION_KEY);
  }

}
