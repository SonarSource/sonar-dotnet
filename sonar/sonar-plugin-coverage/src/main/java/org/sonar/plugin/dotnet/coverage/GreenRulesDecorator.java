/*
 * Maven and Sonar plugin for .Net
 * Copyright (C) 2010 Jose Chillan and Alexandre Victoor
 * mailto: jose.chillan@codehaus.org or alexvictoor@codehaus.org
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02
 */

package org.sonar.plugin.dotnet.coverage;

import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.api.batch.Decorator;
import org.sonar.api.batch.DecoratorContext;
import org.sonar.api.batch.DependsUpon;
import org.sonar.api.measures.CoreMetrics;
import org.sonar.api.measures.Metric;
import org.sonar.api.profiles.RulesProfile;
import org.sonar.api.resources.Project;
import org.sonar.api.resources.Resource;
import org.sonar.api.resources.ResourceUtils;
import org.sonar.api.rules.ActiveRule;
import org.sonar.api.rules.Violation;

public class GreenRulesDecorator implements Decorator {
  
  private final static Logger log = LoggerFactory.getLogger(GreenRulesDecorator.class);
  
  private RulesProfile profile;

  public GreenRulesDecorator(RulesProfile profile) {
    this.profile = profile;
  }

  @Override
  public boolean shouldExecuteOnProject(Project project) {
    // TODO Auto-generated method stub
    return true;
  }
  
  @DependsUpon
  public List<Metric> generatesViolationsMetrics() {
    return Arrays.asList(CoreMetrics.VIOLATIONS,
        CoreMetrics.BLOCKER_VIOLATIONS, CoreMetrics.CRITICAL_VIOLATIONS, CoreMetrics.MAJOR_VIOLATIONS, CoreMetrics.MINOR_VIOLATIONS, CoreMetrics.INFO_VIOLATIONS);
  }

  @Override
  public void decorate(Resource resource, DecoratorContext context) {
    if (ResourceUtils.isProject(resource)) {
      
      List<ActiveRule> activeRules = profile.getActiveRules();
      Set<String> rulesKeys = new HashSet<String>();
      for (ActiveRule activeRule : activeRules) {
        rulesKeys.add(activeRule.getRuleKey());
      }
      
      log.info("Rules activated : {}", rulesKeys.size());
      
      List<Violation> violations = context.getViolations();
      for (Violation violation : violations) {
        String key = violation.getRule().getKey();
        rulesKeys.remove(key);
      }
      
      log.info("Rules without violations : {}", rulesKeys.size());
      
      for (String key : rulesKeys) {
        log.info("Rule : {}", key);
      }
    }

  }

}
