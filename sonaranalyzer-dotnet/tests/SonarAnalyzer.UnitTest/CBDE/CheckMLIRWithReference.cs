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
    }
}
