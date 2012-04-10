package org.sonar.plugins.csharp.ndeps;

import static org.hamcrest.Matchers.is;
import static org.junit.Assert.assertThat;

import org.junit.Test;

public class NDepsPluginTest {

  @Test
  public void shouldDeclareCorrectExtensions() {
    NDepsPlugin plugin = new NDepsPlugin();
    assertThat(plugin.getExtensions().size(), is(3));
  }

}
