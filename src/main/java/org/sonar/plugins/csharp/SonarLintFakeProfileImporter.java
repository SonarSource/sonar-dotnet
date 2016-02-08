/*
 * SonarQube C# Plugin
 * Copyright (C) 2014-2016 SonarSource SA
 * mailto:contact AT sonarsource DOT com
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
package org.sonar.plugins.csharp;

import java.io.Reader;
import org.sonar.api.profiles.ProfileImporter;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.utils.ValidationMessages;

// SONARCS-558 workaround for SONAR-6969
public class SonarLintFakeProfileImporter extends ProfileImporter {

  public SonarLintFakeProfileImporter() {
    super("sonarlint-vs-cs-fake", "Technical importer for the MSBuild SonarQube Scanner");
    setSupportedLanguages(CSharpPlugin.LANGUAGE_KEY);
  }

  @Override
  public RulesProfile importProfile(Reader reader, ValidationMessages messages) {
    messages.addErrorText("The technical importer for the MSBuild SonarQube Scanner cannot be used.");
    return RulesProfile.create(getName(), CSharpPlugin.LANGUAGE_KEY);
  }

}
