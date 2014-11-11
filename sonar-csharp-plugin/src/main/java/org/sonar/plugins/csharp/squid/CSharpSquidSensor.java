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
package org.sonar.plugins.csharp.squid;

import com.google.common.collect.Lists;
import com.sonar.csharp.checks.CheckList;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceMember;
import com.sonar.csharp.squid.metric.CSharpFileLinesVisitor;
import com.sonar.csharp.squid.metric.FileProvider;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.sslr.api.Grammar;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.checks.AnnotationCheckFactory;
import org.sonar.api.config.Settings;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.measures.PersistenceMode;
import org.sonar.api.measures.RangeDistributionBuilder;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.File;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.Violation;
import org.sonar.api.scan.filesystem.FileQuery;
import org.sonar.api.scan.filesystem.ModuleFileSystem;
import org.sonar.plugins.csharp.api.CSharp;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.csharp.squid.check.CSharpCheck;
import org.sonar.squidbridge.AstScanner;
import org.sonar.squidbridge.SquidAstVisitor;
import org.sonar.squidbridge.api.CheckMessage;
import org.sonar.squidbridge.api.SourceCode;
import org.sonar.squidbridge.api.SourceFile;
import org.sonar.squidbridge.indexer.QueryByParent;
import org.sonar.squidbridge.indexer.QueryByType;

import java.util.Collection;
import java.util.List;
import java.util.Locale;
import java.util.Set;

