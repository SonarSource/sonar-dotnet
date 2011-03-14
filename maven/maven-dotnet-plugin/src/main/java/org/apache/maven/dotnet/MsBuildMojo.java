/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
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

package org.apache.maven.dotnet;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;

/**
 * Generic MsBuild mojo. Any msbuild script file with its parameters may 
 * be set up from maven, the execution may be attached to any maven phase.
 * 
 * @goal msbuild
 * @description Generic MsBuild mojo
 * 
 * @author Alexandre Victoor
 *
 */
public class MsBuildMojo extends AbstractDotNetBuildMojo {
  
  /**
   * Command line parameters for MsBuild
   * @parameter 
   */
  private List<String> parameters;
  
  /**
   * MsBuild script file to execute.
   * @parameter expression="${msbuild.script}"
   */
  private String script;
  
  /**
   * MsBuild target (optional).
   * If no target specified, the default target of the msbuild script will be executed.
   * @parameter expression="${msbuild.target}"
   */
  private String target;
  

  private void executeMsBuild() throws MojoExecutionException, MojoFailureException { 
    if (StringUtils.isEmpty(script)) {
      throw new MojoFailureException("Script parameter is mandatory");
    }
    File scriptFile = new File(script);
    if (!scriptFile.exists()) {
      throw new MojoExecutionException("Msbuild script file not found : " + scriptFile);
    }
    
    List<String> msbuildParameters = new ArrayList<String>();
    msbuildParameters.add(toCommandPath(scriptFile));
   
    if (!StringUtils.isEmpty(target)) {
      msbuildParameters.add("/t:"+target);
    }
    
    if (parameters!=null) {
      msbuildParameters.addAll(parameters);
    }
    
    File executable = getMsBuildCommand();
    
    launchCommand(executable, msbuildParameters, "build", 0, true);
  }
  
  @Override
  protected void executeProject(VisualStudioProject visualProject) throws MojoExecutionException, MojoFailureException {
    executeMsBuild();
  }

  @Override
  protected void executeSolution(VisualStudioSolution visualSolution) throws MojoExecutionException, MojoFailureException {
    executeMsBuild();
  }

}
