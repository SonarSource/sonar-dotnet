/*
 * Sonar C# Plugin :: Gendarme
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

package org.sonar.plugins.csharp.gendarme.results;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertThat;
import static org.mockito.Mockito.*;

import java.io.File;

import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.Violation;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

import com.google.common.collect.Lists;

@SuppressWarnings("rawtypes")
public class GendarmeViolationMakerTest {

  private static GendarmeViolationMaker violationMaker;
  private static Project project;
  private static SensorContext context;
  private static CSharpResourcesBridge resourcesBridge;
  private static Rule aRule;
  private static Resource aFileIMoney;
  private static Resource aFileMoney;

  @BeforeClass
  public static void init() {
    aRule = Rule.create("gendarme", "Rule", "Rule").setSeverity(RulePriority.BLOCKER);
    aFileIMoney = new org.sonar.api.resources.File("IMoney");
    aFileMoney = new org.sonar.api.resources.File("Money");

    project = mock(Project.class);
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSourceDirs()).thenReturn(Lists.newArrayList(new File("C:\\Sonar\\Example")));
    when(project.getFileSystem()).thenReturn(fileSystem);
    resourcesBridge = createFakeBridge();
  }

  @Before
  public void reinitViolationMaker() {
    context = mock(SensorContext.class);
    violationMaker = new GendarmeViolationMaker(mock(MicrosoftWindowsEnvironment.class), project, context, resourcesBridge);
    violationMaker.setCurrentRule(aRule);
    violationMaker.setCurrentDefaultViolationMessage("Default Message");
    violationMaker.setCurrentTargetName("Example.Core.IMoney Example.Core.Money::AddMoney(Example.Core.Money)");
    violationMaker.setCurrentTargetAssembly("Example.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
    violationMaker.setCurrentLocation("Example.Core.IMoney Example.Core.Money::AddMoney(Example.Core.Money)");
    violationMaker.setCurrentSource("");
    violationMaker.setCurrentMessage("Message");
  }

  @Test
  public void testCreateViolationWithNoResourceFound() throws Exception {
    violationMaker.registerRuleType(aRule, "Assembly");
    violationMaker.setCurrentTargetName("Foo)");
    violationMaker.setCurrentLocation("Foo");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is((Resource) project));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationOnAssembly() throws Exception {
    violationMaker.registerRuleType(aRule, "Assembly");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is((Resource) project));
    assertThat(violation.getSeverity(), is(RulePriority.BLOCKER));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithNoSourceInfoButLocationForMethod() throws Exception {
    violationMaker.registerRuleType(aRule, "Method");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is(aFileMoney));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithNoSourceInfoButLocationForType() throws Exception {
    violationMaker.registerRuleType(aRule, "Type");
    violationMaker.setCurrentLocation("Example.Core.IMoney");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is(aFileIMoney));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithNoSourceInfoAndNoLocationForMethod() throws Exception {
    violationMaker.registerRuleType(aRule, "Method");
    violationMaker.setCurrentLocation("");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is(aFileMoney));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithNoSourceInfoAndNoLocationForType() throws Exception {
    violationMaker.registerRuleType(aRule, "Type");
    violationMaker.setCurrentLocation("");
    violationMaker.setCurrentTargetName("Example.Core.IMoney");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is(aFileIMoney));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithNoSourceInfoButLocationForInnerType() throws Exception {
    violationMaker.registerRuleType(aRule, "Type");
    violationMaker.setCurrentLocation("Example.Core.IMoney/InnerClass");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource(), is(aFileIMoney));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithMessage() throws Exception {
    violationMaker.setCurrentSource("C:\\Sonar\\Example\\Example.Core\\Money.cs");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource().getKey(), is("Example.Core/Money.cs"));
    assertNull(violation.getLineId());
    assertThat(violation.getMessage(), is("Message"));
  }
  
  @Test
  public void testCreateViolationOutsideProject() throws Exception {
    violationMaker.setCurrentSource("C:\\Outside\\Example\\Example.Core\\Money.cs");
    Violation violation = violationMaker.createViolation();
    assertNull(violation);
    verifyZeroInteractions(context);
  }

  @Test
  public void testCreateViolationWithNoMessage() throws Exception {
    violationMaker.setCurrentSource("C:\\Sonar\\Example\\Example.Core\\Money.cs");
    violationMaker.setCurrentMessage("");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource().getKey(), is("Example.Core/Money.cs"));
    assertThat(violation.getMessage(), is("Default Message"));
  }

  @Test
  public void testCreateViolationWithSourceInfoButNoLine() throws Exception {
    violationMaker.setCurrentSource("C:\\Sonar\\Example\\Example.Core\\Money.cs");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource().getKey(), is("Example.Core/Money.cs"));
    assertNull(violation.getLineId());
  }

  @Test
  public void testCreateViolationWithSourceInfoAndOnlyLine() throws Exception {
    violationMaker.setCurrentSource("C:\\Sonar\\Example\\Example.Core\\Money.cs(~56)");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource().getKey(), is("Example.Core/Money.cs"));
    assertThat(violation.getLineId(), is(56));
  }

  @Test
  public void testCreateViolationWithSourceInfoAndLineAndColumn() throws Exception {
    violationMaker.setCurrentSource("C:\\Sonar\\Example\\Example.Core\\Money.cs(56,45)");
    Violation violation = violationMaker.createViolation();
    assertThat(violation.getResource().getKey(), is("Example.Core/Money.cs"));
    assertThat(violation.getLineId(), is(56));
  }

  @SuppressWarnings("unchecked")
  private static CSharpResourcesBridge createFakeBridge() {
    CSharpResourcesBridge bridge = mock(CSharpResourcesBridge.class);
    when(bridge.getFromTypeName("Example.Core.Money")).thenReturn(aFileMoney);
    when(bridge.getFromTypeName("Example.Core.IMoney")).thenReturn(aFileIMoney);
    when(bridge.getFromTypeName("Example.Core.IMoney.InnerClass")).thenReturn(aFileIMoney);
    return bridge;
  }

}
