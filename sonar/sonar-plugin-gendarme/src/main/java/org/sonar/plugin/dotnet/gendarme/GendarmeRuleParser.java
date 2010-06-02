package org.sonar.plugin.dotnet.gendarme;

import java.util.List;

import org.sonar.api.Extension;

public interface GendarmeRuleParser extends Extension {

	public List<GendarmeRule> parseRuleConfiguration(
	    String rawConfiguration);

}