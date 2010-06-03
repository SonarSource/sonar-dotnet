package org.sonar.plugin.dotnet.gendarme;

import java.util.List;

import org.sonar.api.Extension;
import org.sonar.api.rules.ActiveRule;

public interface GendarmeRuleMarshaller extends Extension {

	public String marshall(List<ActiveRule> rules);
	
}