public final class CSharpSquidSensor implements Sensor {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpSquidSensor.class);
  private static final Number[] METHOD_DISTRIB_BOTTOM_LIMITS = {1, 2, 4, 6, 8, 10, 12};
  private static final Number[] FILES_DISTRIB_BOTTOM_LIMITS = {0, 5, 10, 20, 30, 60, 90};

  private final Settings settings;
  private final CSharp cSharp;
  private final ModuleFileSystem fileSystem;
  private final AnnotationCheckFactory annotationCheckFactory;
  private final FileLinesContextFactory fileLinesContextFactory;

  private Project project;
  private SensorContext context;
  private AstScanner<Grammar> scanner;

  public CSharpSquidSensor(Settings settings, CSharp cSharp,
    ModuleFileSystem fileSystem, RulesProfile profile, FileLinesContextFactory fileLinesContextFactory) {
    this(settings, cSharp, fileSystem, profile, fileLinesContextFactory, new CSharpCheck[0]);
  }

  public CSharpSquidSensor(Settings settings, CSharp cSharp,
    ModuleFileSystem fileSystem, RulesProfile profile, FileLinesContextFactory fileLinesContextFactory,
    CSharpCheck[] cSharpChecks) {
    this.settings = settings;
    this.cSharp = cSharp;
    this.fileSystem = fileSystem;
    this.fileLinesContextFactory = fileLinesContextFactory;

    Collection<Class> allChecks = CSharpCheck.toCollection(cSharpChecks);
    allChecks.addAll(CheckList.getChecks());
    this.annotationCheckFactory = AnnotationCheckFactory.create(profile, CSharpSquidConstants.REPOSITORY_KEY, allChecks);
  }

  @DependsUpon
  public String dependsUponNSonarQubeAnalysis() {
    return "NSonarQubeAnalysis";
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    return !filesToAnalyze().isEmpty();
  }

  @Override
  public void analyse(Project project, SensorContext context) {
    this.project = project;
    this.context = context;

    Collection<SquidAstVisitor<Grammar>> squidChecks = annotationCheckFactory.getChecks();
    List<SquidAstVisitor<Grammar>> visitors = Lists.newArrayList(squidChecks);
    // TODO: remove the following line & class once SSLR Squid bridge computes NCLOC_DATA_KEY & COMMENT_LINES_DATA_KEY
    visitors.add(new CSharpFileLinesVisitor(new FileProvider(project), fileLinesContextFactory));
    scanner = CSharpAstScanner.create(createParserConfiguration(project), visitors.toArray(new SquidAstVisitor[visitors.size()]));
    scanner.scanFiles(filesToAnalyze());

    Collection<SourceCode> squidSourceFiles = scanner.getIndex().search(new QueryByType(SourceFile.class));
    saveMeasures(squidSourceFiles);
  }

  private List<java.io.File> filesToAnalyze() {
    return fileSystem.files(FileQuery.onSource().onLanguage(CSharpConstants.LANGUAGE_KEY));
  }

  private CSharpConfiguration createParserConfiguration(Project project) {
    CSharpConfiguration conf = new CSharpConfiguration(fileSystem.sourceCharset());
    conf.setIgnoreHeaderComments(settings.getBoolean(CSharpSquidConstants.IGNORE_HEADER_COMMENTS));
    return conf;
  }

  private void saveMeasures(Collection<SourceCode> sourceFiles) {
    for (SourceCode squidFileCode : sourceFiles) {
      SourceFile squidFile = (SourceFile) squidFileCode;

      /* Create the sonar file */
      File sonarFile = File.fromIOFile(new java.io.File(squidFile.getKey()), project);
      sonarFile.setLanguage(cSharp);

      /* Files complexity distribution */
      saveFilesComplexityDistribution(sonarFile, squidFile);

      /* Methods complexity distribution */
      saveMethodsComplexityDistribution(sonarFile, squidFile);

      /* Check messages */
      saveViolations(squidFile, sonarFile);

      /* Metrics at the file level */
      saveMeasures(sonarFile, squidFile);
    }
  }

  private void saveMeasures(Resource sonarFile, SourceCode squidFile) {
    context.saveMeasure(sonarFile, CoreMetrics.CLASSES, squidFile.getDouble(CSharpMetric.CLASSES));
    context.saveMeasure(sonarFile, CoreMetrics.FUNCTIONS, squidFile.getDouble(CSharpMetric.METHODS));
    context.saveMeasure(sonarFile, CoreMetrics.FILES, squidFile.getDouble(CSharpMetric.FILES));
    context.saveMeasure(sonarFile, CoreMetrics.NCLOC, squidFile.getDouble(CSharpMetric.LINES_OF_CODE));
    context.saveMeasure(sonarFile, CoreMetrics.STATEMENTS, squidFile.getDouble(CSharpMetric.STATEMENTS));
    context.saveMeasure(sonarFile, CoreMetrics.ACCESSORS, squidFile.getDouble(CSharpMetric.ACCESSORS));
    context.saveMeasure(sonarFile, CoreMetrics.COMPLEXITY, squidFile.getDouble(CSharpMetric.COMPLEXITY));
    context.saveMeasure(sonarFile, CoreMetrics.PUBLIC_API, squidFile.getDouble(CSharpMetric.PUBLIC_API));
    context.saveMeasure(sonarFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API,
      squidFile.getDouble(CSharpMetric.PUBLIC_API) - squidFile.getDouble(CSharpMetric.PUBLIC_DOC_API));
  }

  private void saveViolations(SourceCode squidFile, File sonarFile) {
    Set<CheckMessage> messages = squidFile.getCheckMessages();
    if (messages != null) {
      for (CheckMessage message : messages) {
        @SuppressWarnings("unchecked")
        Violation violation = Violation.create(annotationCheckFactory.getActiveRule(message.getCheck()), sonarFile);
        violation.setLineId(message.getLine());
        violation.setMessage(message.getText(Locale.ENGLISH));
        context.saveViolation(violation);
      }
    }
  }

  private void saveFilesComplexityDistribution(File sonarFile, SourceFile squidFile) {
    RangeDistributionBuilder complexityDistribution = new RangeDistributionBuilder(CoreMetrics.FILE_COMPLEXITY_DISTRIBUTION, FILES_DISTRIB_BOTTOM_LIMITS);
    complexityDistribution.add(squidFile.getDouble(CSharpMetric.COMPLEXITY));
    context.saveMeasure(sonarFile, complexityDistribution.build().setPersistenceMode(PersistenceMode.MEMORY));
  }

  private void saveMethodsComplexityDistribution(File sonarFile, SourceFile squidFile) {
    Collection<SourceCode> squidMethods = scanner.getIndex().search(new QueryByParent(squidFile), new QueryByType(SourceMember.class));
    RangeDistributionBuilder complexityMethodDistribution = new RangeDistributionBuilder(CoreMetrics.FUNCTION_COMPLEXITY_DISTRIBUTION,
      METHOD_DISTRIB_BOTTOM_LIMITS);

    for (SourceCode squidMethod : squidMethods) {
      complexityMethodDistribution.add(squidMethod.getDouble(CSharpMetric.COMPLEXITY));
    }

    context.saveMeasure(sonarFile, complexityMethodDistribution.build().setPersistenceMode(PersistenceMode.MEMORY));
  }

}
