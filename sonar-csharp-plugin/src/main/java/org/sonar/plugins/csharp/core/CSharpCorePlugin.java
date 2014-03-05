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

import com.google.common.collect.ImmutableList;
import org.sonar.api.Properties;
import org.sonar.api.Property;
import org.sonar.api.PropertyType;
import org.sonar.api.SonarPlugin;
import org.sonar.plugins.csharp.api.CSharp;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.squid.CSharpRuleProfile;
import org.sonar.plugins.csharp.squid.CSharpRuleRepository;
import org.sonar.plugins.csharp.squid.CSharpSquidConstants;
import org.sonar.plugins.csharp.squid.CSharpSquidSensor;
import org.sonar.plugins.csharp.squid.colorizer.CSharpSourceCodeColorizer;
import org.sonar.plugins.csharp.squid.cpd.CSharpCPDMapping;

import java.util.List;

@Properties({
  @Property(
    key = CSharpConstants.FILE_SUFFIXES_KEY,
    defaultValue = CSharpConstants.FILE_SUFFIXES_DEFVALUE,
    name = "File suffixes",
    description = "Comma-separated list of suffixes of files to analyze.",
    project = true, global = true
  ),
  @Property(
    key = CSharpSquidConstants.IGNORE_HEADER_COMMENTS,
    defaultValue = "true",
    name = "Ignore header comments",
    description = "Set to 'true' to enable, or 'false' to disable.",
    project = true, global = true,
    type = PropertyType.BOOLEAN)
})
public class CSharpCorePlugin extends SonarPlugin {

  /**
   * {@inheritDoc}
   */
  @Override
  public List getExtensions() {
    ImmutableList.Builder builder = ImmutableList.builder();

    builder.add(
      CSharp.class,

      // Sensors
      CSharpSourceImporter.class,

      // Common Rules
      CSharpCommonRulesEngine.class,
      CSharpCommonRulesDecorator.class,

      // C# Squid
      CSharpCPDMapping.class,
      CSharpSourceCodeColorizer.class,
      CSharpSquidSensor.class,

      // rules
      CSharpRuleRepository.class,
      CSharpRuleProfile.class);

    builder.addAll(CSharpFxCopProvider.extensions());
    builder.addAll(CSharpCodeCoverageProvider.extensions());
    builder.addAll(CSharpReSharperProvider.extensions());

    return builder.build();
  }

}
