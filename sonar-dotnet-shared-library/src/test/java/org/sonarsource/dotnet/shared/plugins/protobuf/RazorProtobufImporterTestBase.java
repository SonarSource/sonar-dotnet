package org.sonarsource.dotnet.shared.plugins.protobuf;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import org.junit.Before;
import org.junit.Rule;
import org.slf4j.event.Level;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.batch.fs.internal.FileMetadata;
import org.sonar.api.batch.fs.internal.TestInputFileBuilder;
import org.sonar.api.batch.sensor.internal.SensorContextTester;
import org.sonar.api.notifications.AnalysisWarnings;
import org.sonar.api.testfixtures.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;

public class RazorProtobufImporterTestBase {
  protected final static File TEST_DATA_DIR = new File("src/test/resources/RazorProtobufImporter");
  protected final SensorContextTester sensorContext = SensorContextTester.create(TEST_DATA_DIR);
  protected DefaultInputFile CasesInputFile;
  protected DefaultInputFile OverlapSymbolReferencesInputFile;
  protected DefaultInputFile ProgramInputFile;

  @Rule
  public LogTester logTester = new LogTester();

  @Before
  public void setUp() throws FileNotFoundException {
    logTester.setLevel(Level.TRACE);
    CasesInputFile = addTestFileToContext("Cases.razor");
    OverlapSymbolReferencesInputFile = addTestFileToContext("OverlapSymbolReferences.razor");
    ProgramInputFile = addTestFileToContext("Program.cs");
  }

  private DefaultInputFile addTestFileToContext(String testFilePath) throws FileNotFoundException {
    var testFile = new File(TEST_DATA_DIR, testFilePath);
    assertThat(testFile).withFailMessage("no such file: " + testFilePath).isFile();
    var inputFile = new TestInputFileBuilder("dummyKey", testFilePath)
      .setMetadata(new FileMetadata(mock(AnalysisWarnings.class)).readMetadata(new FileReader(testFile)))
      .build();
    sensorContext.fileSystem().add(inputFile);
    return inputFile;
  }
}
