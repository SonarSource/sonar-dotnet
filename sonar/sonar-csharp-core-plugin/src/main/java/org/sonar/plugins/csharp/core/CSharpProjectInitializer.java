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
package org.sonar.plugins.csharp.core;

import java.util.Arrays;

import org.apache.commons.configuration.Configuration;
import org.apache.commons.lang.ArrayUtils;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Initializer;
import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.api.CSharpConstants;

/**
 * Class used to initialize C# projects.
 */
public class CSharpProjectInitializer extends Initializer {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpProjectInitializer.class);

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey());
  }

  @Override
  public void execute(Project project) {
    Configuration projectConfiguration = project.getConfiguration();
    // Handling encoding
    if (StringUtils.isBlank(projectConfiguration.getString("sonar.sourceEncoding", ""))) {
      LOG.info("'sonar.sourceEncoding' has not been defined: setting it to default value 'UTF-8'.");
      projectConfiguration.setProperty("sonar.sourceEncoding", "UTF-8");
    }

    // Handling exclusions
    if (projectConfiguration.getBoolean(CSharpConstants.EXCLUDE_GENERATED_CODE_KEY, CSharpConstants.EXCLUDE_GENERATED_CODE_DEFVALUE)) {
      String[] exclusions = projectConfiguration.getStringArray("sonar.exclusions");
      Object[] newExclusions = ArrayUtils.addAll(exclusions, CSharpConstants.DEFAULT_FILES_TO_EXCLUDE);
      projectConfiguration.setProperty("sonar.exclusions", newExclusions);
      // TODO : remove the following line once SONAR-2827 has been fixed
      project.setExclusionPatterns(Arrays.asList(newExclusions).toArray(new String[newExclusions.length]));
    }
  }

}
