/*
 * Sonar C# Plugin :: NDeps
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
package org.sonar.plugins.csharp.ndeps.results;

import static org.hamcrest.Matchers.hasItem;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyString;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.io.File;
import java.util.List;

import org.junit.Before;
import org.junit.Test;
import org.mockito.ArgumentCaptor;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.design.Dependency;
import org.sonar.api.resources.Library;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.test.TestUtils;

public class NDepsResultParserTest {

  private NDepsResultParser parser;
  private CSharpResourcesBridge bridge;
  private SensorContext context;
  private Project project;

  @Before
  public void setUp() {
    project = mock(Project.class);
    Project rootProject = mock(Project.class);
    when(project.getParent()).thenReturn(rootProject);

    VisualStudioSolution vsSolution = mock(VisualStudioSolution.class);
    VisualStudioProject vsProject = mock(VisualStudioProject.class);
    when(vsSolution.getProject("Example.Core")).thenReturn(vsProject);
    when(vsSolution.getProjectFromSonarProject(project)).thenReturn(vsProject);

    bridge = mock(CSharpResourcesBridge.class);
    when(bridge.getFromTypeName(anyString())).thenAnswer(new Answer<Resource<?>>() {
      public Resource<?> answer(InvocationOnMock invocation) throws Throwable {
        org.sonar.api.resources.File file = new org.sonar.api.resources.File("Example.Core:" + (String) invocation.getArguments()[0]);
        return file;
      }
    });

    context = mock(SensorContext.class);

    MicrosoftWindowsEnvironment env = mock(MicrosoftWindowsEnvironment.class);
    when(env.getCurrentSolution()).thenReturn(vsSolution);
    parser = new NDepsResultParser(env, bridge, project, context);
  }

  @Test
  public void testParse() {
    File report = TestUtils.getResource("/ndeps-report.xml");

    parser.parse("compile", report);

    ArgumentCaptor<Library> libCaptor = ArgumentCaptor.forClass(Library.class);
    // there's only 1 "Reference"
    verify(context, times(1)).getResource(libCaptor.capture());
    Library library = libCaptor.getValue();
    assertEquals("mscorlib", library.getName());

    ArgumentCaptor<String> classCaptor = ArgumentCaptor.forClass(String.class);
    // getFromTypeName is called twice per "to" dependencies, and there are 6 "to" dependencies
    verify(bridge, times(6 * 2)).getFromTypeName(classCaptor.capture());
    List<String> classNames = classCaptor.getAllValues();
    assertTrue(classNames.contains("Example.Core.IMoney"));
    assertTrue(classNames.contains("Example.Core.Money"));
    assertTrue(classNames.contains("Example.Core.MoneyBag"));
    // Classes without dependencies
    assertFalse(classNames.contains("Example.Core.Dummy"));
    assertFalse(classNames.contains("Example.Core.SampleMeasure/Possible"));
    assertFalse(classNames.contains("Example.Core.SampleMeasure"));
    assertFalse(classNames.contains("Example.Core.Model.SubType"));

    ArgumentCaptor<Dependency> dependencyCaptor = ArgumentCaptor.forClass(Dependency.class);
    // 1 library dependency + the 6*2 file dependencies
    verify(context, times(1 + 6 * 2)).saveDependency(dependencyCaptor.capture());
    List<Dependency> depsList = dependencyCaptor.getAllValues();
    // verify only files...
    assertThatDepsListContains(depsList, "Example.Core:Example.Core.IMoney", "Example.Core:Example.Core.Money");
    assertThatDepsListContains(depsList, "Example.Core:Example.Core.IMoney", "Example.Core:Example.Core.MoneyBag");
    assertThatDepsListContains(depsList, "Example.Core:Example.Core.Money", "Example.Core:Example.Core.IMoney");
    assertThatDepsListContains(depsList, "Example.Core:Example.Core.Money", "Example.Core:Example.Core.MoneyBag");
    assertThatDepsListContains(depsList, "Example.Core:Example.Core.MoneyBag", "Example.Core:Example.Core.Money");
    assertThatDepsListContains(depsList, "Example.Core:Example.Core.MoneyBag", "Example.Core:Example.Core.IMoney");
  }

  private void assertThatDepsListContains(List<Dependency> depsList, String from, String to) {
    assertThat(depsList, hasItem(new Dependency(new org.sonar.api.resources.File(from), new org.sonar.api.resources.File(to))));
  }

}
