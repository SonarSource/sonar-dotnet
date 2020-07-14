using System;
using Net5;

Console.WriteLine("Hello World!");
MyClass.Foo();

var x = new MyClass.PublicPerson { FirstName = "C#", LastName = "9" };
Console.WriteLine(x.FirstName + x.LastName);

LocalRecord y = new("1", "2");
Console.WriteLine(y.A + y.B);

public record LocalRecord(string A, string B);
