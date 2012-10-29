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
package org.sonar.plugins.csharp.squid.cpd;

import org.apache.commons.configuration.BaseConfiguration;
import org.junit.Before;
import org.junit.Test;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.plugins.csharp.api.CSharp;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.when;

public class CSharpCPDMappingTest {

  @Mock
  private CSharp language;
  @Mock
  private Project project;
  @Mock
  private ProjectFileSystem projectFileSystem;;

  @Before
  public void init() {
    MockitoAnnotations.initMocks(this);
    when(project.getFileSystem()).thenReturn(projectFileSystem);
    when(project.getConfiguration()).thenReturn(new BaseConfiguration());
  }

  @Test
  public void test() {
    CSharpCPDMapping mapping = new CSharpCPDMapping(language, project);

    assertThat(mapping.getLanguage()).isSameAs(language);
    assertThat(mapping.getTokenizer()).isInstanceOf(CSharpCPDTokenizer.class);
  }

}
