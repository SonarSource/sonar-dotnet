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
import org.sonar.api.component.ResourcePerspectives;
import org.sonar.api.config.PropertyDefinition;
import org.sonar.api.config.Settings;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Qualifiers;
import org.sonar.api.rules.XMLRuleParser;
import org.sonar.api.scan.filesystem.ModuleFileSystem;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.resharper.ReSharperConfiguration;
import org.sonar.plugins.resharper.ReSharperRuleRepository;
import org.sonar.plugins.resharper.ReSharperSensor;

import java.util.List;

public class CSharpReSharperProvider {

  private static final String CATEGORY = "C#";
  private static final String SUBCATEGORY = "ReSharper";

  private static final String RESHARPER_PROJECT_NAME_PROPERTY_KEY = "sonar.csharp.resharper.project.name";
  private static final String RESHARPER_SOLUTION_FILE_PROPERTY_KEY = "sonar.csharp.resharper.solution.file";
  private static final String RESHARPER_INSPECTCODE_PATH_PROPERTY_KEY = "sonar.csharp.resharper.inspectcode.path";

  private static final ReSharperConfiguration RESHARPER_CONF = new ReSharperConfiguration(
    CSharpConstants.LANGUAGE_KEY,
    CSharpConstants.LANGUAGE_KEY + "-resharper",
    RESHARPER_PROJECT_NAME_PROPERTY_KEY,
    RESHARPER_SOLUTION_FILE_PROPERTY_KEY,
    RESHARPER_INSPECTCODE_PATH_PROPERTY_KEY);

  public static List extensions() {
    return ImmutableList.of(
      CSharpReSharperRuleRepository.class,
      CSharpReSharperSensor.class,
      PropertyDefinition.builder(RESHARPER_PROJECT_NAME_PROPERTY_KEY)
        .name("Visual Studio project name")
        .description("Example: MyLibrary")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(RESHARPER_SOLUTION_FILE_PROPERTY_KEY)
        .name("Solution file")
        .description("Example: C:\\Projects\\MyProject\\MySolution.sln")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build(),
      PropertyDefinition.builder(RESHARPER_INSPECTCODE_PATH_PROPERTY_KEY)
        .name("Path to inspectcode.exe")
        .description("Example: C:\\Program Files\\jb-commandline-8.1.23.523\\inspectcode.exe")
        .category(CATEGORY)
        .subCategory(SUBCATEGORY)
        .onQualifiers(Qualifiers.PROJECT, Qualifiers.MODULE)
        .build());
  }

  public static class CSharpReSharperRuleRepository extends ReSharperRuleRepository {

    public CSharpReSharperRuleRepository(XMLRuleParser xmlRuleParser) {
      super(RESHARPER_CONF, xmlRuleParser);
    }

  }

  public static class CSharpReSharperSensor extends ReSharperSensor {

    public CSharpReSharperSensor(Settings settings, RulesProfile profile, ModuleFileSystem fileSystem, ResourcePerspectives perspectives) {
      super(RESHARPER_CONF, settings, profile, fileSystem, perspectives);
    }

  }

}
