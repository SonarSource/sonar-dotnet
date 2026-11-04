interface IDrawable { }
interface IFoo { }
interface IBar { }

closed class Shape { }
class Circle : Shape { }
class Square : IDrawable { }

class Cat { }
class Dog { }
union Pet(Cat, Dog) : IFoo;
class Foo : IFoo { }
class Bar : IBar { }

class Sample
{
    void Closed(Shape shape)
    {
        var drawable = (IDrawable)shape; // Noncompliant
    }

    void Union(IBar bar, IFoo foo, Pet pet)
    {
        var a = (IFoo)bar; // Noncompliant
        var b = (IFoo)pet;
        var c = (Pet)foo;
    }
}
