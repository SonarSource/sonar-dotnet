using System;
using static System.Console;

Person p = new ("rich", 98);

var maybeIll = p switch
{
    {temperature: <97 }                 => true,
    {temperature: >99}                  => true,
    {Name: "rich", temperature: var temp} when temp switch
    {
        >97 and <99     => true,
        _               => false
    }                                   => true,
    {temperature: >97 and <99}          => false,
    _                                   => false
};

WriteLine($"{p.Name} should go to the doctor: {maybeIll}");

public record Person(string Name, int temperature);
