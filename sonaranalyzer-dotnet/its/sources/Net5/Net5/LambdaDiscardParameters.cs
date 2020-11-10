using System;
using System.Linq;

namespace Net5
{
    public class LambdaDiscardParameters
    {
        // Single _ => variable
        // Two or more = > discard
        public void Method()
        {
            var items = Enumerable.Range(0, 2).SelectMany((_, _) => Enumerable.Range(0, 1).SelectMany((_, _) => Enumerable.Range(0, 1))).ToList(); // i don't need parameters in nested
            items.ForEach(_ => Console.WriteLine($"This is a variable: {_}"));
            items.Select((_, _) => 0);

            _ = InvokeFunc((_, _) => true);
            _ = InvokeFunc((_, _) => { return true; });

            Func<int, string, int> explicitTypes2 = (int _, string _) => 1;
            Func<int, string, bool, int> explicitTypes3 = (int _, string _, bool _) => 1;

            LocalFunction(40, 1, 1);

            // These are not discards but parameter names
            int LocalFunction(int _, int _2, int __) => _ + _2 + __;
        }

        public Func<int, int, Func<int, int, bool>> Nested = (_ ,_) => (_,_) => true;

        private bool InvokeFunc(Func<bool, bool, bool> func) =>
            func(true, true);
    }     
}
