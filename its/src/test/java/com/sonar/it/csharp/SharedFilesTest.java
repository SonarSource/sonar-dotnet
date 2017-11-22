package com.sonar.it.csharp;

import com.sonar.orchestrator.Orchestrator;
import java.nio.file.Path;
import java.util.List;
import org.junit.Before;
import org.junit.ClassRule;
import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonarqube.ws.Issues.Issue;

import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static com.sonar.it.csharp.Tests.getIssues;

import static com.sonar.it.csharp.Tests.getComponent;
import static org.assertj.core.api.Assertions.assertThat;

public class SharedFilesTest {
  @Rule
  public TemporaryFolder temp = new TemporaryFolder();

  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @Before
  public void init() throws Exception {
    orchestrator.resetData();
  }
  
  @Test
  public void should_analyze_shared_files() throws Exception {
    Path projectDir = Tests.projectDir(temp, "SharedFilesTest");
    orchestrator.executeBuild(Tests.newScanner(projectDir)
      .addArgument("begin")
      .setProjectKey("SharedFilesTest")
      .setProjectName("SharedFilesTest")
      .setProjectVersion("1.0")
      .setProperty("sonar.cs.vscoveragexml.reportsPaths", "reports/visualstudio.coveragexml"));

    Tests.runMSBuild(orchestrator, projectDir, "/t:Rebuild");

    orchestrator.executeBuild(Tests.newScanner(projectDir)
      .addArgument("end"));

    assertThat(getComponent("SharedFilesTest:Class1.cs")).isNotNull();
    assertThat(getComponent("SharedFilesTest:SharedFilesTest:77C8C6B5-18EC-45D4-8DA8-17A6525450A4:Program1.cs")).isNotNull();
    assertThat(getComponent("SharedFilesTest:SharedFilesTest:0FAF9365-FC72-4DF6-A466-7C432E85F2A8:Program.cs")).isNotNull();

    // shared file in the solution should have measures and issues
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "files")).isEqualTo(1);
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "lines")).isEqualTo(9);
    assertThat(getMeasureAsInt("SharedFilesTest:Class1.cs", "ncloc")).isEqualTo(7);
    
    List<Issue> issues = getIssues("SharedFilesTest:Class1.cs");
    assertThat(issues).hasSize(2);
  }
}
