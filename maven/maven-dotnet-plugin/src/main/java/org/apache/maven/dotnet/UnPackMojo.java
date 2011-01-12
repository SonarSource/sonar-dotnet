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

package org.apache.maven.dotnet;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.PrintStream;
import java.util.Date;
import java.util.Iterator;
import java.util.LinkedHashSet;
import java.util.Set;

import org.apache.maven.artifact.Artifact;
import org.apache.maven.artifact.repository.ArtifactRepository;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.apache.maven.plugin.MojoExecutionException;
import org.apache.maven.plugin.MojoFailureException;
import org.codehaus.plexus.archiver.ArchiverException;
import org.codehaus.plexus.archiver.zip.ZipUnArchiver;
import org.codehaus.plexus.logging.Logger;
import org.codehaus.plexus.logging.console.ConsoleLogger;
import org.codehaus.plexus.util.DirectoryWalkListener;
import org.codehaus.plexus.util.DirectoryWalker;
import org.codehaus.plexus.util.FileUtils;

/**
 * Download dependencies and unpack format SLN (zip)
 * 
 * @goal unpack
 * @requiresDependencyResolution
 * @phase unpack
 * @description package a .Net project or solution
 * @author Mathias COCHERIL Jun 23, 2010
 */
public class UnPackMojo extends AbstractDotNetMojo {

  final static int BIG_ARCHIVE_SIZE = (1 << 20) * 10; // 10Mb
  final static String LOCAL_DIR_NAME = "local",
  DOTNET_DIR_NAME = "dotnet",
  PROP_PREFIX = DOTNET_DIR_NAME + ".",
  PROP_UNPACK_ROOT = PROP_PREFIX + "unpack.root",
  PROP_OFFSET = PROP_PREFIX + "offset",
  DOTNET_DUMP_CSPROJ_VARS = PROP_PREFIX + "asp.file";

  private String unPackRoot;

  private String offset;

  static Set<String> dllDirs = new LinkedHashSet<String>();

  ConsoleLogger consolLogger = new ConsoleLogger(Logger.LEVEL_ERROR, "console");

  /**
   * @parameter default-value="${localRepository}"
   * @required
   */
  private ArtifactRepository localRepository;  // NOSONAR

  @Override
  protected void executeProject(VisualStudioProject visualProject) throws MojoExecutionException, MojoFailureException {
  }

  @Override
  protected void executeSolution(VisualStudioSolution visualSolution) throws MojoExecutionException, MojoFailureException {
  }

