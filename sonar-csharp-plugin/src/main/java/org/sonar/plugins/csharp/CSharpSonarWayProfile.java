/*
 * SonarC#
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
package org.sonar.plugins.csharp;

import org.sonar.api.profiles.XMLProfileParser;
import org.sonarsource.dotnet.shared.plugins.AbstractSonarWayProfile;

public class CSharpSonarWayProfile extends AbstractSonarWayProfile {
  private static final String PROFILE_XML_PATH = "/org/sonar/plugins/csharp/profile.xml";

  public CSharpSonarWayProfile(XMLProfileParser xmlParser) {
    super(xmlParser, PROFILE_XML_PATH);
  }
}
