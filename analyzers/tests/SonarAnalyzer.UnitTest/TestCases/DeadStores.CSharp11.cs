using System;


string x = "";   // FN
x = """Test2"""; // FN
Foo(x);

void RawStringLiterals(string param)
{
    param = """Test""";      // Noncompliant

    string x = ""; // Compliant, ignored value
    x = """Test2""";
    Foo(x);

    string y = """Test1"""; // Noncompliant
    y = """Test2""";
    Foo(y);
}

static void Foo(object x){ }
