package org.sonar.plugin.dotnet.srcmon;

import static org.junit.Assert.*;

import java.io.File;
import java.util.List;

import org.junit.Test;
import org.sonar.plugin.dotnet.srcmon.model.FileMetrics;

public class SourceMonitorResultParserTest {

  @Test
  public void testParse() {
    SourceMonitorResultParser parser = new SourceMonitorResultXpathParser();
    File projectDirectory = new File("target/test-classes/solution/MessyTestSolution");
    File reportFile = new File("target/test-classes/solution/MessyTestSolution/target/metrics-report.xml");
    File moneyFile = new File("target/test-classes/solution/MessyTestSolution/MessyTestApplication/Money.cs");
    List<FileMetrics> metrics = parser.parse(projectDirectory, reportFile);
    assertNotNull(metrics);
    assertEquals(5, metrics.size());
    FileMetrics firstFile = metrics.get(0);
    assertEquals(62, firstFile.getComplexity());
    assertEquals(moneyFile.getAbsoluteFile(), firstFile.getSourcePath());
    assertEquals(3, firstFile.getCountClasses());
    assertEquals(29, firstFile.getCommentLines());
    assertEquals(1.77, firstFile.getAverageComplexity(),0.00001D);
    assertEquals("MessyTestApplication.Money", firstFile.getClassName());
    assertEquals(10, firstFile.getCountAccessors());
    assertEquals(53, firstFile.getCountBlankLines());
    assertEquals(3, firstFile.getCountCalls());
    assertEquals(387, firstFile.getCountLines());
    assertEquals(11, firstFile.getCountMethods());
    assertEquals(4, firstFile.getCountMethodStatements());
    assertEquals(199, firstFile.getCountStatements());
    assertEquals(10, firstFile.getDocumentationLines());
    
    // TODO does not seem to be used
    //assertEquals(0, firstFile.getPercentCommentLines(), 0.00001D);
    //assertEquals(10, firstFile.getPercentDocumentationLines(), 0.00001D);
  }

}
