/*
 * Sonar C# Plugin :: Core
 * Copyright (C) 2010 Jose Chillan, Alexandre Victoor and SonarSource
 * dev@sonar.codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */
package org.sonar.plugins.csharp;

import com.google.common.collect.ImmutableSet;
import org.junit.Before;
import org.junit.Test;
import org.mockito.ArgumentCaptor;
import org.mockito.Mockito;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.fs.internal.DefaultFileSystem;
import org.sonar.api.batch.fs.internal.DefaultInputFile;
import org.sonar.api.component.ResourcePerspectives;
import org.sonar.api.config.Settings;
import org.sonar.api.issue.Issuable;
import org.sonar.api.issue.Issuable.IssueBuilder;
import org.sonar.api.issue.Issue;
import org.sonar.api.issue.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.measures.Measure;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rule.RuleKey;

import java.io.File;

import static org.fest.assertions.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CSharpSensorTest {

  private SensorContext context;
  private DefaultInputFile inputFile;
  private DefaultFileSystem fs;
  private FileLinesContext fileLinesContext;
  private FileLinesContextFactory fileLinesContextFactory;
  private NSonarQubeAnalyzerExtractor extractor;
  private NoSonarFilter noSonarFilter;
  private ResourcePerspectives perspectives;
  private Issuable issuable;
  private IssueBuilder issueBuilder;
  private Issue issue;

  @Test
  public void shouldExecuteOnProject() {
    DefaultFileSystem fs = new DefaultFileSystem();

    CSharpSensor sensor =
      new CSharpSensor(
        mock(Settings.class), mock(NSonarQubeAnalyzerExtractor.class),
        fs,
        mock(FileLinesContextFactory.class), mock(NoSonarFilter.class), mock(RulesProfile.class), mock(ResourcePerspectives.class));

    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("foo").setAbsolutePath("foo").setLanguage("java"));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isFalse();

    fs.add(new DefaultInputFile("bar").setAbsolutePath("bar").setLanguage("cs"));
    assertThat(sensor.shouldExecuteOnProject(mock(Project.class))).isTrue();
  }

  @Before
  public void init() {
    fs = new DefaultFileSystem();
    fs.setWorkDir(new File("src/test/resources/CSharpSensorTest"));

    inputFile = new DefaultInputFile("MyClass.cs").setAbsolutePath("MyClass.cs");
    fs.add(inputFile);

    fileLinesContext = mock(FileLinesContext.class);
    fileLinesContextFactory = mock(FileLinesContextFactory.class);
    when(fileLinesContextFactory.createFor(inputFile)).thenReturn(fileLinesContext);

    extractor = mock(NSonarQubeAnalyzerExtractor.class);
    when(extractor.executableFile()).thenReturn(new File("src/test/resources/CSharpSensorTest/fake.bat"));

    noSonarFilter = mock(NoSonarFilter.class);
    perspectives = mock(ResourcePerspectives.class);
    issuable = mock(Issuable.class);
    issueBuilder = mock(IssueBuilder.class);
    when(issuable.newIssueBuilder()).thenReturn(issueBuilder);
    issue = mock(Issue.class);
    when(issueBuilder.build()).thenReturn(issue);
    when(perspectives.as(Mockito.eq(Issuable.class), Mockito.any(Resource.class))).thenReturn(issuable);

    CSharpSensor sensor =
      new CSharpSensor(
        mock(Settings.class), extractor,
        fs,
        fileLinesContextFactory, noSonarFilter, mock(RulesProfile.class), perspectives);

    context = mock(SensorContext.class);
    sensor.analyse(mock(Project.class), context);
  }

  @Test
  public void metrics() {
    verify(context).saveMeasure(inputFile, CoreMetrics.LINES, 27d);
    verify(context).saveMeasure(inputFile, CoreMetrics.CLASSES, 1d);
    verify(context).saveMeasure(inputFile, CoreMetrics.ACCESSORS, 5d);
    verify(context).saveMeasure(inputFile, CoreMetrics.STATEMENTS, 2d);
    verify(context).saveMeasure(inputFile, CoreMetrics.FUNCTIONS, 3d);
    verify(context).saveMeasure(inputFile, CoreMetrics.PUBLIC_API, 4d);
    verify(context).saveMeasure(inputFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API, 2d);
    verify(context).saveMeasure(inputFile, CoreMetrics.COMPLEXITY, 3d);
  }

  @Test
  public void distribution() {
    ArgumentCaptor<Measure> captor = ArgumentCaptor.forClass(Measure.class);
    verify(context, Mockito.times(2)).saveMeasure(Mockito.eq(inputFile), captor.capture());
    int i = 0;
    for (Measure measure : captor.getAllValues()) {
      if (measure.getMetric().equals(CoreMetrics.FILE_COMPLEXITY_DISTRIBUTION)) {
        i++;
        assertThat(measure.getData()).isEqualTo("0=1;5=0;10=0;20=0;30=0;60=0;90=0");
      } else if (measure.getMetric().equals(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION)) {
        i++;
        assertThat(measure.getData()).isEqualTo("1=3;2=0;4=0;6=0;8=0;10=0;12=0");
      }
    }
    assertThat(i).isEqualTo(2);
  }

  @Test
  public void commentsAndNoSonar() {
    verify(noSonarFilter).addComponent(inputFile.key(), ImmutableSet.of(8));
    verify(context).saveMeasure(inputFile, CoreMetrics.COMMENT_LINES, 2d);
  }

  @Test
  public void devCockpit() {
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 3, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.COMMENT_LINES_DATA_KEY, 7, 1);

    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 1, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 12, 1);
    verify(fileLinesContext).setIntValue(CoreMetrics.NCLOC_DATA_KEY, 13, 1);
  }

  @Test
  public void issue() {
    verify(issueBuilder).ruleKey(RuleKey.of(CSharpPlugin.REPOSITORY_KEY, "S1186"));
    verify(issueBuilder).message("Add a nested comment explaining why this method is empty, throw an NotSupportedException or complete the implementation.");
    verify(issueBuilder).line(16);
    verify(issuable).addIssue(issue);
  }

}
