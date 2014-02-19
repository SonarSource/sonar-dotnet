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

import com.google.common.base.Joiner;
import org.apache.commons.lang.ArrayUtils;
import org.sonar.api.batch.Initializer;
import org.sonar.api.config.Settings;
import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.dotnet.api.DotNetConstants;

/**
 * Class used to initialize C# projects.
 */
public class CSharpProjectInitializer extends Initializer {

  private final Settings projectSettings;

  public CSharpProjectInitializer(Settings settings) {
    this.projectSettings = settings;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    // FIXME: deprecated
    return CSharpConstants.LANGUAGE_KEY.equals(project.getLanguageKey());
  }

  @Override
  public void execute(Project project) {
    // Handling exclusions
    if (projectSettings.getBoolean(DotNetConstants.EXCLUDE_GENERATED_CODE_KEY)) {
      String[] exclusions = projectSettings.getStringArray("sonar.exclusions");
      String[] newExclusions = (String[]) ArrayUtils.addAll(exclusions, CSharpConstants.DEFAULT_FILES_TO_EXCLUDE);
      projectSettings.setProperty("sonar.exclusions", Joiner.on(',').join(newExclusions));

      // To be removed
      setPropertyOnDeprecatedConfiguration(project, "sonar.exclusions", newExclusions);
    }
  }

  // We must still use the Apache Configuration object as it is still used by Sonar in some cases, notably
  // the ones that this class handles:
  // - project.getFileSystem().getSourceCharset()
  // - exclusions
  // TODO: remove all this code when Apache Configuration has completely been removed
  private void setPropertyOnDeprecatedConfiguration(Project project, String key, Object value) {
    // FIXME: deprecated
    project.getConfiguration().setProperty(key, value);
  }

}
