package org.sonar.plugin.dotnet.partcover;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.w3c.dom.Element;

public class PartCover2ParsingStrategy extends AbstractParsingStrategy {

	private final static Logger log = LoggerFactory.getLogger(PartCover2ParsingStrategy.class);

	private String partcoverExactVersion;

	@Override
	String findAssemblyName(Element methodElement) {
		Element typeElement = (Element) methodElement.getParentNode();
    return typeElement.getAttribute("asm");
	}

	@Override
	boolean isCompatible(Element element) {
		String version = element.getAttribute("ver");
		// Evaluates the part cover version
		final boolean result;
		if (version.startsWith(partcoverExactVersion)) {
			log.debug("Using PartCover " + partcoverExactVersion + " report format");
			result = true;
		} else {
			log.debug("Not using PartCover " + partcoverExactVersion + " report format");
			result = false;
		}
		return result;
	}

	public void setPartcoverExactVersion(String partcoverExactVersion) {
		this.partcoverExactVersion = partcoverExactVersion;
	}

}
