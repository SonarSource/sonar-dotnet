/**
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexandre.victoor@codehaus.org
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
import java.util.*;
import net.sourceforge.pmd.cpd.Match;
import net.sourceforge.pmd.cpd.TokenEntry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.CpdMapping;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Measure;
import org.sonar.api.resources.*;

/**
 * A CPD analyzer class 
 * @author Alexandre VICTOOR Apr 6, 2010
 */
public class CpdAnalyser {
	private static final Logger	LOG	= LoggerFactory.getLogger(CpdAnalyser.class);

	private SensorContext		context;
	private CpdMapping			cpdMapping;
	private List<File>			sourceDirs;

	public CpdAnalyser(SensorContext context, CpdMapping cpdMapping, List<File> sourceDirs) {

		this.cpdMapping = cpdMapping;
		this.sourceDirs = sourceDirs;
		this.context = context;
	}

	public void analyse(Iterator<Match> matches) {

		Map<Resource, DuplicationsData> duplicationsData = new HashMap<Resource, DuplicationsData>();
		while (matches.hasNext()) {
			Match match = matches.next();
			int duplicatedLines = match.getLineCount();

			TokenEntry firstMark = match.getFirstMark();
			String filename1 = firstMark.getTokenSrcID();
			int line1 = firstMark.getBeginLine();
			Resource file1 = cpdMapping.createResource(new File(filename1), sourceDirs);
			if (file1 == null) {
				LOG.warn("CPD - File not found : {}", filename1);
				continue;
			}

			TokenEntry secondMark = match.getSecondMark();
			String filename2 = secondMark.getTokenSrcID();
			int line2 = secondMark.getBeginLine();
			Resource file2 = cpdMapping.createResource(new File(filename2), sourceDirs);
			if (file2 == null) {
				LOG.warn("CPD - File not found : {}", filename2);
				continue;
			}

			processClassMeasure(duplicationsData, file2, line2, file1, line1, duplicatedLines);
			processClassMeasure(duplicationsData, file1, line1, file2, line2, duplicatedLines);
		}

		for (DuplicationsData data : duplicationsData.values()) {
			data.saveUsing(context);
		}
	}

	private void processClassMeasure(Map<Resource, DuplicationsData> fileContainer, Resource file, int duplicationStartLine, Resource targetFile, int targetDuplicationStartLine, int duplicatedLines) {

		if (file != null && targetFile != null) {
			DuplicationsData data = fileContainer.get(file);
			if (data == null) {
				data = new DuplicationsData(file, context);
				fileContainer.put(file, data);
			}
			data.cumulate(targetFile, (double) targetDuplicationStartLine, (double) duplicationStartLine, (double) duplicatedLines);
		}
	}

	private class DuplicationsData {
		protected double			duplicatedLines;
		protected double			duplicatedBlocks;
		protected Resource			resource;
		private SensorContext		context;
		private List<StringBuilder>	duplicationXMLEntries	= new ArrayList<StringBuilder>();

		private DuplicationsData(Resource resource, SensorContext context) {

			this.context = context;
			this.resource = resource;
		}

		protected void cumulate(Resource targetResource, Double targetDuplicationStartLine, Double duplicationStartLine, Double duplicatedLines) {

			StringBuilder xml = new StringBuilder();
			xml.append("<duplication lines=\"").append(duplicatedLines.intValue()).append("\" start=\"").append(duplicationStartLine.intValue()).append("\" target-start=\"").append(targetDuplicationStartLine.intValue()).append(
					"\" target-resource=\"").append(context.saveResource(targetResource)).append("\"/>");

			duplicationXMLEntries.add(xml);

			this.duplicatedLines += duplicatedLines;
			this.duplicatedBlocks++;
		}

		protected void saveUsing(SensorContext context) {

			context.saveMeasure(resource, CoreMetrics.DUPLICATED_FILES, 1d);
			context.saveMeasure(resource, CoreMetrics.DUPLICATED_LINES, duplicatedLines);
			context.saveMeasure(resource, CoreMetrics.DUPLICATED_BLOCKS, duplicatedBlocks);
			context.saveMeasure(resource, new Measure(CoreMetrics.DUPLICATIONS_DATA, getDuplicationXMLData()));
		}

		private String getDuplicationXMLData() {

			StringBuilder duplicationXML = new StringBuilder("<duplications>");
			for (StringBuilder xmlEntry : duplicationXMLEntries) {
				duplicationXML.append(xmlEntry);
			}
			duplicationXML.append("</duplications>");
			return duplicationXML.toString();
		}
	}
}
