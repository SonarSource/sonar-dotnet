/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Resources;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.PackagingTests
{
    [TestClass]
    public class RuleTypeTests
    {
        #region ruleTypesCs
        private static readonly IImmutableDictionary<string, string> ruleTypesCs = new Dictionary<string, string>
        {
            ["100"] = "CODE_SMELL",
            ["101"] = "CODE_SMELL",
            ["103"] = "CODE_SMELL",
            ["104"] = "CODE_SMELL",
            ["105"] = "CODE_SMELL",
            ["107"] = "CODE_SMELL",
            ["108"] = "CODE_SMELL",
            ["110"] = "CODE_SMELL",
            ["112"] = "CODE_SMELL",
            ["121"] = "CODE_SMELL",
            ["122"] = "CODE_SMELL",
            ["125"] = "CODE_SMELL",
            ["126"] = "CODE_SMELL",
            ["127"] = "CODE_SMELL",
            ["131"] = "CODE_SMELL",
            ["134"] = "CODE_SMELL",
            ["818"] = "CODE_SMELL",
            ["907"] = "CODE_SMELL",
            ["927"] = "CODE_SMELL",
            ["1006"] = "CODE_SMELL",
            ["1066"] = "CODE_SMELL",
            ["1067"] = "CODE_SMELL",
            ["1104"] = "VULNERABILITY",
            ["1109"] = "CODE_SMELL",
            ["1116"] = "CODE_SMELL",
            ["1117"] = "CODE_SMELL",
            ["1118"] = "CODE_SMELL",
            ["1121"] = "CODE_SMELL",
            ["1123"] = "CODE_SMELL",
            ["1125"] = "CODE_SMELL",
            ["1134"] = "CODE_SMELL",
            ["1135"] = "CODE_SMELL",
            ["1144"] = "CODE_SMELL",
            ["1145"] = "BUG",
            ["1155"] = "CODE_SMELL",
            ["1163"] = "CODE_SMELL",
            ["1172"] = "CODE_SMELL",
            ["1185"] = "CODE_SMELL",
            ["1186"] = "CODE_SMELL",
            ["1206"] = "BUG",
            ["1210"] = "BUG",
            ["1215"] = "CODE_SMELL",
            ["1226"] = "CODE_SMELL",
            ["1227"] = "CODE_SMELL",
            ["1244"] = "BUG",
            ["1301"] = "CODE_SMELL",
            ["1309"] = "CODE_SMELL",
            ["1313"] = "VULNERABILITY",
            ["1449"] = "BUG",
            ["1450"] = "CODE_SMELL",
            ["1451"] = "CODE_SMELL",
            ["1479"] = "CODE_SMELL",
            ["1481"] = "CODE_SMELL",
            ["1541"] = "CODE_SMELL",
            ["1643"] = "CODE_SMELL",
            ["1656"] = "BUG",
            ["1659"] = "CODE_SMELL",
            ["1694"] = "CODE_SMELL",
            ["1697"] = "BUG",
            ["1698"] = "CODE_SMELL",
            ["1699"] = "CODE_SMELL",
            ["1751"] = "BUG",
            ["1764"] = "BUG",
            ["1848"] = "BUG",
            ["1854"] = "CODE_SMELL",
            ["1858"] = "CODE_SMELL",
            ["1862"] = "BUG",
            ["1871"] = "CODE_SMELL",
            ["1905"] = "CODE_SMELL",
            ["1939"] = "CODE_SMELL",
            ["1940"] = "CODE_SMELL",
            ["1944"] = "BUG",
            ["1994"] = "CODE_SMELL",
            ["2068"] = "VULNERABILITY",
            ["2070"] = "VULNERABILITY",
            ["2123"] = "BUG",
            ["2156"] = "CODE_SMELL",
            ["2178"] = "CODE_SMELL",
            ["2184"] = "BUG",
            ["2190"] = "BUG",
            ["2197"] = "CODE_SMELL",
            ["2201"] = "BUG",
            ["2219"] = "CODE_SMELL",
            ["2223"] = "CODE_SMELL",
            ["2225"] = "BUG",
            ["2228"] = "VULNERABILITY",
            ["2234"] = "BUG",
            ["2259"] = "BUG",
            ["2275"] = "BUG",
            ["2278"] = "VULNERABILITY",
            ["2290"] = "CODE_SMELL",
            ["2291"] = "CODE_SMELL",
            ["2292"] = "CODE_SMELL",
            ["2306"] = "CODE_SMELL",
            ["2325"] = "CODE_SMELL",
            ["2326"] = "CODE_SMELL",
            ["2328"] = "BUG",
            ["2330"] = "CODE_SMELL",
            ["2333"] = "CODE_SMELL",
            ["2339"] = "CODE_SMELL",
            ["2342"] = "CODE_SMELL",
            ["2344"] = "CODE_SMELL",
            ["2345"] = "BUG",
            ["2346"] = "CODE_SMELL",
            ["2357"] = "CODE_SMELL",
            ["2360"] = "CODE_SMELL",
            ["2365"] = "CODE_SMELL",
            ["2368"] = "CODE_SMELL",
            ["2372"] = "CODE_SMELL",
            ["2376"] = "CODE_SMELL",
            ["2386"] = "VULNERABILITY",
            ["2387"] = "CODE_SMELL",
            ["2436"] = "CODE_SMELL",
            ["2437"] = "CODE_SMELL",
            ["2486"] = "CODE_SMELL",
            ["2551"] = "BUG",
            ["2583"] = "BUG",
            ["2589"] = "CODE_SMELL",
            ["2674"] = "BUG",
            ["2681"] = "BUG",
            ["2688"] = "BUG",
            ["2692"] = "CODE_SMELL",
            ["2696"] = "CODE_SMELL",
            ["2737"] = "CODE_SMELL",
            ["2743"] = "BUG",
            ["2757"] = "BUG",
            ["2758"] = "BUG",
            ["2760"] = "CODE_SMELL",
            ["2761"] = "BUG",
            ["2930"] = "BUG",
            ["2931"] = "BUG",
            ["2933"] = "CODE_SMELL",
            ["2934"] = "BUG",
            ["2952"] = "BUG",
            ["2953"] = "CODE_SMELL",
            ["2955"] = "BUG",
            ["2971"] = "CODE_SMELL",
            ["2995"] = "BUG",
            ["2996"] = "BUG",
            ["2997"] = "BUG",
            ["3005"] = "BUG",
            ["3010"] = "BUG",
            ["3052"] = "CODE_SMELL",
            ["3168"] = "BUG",
            ["3169"] = "CODE_SMELL",
            ["3172"] = "BUG",
            ["3215"] = "CODE_SMELL",
            ["3216"] = "CODE_SMELL",
            ["3217"] = "CODE_SMELL",
            ["3218"] = "CODE_SMELL",
            ["3220"] = "BUG",
            ["3234"] = "CODE_SMELL",
            ["3235"] = "CODE_SMELL",
            ["3236"] = "CODE_SMELL",
            ["3237"] = "CODE_SMELL",
            ["3240"] = "CODE_SMELL",
            ["3241"] = "CODE_SMELL",
            ["3244"] = "BUG",
            ["3246"] = "CODE_SMELL",
            ["3247"] = "CODE_SMELL",
            ["3249"] = "BUG",
            ["3251"] = "CODE_SMELL",
            ["3253"] = "CODE_SMELL",
            ["3254"] = "CODE_SMELL",
            ["3256"] = "CODE_SMELL",
            ["3257"] = "CODE_SMELL",
            ["3261"] = "CODE_SMELL",
            ["3262"] = "CODE_SMELL",
            ["3263"] = "BUG",
            ["3264"] = "CODE_SMELL",
            ["3265"] = "CODE_SMELL",
            ["3346"] = "BUG",
            ["3376"] = "CODE_SMELL",
            ["3397"] = "BUG",
            ["3427"] = "CODE_SMELL",
            ["3440"] = "CODE_SMELL",
            ["3441"] = "CODE_SMELL",
            ["3442"] = "CODE_SMELL",
            ["3443"] = "CODE_SMELL",
            ["3444"] = "CODE_SMELL",
            ["3445"] = "CODE_SMELL",
            ["3447"] = "CODE_SMELL",
            ["3449"] = "BUG",
            ["3450"] = "CODE_SMELL",
            ["3451"] = "CODE_SMELL",
            ["3453"] = "BUG",
            ["3456"] = "BUG",
            ["3457"] = "CODE_SMELL",
            ["3458"] = "CODE_SMELL",
            ["3459"] = "CODE_SMELL",
            ["3466"] = "BUG",
            ["3532"] = "CODE_SMELL",
            ["3597"] = "CODE_SMELL",
            ["3598"] = "BUG",
            ["3600"] = "CODE_SMELL",
            ["3603"] = "BUG",
            ["3604"] = "CODE_SMELL",
            ["3610"] = "BUG",
            ["3626"] = "CODE_SMELL",
            ["3655"] = "BUG",
            ["3776"] = "CODE_SMELL",
            ["3869"] = "BUG",
            ["3871"] = "CODE_SMELL",
            ["3872"] = "CODE_SMELL",
            ["3874"] = "CODE_SMELL",
            ["3875"] = "CODE_SMELL",
            ["3876"] = "CODE_SMELL",
            ["3877"] = "CODE_SMELL",
            ["3880"] = "CODE_SMELL",
            ["3881"] = "BUG",
            ["3884"] = "VULNERABILITY",
            ["3885"] = "BUG",
            ["3887"] = "BUG",
            ["3889"] = "BUG",
            ["3897"] = "CODE_SMELL",
            ["3898"] = "CODE_SMELL",
            ["3902"] = "CODE_SMELL",
            ["3903"] = "BUG",
            ["3904"] = "BUG",
            ["3909"] = "CODE_SMELL",
            ["3925"] = "BUG",
            ["3926"] = "BUG",
            ["3928"] = "CODE_SMELL",
            ["3962"] = "CODE_SMELL",
            ["3963"] = "CODE_SMELL",
            ["3967"] = "CODE_SMELL",
            ["3971"] = "CODE_SMELL",
            ["3981"] = "BUG",
            ["3984"] = "BUG",
            ["3990"] = "CODE_SMELL",
            ["3992"] = "CODE_SMELL",
            ["3993"] = "CODE_SMELL",
            ["3994"] = "CODE_SMELL",
            ["3995"] = "CODE_SMELL",
            ["3996"] = "CODE_SMELL",
            ["3997"] = "CODE_SMELL",
            ["3998"] = "CODE_SMELL",
            ["4000"] = "CODE_SMELL",
            ["4002"] = "CODE_SMELL",
            ["4004"] = "CODE_SMELL",
            ["4005"] = "CODE_SMELL",
            ["4016"] = "CODE_SMELL",
            ["4017"] = "CODE_SMELL",
            ["4018"] = "CODE_SMELL",
            ["4022"] = "CODE_SMELL",
            ["4023"] = "CODE_SMELL",
        }.ToImmutableDictionary();
        #endregion

        #region ruleTypesVb
        private static readonly IImmutableDictionary<string, string> ruleTypesVb = new Dictionary<string, string>
        {
            ["101"] = "CODE_SMELL",
            ["103"] = "CODE_SMELL",
            ["104"] = "CODE_SMELL",
            ["105"] = "CODE_SMELL",
            ["112"] = "CODE_SMELL",
            ["114"] = "CODE_SMELL",
            ["117"] = "CODE_SMELL",
            ["122"] = "CODE_SMELL",
            ["131"] = "CODE_SMELL",
            ["134"] = "CODE_SMELL",
            ["139"] = "CODE_SMELL",
            ["1067"] = "CODE_SMELL",
            ["1147"] = "CODE_SMELL",
            ["1197"] = "CODE_SMELL",
            ["1226"] = "CODE_SMELL",
            ["1541"] = "CODE_SMELL",
            ["1542"] = "CODE_SMELL",
            ["1643"] = "CODE_SMELL",
            ["1645"] = "CODE_SMELL",
            ["1654"] = "CODE_SMELL",
            ["1656"] = "BUG",
            ["1659"] = "CODE_SMELL",
            ["1764"] = "BUG",
            ["1862"] = "BUG",
            ["1871"] = "CODE_SMELL",
            ["2178"] = "CODE_SMELL",
            ["2304"] = "CODE_SMELL",
            ["2339"] = "CODE_SMELL",
            ["2340"] = "CODE_SMELL",
            ["2342"] = "CODE_SMELL",
            ["2343"] = "CODE_SMELL",
            ["2344"] = "CODE_SMELL",
            ["2345"] = "BUG",
            ["2346"] = "CODE_SMELL",
            ["2347"] = "CODE_SMELL",
            ["2348"] = "CODE_SMELL",
            ["2349"] = "CODE_SMELL",
            ["2352"] = "CODE_SMELL",
            ["2353"] = "CODE_SMELL",
            ["2354"] = "CODE_SMELL",
            ["2355"] = "CODE_SMELL",
            ["2357"] = "CODE_SMELL",
            ["2358"] = "CODE_SMELL",
            ["2359"] = "CODE_SMELL",
            ["2360"] = "CODE_SMELL",
            ["2362"] = "CODE_SMELL",
            ["2363"] = "CODE_SMELL",
            ["2364"] = "CODE_SMELL",
            ["2365"] = "CODE_SMELL",
            ["2366"] = "CODE_SMELL",
            ["2367"] = "CODE_SMELL",
            ["2368"] = "CODE_SMELL",
            ["2369"] = "CODE_SMELL",
            ["2370"] = "CODE_SMELL",
            ["2372"] = "CODE_SMELL",
            ["2373"] = "CODE_SMELL",
            ["2374"] = "CODE_SMELL",
            ["2375"] = "CODE_SMELL",
            ["2376"] = "CODE_SMELL",
            ["2429"] = "CODE_SMELL",
            ["2951"] = "CODE_SMELL",
            ["3385"] = "CODE_SMELL",
        }.ToImmutableDictionary();
        #endregion

        [TestMethod]
        public void DetectRuleTypeChanges_CS()
        {
            DetectTypeChanges(csharp::SonarAnalyzer.RspecStrings.ResourceManager, ruleTypesCs, nameof(ruleTypesCs));
        }

        [TestMethod]
        public void DetectRuleTypeChanges_VB()
        {
            DetectTypeChanges(vbnet::SonarAnalyzer.RspecStrings.ResourceManager, ruleTypesVb, nameof(ruleTypesVb));
        }

        private static void DetectTypeChanges(ResourceManager resourceManager, IImmutableDictionary<string, string> expectedTypes, string expectedTypesName)
        {
            var items = Enumerable
                .Range(1, 10000)
                .Select(i => new
                {
                    ExpectedType = expectedTypes.GetValueOrDefault(i.ToString()),
                    ActualType = resourceManager.GetString($"S{i}_Type"),
                    RuleId = i,
                })
                .Where(x => x.ActualType != x.ExpectedType)
                .ToList();

            // IMPORTANT: If this test fails, you should add the types of the new rules
            // in the dictionaries above. It is a manual task, sorry.
            var newRules = items.Where(x => x.ExpectedType == null);
            newRules.Should().BeEmpty($"you need to add the rules in {expectedTypesName}");

            // IMPORTANT: Rules should not be deleted without careful consideration and prior
            // deprecation. We need to notify the platform team as well.
            var deletedRules = items.Where(x => x.ActualType == null);
            deletedRules.Should().BeEmpty($"YOU SHOULD NEVER DELETE RULES!");

            // IMPORTANT: If this test fails, you should update the types of the changed rules
            // in the dictionaries above. Also add a GitHub issue specifying the change of type 
            // and update peach and next.
            var changedRules = items.Where(x => x.ActualType != null && x.ExpectedType != null);
            changedRules.Should().BeEmpty($"you need to change the rules in {expectedTypesName}.");
        }
    }
}
