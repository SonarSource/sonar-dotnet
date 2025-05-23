﻿using System;

int x = 5;
int y = 6;

y = y switch
{
    not 5 => 5 // Noncompliant
};

y = y switch
{
    5 => 5 // Noncompliant
};

y = y switch
{
    5 when x == 5 => 5 // Noncompliant
};

y = y switch
{
    5 => 5, // Noncompliant
    6 => 6  // Noncompliant
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
    4 => 4,      // Noncompliant
    not 5 => 5,
};

y = x switch
{
    _ => 5       // Noncompliant
};

y = y switch
{
    5 => 6,
    _ => y
};

y = y switch
{
    _ => y       // Noncompliant
};

y = y switch
{
    4 => 4,      // Noncompliant
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

if (someClass.SomeField != 42) // Noncompliant
{
    someClass.SomeField = 42;
}

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
            "wss" => "wss", // Noncompliant FP
            "ws" => "ws", // Noncompliant FP
            _ => throw new ArgumentException($"Cannot convert URI scheme '{uri.Scheme}' to a websocket scheme."),
        };
        return builder.Uri;
    }
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
