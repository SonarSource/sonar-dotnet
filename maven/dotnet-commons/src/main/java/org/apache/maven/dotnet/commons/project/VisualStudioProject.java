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
 * Created on Apr 16, 2009
 */
package org.apache.maven.dotnet.commons.project;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collection;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * A dot net project extracted from a solution
 * 
 * @author Jose CHILLAN Apr 16, 2009
 */
public class VisualStudioProject {
  private final static Logger log = LoggerFactory
      .getLogger(VisualStudioProject.class);

  private String name;
  private File projectFile;
  private ArtifactType type;
  private String assemblyName;
  private String realAssemblyName; // assembly name found in the csproj file no matter what
  private String rootNamespace;
  protected File debugOutputDir;
  protected File releaseOutputDir;
  protected File directory;
  private boolean test;
  private boolean silverlightProject;
  private Map<File, SourceFile> sourceFileMap;
  
  private List<BinaryReference> binaryReferences = new ArrayList<BinaryReference>();

  /**
   * Builds a {@link VisualStudioProject} ...
   * 
   * @param name
   * @param projectFile
   */
  public VisualStudioProject() {
    super();
  }

  /**
   * Gets the relative path of a file contained in the project. <br>
   * For example, if the visual studio project is
   * C:/MySolution/MyProject/MyProject.csProj and the file is
   * C:/MySolution/MyProject/Dummy/Foo.cs, then the result is Dummy/Foo.cs
   * 
   * @param file
   *          the file whose relative path is to be computed
   * @return the relative path, or <code>null</code> if the file is not in the
   *         project subdirectories
   */
  public String getRelativePath(File file) {
    File canonicalDirectory;
    try {
      canonicalDirectory = directory.getCanonicalFile();

      File canonicalFile = file.getCanonicalFile();

      String filePath = canonicalFile.getPath();
      String directoryPath = canonicalDirectory.getPath();
      if (!filePath.startsWith(directoryPath)) {
        // The file is not in the directory
        return null;
      }
      String folder = StringUtils.removeStart(
          StringUtils.removeStart(filePath, directoryPath), "\\");
      return folder;
    } catch (IOException e) {
      if (log.isDebugEnabled()) {
        log.debug("io exception with file "+file, e);
      }
      return null;
    }
  }

  /**
   * Returns the name.
   * 
   * @return The name to return.
   */
  public String getName() {
    return this.name;
  }

  /**
   * Returns the projectFile.
   * 
   * @return The projectFile to return.
   */
  public File getProjectFile() {
    return this.projectFile;
  }

  /**
   * Returns the type.
   * 
   * @return The type to return.
   */
  public ArtifactType getType() {
    return this.type;
  }

  /**
   * Gets the debug generated artifact.
   * 
   * @return
   */
  public File getDebugArtifact() {
    String generatedFileName = getArtifactName();
    File result = new File(debugOutputDir, generatedFileName);
    return result;
  }

  /**
   * Gets the release generated artifact.
   * 
   * @return
   */
  public File getReleaseArtifact() {
    String generatedFileName = getArtifactName();
    File result = new File(releaseOutputDir, generatedFileName);
    return result;
  }

  /**
   * Gets the name of the artifact.
   * 
   * @return
   */
  public String getArtifactName() {
    String generatedFileName = realAssemblyName + "." + getExtension();
    return generatedFileName;
  }

  @Override
  public String toString() {
    return "Project(name=" + name + ", type=" + type + ", test=" + test
        + ", directory=" + directory + ", file=" + projectFile
        + ", assemblyName=" + assemblyName + ", rootNamespace=" + rootNamespace
        + ", debugDir=" + debugOutputDir + ", releaseDir=" + releaseOutputDir
        + ")";
  }

  /**
   * Returns the assemblyName.
   * 
   * @return The assemblyName to return.
   */
  public String getAssemblyName() {
    return this.assemblyName;
  }

  /**
   * Sets the assemblyName.
   * 
   * @param assemblyName
   *          The assemblyName to set.
   */
  public void setAssemblyName(String assemblyName) {
    this.assemblyName = assemblyName;
    if (StringUtils.isEmpty(realAssemblyName)) {
      realAssemblyName = assemblyName;
    }
  }

  /**
   * Returns the rootNamespace.
   * 
   * @return The rootNamespace to return.
   */
  public String getRootNamespace() {
    return this.rootNamespace;
  }

  /**
   * Sets the rootNamespace.
   * 
   * @param rootNamespace
   *          The rootNamespace to set.
   */
  public void setRootNamespace(String rootNamespace) {
    this.rootNamespace = rootNamespace;
  }

  /**
   * Returns the debugOutputDir.
   * 
   * @return The debugOutputDir to return.
   */
  public File getDebugOutputDir() {
    return this.debugOutputDir;
  }

  /**
   * Sets the debugOutputDir.
   * 
   * @param debugOutputDir
   *          The debugOutputDir to set.
   */
  public void setDebugOutputDir(File debugOutputDir) {
    this.debugOutputDir = debugOutputDir;
  }

  /**
   * Returns the releaseOutputDir.
   * 
   * @return The releaseOutputDir to return.
   */
  public File getReleaseOutputDir() {
    return this.releaseOutputDir;
  }

  /**
   * Sets the releaseOutputDir.
   * 
   * @param releaseOutputDir
   *          The releaseOutputDir to set.
   */
  public void setReleaseOutputDir(File releaseOutputDir) {
    this.releaseOutputDir = releaseOutputDir;
  }

  /**
   * Returns the directory.
   * 
   * @return The directory to return.
   */
  public File getDirectory() {
    return this.directory;
  }

