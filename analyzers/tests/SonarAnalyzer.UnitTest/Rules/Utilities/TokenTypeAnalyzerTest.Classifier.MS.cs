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
    [DataRow("ex is [t:ArgumentException]")]
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
        """, allowSemanticModel);
}
