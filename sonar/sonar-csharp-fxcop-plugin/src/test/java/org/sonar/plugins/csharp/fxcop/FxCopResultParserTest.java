/*
 * Sonar C# Plugin :: FxCop
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

package org.sonar.plugins.csharp.fxcop;

import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyObject;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.io.File;
import java.nio.charset.Charset;

import org.apache.commons.lang.StringUtils;
import org.junit.Before;
import org.junit.Test;
import org.mockito.invocation.InvocationOnMock;
import org.mockito.stubbing.Answer;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RuleFinder;
import org.sonar.api.rules.RuleQuery;
import org.sonar.api.rules.Violation;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpResourcesBridge;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.api.ResourceHelper;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class FxCopResultParserTest {

  private SensorContext context;
  private CSharpResourcesBridge resourcesBridge;
  private FxCopResultParser parser;
  private File resultFile;
  private Rule avoidNamespaceWithFewTypesRule;
  private Rule assembliesShouldHaveValidStrongNamesRule;
  private Rule compoundWordsShouldBeCasedCorrectlyRule;
  private Rule doNotCastUnnecessarilyRule;
  private Rule parameterNamesShouldMatchBaseDeclarationRule;

  @Before
  public void init() {
    context = mock(SensorContext.class);
    resourcesBridge = mock(CSharpResourcesBridge.class);
    
    Project project = mock(Project.class);
    ProjectFileSystem fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSourceDirs()).thenReturn(Lists.newArrayList(new File("C:\\Sonar\\Example")));
    when(project.getFileSystem()).thenReturn(fileSystem);
    
    MicrosoftWindowsEnvironment env = mock(MicrosoftWindowsEnvironment.class);
    VisualStudioSolution solution = mock(VisualStudioSolution.class);
    when(env.getCurrentSolution()).thenReturn(solution);
    VisualStudioProject vsProject = mock(VisualStudioProject.class);
    when(solution.getProjectFromSonarProject(any(Project.class))).thenReturn(vsProject);
    when(solution.getProject(any(File.class))).thenReturn(vsProject);
    ResourceHelper resourceHelper = mock(ResourceHelper.class);
    when(resourceHelper.isResourceInProject(any(Resource.class), any(Project.class))).thenReturn(true);
 
    parser = new FxCopResultParser(env, project, context, newRuleFinder(), resourcesBridge, resourceHelper);
    parser.setEncoding(Charset.forName("UTF-8"));
  }

  @Test
  public void testParseFile1() throws Exception {
    resultFile = TestUtils.getResource("/Results/fxcop-report-1.xml");
    parser.parse(resultFile);

    // Verify calls on C# Resource bridge
    verify(resourcesBridge).getFromTypeName(eq("Example.Core"), eq("IMoney"));

    // Verify calls on context to save violations
    verify(context, times(13)).saveViolation(any(Violation.class));
  }

  @Test
  public void testParseFile2() throws Exception {
    resultFile = TestUtils.getResource("/Results/fxcop-report-2.xml");
    parser.parse(resultFile);

    // Verify calls on C# Resource bridge
    verify(resourcesBridge, times(1)).getFromTypeName(eq("Example.Core"), eq("Money"));
    verify(resourcesBridge, times(1)).getFromTypeName(eq("Example.Core"), eq("MoneyBag"));

    // Verify calls on context to save violations
    verify(context, times(4)).saveViolation(any(Violation.class));
  }

  private RuleFinder newRuleFinder() {
    avoidNamespaceWithFewTypesRule = Rule.create("fxcop", "AvoidNamespacesWithFewTypes", "Avoid namespaces with few types").setConfigKey(
        "AvoidNamespacesWithFewTypes@$(FxCopDir)\\Rules\\DesignRules.dll");
    assembliesShouldHaveValidStrongNamesRule = Rule.create("fxcop", "AssembliesShouldHaveValidStrongNames",
        "Assemblies should have valid strong names").setConfigKey(
        "AssembliesShouldHaveValidStrongNames@$(FxCopDir)\\Rules\\DesignRules.dll");
    compoundWordsShouldBeCasedCorrectlyRule = Rule.create("fxcop", "CompoundWordsShouldBeCasedCorrectly",
        "Compound words should be cased correctly").setConfigKey("CompoundWordsShouldBeCasedCorrectly@$(FxCopDir)\\Rules\\NamingRules.dll");
    doNotCastUnnecessarilyRule = Rule.create("fxcop", "DoNotCastUnnecessarily", "Do not cast unnecessarily").setConfigKey(
        "DoNotCastUnnecessarily@$(FxCopDir)\\Rules\\PerformanceRules.dll");
    parameterNamesShouldMatchBaseDeclarationRule = Rule.create("fxcop", "ParameterNamesShouldMatchBaseDeclaration",
        "Parameter names should match base declaration").setConfigKey(
        "ParameterNamesShouldMatchBaseDeclaration@$(FxCopDir)\\Rules\\NamingRules.dll");
    RuleFinder ruleFinder = mock(RuleFinder.class);
    when(ruleFinder.find((RuleQuery) anyObject())).thenAnswer(new Answer<Rule>() {

      public Rule answer(InvocationOnMock iom) throws Throwable {
        RuleQuery query = (RuleQuery) iom.getArguments()[0];
        Rule rule = null;
        if (StringUtils.equals(query.getKey(), "AssembliesShouldHaveValidStrongNames")) {
          rule = assembliesShouldHaveValidStrongNamesRule;
        } else if (StringUtils.equals(query.getKey(), "AvoidNamespacesWithFewTypes")) {
          rule = avoidNamespaceWithFewTypesRule;
        } else if (StringUtils.equals(query.getKey(), "CompoundWordsShouldBeCasedCorrectly")) {
          rule = compoundWordsShouldBeCasedCorrectlyRule;
        } else if (StringUtils.equals(query.getKey(), "DoNotCastUnnecessarily")) {
          rule = doNotCastUnnecessarilyRule;
        } else if (StringUtils.equals(query.getKey(), "ParameterNamesShouldMatchBaseDeclaration")) {
          rule = parameterNamesShouldMatchBaseDeclarationRule;
        }
        return rule;
      }
    });
    return ruleFinder;
  }

}
