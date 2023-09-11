/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
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
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
extern alias vbnet;

namespace SonarAnalyzer.UnitTest.PackagingTests
{
    [TestClass]
    public class RuleTypeTest
    {
        [TestMethod]
        public void DetectRuleTypeChanges_CS() =>
            DetectTypeChanges(csharp::SonarAnalyzer.RuleCatalog.Rules, RuleTypeMappingCS.Rules, LanguageNames.CSharp, nameof(RuleTypeMappingCS));

        [TestMethod]
        public void DetectRuleTypeChanges_VB() =>
            DetectTypeChanges(vbnet::SonarAnalyzer.RuleCatalog.Rules, RuleTypeMappingVB.Rules, LanguageNames.VisualBasic, nameof(RuleTypeMappingVB));

        [AssertionMethod]
        private static void DetectTypeChanges(Dictionary<string, RuleDescriptor> rules, IImmutableDictionary<string, string> expectedTypes, string language, string expectedTypesName)
        {
            var rulesWithUnmatchingType = Enumerable.Range(1, 10000)
                                  .Select(x => "S" + x)
                                  .Select(x => new
                                  {
                                      RuleId = x,
                                      ActualType = rules.ContainsKey(x) ? rules[x].Type : null,
                                      ExpectedType = expectedTypes.GetValueOrDefault(x)
                                  })
                                  .Where(x => x.ActualType != x.ExpectedType)
                                  .ToList();

            var newRules = rulesWithUnmatchingType.Where(x => x.ExpectedType is null);
            newRules
                .Should()
                .BeEmpty($": there are rules that exist in '{language}::SonarAnalyzer.RuleCatalog.Rules' file but not in {expectedTypesName}. You need to add them to the latter");

            var ruleWithoutActualTypeExists = rulesWithUnmatchingType.Exists(x => x.ActualType is null);
            ruleWithoutActualTypeExists
                .Should()
                .BeFalse($": there are rules that exist in {expectedTypesName} but not in '{language}::SonarAnalyzer.RuleCatalog.Rules' file. You might have forgotten to update the RSpec for sonar-dotnet");

            var changedRules = rulesWithUnmatchingType.Where(x => x.ActualType != null && x.ExpectedType != null);
            changedRules.Should().BeEmpty($": you need to change the rule types in {expectedTypesName}");
        }
    }
}
