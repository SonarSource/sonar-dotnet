/*
 * Sonar C# Plugin :: C# Squid :: Squid
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
package com.sonar.csharp.squid.metric;

import com.google.common.collect.Lists;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.api.Grammar;
import com.sonar.sslr.squid.AstScanner;
import org.apache.commons.io.FileUtils;
import org.junit.Before;
import org.junit.Test;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContext;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.ProjectFileSystem;
import org.sonar.api.resources.Resource;
import org.sonar.squid.api.SourceProject;
import org.sonar.squid.indexer.QueryByType;
import org.sonar.test.TestUtils;

import java.io.File;
import java.nio.charset.Charset;

import static org.mockito.Matchers.any;
import static org.mockito.Matchers.anyInt;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

public class CSharpFileLinesVisitorTest {

  private CSharpFileLinesVisitor fileLinesVisitor;
  @Mock
  private FileLinesContextFactory fileLinesContextFactory;
  @Mock
  private FileLinesContext fileLinesContext;
  @Mock
  private Project project;
  @Mock
  private ProjectFileSystem fileSystem;

  @Before
  public void init() {
    MockitoAnnotations.initMocks(this);

    when(fileSystem.getSourceDirs()).thenReturn(Lists.newArrayList(TestUtils.getResource("/")));
    when(project.getFileSystem()).thenReturn(fileSystem);

    when(fileLinesContextFactory.createFor(any(Resource.class))).thenReturn(fileLinesContext);

    fileLinesVisitor = new CSharpFileLinesVisitor(project, fileLinesContextFactory);
  }

  @Test
  public void testScanFile() {
    AstScanner<Grammar> scanner = CSharpAstScanner.create(new CSharpConfiguration(Charset.forName("UTF-8")), fileLinesVisitor);
    scanner.scanFile(readFile("/metric/Money.cs"));
    SourceProject project = (SourceProject) scanner.getIndex().search(new QueryByType(SourceProject.class)).iterator().next();

    int lines = project.getInt(CSharpMetric.LINES);
    int loc = project.getInt(CSharpMetric.LINES_OF_CODE);
    int comments = project.getInt(CSharpMetric.COMMENT_LINES);

    verify(fileLinesContext, times(loc)).setIntValue(eq(CoreMetrics.NCLOC_DATA_KEY), anyInt(), eq(1));
    verify(fileLinesContext, times(lines - loc)).setIntValue(eq(CoreMetrics.NCLOC_DATA_KEY), anyInt(), eq(0));

    verify(fileLinesContext, times(comments)).setIntValue(eq(CoreMetrics.COMMENT_LINES_DATA_KEY), anyInt(), eq(1));
    verify(fileLinesContext, times(lines - comments)).setIntValue(eq(CoreMetrics.COMMENT_LINES_DATA_KEY), anyInt(), eq(0));
  }

  protected File readFile(String path) {
    return FileUtils.toFile(getClass().getResource(path));
  }

}
