package org.sonar.plugin.dotnet.gendarme;

import static org.junit.Assert.*;
import static org.mockito.Matchers.*;
import static org.mockito.Mockito.*;

import java.io.File;
import java.net.MalformedURLException;
import java.net.URL;

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

public class GendarmeResultParserTest {

  private GendarmeResultParser parser;
  private RulesProfile profile;
  private RulesManager rulesManager;
  private SensorContext context;
  
  @Before
  public void setUp() {
    profile = mock(RulesProfile.class);
    
    
    // set up rules manager
    rulesManager = mock(RulesManager.class);
    Rule dummyRule = mock(Rule.class);
    when(rulesManager.getPluginRule(eq(GendarmePlugin.KEY), anyString())).thenReturn(dummyRule);
    
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

    parser = new GendarmeResultParser(project, context, rulesManager, profile);
  }
  
  @Test
  public void testParse() throws MalformedURLException {
    parser.parse(new URL("file:src/test/resources/gendarme-report-processed.xml"));
    verify(context,atLeast(31)).saveViolation(any(Violation.class));
  }

}
