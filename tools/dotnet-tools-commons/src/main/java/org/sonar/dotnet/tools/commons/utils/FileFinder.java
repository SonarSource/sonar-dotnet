/*
 * .NET tools :: Commons
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
package org.sonar.dotnet.tools.commons.utils;

import java.io.File;
import java.io.FileFilter;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

import org.apache.commons.io.filefilter.DirectoryFileFilter;
import org.apache.commons.io.filefilter.IOFileFilter;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.utils.WildcardPattern;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;

/**
 * Helper class to easily find files using ant style patterns, relative or absolute paths.
 * $(SolutionDir) can be used to specify the folder where the visual studio sln file is located.
 * "../" can be used to climb in the folder tree.
 * Absolute and relative paths are both supported.
 * 
 * @author Alexandre Victoor
 * 
 */
public final class FileFinder {

  private static final String SOLUTION_DIR_KEY = "$(SolutionDir)/";
  private static final String PARENT_DIRECTORY = "../";
  private static final Logger LOG = LoggerFactory.getLogger(FileFinder.class);

  
  private FileFinder() {
  }
  
  /**
   * Find files that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analyzed by sonar
   * @param currentProject
   *          The VS project being analyzed
   * @param patterns
   *          A list of paths or ant style patterns delimited by a comma or a semi-comma
   * @return The files found on the filesystem
   */
  public static Collection<File> findFiles(VisualStudioSolution currentSolution, VisualStudioProject currentProject, String patterns) {
    String[] patternArray = StringUtils.split(patterns, ",;");
    return findFiles(currentSolution, currentProject, patternArray);
  }

  /**
   * Find directories that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analyzed by sonar
   * @param currentProject
   *          The VS project being analyzed
   * @param patternArray
   *          A list of paths or ant style patterns
   * @return The directories found on the filesystem
   */
  public static Collection<File> findDirectories(VisualStudioSolution currentSolution, VisualStudioProject currentProject,
      String... patternArray) {
    Collection<File> files = findFiles(currentSolution, currentProject, patternArray);
    Collection<File> directories = new ArrayList<File>();
    for (File file : files) {
      if (file.isDirectory()) {
        directories.add(file);
      }
    }
    return directories;
  }

  /**
   * Find files that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analyzed by sonar
   * @param currentProject
   *          The VS project being analyzed
   * @param patternArray
   *          A list of paths or ant style patterns
   * @return The files found on the filesystem
   */
  public static Collection<File> findFiles(VisualStudioSolution currentSolution, VisualStudioProject currentProject, String... patternArray) {
    return findFiles(currentSolution, currentProject.getDirectory(), patternArray);
  }
  
  /**
   * Find files that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analyzed by sonar
   * @param defaultWorkPath
   *          A working path that may be relative to solution root directory
   * @param patternArray
   *          A list of paths or ant style patterns
   * @return The files found on the filesystem
   */
  public static Collection<File> findFiles(VisualStudioSolution currentSolution, String defaultWorkPath, String... patternArray) {
    return findFiles(currentSolution, new File(currentSolution.getSolutionDir(), defaultWorkPath), patternArray);
  }
  
  /**
   * Find files that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analyzed by sonar
   * @param defaultWorkDir
   *          A working directory
   * @param patternArray
   *          A list of paths or ant style patterns
   * @return The files found on the filesystem
   */
  @SuppressWarnings("unchecked")
  public static Collection<File> findFiles(VisualStudioSolution currentSolution, File defaultWorkDir, String... patternArray) {

    if (patternArray==null || patternArray.length==0) {
      return Collections.EMPTY_LIST;
    }

    Set<File> result = new HashSet<File>();
    for (String pattern : patternArray) {
      String currentPattern = convertSlash(pattern);
      File workDir = defaultWorkDir;
      if (StringUtils.startsWith(pattern, SOLUTION_DIR_KEY)) {
        workDir = currentSolution.getSolutionDir();
        currentPattern = StringUtils.substringAfter(currentPattern, SOLUTION_DIR_KEY);
      }
      while (StringUtils.startsWith(currentPattern, PARENT_DIRECTORY)) {
        workDir = workDir.getParentFile();
        currentPattern = StringUtils.substringAfter(currentPattern, PARENT_DIRECTORY);
      }
      if (StringUtils.contains(currentPattern,'*')) {
        String prefix = StringUtils.substringBefore(currentPattern, "*");
        if (StringUtils.contains(prefix, '/')) {
          workDir = browse(workDir, prefix);
          currentPattern = "*" + StringUtils.substringAfter(currentPattern, "*");
        }
        IOFileFilter externalFilter 
          = new PatternFilter(workDir, currentPattern);
        listFiles(result, workDir, externalFilter);
        
      } else {
        result.add(browse(workDir, currentPattern));
      }
    }
    
    if (LOG.isDebugEnabled()) {
      if (result.isEmpty()) {
        LOG.warn("No file found using pattern(s) " + StringUtils.join(patternArray, ','));
      } else {
        LOG.debug("The following files have been found using pattern(s) " 
            + StringUtils.join(patternArray, ',') + "\n"
            + StringUtils.join(result, "\n  "));
      }
    }

    return result;
  }
  
  
  public static File browse(File workDir, String path) {
    if (StringUtils.isEmpty(path)) {
      return workDir;
    }
    File file = new File(path);
    if (!file.exists() || !file.isAbsolute()) {
      File currentWorkDir = workDir;
      String currentPath = path;
      while (StringUtils.startsWith(currentPath, PARENT_DIRECTORY)) {
        currentWorkDir = currentWorkDir.getParentFile();
        currentPath = StringUtils.substringAfter(currentPath, PARENT_DIRECTORY);
      }
      file = new File(currentWorkDir, currentPath);
    }
    
    return file;
  }

  private static void listFiles(Collection<File> files, File directory, IOFileFilter filter) {
    if (!directory.exists() || directory.isFile()) {
      return;
    }
    
    File[] found = directory.listFiles((FileFilter) filter);
    if (found != null) {
      files.addAll(Arrays.asList(found));
    }
    File[] subDirectories = directory.listFiles((FileFilter) DirectoryFileFilter.INSTANCE);
    for (File subDirectory : subDirectories) {
      listFiles(files, subDirectory, filter);
    }
  }
  
  private static String convertSlash(String path) {
    return StringUtils.replaceChars(path, '\\', '/');
  }

  private static class PatternFilter implements IOFileFilter {

    private final WildcardPattern pattern;

    public PatternFilter(File workDir, String pattern) {
      String absolutePathPattern = workDir.getAbsolutePath() + "/" + pattern;
      absolutePathPattern = convertSlash(absolutePathPattern);
      this.pattern = WildcardPattern.create(absolutePathPattern);
    }

    public boolean accept(File paramFile) {
      String absolutePath = paramFile.getAbsolutePath();
      final String path = absolutePath;
      return pattern.match(convertSlash(path));
    }

    public boolean accept(File dir, String name) {
      return accept(new File(dir, name));
    }

  }

}
