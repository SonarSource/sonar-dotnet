/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MethodOverrideChangedDefaultValueTest
    {
        [TestMethod]
        public void MethodOverrideChangedDefaultValue() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\MethodOverrideChangedDefaultValue.cs",
                new MethodOverrideChangedDefaultValue(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.NETStandard21);

#if NET
        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\MethodOverrideChangedDefaultValue.CSharp9.cs", new MethodOverrideChangedDefaultValue());

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CSharpPreview() =>
            OldVerifier.VerifyAnalyzerCSharpPreviewLibrary(@"TestCases\MethodOverrideChangedDefaultValue.CSharpPreview.cs", new MethodOverrideChangedDefaultValue());

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CSharpPreview_CodeFix() =>
            OldVerifier.VerifyCodeFix<MethodOverrideChangedDefaultValueCodeFix>(
                @"TestCases\MethodOverrideChangedDefaultValue.CSharpPreview.cs",
                @"TestCases\MethodOverrideChangedDefaultValue.CSharpPreview.Fixed.cs",
                new MethodOverrideChangedDefaultValue(),
                ParseOptionsHelper.CSharpPreview);
#endif

        [TestMethod]
        public void MethodOverrideChangedDefaultValue_CodeFix() =>
            OldVerifier.VerifyCodeFix<MethodOverrideChangedDefaultValueCodeFix>(
                @"TestCases\MethodOverrideChangedDefaultValue.cs",
                @"TestCases\MethodOverrideChangedDefaultValue.Fixed.cs",
                @"TestCases\MethodOverrideChangedDefaultValue.Fixed.Batch.cs",
                new MethodOverrideChangedDefaultValue(),
                ParseOptionsHelper.FromCSharp8);
    }
}
