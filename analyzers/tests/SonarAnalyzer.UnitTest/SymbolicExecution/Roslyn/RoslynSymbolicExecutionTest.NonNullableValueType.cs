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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void PreProcess_NonNullableValueType_NotNullConstraint_Property_Int()
        {
            var validator = SETestContext.CreateCSMethod("""
                public int Property { get; }

                public void Main()
                {
                    var i = Property;
                }
            """).Validator;
            validator.ValidateOrder(
                "LocalReference: i = Property (Implicit)",
                "InstanceReference: Property (Implicit)",
                "PropertyReference: Property",
                "SimpleAssignment: i = Property (Implicit)");
            var i = validator.Symbol("i");
            validator.Validate("PropertyReference: Property", x => x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.Validate("SimpleAssignment: i = Property (Implicit)", x =>
            {
                x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                x.State[i].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            });
        }

        [TestMethod]
        public void PreProcess_NonNullableValueType_NotNullConstraint_Property_Int_BoxedToObject()
        {
            var validator = SETestContext.CreateCSMethod("""
                public int Property { get; }

                public void Main()
                {
                    object i = Property;
                }
            """).Validator;
            validator.ValidateOrder(
                "LocalReference: i = Property (Implicit)",
                "InstanceReference: Property (Implicit)",
                "PropertyReference: Property",
                "Conversion: Property (Implicit)",
                "SimpleAssignment: i = Property (Implicit)");
            var i = validator.Symbol("i");
            validator.Validate("PropertyReference: Property", x => x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.Validate("SimpleAssignment: i = Property (Implicit)", x =>
            {
                x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                x.State[i].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            });
        }

        [TestMethod]
        public void PreProcess_NonNullableValueType_NotNullConstraint_Property_Int_FromUnknownTarget()
        {
            var validator = SETestContext.CreateCSMethod("""
                public int Property { get; }

                public void Main(Sample s)
                {
                    object i = s.Property;
                }
            """).Validator;
            validator.ValidateOrder(
                "LocalReference: i = s.Property (Implicit)",
                "ParameterReference: s",
                "PropertyReference: s.Property",
                "Conversion: s.Property (Implicit)",
                "SimpleAssignment: i = s.Property (Implicit)");
            var i = validator.Symbol("i");
            validator.Validate("PropertyReference: s.Property", x => x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue());
            validator.Validate("SimpleAssignment: i = s.Property (Implicit)", x =>
            {
                x.State[x.Operation].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                x.State[i].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
            });
        }
    }
}
