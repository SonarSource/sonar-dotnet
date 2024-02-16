using System;
using System.IO;
using System.Reflection;

Console.WriteLine("Hello World!");

LocalMethodInMain();

// S3928
if (args == null)
{
    throw new ArgumentException("args", "message");
}

// S2760
int a = 0, b = 1;
if (a == b) { LocalMethodInMain(); }
if (a == b) { AnotherLocalMethodInMain(); }
switch (b)
{
    case > 2:
        break;
}
switch (b)
{
    case > 2:
        break;
}

// S134
do
{
    try
    {
        while (true)
        {
            if (true)
            {
                try
                {
                    while (true)
                    {
                        if (true)
                        { }
                    }
                }
                catch { }
            }
        }
    }
    catch { }
} while (false);

// S3776
string field = null;
int one = 1, two = 2;
if ((one is 1 or 2) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5)
    || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5) || (two is 3 or 5))
{
    field = "";
}

// S3902
Assembly assem = Assembly.GetExecutingAssembly();

// S1199
{
    if (1 < 2)
    {
    }
}

// S2930
var fs0 = new FileStream(@"c:\foo.txt", FileMode.Open);

// S1192
string l1 = "foobar";
string l2 = "foobar";
string l3 = "foobar";
string l4 = "foobar";

// S3241
MyMethod();
int MyMethod() { return 42; }


// S2436
void LocalBar<T1, T2, T3, T4>() { }

// S1186
void LocalMethodInMain() { }

// S1541
void AnotherLocalMethodInMain() {
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
    if (false) { }
}

// S4823
Console.WriteLine(args[0]);

// S4144
void Method1()
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}
void Method2()
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

// S138
void LongFunction(int i)
{
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
    i++;
}

public record Record
{
    // S1144
    private int a;
}


