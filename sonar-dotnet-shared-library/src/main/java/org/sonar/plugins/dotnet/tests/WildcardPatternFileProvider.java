/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */
package org.sonar.plugins.dotnet.tests;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.regex.Pattern;
import java.util.stream.Collectors;
import org.sonar.api.utils.WildcardPattern;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class WildcardPatternFileProvider {
  private static final Logger LOG = LoggerFactory.getLogger(WildcardPatternFileProvider.class);

  private static final String CURRENT_FOLDER = ".";
  private static final String PARENT_FOLDER = "..";

  private static final String RECURSIVE_PATTERN = "**";
  private static final String ZERO_OR_MORE_PATTERN = "*";
  private static final String ANY_PATTERN = "?";

  private static final Pattern SEPARATOR_PATTERN = Pattern.compile("["+ Pattern.quote(File.separator) +"/]");

  private final File baseDir;

  public WildcardPatternFileProvider(File baseDir) {
    this.baseDir = baseDir;
  }

  Set<File> listFiles(String pattern) {

    List<String> elements = Arrays.asList(SEPARATOR_PATTERN.split(pattern));

    List<String> elementsTillFirstWildcard = elementsTillFirstWildcard(elements);
    String pathTillFirstWildcardElement = toPath(elementsTillFirstWildcard);
    File fileTillFirstWildcardElement = new File(pathTillFirstWildcardElement);

    File absoluteFileTillFirstWildcardElement = fileTillFirstWildcardElement.isAbsolute() ? fileTillFirstWildcardElement : new File(baseDir, pathTillFirstWildcardElement);

    LOG.debug("Pattern matcher extracted prefix/absolute path '{}' from the given pattern '{}'.",
      absoluteFileTillFirstWildcardElement.getAbsolutePath(), pattern);

    List<String> wildcardElements = elements.subList(elementsTillFirstWildcard.size(), elements.size());
    if (wildcardElements.isEmpty()) {
      if (absoluteFileTillFirstWildcardElement.exists()) {
        LOG.debug("Pattern matcher returns a single file: '{}'.", absoluteFileTillFirstWildcardElement.getAbsolutePath());
        return new HashSet<>(Collections.singletonList(absoluteFileTillFirstWildcardElement));
      } else {
        LOG.debug("Pattern matcher did not find any files matching the pattern '{}'.", pattern);
        return Collections.emptySet();
      }
    }
    checkNoCurrentOrParentFolderAccess(wildcardElements);

    WildcardPattern wildcardPattern = WildcardPattern.create(toPath(wildcardElements), File.separator);

    LOG.debug("Gathering files for wildcardPattern '{}'.", wildcardPattern);

    Set<File> result = new HashSet<>();
    for (File file : listFiles(absoluteFileTillFirstWildcardElement)) {
      String relativePath = relativize(absoluteFileTillFirstWildcardElement, file);

      if (wildcardPattern.match(relativePath)) {
        LOG.trace("Adding file '{}' to result list.", file.getAbsolutePath());
        result.add(file);
      } else {
        LOG.trace("Skipping file '{}' because it does not match pattern '{}'.",
          file.getAbsolutePath(), wildcardPattern);
      }
    }

    LOG.debug("Pattern matcher returns '{}' files.", result.size());
    return result;
  }

  private static String toPath(List<String> elements) {
    return elements.stream().collect(Collectors.joining(File.separator));
  }

  private static List<String> elementsTillFirstWildcard(List<String> elements) {
    List<String> result = new ArrayList<>();
    for (String element : elements) {
      if (containsWildcard(element)) {
        break;
      }
      result.add(element);
    }
    return result;
  }

  private static void checkNoCurrentOrParentFolderAccess(List<String> elements) {
    for (String element : elements) {
      if (isCurrentOrParentFolder(element)) {
        throw new IllegalArgumentException("Cannot contain '" + CURRENT_FOLDER + "' or '" + PARENT_FOLDER + "' after the first wildcard.");
      }
    }
  }

  private static boolean containsWildcard(String element) {
    return RECURSIVE_PATTERN.equals(element) ||
      element.contains(ZERO_OR_MORE_PATTERN) ||
      element.contains(ANY_PATTERN);
  }

  private static boolean isCurrentOrParentFolder(String element) {
    return CURRENT_FOLDER.equals(element) ||
      PARENT_FOLDER.equals(element);
  }

  private static Set<File> listFiles(File dir) {
    Set<File> result = new HashSet<>();
    listFiles(result, dir);
    return result;
  }

  private static void listFiles(Set<File> result, File dir) {
    File[] files = dir.listFiles();
    if (files != null) {
      result.addAll(Arrays.asList(files));

      for (File file : files) {
        if (file.isDirectory()) {
          listFiles(result, file);
        }
      }
    }
  }

  private static String relativize(File parent, File file) {
    return file.getAbsolutePath().substring(parent.getAbsolutePath().length() + 1);
  }

}
