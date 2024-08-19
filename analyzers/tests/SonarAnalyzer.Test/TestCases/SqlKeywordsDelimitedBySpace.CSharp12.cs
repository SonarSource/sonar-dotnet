using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SomeNamespace; // Required since use of default namespace results into FNs

class PrimaryConstructors
{
    class C1(string sql = "SELECT x" + "FROM y");          // Noncompliant
    struct S1(string sql = "SELECT x" + "FROM y");         // Noncompliant
    record R1(string sql = "SELECT x" + "FROM y");         // Noncompliant
    record struct RS1(string sql = "SELECT x" + "FROM y"); // Noncompliant
}

class DefaultLambdaParameters
{
    void Test()
    {
        var f1 = (string s = "SELECT x" + "FROM y") => s;                   // Noncompliant
        var f2 = (string s1 = "SELECT x", string s2 = "FROM y") => s1 + s2; // Compliant, different strings
    }
}

class CollectionExpressions
{
    void MonoDimensional()
    {
        IList<string> a;
        a = ["SELECT x" + "FROM y"];            // Noncompliant
        a = ["SELECT x" + """FROM y"""];        // Noncompliant
        a = ["SELECT x", "FROM y"];             // Compliant, different strings
        a = [$"SELECT x{"FROM y"}"];            // Noncompliant
        a = [$$$""""SELECT x{{"FROM y"}}""""];  // Compliant
        a = [$$""""SELECT x{{"FROM y"}}""""];   // Noncompliant
    }

    void MultiDimensional()
    {
        IList<IList<string>> a;
        a = [
                ["SELECT x" + "FROM y",  // Noncompliant
                "SELECT x"],
                ["FROM y",
                "SELECT x" + "FROM y"]   // Noncompliant
            ];
    }
}
