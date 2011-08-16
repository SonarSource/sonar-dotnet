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
package org.sonar.plugins.csharp.api;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import org.junit.Test;
import org.sonar.api.utils.SonarException;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;

import com.google.common.collect.Lists;

public class MicrosoftWindowsEnvironmentTest {

  @Test
  public void testIndexOfProjects() {
    VisualStudioProject project1 = mock(VisualStudioProject.class);
    when(project1.getName()).thenReturn("Project #1");
    VisualStudioProject project2 = mock(VisualStudioProject.class);
    when(project2.getName()).thenReturn("Project #2");
    VisualStudioSolution solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(project1, project2));

    MicrosoftWindowsEnvironment microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);
    assertThat(microsoftWindowsEnvironment.getCurrentSolution(), is(solution));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Project #1"), is(project1));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Project #2"), is(project2));
  }

  @Test
  public void testIndexOfProjectsWithSonarBranch() {
    VisualStudioProject project1 = mock(VisualStudioProject.class);
    when(project1.getName()).thenReturn("Project #1");
    VisualStudioProject project2 = mock(VisualStudioProject.class);
    when(project2.getName()).thenReturn("Project #2");
    VisualStudioSolution solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(project1, project2));

    CSharpConfiguration configuration = mock(CSharpConfiguration.class);
    when(configuration.getString("sonar.branch", "")).thenReturn("FooBranch");
    MicrosoftWindowsEnvironment microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment(configuration);
    microsoftWindowsEnvironment.setCurrentSolution(solution);
    assertThat(microsoftWindowsEnvironment.getCurrentSolution(), is(solution));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Project #1"), is(project1));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Project #2"), is(project2));
    // and check with the branch name
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Project #1 FooBranch"), is(project1));
    assertThat(microsoftWindowsEnvironment.getCurrentProject("Project #2 FooBranch"), is(project2));
  }

  @Test(expected = SonarException.class)
  public void testLock() throws Exception {
    MicrosoftWindowsEnvironment microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.lock();
    microsoftWindowsEnvironment.setCurrentSolution(null);
  }

}
