/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
 * Created on Mar 25, 2010
 */
package org.apache.maven.dotnet;

import java.io.File;
import java.io.IOException;

import org.codehaus.plexus.util.cli.CommandLineException;
import org.codehaus.plexus.util.cli.Commandline;

/**
 * An updated command line that uses the Java 5+ {@link ProcessBuilder} instead of the old {@link Runtime#exec(String)} method. It avoids
 * all problems due to spaces in executable path in Windows.
 * 
 * @author Jose CHILLAN Mar 25, 2010
 */
public class Java5CommandLine extends Commandline
{
  /**
   * Executes the command.
   */
  @Override
  public Process execute() throws CommandLineException
  {
    Process process = null;

    try
    {
      File workingDir = getWorkingDirectory();
      if (workingDir == null)
      {
        ProcessBuilder builder = new ProcessBuilder(getCommandline());
        process = builder.start();
      }
      else
      {
        if (!workingDir.exists())
        {
          throw new CommandLineException("Working directory \"" + workingDir.getPath() + "\" does not exist!");
        }
        else if (!workingDir.isDirectory())
        {
          throw new CommandLineException("Path \"" + workingDir.getPath() + "\" does not specify a directory.");
        }

        ProcessBuilder builder = new ProcessBuilder(getCommandline());
        builder.directory(workingDir);
        process = builder.start();
      }
    }
    catch (IOException ex)
    {
      throw new CommandLineException("Error while executing process.", ex);
    }

    return process;
  }

}
