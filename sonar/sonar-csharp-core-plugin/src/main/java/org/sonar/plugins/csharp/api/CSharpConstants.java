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

import java.util.Map;

import com.google.common.collect.Maps;

/**
 * Constants for the C# language.
 */
public final class CSharpConstants {

  private CSharpConstants() {
  }

  public static final String LANGUAGE_KEY = "cs";
  public static final String LANGUAGE_NAME = "C#";

  public static final String FILE_SUFFIXES_KEY = "sonar.csharp.file.suffixes";
  public static final String FILE_SUFFIXES_DEFVALUE = "cs";

  public static final String CSHARP_WAY_PROFILE = "Sonar C# Way";

  public static final String CSHARP_CORE_EXECUTED = "C# Core executed";

  // ----------- Plugin Configuration Properties ----------- //

  public static final String DOTNET_2_0_SDK_DIR_KEY = "sonar.dotnet.2.0.sdk.directory";
  public static final String DOTNET_2_0_SDK_DIR_DEFVALUE = "C:/WINDOWS/Microsoft.NET/Framework/v2.0.50727";

  public static final String DOTNET_3_5_SDK_DIR_KEY = "sonar.dotnet.3.5.sdk.directory";
  public static final String DOTNET_3_5_SDK_DIR_DEFVALUE = "C:/WINDOWS/Microsoft.NET/Framework/v3.5";

  public static final String DOTNET_4_0_SDK_DIR_KEY = "sonar.dotnet.4.0.sdk.directory";
  public static final String DOTNET_4_0_SDK_DIR_DEFVALUE = "C:/WINDOWS/Microsoft.NET/Framework/v4.0.30319";

  public static final String DOTNET_VERSION_KEY = "sonar.dotnet.version";
  public static final String DOTNET_VERSION_DEFVALUE = "4.0";

  public static final String SILVERLIGHT_3_MSCORLIB_LOCATION_KEY = "sonar.silverlight.3.mscorlib.location";
  public static final String SILVERLIGHT_3_MSCORLIB_LOCATION_DEFVALUE = "C:/Program Files/Reference Assemblies/Microsoft/Framework/Silverlight/v3.0";

  public static final String SILVERLIGHT_4_MSCORLIB_LOCATION_KEY = "sonar.silverlight.4.mscorlib.location";
  public static final String SILVERLIGHT_4_MSCORLIB_LOCATION_DEFVALUE = "C:/Program Files/Reference Assemblies/Microsoft/Framework/Silverlight/v4.0";

  public static final String SILVERLIGHT_VERSION_KEY = "sonar.silverlight.version";
  public static final String SILVERLIGHT_VERSION_DEFVALUE = "4";

  public static final String TEST_PROJET_PATTERN_KEY = "sonar.donet.visualstudio.testProjectPattern";
  public static final String TEST_PROJET_PATTERN_DEFVALUE = "*.Tests";

  public static final String SOLUTION_FILE_KEY = "sonar.dotnet.visualstudio.solution.file";
  public static final String SOLUTION_FILE_DEFVALUE = "";

  /**
   * Returns the config key that will allow to retrieve the .NET SDK directory from the plugin configuration.
   * 
   * @param sdkVersion
   *          the wanted .NET version (2.5, 3.0 or 4.0)
   * @return the config key
   */
  public static String getDotnetSdkDirKey(String sdkVersion) {
    return dotnetFrameworkLocationKeys.get(sdkVersion);
  }

  /**
   * Returns the default value for the .NET SDK directory according to the given sdk version.
   * 
   * @param sdkVersion
   *          the wanted .NET version (2.5, 3.0 or 4.0)
   * @return the default value
   */
  public static String getDotnetSdkDirDefaultValue(String sdkVersion) {
    return dotnetFrameworkLocationDefaultValues.get(sdkVersion);
  }

  /**
   * Returns the config key that will allow to retrieve the Silverlight directory from the plugin configuration.
   * 
   * @param silverlightVersion
   *          the wanted Silverlight version (3 or 4)
   * @return the config key
   */
  public static String getSilverlightDirKey(String silverlightVersion) {
    return silverlightFrameworkLocationKeys.get(silverlightVersion);
  }

  /**
   * Returns the default value for the Silverlight directory according to the given silverlight version.
   * 
   * @param silverlightVersion
   *          the wanted Silverlight version (3 or 4)
   * @return the default value
   */
  public static String getSilverlightDirDefaultValue(String silverlightVersion) {
    return silverlightFrameworkLocationDefaultValues.get(silverlightVersion);
  }

  private static Map<String, String> dotnetFrameworkLocationKeys;
  private static Map<String, String> silverlightFrameworkLocationKeys;
  private static Map<String, String> dotnetFrameworkLocationDefaultValues;
  private static Map<String, String> silverlightFrameworkLocationDefaultValues;

  static {
    dotnetFrameworkLocationKeys = Maps.newHashMap();
    dotnetFrameworkLocationKeys.put("2.0", DOTNET_2_0_SDK_DIR_KEY);
    dotnetFrameworkLocationKeys.put("3.5", DOTNET_3_5_SDK_DIR_KEY);
    dotnetFrameworkLocationKeys.put("4.0", DOTNET_4_0_SDK_DIR_KEY);
    silverlightFrameworkLocationKeys = Maps.newHashMap();
    silverlightFrameworkLocationKeys.put("3", SILVERLIGHT_3_MSCORLIB_LOCATION_KEY);
    silverlightFrameworkLocationKeys.put("4", SILVERLIGHT_4_MSCORLIB_LOCATION_KEY);
    dotnetFrameworkLocationDefaultValues = Maps.newHashMap();
    dotnetFrameworkLocationDefaultValues.put("2.0", DOTNET_2_0_SDK_DIR_DEFVALUE);
    dotnetFrameworkLocationDefaultValues.put("3.5", DOTNET_3_5_SDK_DIR_DEFVALUE);
    dotnetFrameworkLocationDefaultValues.put("4.0", DOTNET_4_0_SDK_DIR_DEFVALUE);
    silverlightFrameworkLocationDefaultValues = Maps.newHashMap();
    silverlightFrameworkLocationDefaultValues.put("3", SILVERLIGHT_3_MSCORLIB_LOCATION_DEFVALUE);
    silverlightFrameworkLocationDefaultValues.put("4", SILVERLIGHT_4_MSCORLIB_LOCATION_DEFVALUE);
  }

}
