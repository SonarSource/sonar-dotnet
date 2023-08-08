using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    [DataTestMethod]
    [DataRow("[t:Exception] ex;")]        // -model
    [DataRow("[u:System].Exception ex;")] // +model
    [DataRow("[t:List]<[t:Exception]> ex;")] // -model
    [DataRow("List<[u:System].Exception> ex;")] // +model
    public void IdentifierToken_LocalDeclaration(string declaration, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
        $$"""
        using System;
        using System.Collections.Generic;
        public class Test
        {
            public void M()
            {
                {{declaration}}
            }
        }
        """, allowSemanticModel);

    [DataTestMethod]
    [DataRow("ex is [t:ArgumentException]", false)]
    [DataRow("ex is [u:System].ArgumentException", true)]
    [DataRow("ex is [t:ArgumentException] [u:argEx]", false)]
    [DataRow("ex is ArgumentException { InnerException: [t:InvalidOperationException] }", true)] // ConstantPattern: could also be a constant
    [DataRow("ex is ArgumentException { HResult: [t:Int32].[u:MinValue] }", true)]               // ConstantPattern: could also be a type
    [DataRow("ex is ArgumentException { [u:InnerException]: [t:InvalidOperationException] { } }", false)] // RecursivePattern.Type
    [DataRow("ex is ArgumentException { [u:InnerException].[u:InnerException]: [t:InvalidOperationException] { } [u:inner] }", false)]
    [DataRow("ex as [t:ArgumentException]", false)]
    [DataRow("ex as [u:System].ArgumentException", true)]
    [DataRow("([t:ArgumentException])ex", false)]
    [DataRow("([u:System].ArgumentException)ex", true)]
    public void IdentifierToken_Expressions(string expression, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
        $$"""
        using System;
        using System.Collections.Generic;
        public class Test
        {
            public void M(Exception ex)
            {
                var x = {{expression}};
            }
        }
        """/*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("""([k:string], [t:Exception]) => true,""", true)]
    //[DataRow("""([s:""], [k:null]) => true,""", true)]
    [DataRow("""("", [t:ArgumentException] { HResult: > 2 }) => true,""", false)]
    public void IdentifierToken_SwitchExpressions(string switchBranch, bool allowSemanticModel = true) =>
        ClassifierTestHarness.AssertTokenTypes(
        $$"""
        using System;
        using System.Collections.Generic;
        public class Test
        {
            public void M(Exception ex)
            {
                var x = ("", new Exception()) switch
                {
                    {{switchBranch}}
                    _ => default,
                };
            }
        }
        """/*, allowSemanticModel */);
}
