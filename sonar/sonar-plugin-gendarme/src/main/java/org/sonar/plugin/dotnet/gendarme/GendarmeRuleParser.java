package org.sonar.plugin.dotnet.gendarme;

import java.util.List;

import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;

public interface GendarmeRuleParser extends ServerExtension, BatchExtension {

	public List<GendarmeRule> parseRuleConfiguration(
	    String rawConfiguration);

}