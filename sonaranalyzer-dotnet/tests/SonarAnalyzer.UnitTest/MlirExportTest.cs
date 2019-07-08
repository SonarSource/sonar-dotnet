extern alias csharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.ControlFlowGraph;

namespace SonarAnalyzer.UnitTest
{
    [TestClass]
    public class MlirExportTest
    {
        [TestMethod]
        public void SimpleMethod()
        {
            var code = @"
class C
{
    int Mult(int i, int j)
    {
        return i*j;
    }

    void Empty() {}
    void Nop(int i) { int j = 2*i;}

    int Cond(int i) { return i%2 == 0 ? i/2 : i*3 +1; }
    int Cond2(int i)
    {
        if (i%2 == 0)
            return i/2;
        else
            return i*3 +1;
    }
}
";
            using (var writer = new StreamWriter(Path.Combine(Path.GetTempPath(), "csharp.mlir")))
            {
                ExportFunction(code, writer, "Empty");
                ExportFunction(code, writer, "Nop");
                ExportFunction(code, writer, "Mult");
                ExportFunction(code, writer, "Cond");
                ExportFunction(code, writer, "Cond2");
            }

            var dot = CfgSerializer.Serialize("Cond2", GetCfgForMethod(code, "Cond2"));
        }

        private static void ExportFunction(string code, TextWriter writer, string functionName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(functionName);
            var exporter = new MLIRExporter(writer, semanticModel);
            exporter.ExportFunction(method);
        }
        protected IControlFlowGraph GetCfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(methodName);

            return CSharpControlFlowGraph.Create(method.Body, semanticModel);
        }

    }
}

public class C
{
    public int Mult(int i, int j)
    {
        return i * j;
    }

    public void Empty() { }
    void Nop(int i) { int j = 2 * i; }
    public int Cond(int i) { return i % 2 == 0 ? i / 2 : i * 3 + 1; }
    public int Cond2(int i)
    {
        if (i % 2 == 0)
            return i / 2;
        else
            return i * 3 + 1;
    }
}

