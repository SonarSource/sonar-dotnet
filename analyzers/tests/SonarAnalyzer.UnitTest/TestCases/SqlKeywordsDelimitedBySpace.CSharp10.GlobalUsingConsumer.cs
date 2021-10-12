// global using declared in SqlKeywordsDelimitedBySpace.CSharp10.Global.cs

class Foo
{
    private string field = "SELECT *" + // FN
        "FROM TABLE" +
        "WHERE X = 1;";
}
