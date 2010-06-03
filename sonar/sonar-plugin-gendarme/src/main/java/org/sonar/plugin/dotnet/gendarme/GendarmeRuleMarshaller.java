package org.sonar.plugin.dotnet.gendarme;

import java.util.List;

import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;
import org.sonar.api.rules.ActiveRule;

public interface GendarmeRuleMarshaller extends ServerExtension, BatchExtension {

	public String marshall(List<ActiveRule> rules);
	
}
