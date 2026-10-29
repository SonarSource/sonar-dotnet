namespace CSharp15
{
    public class Cat { }
    public class Dog { }

    public union my_pet(Cat, Dog);              // FN NET-4891
    public union Pet(Cat, Dog);                 // Compliant
    public closed class my_closed { }           // Noncompliant
}
