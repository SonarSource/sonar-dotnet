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
import java.util.List;
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
 * 
 * @author Alexandre Victoor
 * 
 */
public class FileFinder {

  private static final Logger LOG = LoggerFactory.getLogger(FileFinder.class);

  /**
   * Find files that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analysed by sonar
   * @param currentProject
   *          The VS project being analysed
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
   *          The VS solution being analysed by sonar
   * @param currentProject
   *          The VS project being analysed
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
   * TODO clean up the code 
   * Find files that match the given patterns
   * 
   * @param currentSolution
   *          The VS solution being analysed by sonar
   * @param currentProject
   *          The VS project being analysed
   * @param patternArray
   *          A list of paths or ant style patterns
   * @return The files found on the filesystem
   */
  @SuppressWarnings("unchecked")
  public static Collection<File> findFiles(VisualStudioSolution currentSolution, VisualStudioProject currentProject, String... patternArray) {

    if (patternArray==null || patternArray.length==0) {
      return Collections.EMPTY_LIST;
    }
    
    File solutionDir = currentSolution.getSolutionDir();
    String solutionPath = solutionDir.getAbsolutePath();

    File projectDir = currentProject.getDirectory();

    Set<File> result = new HashSet<File>();
    List<WildcardPattern> solutionPatterns = new ArrayList<WildcardPattern>();
    for (String pattern : patternArray) {
      if (StringUtils.startsWithIgnoreCase(pattern, "$(SolutionDir)")) {
        String patternAfterSubst = StringUtils.replaceChars(StringUtils.replace(pattern, "$(SolutionDir)", solutionPath), '\\', '/');
        solutionPatterns.add(WildcardPattern.create(patternAfterSubst));
      } else {
        File file = new File(pattern);
        if (file.exists()) {
          // pattern is an absolute path
          result.add(file);
        } else {
          file = new File(projectDir, pattern);
          if (file.exists()) {
            // pattern is a relative path from the project root
            result.add(file);
          } else {
            // wildcard are used
            String rootPath = StringUtils.substringBefore(pattern, "*");
            File rootDir = new File(rootPath);
            if (rootDir.exists() && rootDir.isAbsolute()) {
              // prefix is an absolute path 
              String patternAfterSubst 
                = StringUtils.replaceChars(rootDir.getAbsolutePath() + "*" + StringUtils.substringAfter(pattern, "*"), '\\', '/');
              IOFileFilter externalFilter = new PatternFilter(null, WildcardPattern.create(patternAfterSubst));
              listFiles(result, rootDir, externalFilter);
            } else {
              String patternAfterSubst = StringUtils.replaceChars(pattern, '\\', '/');
              rootDir = projectDir;
              while (StringUtils.startsWith(patternAfterSubst, "../")) {
                rootDir = rootDir.getParentFile();
                patternAfterSubst = StringUtils.substringAfter(patternAfterSubst, "../");
              }
              if (new File(rootDir, patternAfterSubst).exists()) {
                // there was .. in the pattern but no *
                result.add(new File(rootDir, patternAfterSubst));
              } else {
                // pattern with *
                patternAfterSubst 
                  = StringUtils.replaceChars(rootDir.getAbsolutePath() + File.separator + StringUtils.substringBefore(patternAfterSubst, "*") + "*" + StringUtils.substringAfter(patternAfterSubst, "*"), '\\', '/');
                
                IOFileFilter externalFilter = new PatternFilter(null, WildcardPattern.create(patternAfterSubst));
                listFiles(result, rootDir, externalFilter);
              }
            }
          }
        }

      }
    }

    IOFileFilter solutionFilter = new PatternFilter(null, solutionPatterns);
    listFiles(result, solutionDir, solutionFilter);

    if (LOG.isDebugEnabled()) {
      if (result.isEmpty()) {
        LOG.debug("No file found using pattern(s) " + StringUtils.join(patternArray, ','));
      } else {
        LOG.debug("The following files have been found using pattern(s) " 
            + StringUtils.join(patternArray, ',') + "\n"
            + StringUtils.join(result, "\n  "));
      }
    }

    return result;
  }

  private static void listFiles(Collection<File> files, File directory, IOFileFilter filter) {
    File[] found = directory.listFiles((FileFilter) filter);
    if (found != null) {
      files.addAll(Arrays.asList(found));
    }
    File[] subDirectories = directory.listFiles((FileFilter) DirectoryFileFilter.INSTANCE);
    for (File subDirectory : subDirectories) {
      listFiles(files, subDirectory, filter);
    }
  }

  private static class PatternFilter implements IOFileFilter {

    private final WildcardPattern[] patterns;
    private final String prefix;

    public PatternFilter(String prefix, List<WildcardPattern> patterns) {
      this.prefix = prefix;
      this.patterns = patterns.toArray(new WildcardPattern[patterns.size()]);
    }
    
    public PatternFilter(String prefix, WildcardPattern... patterns) {
      this.prefix = prefix;
      this.patterns = patterns;
    }

    public boolean accept(File paramFile) {
      String absolutePath = paramFile.getAbsolutePath();
      final String path;
      if (StringUtils.isEmpty(prefix)) {
        path = absolutePath;
      } else {
        path = StringUtils.removeStartIgnoreCase(absolutePath, prefix + File.separator);
      }
      return WildcardPattern.match(patterns, StringUtils.replaceChars(path, '\\', '/'));
    }

    public boolean accept(File dir, String name) {
      return accept(new File(dir, name));
    }

  }

}