  /**
   * Launches the MOJO action. <br/>
   * This method downloads pom.xml and packages dependencies and build AssemblySearchPaths file
   * 
   * @throws MojoExecutionException
   * @throws MojoFailureException
   */
  @Override
  public void execute() throws MojoExecutionException, MojoFailureException {

    dllDirs.clear();

    unPackRoot = getProperty(PROP_UNPACK_ROOT);
    if (unPackRoot == null || unPackRoot.length() == 0) {
      unPackRoot = localRepository.getBasedir() + File.separator + DOTNET_DIR_NAME + File.separator + LOCAL_DIR_NAME;
    }

    offset = getProperty(PROP_OFFSET);
    if (offset == null) {
      offset = "";
    }

    Iterator<Artifact> it = this.project.getArtifacts().iterator();
    while (it.hasNext()) {
      Artifact artifact = it.next();

      File packagePath = new File(unPackRoot + File.separator + localRepository.pathOf(artifact));
      if ( !packagePath.getParentFile().exists()) {
        unZipPackage(artifact, localRepository.pathOf(artifact));
      } else {
        getLog().info("UpToDate " + packagePath.getParentFile().getPath());

        WatchedDirectoryWalkListener watchedDirectoryWalkListener = new WatchedDirectoryWalkListener(dllDirs, offset);

        DirectoryWalker dw = new DirectoryWalker();
        dw.setBaseDir(new File(packagePath.getParentFile().getPath()));
        dw.addDirectoryWalkListener(watchedDirectoryWalkListener);

        try {
          dw.scan();
        } catch (IllegalStateException e) {
          throw new MojoExecutionException(packagePath.getParentFile().getPath() + " : " + e.getMessage(), e);
        }
      }
    }

    String csprojvars = getProperty(DOTNET_DUMP_CSPROJ_VARS);
    if (csprojvars != null) {
      getLog().info("writing variables into file " + csprojvars);

      ByteArrayOutputStream baOut = new ByteArrayOutputStream();
      PrintStream psOut = new PrintStream(baOut);

      psOut.println("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
      psOut.println("  <Project ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
      psOut.println("  <!-- Generated by DOTNET PLUGIN !-->");
      psOut.println("  <PropertyGroup>");
      psOut.println("  <AssemblySearchPaths>");
      psOut.print("  $(AssemblySearchPaths)");

      Iterator<String> iter = dllDirs.iterator();
      while (iter.hasNext()) {
        psOut.println(";");
        psOut.print("  " + iter.next());
      }
      psOut.println("");
      psOut.println("  </AssemblySearchPaths>");
      psOut.println("  </PropertyGroup>");
      psOut.println("</Project>");

      FileOutputStream fileOutputStreamOrder = null;
      try {
        fileOutputStreamOrder = new FileOutputStream(new File(csprojvars));
        fileOutputStreamOrder.write(baOut.toByteArray());
      } catch (FileNotFoundException e) {
        throw new MojoExecutionException("Troubles during writing variables", e);
      } catch (IOException e) {
        throw new MojoExecutionException("Troubles during writing variables", e);
      } finally {
        if (fileOutputStreamOrder != null) {
          try {
            fileOutputStreamOrder.close();
          } catch (IOException e) {
            getLog().info(e);
          }
        }
      }
    }
  }

  /**
   * Unzip sln package.
   * 
   * @param artifact
   *          current artifact
   * @param packagePath
   *          path where to unzip the package
   */
  protected void unZipPackage(Artifact artifact, String packagePath) {
    if (artifact.getFile().exists()) {
      {
        if (artifact.getFile().length() > BIG_ARCHIVE_SIZE) {
          getLog().info("  " + artifact.getFile().getName() + " is a big archive. Uncompressing may take a while...");
        }

        try {
          WatchedZipUnArchiver zip = new WatchedZipUnArchiver(dllDirs, offset);
          zip.setSourceFile(artifact.getFile());
          zip.enableLogging(consolLogger);
          if (unPackRoot != null) {
            File unPackRootFile = (new File(unPackRoot + File.separator + packagePath)).getParentFile();
            unPackRootFile.mkdirs();
            zip.setDestDirectory(unPackRootFile);
          } else {
            zip.setDestDirectory(artifact.getFile().getParentFile());
          }

          zip.extract();

          getLog().info("Unarchived " + artifact.getFile().length() + " bytes");
        } catch (ArchiverException e) {
          getLog().warn("Could not untar file "+ artifact.getFile(), e);
        } catch (IOException e) {
          getLog().warn("Could not untar file "+artifact.getFile(),e);
        }
      }
    } else {
      getLog().info("Nothing to untar !" + artifact.getFile());
    }
  }

  private class WatchedZipUnArchiver extends ZipUnArchiver {

    Set<String> dllDirs;
    String offset;

    WatchedZipUnArchiver(Set<String> dllDirs, String offset) {
      this.dllDirs = dllDirs;
      this.offset = offset;
    }

    /**
     * Check for dll in package.
     * 
     */
    @Override
    protected void extractFile(File srcF, File dir, InputStream compressedInputStream, String entryName, Date entryDate, boolean isDirectory) throws IOException {
      super.extractFile(srcF, dir, compressedInputStream, entryName, entryDate, isDirectory);
      File file = FileUtils.resolveFile(dir, entryName);
      if (file.isFile() && file.getPath().startsWith(this.getDestDirectory().getPath() + File.separator + offset)
          && file.getName().endsWith(".dll")) {
        getLog().info("Watched : " + file.getAbsolutePath());
        dllDirs.add(file.getParent());
      }
    }
  }

  private class WatchedDirectoryWalkListener implements DirectoryWalkListener {

    Set<String> dllDirs;
    String offset;

    WatchedDirectoryWalkListener(Set<String> dllDirs, String offset) {
      this.dllDirs = dllDirs;
      this.offset = offset;
    }

    public void directoryWalkStarting(File baseDir) {
      this.baseDir = baseDir.getPath();
    }

    /**
     * Check for dll on file system.
     * 
     */
    public void directoryWalkStep(int percentage, File file) {
      if (file.isFile() && file.getPath().startsWith(this.baseDir + File.separator + offset) && file.getName().endsWith(".dll")) {
        getLog().info("Watched : " + file.getAbsolutePath());
        dllDirs.add(file.getParent());
      }
    }

    public void directoryWalkFinished() {
    }

    private String baseDir;

    @Override
    public void debug(String arg0) {
      getLog().debug(arg0);
    }
  }
}
