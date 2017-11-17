/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2014-2017 SonarSource SA
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
package org.sonarsource.dotnet.shared.plugins;

import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import org.sonar.api.profiles.ProfileDefinition;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.profiles.XMLProfileParser;
import org.sonar.api.utils.ValidationMessages;

public class AbstractSonarWayProfile extends ProfileDefinition {
  private final XMLProfileParser xmlParser;
  private final String profileXmlPath;

  public AbstractSonarWayProfile(XMLProfileParser xmlParser, String profileXmlPath) {
    this.xmlParser = xmlParser;
    this.profileXmlPath = profileXmlPath;
  }

  @Override
  public RulesProfile createProfile(ValidationMessages validation) {
    return xmlParser.parse(new InputStreamReader(getClass().getResourceAsStream(profileXmlPath), StandardCharsets.UTF_8), validation);
  }
}
