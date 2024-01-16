using System.Text;

public class StringFormatValidatorEdgeCases
{
    // https://github.com/SonarSource/sonar-dotnet/issues/2392
    void EdgeCases(string bar)
    {
        var builder = new StringBuilder();
        builder.AppendFormat("&invoice={0}", Foo(bar));
        builder.AppendFormat("&invoice={0}", new object[0]);
        builder.AppendFormat("&invoice={0}", new object[1] { 1 });
        builder.AppendFormat("&invoice={0}", new [] { 1 });
        builder.AppendFormat("&rm=2", new object[0]);
        builder.AppendFormat("&rm=2", new[] { 1 });            // Noncompliant
        builder.AppendFormat("&rm=2", new object[0] { } );     // Noncompliant
        builder.AppendFormat("&rm=2", new object[1] { "a" } ); // Noncompliant
    }

    string Foo(string bar)
    {
        return "";
    }
}
