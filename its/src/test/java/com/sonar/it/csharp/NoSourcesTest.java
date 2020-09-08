package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.ScannerForMSBuild;
import java.nio.file.Path;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static com.sonar.it.csharp.Tests.getMeasureAsInt;
import static org.assertj.core.api.Assertions.assertThat;

public class NoSourcesTest {

  @ClassRule
  public static TemporaryFolder temp = TestUtils.createTempFolder();

  private static final String PROJECT = "ProjectWithNoSources";
  @ClassRule
  public static final Orchestrator orchestrator = Tests.ORCHESTRATOR;

  @BeforeClass
  public static void init() throws Exception {
    TestUtils.reset(orchestrator);

    Path projectDir = Tests.projectDir(temp, "ProjectWithNoSources");

    ScannerForMSBuild beginStep = TestUtils.createBeginStep("ProjectWithNoSources", projectDir);

    ORCHESTRATOR.executeBuild(beginStep);

    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Rebuild");

    ORCHESTRATOR.executeBuild(TestUtils.createEndStep(projectDir));
  }

  @Test
  public void filesAtProjectLevel() {
    assertThat(getProjectMeasureAsInt("violations")).isEqualTo(4);
  }

  /* Helper methods */

  private Integer getProjectMeasureAsInt(String metricKey) {
    return getMeasureAsInt(PROJECT, metricKey);
  }
}
