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

import java.util.Arrays;
import java.util.List;
import org.sonar.api.batch.AbstractSumChildrenDecorator;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.api.resources.*;

public class SumDuplicationsDecorator extends AbstractSumChildrenDecorator {

	public SumDuplicationsDecorator() {

	}

	public List generatesMetrics() {

		return Arrays.asList(new Metric[] { CoreMetrics.DUPLICATED_BLOCKS, CoreMetrics.DUPLICATED_FILES, CoreMetrics.DUPLICATED_LINES });
	}

	protected boolean shouldSaveZeroIfNoChildMeasures() {

		return true;
	}

	public boolean shouldExecuteOnProject(Project project) {

		return true;
	}

	public boolean shouldDecorateResource(Resource resource) {

		return !ResourceUtils.isUnitTestClass(resource);
	}
}