  /**
   * Sets the root directory of the project.
   * For a regular project, this is where is 
   * located the csproj file. 
   * 
   * @param directory
   *          The directory to set.
   */
  public void setDirectory(File directory) {
    try {
      this.directory = directory.getCanonicalFile();
    } catch (IOException e) {
      log.warn("Invalid project directory : " + directory);
    }
  }

  /**
   * Sets the name.
   * 
   * @param name
   *          The name to set.
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * Sets the projectFile.
   * 
   * @param projectFile
   *          The projectFile to set.
   */
  public void setProjectFile(File projectFile) {
    this.projectFile = projectFile;
  }

  /**
   * Sets the type.
   * 
   * @param type
   *          The type to set.
   */
  public void setType(ArtifactType type) {
    this.type = type;
  }

  /**
   * Returns the test.
   * 
   * @return The test to return.
   */
  public boolean isTest() {
    return this.test;
  }

  /**
   * Sets the test.
   * 
   * @param test
   *          The test to set.
   */
  public void setTest(boolean test) {
    this.test = test;
    for (BinaryReference reference : binaryReferences) {
      String scope = test ? "test" : "compile";
      reference.setScope(scope);
    }
  }

  /**
   * Gets the artifact extension (.dll or .exe)
   * 
   * @return the extension
   */
  public String getExtension() {
    switch (type) {
    case EXECUTABLE:
      return "exe";
    case LIBRARY: case WEB:
      return "dll";
    }
    return null;
  }

  /**
   * Gets all the files contained in the project
   * 
   * @return
   */
  public Collection<SourceFile> getSourceFiles() {
    if (sourceFileMap == null) {
      initializeSourceFileMap();
    }
    return sourceFileMap.values();
  }

  private void initializeSourceFileMap() {
    Map<File, SourceFile> allFiles = new LinkedHashMap<File, SourceFile>(); // Case
                                                                            // of
                                                                            // a
                                                                            // regular
                                                                            // project
    if (projectFile != null) {
      List<String> filesPath = VisualStudioUtils.getFilesPath(projectFile);

      for (String filePath : filesPath) {
        try {
          // We build the file and retrieves its canonical path
          File file = new File(directory, filePath).getCanonicalFile();
          String fileName = file.getName();
          String folder = StringUtils.replace(StringUtils.removeEnd(
              StringUtils.removeEnd(filePath, fileName), "\\"), "\\", "/");
          SourceFile sourceFile = new SourceFile(this, file, folder, fileName);
          allFiles.put(file, sourceFile);
        } catch (IOException e) {
          log.error("Bad file :" + filePath, e);
        }
      }

    } else {
      // For web projects, we take all the C# files
      List<File> csharpFiles = listRecursiveFiles(directory, ".cs");
      for (File file : csharpFiles) {
        SourceFile sourceFile = new SourceFile(this, file, file.getParent(),
            file.getName());
        allFiles.put(file, sourceFile);
      }
    }
    this.sourceFileMap = allFiles;
  }

  protected Map<File, SourceFile> getSourceFileMap() {
    if (sourceFileMap == null) {
      initializeSourceFileMap();
    }
    return sourceFileMap;
  }

  /**
   * Recursively lists the files of a directory
   * 
   * @param dir
   *          the directory to list
   * @param extension
   * @return
   */
  private List<File> listRecursiveFiles(File dir, String extension) {
    List<File> result = new ArrayList<File>();
    File[] content = dir.listFiles();
    if (content != null) {
      String extensionLowCase = extension.toLowerCase();
      for (File file : content) {
        if (file.isDirectory()) {
          List<File> matching = listRecursiveFiles(file, extension);
          result.addAll(matching);
        } else {
          // Look for matching file names
          String fileName = file.getName().toLowerCase();
          if (fileName.endsWith(extensionLowCase)) {
            result.add(file);
          }
        }
      }
    }
    return result;
  }

  /**
   * Gets the source representation of a given file.
   * 
   * @param file
   *          the file to retrieve
   * @return the associated source file, or <code>null</code> if the file is not
   *         included in the assembly.
   */
  public SourceFile getFile(File file) {
    File currentFile;
    try {
      currentFile = file.getCanonicalFile();
    } catch (IOException e) {
      // File not found
      if (log.isDebugEnabled()) {
        log.debug("file not found "+file, e);
      }
      return null;
    }
    // We ensure the source files are loaded
    getSourceFiles();
    return sourceFileMap.get(currentFile);
  }

  /**
   * Test if this project is a parent directory of the given file.
   * 
   * @param file
   *          the file to check
   * @return <code>true</code> if the file is under this project
   */
  public boolean isParentDirectoryOf(File file) {
    return VisualStudioUtils.isSubDirectory(directory, file);
  }

  /**
   * Checks if the project contains a given source file.
   * 
   * @param file
   *          the file to check
   * @return <code>true</code> if the project contains the file
   */
  public boolean contains(File file) {
    try {
      File currentFile = file.getCanonicalFile();
      // We ensure the source files are loaded
      getSourceFiles();
      return sourceFileMap.containsKey(currentFile);
    } catch (IOException e) {
      log.debug("file error", e);
    }

    return false;
  }

  public boolean isWebProject() {
    return (projectFile == null);
  }

  
  /**
   * @param silverlightProject true if it is a silverlight project
   */
  public void setSilverlightProject(boolean silverlightProject) {
    this.silverlightProject = silverlightProject;
  }

  /**
   * @return true if it is a silverlight project
   */
  public boolean isSilverlightProject() {
    return silverlightProject;
  }

  public List<BinaryReference> getBinaryReferences() {
    return binaryReferences;
  }

  public void setBinaryReferences(List<BinaryReference> binaryReferences) {
    this.binaryReferences = binaryReferences;
  }

}
