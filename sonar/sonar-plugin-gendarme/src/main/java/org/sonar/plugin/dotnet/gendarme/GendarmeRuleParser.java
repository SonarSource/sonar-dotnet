package org.sonar.plugin.dotnet.gendarme;

import java.util.Collection;
import org.sonar.api.BatchExtension;
import org.sonar.api.ServerExtension;

/**
 * Implementations of this interfaces have the 
 * responsibility of parsing gendarme xml config files
 * 
 * @author Alexandre Victoor
 *
 */
public interface GendarmeRuleParser extends ServerExtension, BatchExtension {

	public Collection<GendarmeRule> parseRuleConfiguration(
	    String rawConfiguration);

}