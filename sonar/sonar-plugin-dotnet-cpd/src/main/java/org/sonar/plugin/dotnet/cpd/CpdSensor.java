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

package org.sonar.plugin.dotnet.cpd;

import java.io.File;
import java.io.FilenameFilter;
import java.io.IOException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import net.sourceforge.pmd.cpd.CPD;
import net.sourceforge.pmd.cpd.CsLanguage;
import net.sourceforge.pmd.cpd.TokenEntry;

import org.apache.maven.dotnet.commons.project.DotNetProjectException;
import org.apache.maven.dotnet.commons.project.SourceFile;
import org.apache.maven.dotnet.commons.project.VisualStudioProject;
import org.apache.maven.dotnet.commons.project.VisualStudioSolution;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.CpdMapping;
import org.sonar.api.batch.Sensor;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.resources.Project;
import org.sonar.plugin.dotnet.core.project.VisualUtils;

/**
 * Copy/paste from the original CpdSensor of the java cpd plugin.
 * 
 * @author Alexandre VICTOOR
 * 
 */
public class CpdSensor implements Sensor {

	private final static Logger	log	= LoggerFactory.getLogger(CpdSensor.class);

	public CpdSensor() {

		// default empty constructor
	}

	private void saveResults(CPD cpd, CpdMapping mapping, Project project, SensorContext context) throws DotNetProjectException {

		VisualStudioSolution solution = VisualUtils.getSolution(project);
		List<VisualStudioProject> projects = solution.getProjects();
		List<File> sourceDirs = new ArrayList<File>();
		for (VisualStudioProject visualStudioProject : projects) {
			sourceDirs.add(visualStudioProject.getDirectory());
		}

		CpdAnalyser cpdAnalyser = new CpdAnalyser(context, mapping, sourceDirs);
		cpdAnalyser.analyse(cpd.getMatches());
	}

	public boolean shouldExecuteOnProject(Project project)
	  {
	    String packaging = project.getPackaging();
	    // We only accept the "sln" packaging
	    return "sln".equals(packaging);
	  }

	public void analyse(Project project, SensorContext context) {

		try {
			CpdMapping mapping = getMapping(project);
			CPD cpd = executeCPD(project, mapping, project.getFileSystem().getSourceCharset());
			saveResults(cpd, mapping, project, context);
		} catch (Exception e) {
			throw new CpdException(e);
		}
	}

	private CpdMapping getMapping(Project project) {

		return new CsCpdMappind(project);
	}

	private CPD executeCPD(Project project, CpdMapping mapping, Charset encoding) throws IOException, DotNetProjectException {

		CPD cpd = configureCPD(project, mapping, encoding);
		cpd.go();
		return cpd;

	}

	private CPD configureCPD(Project project, CpdMapping mapping, Charset encoding) throws IOException, DotNetProjectException {

		TokenEntry.clearImages();
		int minTokens = project.getConfiguration().getInt("sonar.cpd.minimumTokens", 100);

		;
		CPD cpd = new CPD(minTokens, new CsLanguage());
		cpd.setEncoding(encoding.name());
		cpd.add(getCsFiles(project));
		return cpd;
	}

	private List<File> getCsFiles(Project project) throws DotNetProjectException {

		VisualStudioSolution solution = VisualUtils.getSolution(project);
		List<VisualStudioProject> projects = solution.getProjects();
		FilenameFilter filter = new CsLanguage().getFileFilter();

		List<File> csFiles = new ArrayList<File>();

		for (VisualStudioProject visualStudioProject : projects) {
			if (visualStudioProject.isTest()) {
				log.debug("skipping test project " + visualStudioProject.getName());
			} else {
				Collection<SourceFile> sources = visualStudioProject.getSourceFiles();
				for (SourceFile sourceFile : sources) {
					if (filter.accept(sourceFile.getFile().getParentFile(), sourceFile.getName())) {
						csFiles.add(sourceFile.getFile());
					}
				}
			}
		}
		return csFiles;
	}

	public String toString() {

		return getClass().getSimpleName();
	}

}