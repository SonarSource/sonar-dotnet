/*
 * Sonar .NET Plugin :: NDeps
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

import org.sonar.plugins.dotnet.api.utils.ResourceHelper;

import com.google.common.collect.Collections2;

import org.apache.commons.lang.StringUtils;

import org.sonar.api.measures.CoreMetrics;

import org.powermock.api.mockito.PowerMockito;

import org.junit.runner.RunWith;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import com.google.common.collect.Lists;

import org.sonar.plugins.dotnet.api.DotNetConfiguration;

import org.sonar.plugins.dotnet.api.microsoft.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioProject;
import org.sonar.plugins.dotnet.api.microsoft.VisualStudioSolution;

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
import org.sonar.plugins.dotnet.api.DotNetResourceBridge;
import org.sonar.plugins.dotnet.api.DotNetResourceBridges;
import org.sonar.test.TestUtils;

import java.io.File;
import java.util.Collections;
import java.util.List;

import static org.hamcrest.Matchers.hasItem;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Matchers.*;
import static org.mockito.Mockito.*;

@RunWith(PowerMockRunner.class)
public class NDepsResultParserTest {

  private NDepsResultParser parser;
  private DotNetResourceBridge resourcesBridge;
  private SensorContext context;
  private Project project;
  private Project project2;
  private VisualStudioSolution vsSolution;
  private VisualStudioProject vsProject;
  private VisualStudioProject vsProject2;

  @Before
  public void setUp() {
    project = PowerMockito.mock(Project.class);
    project2 = PowerMockito.mock(Project.class);
    Project rootProject = mock(Project.class);
    when(project.getParent()).thenReturn(rootProject);
    when(project.getLanguageKey()).thenReturn("cs");

    when(project2.getParent()).thenReturn(rootProject);
    when(project2.getLanguageKey()).thenReturn("cs");

    when(rootProject.getModules()).thenReturn(Lists.newArrayList(project, project2));

    vsSolution = mock(VisualStudioSolution.class);
    vsProject = mock(VisualStudioProject.class);
    when(vsSolution.getProjectFromSonarProject(project)).thenReturn(vsProject);
    when(vsProject.getName()).thenReturn("Example.Core");
    vsProject2 = mock(VisualStudioProject.class);
    when(vsSolution.getProjectFromSonarProject(project2)).thenReturn(vsProject2);
    when(vsProject2.getName()).thenReturn("Example.Common");

    resourcesBridge = mock(DotNetResourceBridge.class);
    when(resourcesBridge.getFromTypeName(anyString())).thenAnswer(new Answer<Resource<?>>() {
      public Resource<?> answer(InvocationOnMock invocation) throws Throwable {
        org.sonar.api.resources.File file = new org.sonar.api.resources.File("Example.Core:" + (String) invocation.getArguments()[0]);
        return file;
      }
    });
    when(resourcesBridge.getLanguageKey()).thenReturn("cs");
    DotNetResourceBridges bridges = new DotNetResourceBridges(new DotNetResourceBridge[] {resourcesBridge});

    context = mock(SensorContext.class);

    MicrosoftWindowsEnvironment env = mock(MicrosoftWindowsEnvironment.class);
    when(env.getCurrentSolution()).thenReturn(vsSolution);

    ResourceHelper resourceHelper = mock(ResourceHelper.class);
    when(resourceHelper.isResourceInProject(any(Resource.class), any(Project.class))).thenReturn(true);

    parser = new NDepsResultParser(env, bridges, project, context, mock(DotNetConfiguration.class), resourceHelper);
  }

  @Test
  public void testParse() {
    when(vsSolution.getProject("Example.Core")).thenReturn(vsProject);

    File report = TestUtils.getResource("/ndeps-report.xml");

    parser.parse("compile", report);

    ArgumentCaptor<Library> libCaptor = ArgumentCaptor.forClass(Library.class);
    // there's only 1 "Reference"
    verify(context, times(1)).getResource(libCaptor.capture());
    Library library = libCaptor.getValue();
    assertEquals("mscorlib", library.getName());

    ArgumentCaptor<String> classCaptor = ArgumentCaptor.forClass(String.class);
    // getFromTypeName is called twice per "to" dependencies, and there are 6 "to" dependencies
    verify(resourcesBridge, atLeast(6 * 2)).getFromTypeName(classCaptor.capture());
    List<String> classNames = classCaptor.getAllValues();

    // classes with dependency and with LCOM4 data
    assertTrue(classNames. contains("Example.Core.IMoney"));
    assertTrue(classNames.contains("Example.Core.Money"));
    assertTrue(classNames.contains("Example.Core.MoneyBag"));
    // Classes without dependencies
    assertFalse(classNames.contains("Example.Core.Dummy"));
    assertFalse(classNames.contains("Example.Core.SampleMeasure/Possible"));

    // Classes without dependencies
    assertTrue(classNames.contains("Example.Core.SampleMeasure"));
    assertTrue(classNames.contains("Example.Core.Model.SubType"));

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

    verify(context, times(1)).saveMeasure(any(org.sonar.api.resources.File.class), eq(CoreMetrics.LCOM4), eq(3d));
    verify(context, times(2)).saveMeasure(any(org.sonar.api.resources.File.class), eq(CoreMetrics.LCOM4), eq(2d));
  }

  @PrepareForTest({Resource.class})
  @Test
  public void shouldParseAndDetectProjectDependencyWithoutLcom() {
    PowerMockito.when(project.getKey()).thenReturn("null:Example.Core");
    PowerMockito.when(project2.getKey()).thenReturn("null:Example.Common");

    when(vsSolution.getProject("Core")).thenReturn(vsProject);
    when(vsSolution.getProject("Common")).thenReturn(vsProject2);

    File report = TestUtils.getResource("/ndeps-report-projectdependency.xml");

    parser.parse("compile", report);

    ArgumentCaptor<Library> libCaptor = ArgumentCaptor.forClass(Library.class);
    // there's only 1 "Reference" to an external library
    verify(context, times(1)).getResource(libCaptor.capture());
    Library library = libCaptor.getValue();
    assertEquals("mscorlib", library.getName());

    ArgumentCaptor<Dependency> dependencyCaptor = ArgumentCaptor.forClass(Dependency.class);
    // 1 library dependency + 1 project dependency + 1 * 2 file dependencies
    verify(context, times(4)).saveDependency(dependencyCaptor.capture());
    List<Dependency> depsList = dependencyCaptor.getAllValues();

    assertThat(depsList, hasItem(new Dependency(project, project2)));
    assertThat(depsList, hasItem(new Dependency(project, library)));
  }

  private void assertThatDepsListContains(List<Dependency> depsList, String from, String to) {
    assertThat(depsList, hasItem(new Dependency(new org.sonar.api.resources.File(from), new org.sonar.api.resources.File(to))));
  }

}
