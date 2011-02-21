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
 * Created on Apr 22, 2010
 *
 */
package org.sonar.plugin.dotnet.srcmon.test;

import static org.hamcrest.CoreMatchers.equalTo;
import static org.hamcrest.CoreMatchers.is;

import org.junit.Assert;
import org.junit.Test;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.sonar.plugin.dotnet.srcmon.model.Distribution;
import org.sonar.plugin.dotnet.srcmon.model.DistributionClassification;
/**
 * Tests the distribution class.
 * @author Jose CHILLAN Apr 22, 2010
 */
public class TestDistribution
{
  
  private final static Logger log = LoggerFactory.getLogger(TestDistribution.class);
  
  @Test
  public void testSimpleDistribution()
  {
    DistributionClassification classification = new DistributionClassification(1, 5, 10, 20);
    Distribution distribA = new Distribution(classification);
    
    // Computes the distribution for a list of values
    for (int idx = 0; idx < 30; idx++)
    {
      distribA.addEntry(idx);
    }
    Assert.assertThat(distribA.toSonarRepresentation(), is(equalTo("1=5;5=5;10=10;20=10")));
  }
  
  
  @Test
  public void testDistributionCombining()
  {
    DistributionClassification classification = new DistributionClassification(1, 5, 10, 20);
    Distribution distribA = new Distribution(classification);
    Distribution distribB = new Distribution(classification);
    
    // Computes the distribution for a list of values
    for (int idx = 0; idx < 30; idx++)
    {
      distribA.addEntry(idx);
      distribB.addEntry(idx*2);
    }
    
    log.info("Distribution : " + distribA.toSonarRepresentation() );
    Assert.assertThat(distribA.toSonarRepresentation(), is(equalTo("1=5;5=5;10=10;20=10")));
    Assert.assertThat(distribB.toSonarRepresentation(), is(equalTo("1=3;5=2;10=5;20=20")));

    Distribution distribC = new Distribution(classification);
    distribC.combine(distribA);
    distribC.combine(distribB);
    Assert.assertThat(distribC.toSonarRepresentation(), is(equalTo("1=8;5=7;10=15;20=30")));
  }
  
  
}
