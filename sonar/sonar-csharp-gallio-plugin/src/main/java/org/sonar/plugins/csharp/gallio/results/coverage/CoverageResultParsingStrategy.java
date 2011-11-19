package org.sonar.plugins.csharp.gallio.results.coverage;

import org.codehaus.staxmate.in.SMInputCursor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.plugins.csharp.gallio.results.coverage.model.ParserResult;


public interface CoverageResultParsingStrategy {

  public boolean isCompatible(SMInputCursor rootCursor);
  
  public ParserResult parse(SensorContext ctx, Project sonarProject, SMInputCursor cursor);
  
}
