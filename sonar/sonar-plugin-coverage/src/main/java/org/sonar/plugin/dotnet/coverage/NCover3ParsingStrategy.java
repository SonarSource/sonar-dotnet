package org.sonar.plugin.dotnet.coverage;

import org.w3c.dom.Element;

public class NCover3ParsingStrategy extends AbstractParsingStrategy {

	public NCover3ParsingStrategy() {
		setFilePath("/coverage/documents/doc[@url!=\"None\"]");
    setMethodPath("/coverage/module/class/method");
		setPointElement("seqpnt");
		setFileIdPointAttribute("doc");
		setCountVisitsPointAttribute("vc");
		setStartLinePointAttribute("l");
		setEndLinePointAttribute("el");
	}
	
	@Override
	String findAssemblyName(Element methodElement) {
		Element moduleElement = (Element) methodElement.getParentNode().getParentNode();
		String assemblyName = moduleElement.getAttribute("assembly");
		return assemblyName;
	}

	@Override
  boolean isCompatible(Element rootElement) {
	  return "coverage".equals(rootElement.getNodeName());
  }

}
