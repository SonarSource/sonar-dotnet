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

using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [DataTestMethod]
        [DataRow("42", "(object)value")]
        [DataRow("42", "value as object")]
        [DataRow("42", "(IComparable)value")]
        [DataRow("42", "(object)(IComparable)value")]
        [DataRow("42", "value as IComparable")] // we don't need to care about "42 as IEnumerable" because error CS0039 is raised
        [DataRow("DateTime.Now", "(IComparable)value")]
        [DataRow("Unknown<TStruct>()", "value as object")]
        [DataRow("Unknown<TStruct>()", "(IComparable)value")]
        [DataRow("Unknown<TComparableStruct>()", "value as IComparable")]
        // 42 as TComparableClass or
        // Unknown<TComparableStruct>() as TComparableClass always return null and are not under test
        public void Conversion_Boxing(string declaration, string boxing)
        {
            var code = @$"
var value = {declaration};
var result = {boxing};
Tag(""Value"", value);
Tag(""Result"", result);
";
            var validator = ConversionValidatorCS(code).Validator;
            validator.ValidateTag("Value", x => x.Should().BeNull());
            validator.ValidateTag("Result", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
        }

        [TestMethod]
        [DataRow("42", "value as TClass")]
        [DataRow("42", "value as TComparableClass")]
        [DataRow("Unknown<TStruct>()", "value as IComparable")]
        [DataRow("Unknown<int?>()", "value as IComparable")]
        [DataRow("Unknown<int?>()", "(object)value")]
        public void Conversion_Boxing_NotApplicable(string declaration, string boxing)
        {
            var code = @$"
var value = {declaration};
var result = {boxing};
Tag(""Value"", value);
Tag(""Result"", result);
";
            var validator = ConversionValidatorCS(code).Validator;
            validator.ValidateTag("Value", x => x.Should().BeNull());
            validator.ValidateTag("Result", x => x.Should().BeNull());
        }

        [DataTestMethod]
        [DataRow("object", "(int)value")]
        [DataRow("object", "(int)(byte)value")]
        [DataRow("object", "(int)(object)(byte)value")]
        [DataRow("IComparable", "(int)value")]
        [DataRow("object", "(TStruct)value")]
        [DataRow("IComparable", "(TStruct)value")]
        [DataRow("IComparable", "(TComparableStruct)value")]
        [DataRow("IComparable<int>", "(int)value")]
        public void Conversion_Unboxing(string type, string unboxing)
        {
            var code = @$"
var value = Unknown<{type}>();
var result = {unboxing};
Tag(""Value"", value);
Tag(""Result"", result);
";
            var validator = ConversionValidatorCS(code).Validator;
            validator.ValidateTag("Value", x => x.HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.ValidateTag("Result", x => (x == null || !x.AllConstraints.Any()).Should().BeTrue());
        }

        [DataTestMethod]
        [DataRow("int?", "(int)value")]
        [DataRow("int", "(int?)null")]
        public void Conversion_Unboxing_Nullable(string type, string unboxing)
        {
            var code = @$"
var value = Unknown<{type}>();
var result = {unboxing};
Tag(""Value"", value);
Tag(""Result"", result);
";
            var validator = ConversionValidatorCS(code).Validator;
            validator.ValidateTag("Value", x => x.Should().BeNull());
            validator.ValidateTag("Result", x => x.Should().BeNull());
        }

        private static SETestContext ConversionValidatorCS(string methodBody) =>
            new($@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Sample<T, TClass, TStruct, TComparableClass, TComparableStruct>
    where TClass: class
    where TStruct: struct
    where TComparableClass: class, IComparable
    where TComparableStruct: struct, IComparable
{{
    public void Main()
    {{
        {methodBody}
    }}
    private static void Tag<T>(string name, T arg = default) {{ }}
    private static T Unknown<T>() => default;
}}
", AnalyzerLanguage.CSharp, Array.Empty<SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicCheck>());
    }
}
