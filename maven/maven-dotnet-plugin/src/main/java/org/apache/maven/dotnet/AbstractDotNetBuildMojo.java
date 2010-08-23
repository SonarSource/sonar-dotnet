/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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
 * Created on Jun 18, 2009
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.maven.plugin.MojoExecutionException;
import com.ibm.icu.util.StringTokenizer;

/**
 * A base class for all build relevant mojos.
 * 
 * @author Jose CHILLAN Mar 25, 2010
 */
public abstract class AbstractDotNetBuildMojo extends AbstractDotNetMojo {

  /**
   * Installation directory of the .Net SDK 2.0
   * 
   * @parameter expression="${dotnet.2.0.sdk.directory}"
   *            default-value="C:/WINDOWS/Microsoft.NET/Framework/v2.0.50727"
   */
  private String dotnet_2_0_sdk_directory;

  /**
   * Installation directory of the .Net SDK 3.5
   * 
   * @parameter expression="${dotnet.3.5.sdk.directory}"
   *            default-value="C:/WINDOWS/Microsoft.NET/Framework/v3.5"
   */
  private String dotnet_3_5_sdk_directory;

  /**
   * Version of the MSBuild tool to use, which is the installed version of the
   * SDK. Possible values are 2.0 and 3.5.
   * 
   * @parameter expression="${dotnet.tool.version}" default-value="3.5"
   */
  protected String toolVersion;

  /**
   * The build configurations to use for the project or solution, separated by
   * colons as in "Debug,Release".
   * 
   * @parameter expression="${msbuild.configurations}"
   *            alias="${buildConfigurations}" default-value="Debug"
   */
  protected String buildConfigurations = "Debug";

  /**
   * Gets the MSBuild.exe command, depending on the tool version.
   * 
   * @return the File representing the MSBuild.exe command
   * @throws MojoExecutionException
   */
  protected File getMsBuildCommand() throws MojoExecutionException {

    // This may depend on the version in the future
    String commandName = "MSBuild.exe";
    File executable;
    if ("3.5".equals(toolVersion)) {
      executable = new File(dotnet_3_5_sdk_directory, commandName);
    } else {
      executable = new File(dotnet_2_0_sdk_directory, commandName);
    }

    if (!executable.exists()) {
      throw new MojoExecutionException(
          "Could not find the MSBuild executable for the version "
              + toolVersion
              + ". Please "
              + "ensure you have properly defined the properties 'dotnet.2.0.sdk.directory' or 'dotnet.3.5.sdk.directory'");
    }
    return executable;
  }

  /**
   * Gets all the build configurations configured.
   * 
   * @return a non <code>null</code> list of build configurations
   */
  protected List<String> getBuildConfigurations() {
    StringTokenizer tokenizer = new StringTokenizer(buildConfigurations, ",");
    List<String> result = new ArrayList<String>();
    // Extracts all the configurations
    while (tokenizer.hasMoreTokens()) {
      String config = tokenizer.nextToken();
      result.add(config.trim());
    }
    return result;
  }
}
