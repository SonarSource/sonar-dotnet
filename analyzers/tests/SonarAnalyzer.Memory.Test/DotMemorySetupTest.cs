/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using JetBrains.dotMemoryUnit;

namespace SonarAnalyzer.Memory.Test;

[TestClass]
public class DotMemorySetupTest
{
    [TestMethod]
    [DotMemoryUnit(FailIfRunWithoutSupport = true)]
    public void EnsureDotMemoryUTsAreExecutedWithTheRightRunner()
    {
        try
        {
            dotMemory.Check(x => { });
        }
        catch (DotMemoryUnitException ex) when (ex.Message.StartsWith("The test was run without the support for dotMemory Unit."))
        {
            if (TestEnvironment.IsAzureDevOpsContext)
            {
                Assert.Fail("Memory tests are not executed correctly. Make sure you run the memory tests with the appropiate test runner. See https://www.jetbrains.com/help/dotmemory-unit/Using_dotMemory_Unit_Standalone_Runner.html for details.");
            }
            else
            {
                Assert.Inconclusive("The tests in this assembly need to be executed with the dotMemory tools. See 'RunMemoryTest.ps1' for details.");
            }
        }
    }
}
