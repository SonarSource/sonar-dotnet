namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    [DataTestMethod]
    [DataRow("[u:aInstance]", false)]
    public void IdentifierToken_SimpleMemberAccess(string memberAccessExpression, bool allowSemanticModel) =>
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
                    _ = {{memberAccessExpression}};
                }
            }
            """/*, allowSemanticModel */);
}
