using System;
using System.Collections.Generic;
using System.Linq;

namespace Net5
{
    public class LambdaDiscardParameters
    {
        public void Method()
        {
            var items = Enumerable.Range(0, 2).SelectMany(_ => Enumerable.Range(0, 1).SelectMany(_ => Enumerable.Range(0, 1))).ToList(); // i don't need parameters in nested
            items.ForEach(_ => Console.WriteLine($"Discard test {_}"));

            LinqQuery(items);

            _ = Bar(_ => { return true; });

            Func<int, string, int> func = (int _, string _) => 1;

            LocalFunction(1, 1);

            void LocalFunction(int _, int _2) { }
        }

        private bool Bar(Func<bool, bool> func)
        {
            return func(true);
        }

        private IEnumerable<int> LinqQuery(List<int> list) =>
            from _ in NoOp()
            from k in list
            select k;

        private IEnumerable<int> NoOp() => new []{1};
    }     
}
