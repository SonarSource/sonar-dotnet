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

package org.apache.maven.dotnet;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.LineNumberReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.plugin.AbstractMojo;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.apache.maven.plugin.logging.Log;
import org.apache.maven.project.MavenProject;
import org.codehaus.plexus.util.SelectorUtils;
import org.codehaus.plexus.util.StringUtils;
import org.codehaus.plexus.util.cli.CommandLineException;
import org.codehaus.plexus.util.cli.CommandLineUtils;
import org.codehaus.plexus.util.cli.Commandline;
import org.codehaus.plexus.util.cli.StreamConsumer;

/**
 * A utilitary class to factor some features for the DotNet relative mojos.
 * 
 * @author Jose CHILLAN Apr 14, 2009
 */
public abstract class AbstractDotNetMojo
  extends AbstractMojo
{
  /**
   * Name of the file that contains the names of the files to export in a resource folder
   */
  private final static String   CONTENT_FILE_NAME  = "content.txt";

  /**
   * A utility empty array.
   */
  private static final String[] EMPTY_STRING_ARRAY = new String[0];

  /**
   * The name of the solution to use in case there are multiple solutions in the folder AND none has the same name as the artifact with a
   * ".sln" extension.
   * 
   * @parameter expression="${visual.studio.solution}"
   */
  protected String              solutionName;

  /**
   * The name of the project to use if it doesn't have the same name as the artifact with a ".csproj" extension.
   * 
   * @parameter expression="${visual.studio.project}"
   */
  protected String              projectName;

  /**
   * A pattern that will help to figure out if a project is a test project from its name.
   * The '*' and '?' common jokers are accepted
   * 
   * @parameter expression="${visual.test.project.pattern}" default-value="*.Tests"
   */
  protected String              testProjectPattern;

  /**
   * Version of dotnet to use. Accepted values are '2.0', '3.0' and '3.5'.
   * 
   * @parameter alias="${dotNetVersion}" default-value="2.0"
   */
  protected String              dotNetVersion;

  /**
   * Defines if the build should generate debug symbols (typically .pdb files)
   * @parameter expression="${maven.compiler.debug}" default-value="true"
   */
  protected boolean             debug;

  /**
   * Defines if the plugin can use the maven-dotnet-runtime artifact to export the
   * relevant .Net quality applications instead of using their global path. The defined
   * path is always taken in priority.
   * @parameter expression="${dotnet.use.embedded.runtime}" default-value="false"
   */
  protected boolean             useEmbbededRuntime;

  /**
   * Location of the output files
   * 
   * @parameter expression="${project.build.directory}"
   * @required
   */
  protected File                outputDirectory;

  /**
   * The maven project.
   * 
   * @parameter expression="${project}"
   * @required
   */
  protected MavenProject        project;

  /**
   * Defines if the launched commands should be appended into a "command.txt" file. For debug purpose.
   * 
   * @parameter expression="${trace.commands}"
   */
  protected boolean             traceCommands      = false;

  /**
   * Constructs a @link{AbstractDotNetMojo}.
   */
  public AbstractDotNetMojo()
  {
  }

  /**
   * Launches the MOJO action. <br/>This method checks whether the pom.xml is for a project or a solution and launches the adequate method
   * {@link #executeProject(VisualStudioProject)} or {@link #executeSolution(VisualStudioSolution)}.
   * 
   * @throws MojoExecutionException
   * @throws MojoFailureException
   */
  @Override
  public void execute() throws MojoExecutionException, MojoFailureException
  {
    if (!checkExecutionAllowed())
    {
      return;
    }

    // Here we should add : if the project is a "sln" packaging, then launch for the solution
    if (VisualStudioUtils.SOLUTION_PACKAGING.equals(project.getPackaging()))
    {
      VisualStudioSolution visualSolution = null;
      try
      {
        visualSolution = VisualStudioUtils.getVisualSolution(project, solutionName);
      }
      catch (DotNetProjectException e)
      {
        throw new MojoExecutionException("The solution for project "
                                         + project.getArtifactId()
                                         + " is not a"
                                         + "properly configured Visual Studio solution.\nPlease ensure you have a '.sln' file in "
                                         + project.getBasedir(), e);

      }
      if (visualSolution == null)
      {
        throw new MojoExecutionException("The solution for project "
                                         + project.getArtifactId()
                                         + " is not a"
                                         + "properly configured Visual Studio solution.\nPlease ensure you have a '.sln' file in "
                                         + project.getBasedir());
      }
      executeSolution(visualSolution);
      return;
    }

    VisualStudioProject visualProject = getVisualProject();
    if (visualProject == null)
    {
      getLog().info("The project " + project.getArtifactId() + " is not a visual studio project");
      return;
    }
    executeProject(visualProject);
  }

  /**
   * Launches the action in case the project is a VisualStudio project
   * 
   * @param visualProject
   */
  protected abstract void executeProject(VisualStudioProject visualProject) throws MojoExecutionException, MojoFailureException;

  /**
   * Launches the action in case the project is a VisualStudio solution
   * 
   * @param visualSolution
   */
  protected abstract void executeSolution(VisualStudioSolution visualSolution) throws MojoExecutionException, MojoFailureException;

  /**
   * Gets the Visual Studio project associated to the current project.
   * 
   * @return the project, or <code>null</code> if the project is not a Visual Studio project
   * @throws MojoExecutionException if the current pom.xml correspond to a badly defined project
   */
  public VisualStudioProject getVisualProject() throws MojoExecutionException
  {
    File projectFile;
    try
    {
      projectFile = VisualStudioUtils.getVisualFile(project, projectName, ".csproj", "project");
    }
    catch (DotNetProjectException except)
    {
      throw new MojoExecutionException("Could not create a VisualStudio project", except);
    }
    // No solution defined
    if (projectFile == null)
    {
      getLog().debug("The project " + project.getName() + " is not a Visual Studio project");
      return null;
    }
    try
    {
      VisualStudioProject visualProject = VisualStudioUtils.getProject(projectFile);
      assessTestProject(visualProject);
      return visualProject;
    }
    catch (Exception e)
    {
      throw new MojoExecutionException("Could not extract the project information for " + projectFile, e);
    }
  }

  /**
   * Gets the solution corresponding to the current project if applicable.
   * 
   * @return the generated solution, or <code>null</code> if the project is not a solution
   */
  protected VisualStudioSolution getVisualSolution() throws MojoExecutionException
  {
    File basedir = project.getBasedir();
    File solutionFile;
    try
    {
      solutionFile = VisualStudioUtils.getVisualFile(project, solutionName, ".sln", "solution");
    }
    catch (DotNetProjectException except)
    {
      throw new MojoExecutionException("Could not create a VisualStudio solution", except);
    }

    // No solution defined
    if (solutionFile == null)
    {
      getLog().debug("The project " + project.getName() + " is not a Visual Studio solution");
      return null;
    }
    // We define the solution file
    solutionName = solutionFile.getName();
    // We try to build the solution
    VisualStudioSolution solution;
    try
    {
      solution = VisualStudioUtils.getSolution(basedir, solutionName);
    }
    catch (Exception e)
    {
      throw new MojoExecutionException("Could not extract the solution information for " + solutionFile, e);
    }
    List<VisualStudioProject> projects = solution.getProjects();
    // We define for each project if it is a test project
    for (VisualStudioProject visualStudioProject : projects)
    {
      assessTestProject(visualStudioProject);
    }
    return solution;
  }

  /**
   * @param visualStudioProject
   */
  private void assessTestProject(VisualStudioProject visualStudioProject)
  {
    String assemblyName = visualStudioProject.getAssemblyName();
    if (SelectorUtils.match(testProjectPattern, assemblyName))
    {
      getLog().debug("The project " + visualStudioProject.getName() + " has been qualified as a test project");
      visualStudioProject.setTest(true);
    }
  }

  /**
   * Gets the generated assembly according to the run configuration
   * 
   * @param visualProject the visual project
   * @return the generated assembly
   * @throws MojoFailureException
   */
  protected File getGeneratedAssembly(VisualStudioProject visualProject) throws MojoFailureException
  {
    File assembly;
    if (debug)
    {
      assembly = visualProject.getDebugArtifact();
    }
    else
    {
      assembly = visualProject.getReleaseArtifact();
    }

    return assembly;
  }

  /**
   * Deletes a set of file before generation
   * 
   * @param files the files to delete
   */
  protected void deleteFiles(File... files)
  {
    for (File file : files)
    {
      if (file.exists())
      {
        file.delete();
      }
    }
  }

  /**
   * Ensures that the current running system is a Windows version
   * 
   * @throws MojoExecutionException if the system is not windows
   */
  protected void ensureWindowsSystem() throws MojoExecutionException
  {
    String osName = System.getProperty("os.name").toLowerCase();
    if (!osName.contains("windows"))
    {
      throw new MojoExecutionException("The task must be launched on a Windows system");
    }
  }

  /**
   * Ensure that the java version is at least 1.6
   * 
   * @throws MojoExecutionException
   */
  protected void ensureJavaVersion() throws MojoExecutionException
  {
    String version = System.getProperty("java.version");
    int versionValue = Integer.parseInt(new StringBuilder().append(version.charAt(0)).append(version.charAt(2)).toString());
    if (versionValue < 16)
    {
      throw new MojoExecutionException("The C# reporting plugin requires to be run in at least the JDK 6.0");
    }
  }

  /**
   * Launches a command line, redirecting the stream to the maven logs.
   * 
   * @param reportType a display type for the report
   * @param acceptedMask a mask for the exit code of the command that is accepted (put 0 if you don't know)
   * @param redirectLogsToDebug <code>true</code> if the logs of the executable have to be directed to the debug. This is useful if the executable is
   * verbose, to avoid to log useless messages in case of success.
   * @param commandline the command ready to be executed
   * @param redirectLogsToDebug <code>true</code> if the log of the command should be put in debug. This is useful for the case of verbose executables
   * @return the status of the command after execution
   * @throws MojoExecutionException if the execution command could not be launched for any reason
   * @throws MojoFailureException if the command status after execution is not satisfying
   */
  protected int launchCommand(File executable, List<String> arguments, String reportType, int acceptedMask, boolean redirectLogsToDebug)
                                                                                                                             throws MojoExecutionException,
                                                                                                                             MojoFailureException
  {
    Commandline commandline = generateCommandLine(executable, arguments);
    return launchCommand(commandline, reportType, acceptedMask, redirectLogsToDebug, true);
  }

  /**
   * Launches a command line, redirecting the stream to the maven logs
   * 
   * @param reportType a display type for the report
   * @param acceptedMask a mask for the exit code of the command that is accepted (put 0 if you don't know)
   * @param redirectLogsToDebug <code>true</code> if the logs of the executable have to be directed to the debug. This is useful if the executable is
   * verbose, to avoid to log useless messages in case of success.
   * @param commandline the command ready to be executed
   * @param redirectLogsToDebug <code>true</code> if the log of the command should be put in debug. This is useful for the case of verbose executables
   * @param throwsFailure <code>true</code> if the method should throw an exception in case of failure
   * @return the status of the command after execution
   * @throws MojoExecutionException if the execution command could not be launched for any reason
   * @throws MojoFailureException if the command status after execution is not satisfying and the throwsFailure parameter is not false
   */
  protected int launchCommand(Commandline commandline, String reportType, int acceptedMask, boolean redirectLogsToDebug,
                              boolean throwsFailure) throws MojoExecutionException, MojoFailureException
  {
    int commandLineResult;
    Log log = getLog();
    String[] commandLineElements = commandline.getCommandline();
    
    CommandStreamConsumer systemOut = new CommandStreamConsumer(log, redirectLogsToDebug);

    try
    {
      // Execute the commandline
      log.debug("Executing command: " + commandline);
      log.debug("Command elements :" + Arrays.toString(commandLineElements));
      ProcessBuilder builder = new ProcessBuilder(Arrays.asList(commandLineElements));
      builder.redirectErrorStream(true);
      if (traceCommands)
      {
        try
        {
          File commandFile = getReportFile("command.txt");
          OutputStream stream = new FileOutputStream(commandFile, true);
          PrintWriter writer = new PrintWriter(stream);
          writer.println("Mojo : " + reportType + " on " + new Timestamp(System.currentTimeMillis()));
          writer.println(commandline);
          writer.println();
          writer.close();
        }
        catch (FileNotFoundException e)
        {
          // Nothing : commands are not logged
        }
      }
      commandLineResult = CommandLineUtils.executeCommandLine(commandline, systemOut, systemOut);

      // Check if nunit-console is not in the path
      if (systemOut.isCommandNotFound())
      {
        throw new MojoExecutionException("Please add the executable for " + reportType + " to your path");
      }
      else if ((commandLineResult & (~acceptedMask)) != 0)
      {
        // If the debug level was not enabled, and the logs were redirected to debug,
        // we display them in case of error
        if (!log.isDebugEnabled() && redirectLogsToDebug)
        {
          log.warn("FAILURE !!!");
          log.warn("Launched command : " + commandline);
          log.warn("");
          log.warn(systemOut.getContent());
        }
        // We throw the exception only if asked for
        if (throwsFailure)
        {
          throw new MojoFailureException("Failure during the " + reportType + " generation that ended with status=" + commandLineResult);
        }
      }
    }
    catch (CommandLineException e)
    {
      throw new MojoExecutionException("Failure executing commandline, " + e.getMessage());
    }
    return commandLineResult;
  }

  /**
   * Generates a command line with the arguments.
   * @param executable
   * @param arguments
   * @return
   */
  protected Commandline generateCommandLine(File executable, List<String> arguments)
  {
    Commandline commandline = new Java5CommandLine();

    // We create the work directory if necessary
    if (!outputDirectory.exists())
    {
      outputDirectory.mkdirs();
    }

    commandline.setWorkingDirectory(outputDirectory.toString());
    commandline.setExecutable(executable.toString());
    commandline.addArguments(arguments.toArray(EMPTY_STRING_ARRAY));
    return commandline;
  }

  /**
   * Exports a resource folder to a subdirectory of the maven build directory. <br/>The folder is supposed to contain a "context.txt" file
   * that contains the list of the files to export, one name per line
   * 
   * @param resourceDir the resource directory to export
   * @param destinationSubFolder the subfolder to use, that will be created under ${project.build.dir}
   * @param application the exported application
   * @return the generated folder
   * @throws MojoExecutionException
   */
  protected File extractFolder(String resourceDir, String destinationSubFolder, String application) throws MojoExecutionException
  {
    if (useEmbbededRuntime == false)
    {
      getLog().warn("The use of the embedded runtime package is not enabled. Please add the settings 'dotnet.use.embedded.runtime=true'");
      throw new MojoExecutionException("The use of the embedded runtime package is not enabled for " + application);
    }
    getLog().debug("Exporting files for " + application);
    String contentFile = resourceDir + "/" + CONTENT_FILE_NAME;
    InputStream contentResource = getClassLoader().getResourceAsStream(contentFile);
    LineNumberReader reader = new LineNumberReader(new InputStreamReader(contentResource));
    String line = null;
    List<String> contentFiles = new ArrayList<String>();
    try
    {
      while ((line = reader.readLine()) != null)
      {
        contentFiles.add(line.trim());
      }
    }
    catch (IOException e)
    {
      throw new MojoExecutionException("Could not extract the files for " + application, e);
    }
    File reportDirectory = getReportDirectory();
    File extractFolder = new File(reportDirectory, destinationSubFolder);
    extractResources(extractFolder, resourceDir, contentFiles, application);
    return extractFolder;
  }

  /**
   * Extracts the resources to a specified directory.
   * 
   * @param destinationDirectory the directory to which the files will be extracted
   * @param resourceDirectory the resource directory from which they are extracted
   * @param resourceNames the name of the resource files to extract
   * @param application the name of the application for debug purpose
   * @throws MojoExecutionException if a file is missing or the folder could not be written to
   */
  protected void extractResources(File destinationDirectory, String resourceDirectory, List<String> resourceNames, String application)
                                                                                                                                      throws MojoExecutionException
  {
    if (!destinationDirectory.exists())
    {
      destinationDirectory.mkdirs();
    }
    for (String resource : resourceNames)
    {
      extractResource(destinationDirectory, resourceDirectory + "/" + resource, resource, application);
    }
  }

  /**
   * Extracts a resource file from the plugin jar to a given destination folder.
   * 
   * @param exportDirectory the destination folder
   * @param resourcePath the full path to the extracted resource
   * @param fileName the name of the file after export (usually the terminal part of resourcePath)
   * @param application the name of the application for debug purpose
   * @return the exported file
   * @throws MojoExecutionException if the resource is not found or the file could no be written
   */
  protected File extractResource(File exportDirectory, String resourcePath, String fileName, String application)
                                                                                                                throws MojoExecutionException
  {
    File exportedFile = new File(exportDirectory, fileName);

    // First we create the parent folder if necessary
    // NOTE here that as the resource may be far in the subtree,
    // the parent is not necessary the export directory
    File parentDirectory = exportedFile.getParentFile();
    if (!parentDirectory.exists())
    {
      getLog().debug("Creating parent export directory : " + parentDirectory);
      parentDirectory.mkdirs();
    }

    // We delete the file to replace it
    if (exportedFile.exists())
    {
      exportedFile.delete();
    }
    // Exports the file
    try
    {
      exportedFile.createNewFile();
      InputStream fileStream = getClassLoader().getResourceAsStream(resourcePath);
      OutputStream out = new FileOutputStream(exportedFile);
      long length = 0;
      byte buf[] = new byte[1024];
      int len;
      // We write all the bytes by block
      while ((len = fileStream.read(buf)) > 0)
      {
        out.write(buf, 0, len);
        length += len;
      }
      out.close();
      fileStream.close();
      getLog().debug("Exported file " + exportedFile + " : " + length + " bytes");
    }
    catch (Exception e)
    {
      // Something went wrong...
      throw new MojoExecutionException("A problem occurred for "
                                       + application
                                       + " while extracting file "
                                       + fileName
                                       + " to "
                                       + exportedFile, e);
    }
    return exportedFile;
  }

  /**
   * Gets a file for a report whose name is given
   * 
   * @param fileName the name of the file
   * @return the file to generate
   */
  protected File getReportFile(String fileName)
  {
    File reportDirectory = getReportDirectory();
    return new File(reportDirectory, fileName);
  }

  /**
   * Gets a file for a report whose name is given
   * 
   * @param fileName the name of the file
   * @return the file to generate
   */
  protected File getReportFile(String fileName, String defaultFileName)
  {
    File reportDirectory = getReportDirectory();
    if (!StringUtils.isEmpty(fileName))
    {
      // We use the file name
      return new File(reportDirectory, fileName);
    }
    return new File(reportDirectory, defaultFileName);
  }

  /**
   * Escapes a file if necessary for command generation.
   * 
   * @param file the file to escape
   * @return the escapes file name
   */
  protected String toCommandPath(File file)
  {
    String absolutePath;
    try
    {
      absolutePath = file.getCanonicalPath();
    }
    catch (IOException e)
    {
      // We try another way of processing
      absolutePath = file.getAbsolutePath();
    }
    return toCommandPath(absolutePath);
  }

  /**
   * Converts a path to a command compatible path, by escaping with quotes if necessary.
   * 
   * @param path
   * @return
   */
  protected String toCommandPath(String path)
  {
    if (path.indexOf(' ') >= 0)
    {
      return '"' + path + '"';
    }
    return path;
  }

  /**
   * Gets the currenet class loader
   * 
   * @return
   */
  protected ClassLoader getClassLoader()
  {
    return Thread.currentThread().getContextClassLoader();
  }

  /**
   * Gets the report directory to use.
   * 
   * @return the report directory
   */
  protected File getReportDirectory()
  {
    String buildPath = project.getBuild().getDirectory();
    File reportDirectory = new File(buildPath);
    reportDirectory.mkdirs();
    return reportDirectory;
  }

  /**
   * Ensures that the execution of the Mojo is allowed. <br/>The current implementation checks that the execution is launched on a Windows
   * system and uses at least Java 6.0. This method may be overridden but should better invoke the super.
   * 
   * @return <code>true</code> if the execution is allowed
   * @throws MojoExecutionException
   */
  protected boolean checkExecutionAllowed() throws MojoExecutionException
  {
    ensureWindowsSystem();
    ensureJavaVersion();
    return true;
  }

  /**
   * A consumer for command outputs.
   * 
   * @author Jose CHILLAN Jul 30, 2009
   */
  private static class CommandStreamConsumer
    implements StreamConsumer
  {
    // TODO: This probably only works on Windows machines
    private static final String COMMAND_NOT_FOUND_FRAGMENT = "is not recognized as an internal or external command";

    private boolean             commandNotFound;
    private boolean             debug;
    private StringBuilder       consumedLines              = new StringBuilder();

    private Log                 log;

    private CommandStreamConsumer(Log log, boolean debug)
    {
      this.log = log;
      this.debug = debug;
    }

    /**
     * Callback each time a line is consumed.
     * 
     * @param line
     */
    @Override
    public void consumeLine(String line)
    {
      consumedLines.append(line + "\n");

      if (debug)
      {
        log.debug(line);
      }
      else
      {
        log.info(line);
      }
      if (line.contains(COMMAND_NOT_FOUND_FRAGMENT))
      {
        commandNotFound = true;
      }
    }

    /**
     * Checks if the command was not found.
     * 
     * @return <code>true</code> if the command was not found
     */
    public boolean isCommandNotFound()
    {
      return commandNotFound;
    }

    /**
     * Gets the content of the stream.
     * 
     * @return the stream content
     */
    public CharSequence getContent()
    {
      return consumedLines;
    }
  }
}