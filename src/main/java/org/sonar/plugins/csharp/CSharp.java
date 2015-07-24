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

import org.sonar.api.config.Settings;
import org.sonar.api.resources.AbstractLanguage;

public class CSharp extends AbstractLanguage {

  private final Settings settings;

  public CSharp(Settings settings) {
    super(CSharpPlugin.LANGUAGE_KEY, CSharpPlugin.LANGUAGE_NAME);
    this.settings = settings;
  }

  @Override
  public String[] getFileSuffixes() {
    return settings.getStringArray(CSharpPlugin.FILE_SUFFIXES_KEY);
  }

}
