/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

package org.sonar.plugin.dotnet.fxcop;

import static org.junit.Assert.*;
import static org.mockito.Matchers.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.net.MalformedURLException;

import org.apache.commons.configuration.Configuration;
import org.apache.maven.dotnet.commons.project.VisualStudioUtils;
import org.apache.maven.project.MavenProject;
import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulesManager;
import org.sonar.api.rules.Violation;

public class FxCopResultParserTest {

  private RulesProfile profile;
  private RulesManager rulesManager;
  private SensorContext context;
  private FxCopResultParser parser;
  
  @Before
  public void setUp() {
    profile = mock(RulesProfile.class);
    
    
    // set up rules manager
    rulesManager = mock(RulesManager.class);
    Rule dummyRule = mock(Rule.class);
    when(rulesManager.getPluginRule(eq(FxCopPlugin.KEY), anyString())).thenReturn(dummyRule);
    
    context = mock(SensorContext.class);
    
    // set up maven project
    MavenProject mvnProject = new MavenProject();
    mvnProject.setPackaging("sln");
    mvnProject.getProperties().put(VisualStudioUtils.VISUAL_SOLUTION_NAME_PROPERTY, "Example.sln");
    File pomFile 
      = new File("target/test-classes/solution/Example/pom.xml");
    mvnProject.setFile(pomFile);
    
    // set up sonar project
    Project project = mock(Project.class);
    when(project.getPom()).thenReturn(mvnProject);
    Configuration configuration = mock(Configuration.class);
    when(project.getConfiguration()).thenReturn(configuration);
    

    parser = new FxCopResultParser(project, context, rulesManager, profile);
  }
  
  @Test
  public void testParse() throws MalformedURLException {
    parser.parse(new File("src/test/resources","fxcop-report-processed.xml"));
    verify(context,atLeast(27)).saveViolation(any(Violation.class));
  }

}
