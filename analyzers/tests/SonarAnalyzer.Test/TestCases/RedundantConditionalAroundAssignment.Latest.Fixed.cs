using System;

class CSharp9
{
    void Method()
    {
        int x = 5;
        int y = 6;

        y = 5;

        y = 5;

        y = 5;

        y = y switch
        {
            5 => 5, // Fixed
            6 => 6  // Fixed
        };

        y = y switch
        {
            not 5 when x == 5 => 5
        };

        y = y switch
        {
            5 => 6
        };

        x = y switch
        {
            5 => 6
        };

        y = y switch
        {
            4 => 4,      // Fixed
            not 5 => 5,
        };

        y = 5;

        y = y switch
        {
            5 => 6,
            _ => y
        };

        y = y;

        y = y switch
        {
            4 => 4,      // Fixed
            not x => 5,  // Error [CS9135]
        };

        int z = y switch
        {
            not 5 => 5
        };

        if ((x, y) is (1, 2)) // FN (is expression not supported yet)
        {
            x = 1;
            y = 2;
        }

        SomeClass someClass = new SomeClass() { SomeField = 42 };

        someClass.SomeField = 42;

        if (someClass is { SomeField: not 42 }) // FN (is and is not expression not supported yet)
        {
            someClass.SomeField = 42;
        }
    }

    public class SomeClass
    {
        public int SomeField;
    }

    record Record
    {
        string x;

        string CompliantProperty1
        {
            init
            {
                if (x != value)
                {
                    x = value;
                }
            }
        }

        string CompliantProperty2
        {
            init { x = value; }
        }
    }
}

public static class SwitchExpressionCases
{
    public static Uri ToWebsocketUri(this Uri uri)
    {
        // See: https://github.com/SonarSource/sonar-dotnet/issues/5246
        var builder = new UriBuilder(uri);
        builder.Scheme = uri.Scheme switch
        {
            "https" => "wss",
            "http" => "ws",
            "wss" => "wss", // Compliant, if this is removed the switch logic changes
            "ws" => "ws",   // Compliant, if this is removed the switch logic changes
            _ => throw new ArgumentException($"Cannot convert URI scheme '{uri.Scheme}' to a websocket scheme."),
        };
        return builder.Uri;
    }

    static string M(string s) =>
        s switch
        {
            null => null, // Compliant, if this is removed "s" will be assigned non-empty is case of null.
            "" => "Empty",
            _ => "Not empty",
        };
}

// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/6447
class Repro6447
{
    void MyMethod()
    {
        string x = null;

        if (x is not null) // FN (is not expression not supported yet)
        {
            x = null;
        }
    }
}

class CSharp10
{
    void Method()
    {
        int y = 6;

        y = 5;

        SomeClass someClass = new SomeClass() { SomeField1 = new SomeOtherClass() { SomeField2 = 42 } };

        someClass.SomeField1.SomeField2 = 42;

        if (someClass is { SomeField1: { SomeField2: not 42 } }) // FN (is expression not supported yet)
        {
            someClass.SomeField1.SomeField2 = 42;
        }

        if (someClass is { SomeField1.SomeField2: not 42 }) // FN (is expression not supported yet)
        {
            someClass.SomeField1.SomeField2 = 42;
        }
    }

    public class SomeClass
    {
        public SomeOtherClass SomeField1;
    }

    public class SomeOtherClass
    {
        public int SomeField2;
    }
}

public class SomeClass
{
    public void Noncompliant(byte[] bytes)
    {
        if (bytes is [not 1, .., not 3]) // FN (is expression not supported yet)
        {
            bytes[0] = 1;
            bytes[^1] = 3;
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-443
    void IndexAccess()
    {
        DataBuffer buffer = new()
        {
            [-1] = -1,
            [1] = 0,
            [2] = 1,
            [3] = 2,
            [4] = 3,
            [5] = 4,
            [6] = 5,
            [7] = 6,
            [8] = 7,
            [9] = 8
        };
        if (buffer[^1] != 9)
        {
            buffer[^1] = 9; // FN
        }
    }
}

public class DataBuffer
{
    public int this[Index index]
    {
        get => 1;
        set { /* not relevant */ }
    }
}

class FieldKeyword
{
    int Property
    {
        get
        {
            field = 0;

            return field;
        }
        set
        {
            if (field != 0) // Compliant, don't raise in setter
            {
                field = 0;
            }
        }
    }
}

class NullConditionalAssignment
{
    int x;

    void Method(NullConditionalAssignment n, int a)
    {
        if (n.x != a)   // FN NET-2751
        {
            n?.x = a;
        }

        n?.x = a;       // Compliant
    }
}
