package org.sonar.plugins.csharp.ndeps;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.io.File;

import org.junit.Before;
import org.junit.Test;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioProject;
import org.sonar.dotnet.tools.commons.visualstudio.VisualStudioSolution;
import org.sonar.dotnet.tools.ndeps.NDepsCommandBuilder;
import org.sonar.dotnet.tools.ndeps.NDepsRunner;
import org.sonar.plugins.csharp.api.CSharpConfiguration;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.csharp.ndeps.results.NDepsResultParser;
import org.sonar.test.TestUtils;

import com.google.common.collect.Lists;

public class NDepsSensorTest {

  private ProjectFileSystem fileSystem;
  private VisualStudioSolution solution;
  private VisualStudioProject vsProject1;
  private VisualStudioProject vsProject2;
  private MicrosoftWindowsEnvironment microsoftWindowsEnvironment;
  private CSharpConfiguration configuration;
  private NDepsResultParser nDepsResultParser;
  private NDepsSensor nDepsSensor;
  private File reportFile;

  @Before
  public void init() {
    fileSystem = mock(ProjectFileSystem.class);
    when(fileSystem.getSonarWorkingDirectory()).thenReturn(TestUtils.getResource("/Sensor"));

    vsProject1 = mock(VisualStudioProject.class);
    when(vsProject1.getName()).thenReturn("Project #1");
    vsProject2 = mock(VisualStudioProject.class);
    when(vsProject2.getName()).thenReturn("Project Test");
    when(vsProject2.isTest()).thenReturn(true);
    solution = mock(VisualStudioSolution.class);
    when(solution.getProjects()).thenReturn(Lists.newArrayList(vsProject1, vsProject2));

    microsoftWindowsEnvironment = new MicrosoftWindowsEnvironment();
    microsoftWindowsEnvironment.setCurrentSolution(solution);

    configuration = mock(CSharpConfiguration.class);
    nDepsResultParser = mock(NDepsResultParser.class);
    nDepsSensor = new NDepsSensor(fileSystem, microsoftWindowsEnvironment, configuration, nDepsResultParser);

    reportFile = TestUtils.getResource("/ndeps-report.xml");
  }

  @Test
  public void shouldNotAnalyseResultsForUnexistingFile() {
    nDepsSensor.analyseResults(mock(Project.class), new File("target/foo.txt"));
    verify(nDepsResultParser, never()).parse("compile", reportFile);
  }

  @Test
  public void shouldAnalyseResults() {
    nDepsSensor.analyseResults(mock(Project.class), reportFile);
    verify(nDepsResultParser).parse("compile", reportFile);
  }

  @Test
  public void shouldAnalyseResultsForTestProject() {
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Project Test");
    nDepsSensor.analyseResults(project, reportFile);
    verify(nDepsResultParser).parse("test", reportFile);
  }

  @Test
  public void shouldLaunchNDeps() throws Exception {
    when(configuration.getString(CSharpConstants.BUILD_CONFIGURATIONS_KEY, CSharpConstants.BUILD_CONFIGURATIONS_DEFVALUE)).thenReturn("Release");
    when(configuration.getInt(NDepsConstants.TIMEOUT_MINUTES_KEY, NDepsConstants.TIMEOUT_MINUTES_DEFVALUE)).thenReturn(3);
    Project project = mock(Project.class);
    when(project.getName()).thenReturn("Project #1");

    NDepsCommandBuilder builder = NDepsCommandBuilder.createBuilder(solution, vsProject1);
    NDepsRunner runner = mock(NDepsRunner.class);
    when(runner.createCommandBuilder(solution, vsProject1)).thenReturn(builder);

    nDepsSensor.launchNDeps(project, runner);
    verify(runner, times(1)).createCommandBuilder(solution, vsProject1);
    verify(runner, times(1)).execute(builder, 3);
  }

}
