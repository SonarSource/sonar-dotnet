package org.sonar.plugins.csharp.api.sensor;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertThat;
import static org.junit.Assert.assertTrue;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import java.io.File;
import java.util.Collections;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import static org.sonar.plugins.csharp.api.CSharpConstants.*;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;

import com.google.common.collect.Lists;



public class AbstractCilRuleBasedCSharpSensorTest {

  class FakeSensor extends AbstractCilRuleBasedCSharpSensor {

    public FakeSensor() {
      super(microsoftWindowsEnvironment, configurationMock, "SomeEngine");
    }

    @Override
    public void analyse(Project project, SensorContext context) {
    }
  }

  private AbstractCSharpSensor sensor;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private VisualStudioProject vsProject1;
  private CSharpConfiguration configurationMock;

  @Before
  public void init() {
    vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getName()).thenReturn("Project #1");
    VisualStudioProject vsProject2 = mock(VisualStudioProject.class);
    when(vsProject2.getName()).thenReturn("Project Test");
    when(vsProject2.isTest()).thenReturn(true);
    VisualStudioSolution solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject1, vsProject2));

    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);

    configurationMock = mock(CSharpConfiguration.class);
    
    sensor = new FakeSensor();
  }

  @Test
  public void testShouldNotExecuteOnTestProject() {
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Project Test");
    when(project.getLanguageKey()).thenReturn("cs");
    assertFalse(sensor.shouldExecuteOnProject(project));
  }
  
  @Test
  public void testShouldNotExecuteOnRootProject() {
    Project project = mock(Project.class);
    when(project.isRoot()).thenReturn(true);
    assertFalse(sensor.shouldExecuteOnProject(project));
  }

  @Test
  public void testShouldExecuteOnNormalProject() {
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Project #1");
    when(project.getLanguageKey()).thenReturn("cs");
    when(configurationMock.getString(BUILD_CONFIGURATIONS_KEY,
          BUILD_CONFIGURATIONS_DEFVALUE)).thenReturn(BUILD_CONFIGURATIONS_DEFVALUE);
    
    when(vsProject1.getGeneratedAssemblies(BUILD_CONFIGURATIONS_DEFVALUE)).thenReturn(Collections.singleton(new File("toto")));
    assertTrue(sensor.shouldExecuteOnProject(project));
  }

}
