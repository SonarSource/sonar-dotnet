using System;

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

if ((x,y) is (1,2)) // FN (is expression not supported yet)
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
