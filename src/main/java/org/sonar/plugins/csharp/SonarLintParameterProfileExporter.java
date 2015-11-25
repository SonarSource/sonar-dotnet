/*
 * SonarQube C# Plugin
 * Copyright (C) 2014 SonarSource
 * sonarqube@googlegroups.com
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
package org.sonar.plugins.csharp;

import com.google.common.base.Throwables;
import org.sonar.api.profiles.ProfileExporter;
import org.sonar.api.profiles.RulesProfile;

import java.io.File;
import java.io.IOException;
import java.io.Writer;
import java.util.Collections;

public class SonarLintParameterProfileExporter extends ProfileExporter {

  public SonarLintParameterProfileExporter() {
    super("sonarlint-vs-param-cs", "Technical exporter for the MSBuild SonarQube Scanner");
    setSupportedLanguages(CSharpPlugin.LANGUAGE_KEY);
  }

  @Override
  public void exportProfile(RulesProfile ruleProfile, Writer writer) {
    String contents = CSharpSensor.analysisSettings(false, false, true, ruleProfile, Collections.<File>emptyList());

    try {
      writer.write(contents);
    } catch (IOException e) {
      throw Throwables.propagate(e);
    }
  }

}
