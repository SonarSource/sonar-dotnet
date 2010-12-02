package org.sonar.plugin.dotnet.srcmon;

import java.io.File;
import java.util.List;

import org.sonar.plugin.dotnet.srcmon.model.FileMetrics;

public interface SourceMonitorResultParser {

  /**
   * Parses the report.
   * 
   * @param reportFile
   */
  public abstract List<FileMetrics> parse(File directory, File reportFile);

}