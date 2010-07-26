/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * Sonar is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * Sonar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Sonar; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

/*
 * Created on May 14, 2009
 */
package org.sonar.plugin.dotnet.coverage;

import static org.sonar.plugin.dotnet.core.Constant.SONAR_EXCLUDE_GEN_CODE_KEY;

import java.io.File;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.List;
import java.util.Map;

import org.apache.maven.dotnet.commons.GeneratedCodeFilter;
import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.batch.maven.MavenPluginHandler;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.measures.PersistenceMode;
import org.sonar.api.measures.PropertiesBuilder;
import org.sonar.api.resources.Project;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.plugin.dotnet.core.AbstractDotnetSensor;
import org.sonar.plugin.dotnet.core.project.VisualUtils;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.sonar.plugin.dotnet.core.resource.CLRAssembly;
import org.sonar.plugin.dotnet.coverage.model.CoverableSource;
import org.sonar.plugin.dotnet.coverage.model.FileCoverage;
import org.sonar.plugin.dotnet.coverage.model.ProjectCoverage;
import org.sonar.plugin.dotnet.coverage.model.SourceLine;

/**
 * Collects the results from a PartCover report. Most of the work is delegator
 * to {@link PartCoverResultParser}.
 * 
 * @author Jose CHILLAN May 14, 2009
 */
public class PartCoverSensor extends AbstractDotnetSensor {
	public static final String PART_COVER_REPORT_XML = "coverage-report.xml";
	private final static Logger log = LoggerFactory
	    .getLogger(PartCoverSensor.class);
	private final PropertiesBuilder<String, Integer> lineHitsBuilder = new PropertiesBuilder<String, Integer>(
	    CoreMetrics.COVERAGE_LINE_HITS_DATA);
	private CoveragePluginHandler pluginHandler;

	/**
	 * Constructs the collector Constructs a @link{PartCoverCollector}.
	 */
	public PartCoverSensor(CoveragePluginHandler pluginHandler) {
		this.pluginHandler = pluginHandler;
	}

	/**
	 * Proceeds to the analysis.
	 * 
	 * @param project
	 * @param context
	 */
	@Override
	public void analyse(Project project, SensorContext context) {
		File report = findReport(project, PART_COVER_REPORT_XML);
		if (report == null) {
			return;
		}
		PartCoverResultParser parser = new PartCoverResultParser();
		URL url;
		try {
			url = report.toURI().toURL();
		} catch (MalformedURLException e) {
			return;
		}

		// We parse the file
		parser.parse(url);
		List<FileCoverage> files = parser.getFiles();
		List<ProjectCoverage> projects = parser.getProjects();

		boolean excludeGeneratedCode = 
    	project.getConfiguration().getBoolean(SONAR_EXCLUDE_GEN_CODE_KEY, true);
		
		// Collect the files
		for (FileCoverage fileCoverage : files) {
			File sourcePath = fileCoverage.getFile();
      if (excludeGeneratedCode && GeneratedCodeFilter.INSTANCE.isGenerated(sourcePath.getName())) {
      	// we will not include the generated code
      	// in the sonar database
      	log.info("Ignoring generated cs file "+sourcePath);
      	continue;
      }
			collectFile(project, context, fileCoverage);
		}

		// Collect the projects
		int countLines = 0;
		int coveredLines = 0;

		for (ProjectCoverage projectCoverage : projects) {
			collectAssembly(project, context, projectCoverage);
			countLines += projectCoverage.getCountLines();
			coveredLines += projectCoverage.getCoveredLines();
		}

		// Computes the global coverage
		double coverage = Math.round(100. * coveredLines / countLines) * 0.01;
		context.saveMeasure(CoreMetrics.COVERAGE, convertPercentage(coverage));
		context.saveMeasure(CoverageMetrics.ELOC, (double) countLines);
	}

	/**
	 * Collects the coverage at the assembly level
	 * 
	 * @param context
	 * @param projectCoverage
	 */
	private void collectAssembly(Project project, SensorContext context,
	    ProjectCoverage projectCoverage) {
		double coverage = projectCoverage.getCoverage();
		String assemblyName = projectCoverage.getAssemblyName();
		VisualStudioSolution solution;
		try {
			solution = VisualUtils.getSolution(project);

			VisualStudioProject visualProject = solution.getProject(assemblyName);
			if (visualProject != null) {
				CLRAssembly assemblyResource = new CLRAssembly(visualProject);
				context.saveMeasure(assemblyResource, CoreMetrics.COVERAGE,
				    convertPercentage(coverage));
				context.saveMeasure(assemblyResource, CoverageMetrics.ELOC,
				    (double) projectCoverage.getCountLines());
			}
		} catch (DotNetProjectException e) {
			log.debug("Could not find a .Net project : ", e);
		}
	}

	/**
	 * Collects the results for a class
	 * 
	 * @param context
	 * @param classCoverage
	 */
	private void collectFile(Project project, SensorContext context,
	    FileCoverage fileCoverage) {
		File filePath = fileCoverage.getFile();
		CSharpFile fileResource = CSharpFile.from(project, filePath, false);
		double coverage = fileCoverage.getCoverage();
		// We have the effective number of lines here
		context.saveMeasure(fileResource, CoverageMetrics.ELOC,
		    (double) fileCoverage.getCountLines());

		context.saveMeasure(fileResource, CoreMetrics.COVERAGE,
		    convertPercentage(coverage));
		context.saveMeasure(fileResource, getHitData(fileCoverage));
	}

	/**
	 * Generates a measure that contains the visits of each line of the source
	 * file.
	 * 
	 * @param coverable
	 *          the source file result
	 * @return a measure to store
	 */
	private Measure getHitData(CoverableSource coverable) {
		lineHitsBuilder.clear();
		Map<Integer, SourceLine> lines = coverable.getLines();
		for (SourceLine line : lines.values()) {
			int lineNumber = line.getLineNumber();
			int countVisits = line.getCountVisits();
			lineHitsBuilder.add(Integer.toString(lineNumber), countVisits);
		}
		Measure hitData = lineHitsBuilder.build().setPersistenceMode(
		    PersistenceMode.DATABASE);
		return hitData;
	}

	/**
	 * Gets the plugin handle.
	 * 
	 * @param project
	 *          he project to process.
	 * @return the plugin handler for the project
	 */
	@Override
	public MavenPluginHandler getMavenPluginHandler(Project project) {
		return pluginHandler;
	}

	/**
	 * Converts a number to a percentage
	 * 
	 * @param percentage
	 * @return
	 */
	private double convertPercentage(Number percentage) {
		return ParsingUtils.scaleValue(percentage.doubleValue() * 100.0);
	}

}
