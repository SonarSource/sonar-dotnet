/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.CBDE
{
    public class MlirExporterMetrics
    {
        private int supportedFunctionsCount;
        private int unsupportedFunctionsCount;
        private readonly Dictionary<SyntaxKind, int> unsupportedSyntaxes;

        public MlirExporterMetrics()
        {
            supportedFunctionsCount = 0;
            unsupportedFunctionsCount = 0;
            unsupportedSyntaxes = new Dictionary<SyntaxKind, int>();
        }

        public void AddSupportedFunction()
        {
            ++supportedFunctionsCount;
        }
        public void AddUnsupportedFunction(SyntaxKind unsupportedSyntax)
        {
            ++unsupportedFunctionsCount;
            unsupportedSyntaxes[unsupportedSyntax] = unsupportedSyntaxes.TryGetValue(unsupportedSyntax, out int currentVal) ? ++currentVal : 1;
        }

        public String Dump()
        {
            if (supportedFunctionsCount == 0 && unsupportedFunctionsCount == 0)
            {
                return "";
            }
            var metricsStr = new StringBuilder($"exported {supportedFunctionsCount} \n" +
                $"unsupported {unsupportedFunctionsCount} \n");
            foreach (KeyValuePair<SyntaxKind, int> entry in unsupportedSyntaxes)
            {
                metricsStr.AppendLine($"{entry.Key.ToString()} {entry.Value}");
            }
            metricsStr.AppendLine();
            return metricsStr.ToString();
        }
    }
}
