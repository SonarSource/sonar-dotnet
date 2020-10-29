using System;
using static System.Console;

var one = new One("a", 1);
var oneprime = one with {Prop1 = "b", Prop2 = 2};

WriteLine(one);
WriteLine(oneprime);
WriteLine($"Equality: {one == oneprime}");

var two = new Two("a") {Prop2 = 1};
var twoprime = two with {Prop1 = "b", Prop2 = 2};

WriteLine(two);
WriteLine(twoprime);

var three = new Three("a", 1);
var threeprime = three with {Prop1 = "b"};

WriteLine(three);
WriteLine(threeprime);

var four = new Four("a", 1);

WriteLine(four);

public record One(string Prop1, int Prop2);

public record OneEquivalent
{
    public OneEquivalent(string prop1, int prop2)
    {
        Prop1 = prop1;
        Prop2 = prop2;
    }

    public string Prop1 { get; init; }
    public int Prop2 { get; init; }
}

public record Two(string Prop1)
{
    public int Prop2 { get; init; }
}

public record Three
{
    public Three(string prop1, int prop2)
    {
        Prop1 = prop1;
        Prop2 = prop2;
    }
    
    public string Prop1 { get; init; }
    public int Prop2 { get; }
}

public record Four(string prop1, int prop2)
{
    public string Prop1 => prop1;
    public int Prop2 => prop2;
}
