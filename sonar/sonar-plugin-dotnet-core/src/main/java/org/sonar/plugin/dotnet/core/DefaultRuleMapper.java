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

/*
 * Created on Sep 1, 2009
 */
package org.sonar.plugin.dotnet.core;

import org.sonar.api.rules.RulePriority;
import org.sonar.api.rules.RulePriorityMapper;

/**
 * A default implementation for the {@link RulePriorityMapper} interface.
 * 
 * @author Jose CHILLAN Sep 1, 2009
 */
public class DefaultRuleMapper implements RulePriorityMapper<String, String> {
  /**
   * Constructs a @link{DefaultRuleMapper}.
   */
  public DefaultRuleMapper() {
  }

  /**
   * @param priority
   * @return
   */
  @Override
  public RulePriority from(String level) {
    if ("1".equals(level)) {
      return RulePriority.BLOCKER;
    }
    if ("2".equals(level)) {
      return RulePriority.CRITICAL;
    }
    if ("3".equals(level)) {
      return RulePriority.MAJOR;
    }
    if ("4".equals(level)) {
      return RulePriority.MINOR;
    }
    if ("5".equals(level)) {
      return RulePriority.INFO;
    }
    return null;
  }

  /**
   * @param priority
   * @return
   */
  @Override
  public String to(RulePriority priority) {
    if (priority.equals(RulePriority.BLOCKER)) {
      return "1";
    }
    if (priority.equals(RulePriority.CRITICAL)) {
      return "2";
    }
    if (priority.equals(RulePriority.MAJOR)) {
      return "3";
    }
    if (priority.equals(RulePriority.MINOR)) {
      return "4";
    }
    if (priority.equals(RulePriority.INFO)) {
      return "5";
    }
    throw new IllegalArgumentException("Level not supported: " + priority);

  }

}
