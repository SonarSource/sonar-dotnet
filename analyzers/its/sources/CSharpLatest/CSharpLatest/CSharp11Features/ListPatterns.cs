namespace CSharpLatest.CSharp11Features;

internal class ListPatterns
{
    public void Method(string[][] data)
    {
        int[] one = { 1 };
        int[] odd = { 1, 3, 5 };
        int[] even = { 2, 4, 6 };
        int[] fib = { 1, 1, 2, 3, 5 };

        Console.WriteLine(odd is [1, 3, 5]); // true
        Console.WriteLine(even is [1, 3, 5]); // false (values)
        Console.WriteLine(one is [1, 3, 5]); // false (length)

        Console.WriteLine(odd is [1, _, _]); // true
        Console.WriteLine(odd is [_, 3, _]); // true
        Console.WriteLine(even is [_, _, 5]); // false (last value)

        Console.WriteLine(odd is [1, .., 3, _]); // true
        Console.WriteLine(fib is [1, .., 3, _]); // true

        Console.WriteLine(odd is [1, _, 5, ..]); // true
        Console.WriteLine(fib is [1, _, 5, ..]); // false

        Console.WriteLine(odd is [_, > 1, ..]); // true
        Console.WriteLine(even is [_, > 1, ..]); // true
        Console.WriteLine(fib is [_, > 1, ..]); // false

        decimal balance = 0m;
        foreach (var transaction in data)
        {
            balance += transaction switch
            {
                [_, "DEPOSIT", _, var amount] => decimal.Parse(amount),
                [_, "WITHDRAWAL", .., var amount] => -decimal.Parse(amount),
                [_, "INTEREST", var amount] => decimal.Parse(amount),
                [_, "FEE", var fee] => -decimal.Parse(fee),
                _ => throw new InvalidOperationException($"Record {transaction} is not in the expected format!"),
            };
            Console.WriteLine($"Record: {transaction}, New balance: {balance:C}");
        }
    }
}
