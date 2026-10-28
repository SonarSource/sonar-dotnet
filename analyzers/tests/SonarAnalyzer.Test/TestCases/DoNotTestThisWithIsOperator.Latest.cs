public class Cat;
public class Dog;

public union Pet(Cat, Dog)
{
    public string Describe() =>
        this switch // Noncompliant FP NET-4907
        {
            Cat => "cat", // Secondary
            Dog => "dog", // Secondary
        };

    public bool IsCat() => this is Cat; // Noncompliant FP NET-4907
}

public closed class Shape
{
    public string Describe() =>
        this switch // Noncompliant
        {
            Circle => "circle", // Secondary
            Square => "square", // Secondary
        };

    public bool IsCircle() => this is Circle; // Noncompliant
}

public class Circle : Shape;
public class Square : Shape;
