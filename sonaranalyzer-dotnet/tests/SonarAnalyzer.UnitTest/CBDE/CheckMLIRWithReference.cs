using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.CBDE
{
    [TestClass]
    public class CheckMlirWithReference
    {
        [ClassInitialize]
        public static void checkExecutableExists(TestContext tc)
        {
            MlirTestUtilities.checkExecutableExists();
        }

        public TestContext TestContext { get; set; } // Set automatically by MsTest

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
  br ^bb2
^bb2:	// pred: ^bb1
  return
}

";
            MlirTestUtilities.ValidateWithReference(code, expected, TestContext.TestName);
        }
    }
}
