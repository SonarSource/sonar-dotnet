namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    [DataTestMethod]
    [DataRow("[u:aInstance]", false)]                              // Some simple identifier syntax in an ordinary expression context must be boud to a field/property/local or something else that produces a value, but it can not be a type
    [DataRow("aInstance.InstanceProp.[u:InstanceProp]", false)]    // Most right can not be a type in an ordinary expression context
    [DataRow("[u:aInstance].[u:InstanceProp].InstanceProp", true)] // Could be types
    [DataRow("[t:A].StaticProp", true)]                            // Here it starts with a type
    [DataRow("A.[u:StaticProp]", false)]                           // Most right hand side can not be a type
    [DataRow("[t:A].[t:B].[u:StaticProp].InstanceProp", true)]     // Mixture: some nested types and then properties
    [DataRow("A.B.StaticProp.[u:InstanceProp]", false)]            // The most right hand side
    [DataRow("A.B.[u:StaticProp]?.[u:InstanceProp]", false)]       // Can not be a type on the left side of a ?.
    [DataRow("[u:Prop]?.[u:InstanceProp]?.[u:InstanceProp]", false)] // Can not be a type on the left side of a ?. or on the right
    [DataRow("global::[t:A].StaticProp", true)]                    // Can be a namespace or type
    [DataRow("this.[u:Prop]", false)]                              // Right of this: must be a property/field
    [DataRow("this.[u:Prop].[u:InstanceProp].[u:InstanceProp]", false)] // Right of this: must be properties or fields
    [DataRow("(true ? Prop : Prop).[u:InstanceProp].[u:InstanceProp]", false)] // Right of some expression: must be properties or fields
    [DataRow("[t:A]<int>.StaticProp", false)] // Generic name. Must be a type because not in an invocation context, like A<int>()
    [DataRow("A<int>.[u:StaticProp]", false)] // Most right hand side
    [DataRow("A<int>.[u:StaticProp].InstanceProp", true)] // Not the right hand side, could be a nested type
    [DataRow("A<int>.[t:B].StaticProp", true)] // Not the right hand side, is a nested type
    [DataRow("[t:A]<int>.[u:StaticProp]?.[u:InstanceProp]", false)] // Can all be infered from the positions
    [DataRow("[t:A]<int>.[t:B]<int>.[u:StaticProp]", false)] // Generic names must be types and StaticProp is most right hand side
    [DataRow("[t:A]<int>.[u:StaticM]<int>().[u:InstanceProp]", false)] // A must be a type StaticM is invoked and InstanceProp is after the invocation
    [DataRow("A<int>.StaticM<int>().[u:InstanceProp].InstanceProp", false)] // Is right from invocation
    public void IdentifierToken_SimpleMemberAccess_InOrdinaryExpression(string memberAccessExpression, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            public class A
            {
                public static A StaticProp { get; }
                public A InstanceProp { get; }
                public A this[int i] => new A();

                public class B
                {
                    public static A StaticProp { get; }
                }
            }

            public class A<TA> : A
            {
                public A<TA> InstanceProp { get; }
                public static A<TA> StaticProp { get; }

                public class B<TB> : B
                {
                    public static B<TB> StaticProp { get; }
                    public A<TM> M<TM>() => new A<TM>();
                    public static A<TM> StaticM<TM>() => new A<TM>();
                }

                public A<TM> M<TM>() => new A<TM>();
                public static A<TM> StaticM<TM>() => new A<TM>();
            }

            public class Test
            {
                public A Prop { get; }
                public void M()
                {
                    var aInstance = new A<int>();
                    // Ordinary expression context: no typeof(), no nameof(), no ExpressionColon (in pattern) or the like
                    _ = {{memberAccessExpression}};
                }
            }
            """/*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[u:System]", true)]
    [DataRow("[t:Exception]", true)]
    [DataRow("[t:HashSet]<[t:Int32]>.Enumerator", false)]
    [DataRow("HashSet<Int32>.[t:Enumerator]", true)]
    [DataRow("[u:System].[u:Linq].[t:Enumerable].[u:Where]", true)]
    public void IdentifierToken_SimpleMemberAccess_NameOf(string memberaccess, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M()
                {
                    _ = nameof({{memberaccess}});
                }
            }
            """/*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("{ [u:InnerException].[u:InnerException]: {} }", false)] // in SubpatternSyntax.ExpressionColon context. Must be properties
    [DataRow("{ [u:InnerException].[u:InnerException].[u:Data]: {} }", false)] // in SubpatternSyntax.ExpressionColon context. Must be properties
    public void IdentifierToken_SimpleMemberAccess_ExpressionColon(string pattern, bool allowSemanticModel) =>
        // found in SubpatternSyntax.ExpressionColon
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(Exception ex)
                {
                    _ = ex is {{pattern}};
                }
            }
            """/*, allowSemanticModel */);

    [DataTestMethod]
    [DataRow("[t:Int32]", true)]
    [DataRow("[u:i]", true)]
    [DataRow("[t:Int32].[u:MaxValue]", true)]
    [DataRow("[u:System].[t:Int32]", true)]
    [DataRow("not [t:Int32]", true)]
    [DataRow("not [u:i]", true)]
    [DataRow("not [t:Int32].[u:MaxValue]", true)]
    [DataRow("not [u:System].[t:Int32]", true)]
    public void IdentifierToken_SimpleMemberAccess_Is(string pattern, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(object o)
                {
                    const int i = 42;
                    _ = o is {{pattern}};
                }
            }
            """, allowSemanticModel);

    [DataTestMethod]
    [DataRow("[t:Int32]", true)]
    [DataRow("[u:i]", true)]
    [DataRow("[t:Int32].[u:MaxValue]", true)]
    [DataRow("[u:System].[t:Int32].[u:MaxValue]", true)]
    [DataRow("not [t:Int32]", true)]
    [DataRow("not [u:i]", true)]
    [DataRow("not [t:Int32].[u:MaxValue]", true)]
    [DataRow("not [u:System].[t:Int32]", true)]
    public void IdentifierToken_SimpleMemberAccess_SwitchArm(string pattern, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(object o)
                {
                    const int i = 42;
                    switch(o)
                    {
                        case {{pattern}}: break;
                    }
                }
            }
            """, allowSemanticModel);

    [DataTestMethod]
    [DataRow("[t:Int32]", true)]
    [DataRow("[u:i]", true)]
    [DataRow("[t:Int32].[u:MaxValue]", true)]
    [DataRow("[u:System].[t:Int32].[u:MaxValue]", true)]
    [DataRow("not [t:Int32]", true)]
    [DataRow("not [u:i]", true)]
    [DataRow("not [t:Int32].[u:MaxValue]", true)]
    [DataRow("not [u:System].[t:Int32]", true)]
    public void IdentifierToken_SimpleMemberAccess_SwitchExpression(string pattern, bool allowSemanticModel) =>
        ClassifierTestHarness.AssertTokenTypes($$"""
            using System;
            using System.Collections.Generic;
            public class C
            {
                public void M(object o)
                {
                    const int i = 42;
                    _ = o switch
                    {
                        {{pattern}} => true,
                    };
                }
            }
            """, allowSemanticModel);
}
