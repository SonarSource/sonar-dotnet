namespace Calculator;

public class Calculator
{
    public int Add(int a, int b, Predicate<int> predicate)
    {
        var sum = a + b;
        return predicate(sum)
            ? sum
            : 0;
    }
}
