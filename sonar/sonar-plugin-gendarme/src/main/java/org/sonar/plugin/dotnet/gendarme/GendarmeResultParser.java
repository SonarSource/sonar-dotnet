package org.sonar.plugin.dotnet.gendarme;

import java.io.File;
import java.net.URL;
import java.text.ParseException;
import java.util.List;

import org.apache.commons.lang.StringUtils;
import org.sonar.api.batch.SensorContext;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Rule;
import org.sonar.api.rules.RulesManager;
import org.sonar.api.rules.Violation;
import org.sonar.api.utils.ParsingUtils;
import org.sonar.plugin.dotnet.core.AbstractXmlParser;
import org.sonar.plugin.dotnet.core.resource.CSharpFile;
import org.w3c.dom.Element;

public class GendarmeResultParser extends AbstractXmlParser {
	private Project project;
	private SensorContext context;
	private RulesManager rulesManager;
	private RulesProfile profile;

	/**
	 * Constructs a @link{GendarmeResultParser}.
	 * 
	 * @param project
	 * @param context
	 * @param rulesManager
	 * @param profile
	 */
	public GendarmeResultParser(Project project, SensorContext context,
	    RulesManager rulesManager, RulesProfile profile) {
		super();
		this.project = project;
		this.context = context;
		this.rulesManager = rulesManager;
		this.profile = profile;
	}
	
	/**
   * Parses a processed violation file.
   * 
   * @param stream
   */
  public void parse(URL url)
  {
    List<Element> issues = extractElements(url, "//issue");
    // We add each issue
    for (Element issueElement : issues)
    {
      
      String key = getNodeContent(issueElement, "key");
      String source = getNodeContent(issueElement, "source");
      String message = getNodeContent(issueElement, "message");
      
      if (source == null) {
      	continue;
      }
      
      String filePath = StringUtils.substringBefore(source, "(");
      String lineNumber = StringUtils.substring(StringUtils.substringBetween(source, "(", ")"), 1);
      
      Resource<?> resource = getResource(filePath);
      Integer line = getIntValue(lineNumber);
      Rule rule = rulesManager.getPluginRule(GendarmePlugin.KEY, key);
      if (rule == null)
      {
        // We skip the rules that were not registered
        continue;
      }
      ActiveRule activeRule = profile.getActiveRule(GendarmePlugin.KEY, key);
      Violation violation = new Violation(rule, resource);
      violation.setLineId(line);
      violation.setMessage(message);
      if (activeRule != null)
      {
        violation.setPriority(activeRule.getPriority());
      }

      // We store the violation
      context.saveViolation(violation);
    }
  }
  
  public Resource<?> getResource(String filePath)
  {
    if (StringUtils.isBlank(filePath))
    {
      return null;
    }
    File file = new File(filePath);
    CSharpFile fileResource = CSharpFile.from(project, file, false);
    return fileResource;
  }

  /**
   * Extracts the line number.
   * 
   * @param lineStr
   * @return
   */
  protected Integer getIntValue(String lineStr)
  {
    if (StringUtils.isBlank(lineStr))
    {
      return null;
    }
    try
    {
      return (int) ParsingUtils.parseNumber(lineStr);
    }
    catch (ParseException ignore)
    {
      return null;
    }
  }
}
