/*
 * Sonar C# Plugin :: StyleCop
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
package org.sonar.plugins.csharp.stylecop;

import org.sonar.api.batch.DependsUpon;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.stylecop.profiles.StyleCopProfileExporter;

@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
public class RegularStyleCopSensor extends StyleCopSensor {

  public RegularStyleCopSensor(ProjectFileSystem fileSystem, RulesProfile rulesProfile, StyleCopProfileExporter profileExporter,
      StyleCopResultParser styleCopResultParser, CSharpConfiguration configuration, MicrosoftWindowsEnvironment microsoftWindowsEnvironment) {
    super(fileSystem, rulesProfile, profileExporter, styleCopResultParser, configuration, microsoftWindowsEnvironment);
  }
  
  protected String getRepositoryKey() {
    return StyleCopConstants.REPOSITORY_KEY;
  }

}
