/*
 * Sonar C# Plugin :: C# Squid :: Sonar Plugin
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
package com.sonar.plugins.csharp.squid;

import com.google.common.collect.Lists;
import com.sonar.csharp.checks.CheckList;
import com.sonar.csharp.squid.CSharpConfiguration;
import com.sonar.csharp.squid.api.CSharpGrammar;
import com.sonar.csharp.squid.api.CSharpMetric;
import com.sonar.csharp.squid.api.source.SourceClass;
import com.sonar.csharp.squid.api.source.SourceMember;
import com.sonar.csharp.squid.metric.CSharpFileLinesVisitor;
import com.sonar.csharp.squid.scanner.CSharpAstScanner;
import com.sonar.plugins.csharp.squid.check.CSharpCheck;
import com.sonar.sslr.squid.AstScanner;
import com.sonar.sslr.squid.SquidAstVisitor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.batch.Phase;
import org.sonar.api.batch.ResourceCreationLock;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.checks.AnnotationCheckFactory;
import org.sonar.api.checks.NoSonarFilter;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.FileLinesContextFactory;
import org.sonar.api.measures.PersistenceMode;
import org.sonar.api.measures.RangeDistributionBuilder;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.File;
import org.sonar.api.resources.InputFile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.Violation;
import org.sonar.plugins.csharp.api.CSharp;
import org.sonar.plugins.csharp.api.CSharpConstants;
import org.sonar.plugins.dotnet.api.DotNetConfiguration;
import org.sonar.plugins.dotnet.api.MicrosoftWindowsEnvironment;
import org.sonar.plugins.dotnet.api.sensor.AbstractRegularDotNetSensor;
import org.sonar.squid.api.CheckMessage;
import org.sonar.squid.api.SourceCode;
import org.sonar.squid.api.SourceFile;
import org.sonar.squid.indexer.QueryByParent;
import org.sonar.squid.indexer.QueryByType;

import java.util.Collection;
import java.util.List;
import java.util.Locale;
import java.util.Set;

@DependsUpon(CSharpConstants.CSHARP_CORE_EXECUTED)
@Phase(name = Phase.Name.PRE)
public final class CSharpSquidSensor extends AbstractRegularDotNetSensor {

  private static final Logger LOG = LoggerFactory.getLogger(CSharpSquidSensor.class);
  private static final Number[] METHOD_DISTRIB_BOTTOM_LIMITS = {1, 2, 4, 6, 8, 10, 12};
  private static final Number[] CLASS_DISTRIB_BOTTOM_LIMITS = {0, 5, 10, 20, 30, 60, 90};

  private final CSharp cSharp;
  private final CSharpResourcesBridge cSharpResourcesBridge;
  private final ResourceCreationLock resourceCreationLock;
  private final NoSonarFilter noSonarFilter;
  private final AnnotationCheckFactory annotationCheckFactory;
  private final FileLinesContextFactory fileLinesContextFactory;

  private Project project;
  private SensorContext context;
  private AstScanner<CSharpGrammar> scanner;

  public CSharpSquidSensor(DotNetConfiguration dotNetConfiguration, CSharp cSharp, CSharpResourcesBridge cSharpResourcesBridge, ResourceCreationLock resourceCreationLock,
      MicrosoftWindowsEnvironment microsoftWindowsEnvironment, RulesProfile profile, NoSonarFilter noSonarFilter, FileLinesContextFactory fileLinesContextFactory) {
    this(dotNetConfiguration, cSharp, cSharpResourcesBridge, resourceCreationLock, microsoftWindowsEnvironment, profile, noSonarFilter, fileLinesContextFactory,
        new CSharpCheck[] {});
  }

  public CSharpSquidSensor(DotNetConfiguration dotNetConfiguration, CSharp cSharp, CSharpResourcesBridge cSharpResourcesBridge, ResourceCreationLock resourceCreationLock,
      MicrosoftWindowsEnvironment microsoftWindowsEnvironment, RulesProfile profile, NoSonarFilter noSonarFilter, FileLinesContextFactory fileLinesContextFactory,
      CSharpCheck[] cSharpChecks) {
    super(cSharp, dotNetConfiguration, microsoftWindowsEnvironment, "Squid C#", "");
    this.cSharp = cSharp;
    this.cSharpResourcesBridge = cSharpResourcesBridge;
    this.resourceCreationLock = resourceCreationLock;
    this.noSonarFilter = noSonarFilter;
    this.fileLinesContextFactory = fileLinesContextFactory;

    Collection<Class> allChecks = CSharpCheck.toCollection(cSharpChecks);
    allChecks.addAll(CheckList.getChecks());
    this.annotationCheckFactory = AnnotationCheckFactory.create(profile, CSharpSquidConstants.REPOSITORY_KEY, allChecks);
  }

  @Override
  @SuppressWarnings("unchecked")
  public void analyse(Project project, SensorContext context) {
    this.project = project;
    this.context = context;

    Collection<SquidAstVisitor<CSharpGrammar>> squidChecks = annotationCheckFactory.getChecks();
    List<SquidAstVisitor<CSharpGrammar>> visitors = Lists.newArrayList(squidChecks);
    // TODO: remove the following line & class once SSLR Squid bridge computes NCLOC_DATA_KEY & COMMENT_LINES_DATA_KEY
    visitors.add(new CSharpFileLinesVisitor(project, fileLinesContextFactory));
    scanner = CSharpAstScanner.create(createParserConfiguration(project), (SquidAstVisitor<CSharpGrammar>[]) visitors.toArray(new SquidAstVisitor[visitors.size()]));
    scanner.scanFiles(getFilesToAnalyse(project));

    Collection<SourceCode> squidSourceFiles = scanner.getIndex().search(new QueryByType(SourceFile.class));
    saveMeasures(squidSourceFiles);
  }

  private List<java.io.File> getFilesToAnalyse(Project project) {
    List<java.io.File> result = Lists.newArrayList();
    for (InputFile file : project.getFileSystem().mainFiles(cSharp.getKey())) {
      result.add(file.getFile());
    }

    return result;
  }

  private CSharpConfiguration createParserConfiguration(Project project) {
    CSharpConfiguration conf = new CSharpConfiguration(project.getFileSystem().getSourceCharset());
    conf.setIgnoreHeaderComments(project.getConfiguration().getBoolean(CSharpSquidConstants.IGNORE_HEADER_COMMENTS, true));
    return conf;
  }

  private void saveMeasures(Collection<SourceCode> sourceFiles) {
    for (SourceCode squidFileCode : sourceFiles) {
      SourceFile squidFile = (SourceFile) squidFileCode;

      /* Create the sonar file */
      File sonarFile = File.fromIOFile(new java.io.File(squidFile.getKey()), project);
      sonarFile.setLanguage(cSharp);

      /* Fill the resource bridge API that can be used by other C# plugins to map logical resources to physical ones */
      cSharpResourcesBridge.indexFile(squidFile, sonarFile);

      /* No Sonar */
      noSonarFilter.addResource(sonarFile, squidFile.getNoSonarTagLines());

      /* Classes complexity distribution */
      saveClassesComplexityDistribution(sonarFile, squidFile);

      /* Methods complexity distribution */
      saveMethodsComplexityDistribution(sonarFile, squidFile);

      /* Check messages */
      saveViolations(squidFile, sonarFile);

      /* Metrics at the file level */
      saveMeasures(sonarFile, squidFile);
    }

    // and lock everything to prevent future modifications
    LOG.debug("Locking the C# Resource Bridge and the Sonar Index: future modifications won't be possible.");
    cSharpResourcesBridge.lock();
    resourceCreationLock.lock();
  }

  private void saveMeasures(Resource sonarFile, SourceCode squidFile) {
    context.saveMeasure(sonarFile, CoreMetrics.CLASSES, squidFile.getDouble(CSharpMetric.CLASSES));
    context.saveMeasure(sonarFile, CoreMetrics.FUNCTIONS, squidFile.getDouble(CSharpMetric.METHODS));
    context.saveMeasure(sonarFile, CoreMetrics.FILES, squidFile.getDouble(CSharpMetric.FILES));
    context.saveMeasure(sonarFile, CoreMetrics.LINES, squidFile.getDouble(CSharpMetric.LINES));
    context.saveMeasure(sonarFile, CoreMetrics.NCLOC, squidFile.getDouble(CSharpMetric.LINES_OF_CODE));
    context.saveMeasure(sonarFile, CoreMetrics.STATEMENTS, squidFile.getDouble(CSharpMetric.STATEMENTS));
    context.saveMeasure(sonarFile, CoreMetrics.ACCESSORS, squidFile.getDouble(CSharpMetric.ACCESSORS));
    context.saveMeasure(sonarFile, CoreMetrics.COMPLEXITY, squidFile.getDouble(CSharpMetric.COMPLEXITY));
    context.saveMeasure(sonarFile, CoreMetrics.COMMENT_BLANK_LINES, squidFile.getDouble(CSharpMetric.COMMENT_BLANK_LINES));
    context.saveMeasure(sonarFile, CoreMetrics.COMMENTED_OUT_CODE_LINES, squidFile.getDouble(CSharpMetric.COMMENTED_OUT_CODE_LINES));
    context.saveMeasure(sonarFile, CoreMetrics.COMMENT_LINES, squidFile.getDouble(CSharpMetric.COMMENT_LINES));
    context.saveMeasure(sonarFile, CoreMetrics.PUBLIC_API, squidFile.getDouble(CSharpMetric.PUBLIC_API));
    context.saveMeasure(sonarFile, CoreMetrics.PUBLIC_UNDOCUMENTED_API,
        squidFile.getDouble(CSharpMetric.PUBLIC_API) - squidFile.getDouble(CSharpMetric.PUBLIC_DOC_API));
  }

  private void saveViolations(SourceCode squidFile, File sonarFile) {
    Set<CheckMessage> messages = squidFile.getCheckMessages();
    if (messages != null) {
      for (CheckMessage message : messages) {
        @SuppressWarnings("unchecked")
        Violation violation = Violation.create(annotationCheckFactory.getActiveRule(message.getChecker()), sonarFile);
        violation.setLineId(message.getLine());
        violation.setMessage(message.getText(Locale.ENGLISH));
        context.saveViolation(violation);
      }
    }
  }

  private void saveClassesComplexityDistribution(File sonarFile, SourceFile squidFile) {
    Collection<SourceCode> squidClasses = scanner.getIndex().search(new QueryByParent(squidFile), new QueryByType(SourceClass.class));
    RangeDistributionBuilder complexityClassDistribution = new RangeDistributionBuilder(CoreMetrics.CLASS_COMPLEXITY_DISTRIBUTION,
        CLASS_DISTRIB_BOTTOM_LIMITS);

    for (SourceCode squidClass : squidClasses) {
      complexityClassDistribution.add(squidClass.getDouble(CSharpMetric.COMPLEXITY));
    }

    context.saveMeasure(sonarFile, complexityClassDistribution.build().setPersistenceMode(PersistenceMode.MEMORY));
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
