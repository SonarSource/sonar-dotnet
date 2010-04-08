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
import java.util.List;

import net.sourceforge.pmd.cpd.CsLanguage;
import net.sourceforge.pmd.cpd.Tokenizer;

import org.sonar.api.batch.CpdMapping;
import org.sonar.api.resources.Language;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.plugin.dotnet.core.CSharp;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;

public class CsCpdMappind implements CpdMapping {
	
	private CsLanguage language = new CsLanguage();
	private final Project	project;
	
	public CsCpdMappind(Project project) {
		this.project = project;
	}
	

	public Resource createResource(File file, List<File> sourceDirs) {
		return CSharpFile.from(project, file, false);
	}

	public Language getLanguage() {
		return CSharp.INSTANCE;
	}

	public Tokenizer getTokenizer() {
		return language.getTokenizer();
	}

}
