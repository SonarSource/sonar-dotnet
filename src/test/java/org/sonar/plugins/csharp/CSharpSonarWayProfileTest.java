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

import org.junit.Test;
import org.mockito.Mockito;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.profiles.XMLProfileParser;
import org.sonar.api.utils.ValidationMessages;

import java.io.Reader;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;

public class CSharpSonarWayProfileTest {

  @Test
  public void test() {
    XMLProfileParser xmlParser = mock(XMLProfileParser.class);
    ValidationMessages validation = ValidationMessages.create();
    RulesProfile profile = new CSharpSonarWayProfile(xmlParser).createProfile(validation);
    verify(xmlParser).parse(Mockito.any(Reader.class), Mockito.same(validation));
  }

}
