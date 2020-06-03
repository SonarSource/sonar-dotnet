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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.CBDE
{
    /// <summary>
    /// This class contains tests that generate MLIR code from C# source code, then check that the generated
    /// code is valid and is equal to a specified reference.
    /// It is used instead of MlirExportTest when the generation result is tricky and deserves to be checked thoroughly
    /// </summary>
    [TestClass]
    public class CheckMlirWithReference
    {
        public TestContext TestContext { get; set; } // Set automatically by MsTest

        [ClassInitialize]
        public static void checkExecutableExists(TestContext tc)
        {
            MlirTestUtilities.checkExecutableExists();
        }

        [TestMethod]
        public void TestAssignToLocalVarAndParam()
        {
            var code = @"
void Func(int i) {
    var j = 12;
    j = j + 1;
    i = i + 1;
}
";
            var expected = @"
func @_$invalid$global$code$.Func$int$(%arg0: i32) {
  %0 = cbde.alloca i32
  cbde.store %arg0, %0 : memref<i32>
  br ^bb1
^bb1:	// pred: ^bb0
  %c12_i32 = constant 12 : i32
  %1 = cbde.alloca i32
  cbde.store %c12_i32, %1 : memref<i32>
  %2 = cbde.load %1 : memref<i32>
  %c1_i32 = constant 1 : i32
  %3 = addi %2, %c1_i32 : i32
  cbde.store %3, %1 : memref<i32>
  %4 = cbde.load %0 : memref<i32>
  %c1_i32_0 = constant 1 : i32
  %5 = addi %4, %c1_i32_0 : i32
  cbde.store %5, %0 : memref<i32>
  br ^bb2
^bb2:	// pred: ^bb1
  return
}
";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }

        [TestMethod]
        public void TestBitwiseAndOrXor()
        {
            var code = @"
void Func(int i) {
    var j = 12;

    j = j & i;
    j &= i;

    j = j | i;
    j |= i;

    j = j ^ i;
    j ^= i;
}
";
            var expected = @"
func @_$invalid$global$code$.Func$int$(%arg0: i32) {
  %0 = cbde.alloca i32
  cbde.store %arg0, %0 : memref<i32>
  br ^bb1
^bb1:	// pred: ^bb0
  %c12_i32 = constant 12 : i32
  %1 = cbde.alloca i32
  cbde.store %c12_i32, %1 : memref<i32>
  %2 = cbde.load %1 : memref<i32>
  %3 = cbde.load %0 : memref<i32>
  %4 = and %2, %3 : i32
  cbde.store %4, %1 : memref<i32>
  %5 = cbde.load %1 : memref<i32>
  %6 = cbde.load %0 : memref<i32>
  %7 = and %5, %6 : i32
  cbde.store %7, %1 : memref<i32>
  %8 = cbde.load %1 : memref<i32>
  %9 = cbde.load %0 : memref<i32>
  %10 = or %8, %9 : i32
  cbde.store %10, %1 : memref<i32>
  %11 = cbde.load %1 : memref<i32>
  %12 = cbde.load %0 : memref<i32>
  %13 = or %11, %12 : i32
  cbde.store %13, %1 : memref<i32>
  %14 = cbde.load %1 : memref<i32>
  %15 = cbde.load %0 : memref<i32>
  %16 = xor %14, %15 : i32
  cbde.store %16, %1 : memref<i32>
  %17 = cbde.load %1 : memref<i32>
  %18 = cbde.load %0 : memref<i32>
  %19 = xor %17, %18 : i32
  cbde.store %19, %1 : memref<i32>
  br ^bb2
^bb2:	// pred: ^bb1
  return
}
";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }

        [TestMethod]
        public void MultiplicationDivisionModulo()
        {
            var code = @"
void Func(int i) {
    var j = 12;

    j = j * i;
    j *= i;

    j = j / i;
    j /= i;

    j = j % i;
    j %= i;
}
";
            var expected = @"
func @_$invalid$global$code$.Func$int$(%arg0: i32) {
  %0 = cbde.alloca i32
  cbde.store %arg0, %0 : memref<i32>
  br ^bb1
^bb1:	// pred: ^bb0
  %c12_i32 = constant 12 : i32
  %1 = cbde.alloca i32
  cbde.store %c12_i32, %1 : memref<i32>
  %2 = cbde.load %1 : memref<i32>
  %3 = cbde.load %0 : memref<i32>
  %4 = muli %2, %3 : i32
  cbde.store %4, %1 : memref<i32>
  %5 = cbde.load %1 : memref<i32>
  %6 = cbde.load %0 : memref<i32>
  %7 = muli %5, %6 : i32
  cbde.store %7, %1 : memref<i32>
  %8 = cbde.load %1 : memref<i32>
  %9 = cbde.load %0 : memref<i32>
  %10 = divis %8, %9 : i32
  cbde.store %10, %1 : memref<i32>
  %11 = cbde.load %1 : memref<i32>
  %12 = cbde.load %0 : memref<i32>
  %13 = divis %11, %12 : i32
  cbde.store %13, %1 : memref<i32>
  %14 = cbde.load %1 : memref<i32>
  %15 = cbde.load %0 : memref<i32>
  %16 = remis %14, %15 : i32
  cbde.store %16, %1 : memref<i32>
  %17 = cbde.load %1 : memref<i32>
  %18 = cbde.load %0 : memref<i32>
  %19 = remis %17, %18 : i32
  cbde.store %19, %1 : memref<i32>
  br ^bb2
^bb2:	// pred: ^bb1
  return
}
";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }

        [TestMethod]
        public void LeftRightShift()
        {
            var code = @"
void f( int i )
{
    i << 1;
    i <<= 2;
    i >> 3;
    i >>= 4;
}
";
            var expected = @"
func @_$invalid$global$code$.f$int$(%arg0: i32) {
  %0 = cbde.alloca i32
  cbde.store %arg0, %0 : memref<i32>
  br ^bb1
^bb1:	// pred: ^bb0
  %1 = cbde.load %0 : memref<i32>
  %c1_i32 = constant 1 : i32
  %2 = shlis %1, %c1_i32 : i32
  %3 = cbde.load %0 : memref<i32>
  %c2_i32 = constant 2 : i32
  %4 = shlis %3, %c2_i32 : i32
  cbde.store %4, %0 : memref<i32>
  %5 = cbde.load %0 : memref<i32>
  %c3_i32 = constant 3 : i32
  %6 = cbde.neg %c3_i32 : i32
  %7 = shlis %5, %6 : i32
  %8 = cbde.load %0 : memref<i32>
  %c4_i32 = constant 4 : i32
  %9 = cbde.neg %c4_i32 : i32
  %10 = shlis %8, %9 : i32
  cbde.store %10, %0 : memref<i32>
  br ^bb2
^bb2:	// pred: ^bb1
  return
}
";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }

        [TestMethod]
        public void PrePostIncrementDecrement()
        {
            var code = @"
class A
{
    private long p;
    private readonly int[] indices;

    private long f(int i)
    {
        int result;
        result = ++i;
        result = i++;
        result = --i;
        result = i--;

        return ++p; // Not handled, but should not crash
    }

    private void g(int dim)
    {
         _indices[dimension]++; // Not handled, but should not crash
    }
}
";
            var expected = @"
func @_A.f$int$(%arg0: i32) -> none {
  %0 = cbde.alloca i32
  cbde.store %arg0, %0 : memref<i32>
  br ^bb1
^bb1:	// pred: ^bb0
  %1 = cbde.alloca i32
  %2 = cbde.load %0 : memref<i32>
  %c1_i32 = constant 1 : i32
  %3 = addi %2, %c1_i32 : i32
  cbde.store %3, %0 : memref<i32>
  cbde.store %3, %1 : memref<i32>
  %4 = cbde.load %0 : memref<i32>
  %c1_i32_0 = constant 1 : i32
  %5 = addi %4, %c1_i32_0 : i32
  cbde.store %5, %0 : memref<i32>
  cbde.store %4, %1 : memref<i32>
  %6 = cbde.load %0 : memref<i32>
  %c1_i32_1 = constant 1 : i32
  %7 = subi %6, %c1_i32_1 : i32
  cbde.store %7, %0 : memref<i32>
  cbde.store %7, %1 : memref<i32>
  %8 = cbde.load %0 : memref<i32>
  %c1_i32_2 = constant 1 : i32
  %9 = subi %8, %c1_i32_2 : i32
  cbde.store %9, %0 : memref<i32>
  cbde.store %8, %1 : memref<i32>
  %10 = cbde.unknown : none
  %11 = cbde.unknown : none
  return %11 : none
^bb2:	// no predecessors
  cbde.unreachable
}

func @_A.g$int$(%arg0: i32) {
  %0 = cbde.alloca i32
  cbde.store %arg0, %0 : memref<i32>
  br ^bb1
^bb1:	// pred: ^bb0
  %1 = cbde.unknown : none
  %2 = cbde.unknown : none
  br ^bb2
^bb2:	// pred: ^bb1
  return
}
";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }

        [TestMethod]
        public void Int32_Constant()
        {
            var code = @"
void Func() {
    const int Value = 42;
    var a = int.MaxValue;
    var b = int.MinValue;
    var c = System.Int32.MaxValue;
    var d = Value;
}
";
            var expected = @"
func @_$invalid$global$code$.Func$$() {
  br ^bb1
^bb1:	// pred: ^bb0
  %c42_i32 = constant 42 : i32
  %0 = cbde.alloca i32
  cbde.store %c42_i32, %0 : memref<i32>
  %1 = cbde.unknown : i32
  %c2147483647_i32 = constant 2147483647 : i32
  %2 = cbde.alloca i32
  cbde.store %c2147483647_i32, %2 : memref<i32>
  %3 = cbde.unknown : i32
  %c-2147483648_i32 = constant -2147483648 : i32
  %4 = cbde.alloca i32
  cbde.store %c-2147483648_i32, %4 : memref<i32>
  %5 = cbde.unknown : i32
  %c2147483647_i32_0 = constant 2147483647 : i32
  %6 = cbde.alloca i32
  cbde.store %c2147483647_i32_0, %6 : memref<i32>
  %7 = cbde.load %0 : memref<i32>
  %8 = cbde.alloca i32
  cbde.store %7, %8 : memref<i32>
  br ^bb2
^bb2:	// pred: ^bb1
  return
}
";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }
    }
}
