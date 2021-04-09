package com.sonar.it.csharp;

import com.sonar.it.shared.TestUtils;
import com.sonar.orchestrator.build.BuildResult;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;

import static com.sonar.it.csharp.Tests.ORCHESTRATOR;
import static org.assertj.core.api.Assertions.assertThat;

public class CodeDuplicationTest {
  @ClassRule
  public static final TemporaryFolder temp = TestUtils.createTempFolder();
  private static final String PROJECT = "CodeDuplicationTest";

  private static BuildResult buildResult;

  @BeforeClass
  public static void init() throws Exception {
    TestUtils.reset(ORCHESTRATOR);
    buildResult = Tests.analyzeProject(temp, PROJECT, null);
  }

  @Test
  public void codeDuplicationResultsAreImportedForMainCode() throws Exception {
    assertThat(TestUtils.getDuplication(ORCHESTRATOR, "CodeDuplicationTest:CodeDuplicationMainProj/DuplicatedMainClass1.cs").getDuplicationsList()).isNotEmpty();
  }

  @Test
  public void codeDuplicationResultsAreNotImportedForTestCode() throws Exception {
    assertThat(TestUtils.getDuplication(ORCHESTRATOR, "CodeDuplicationTest:CodeDuplicationTestProj/DuplicatedTestClass1.cs").getDuplicationsList()).isEmpty();
  }
}
